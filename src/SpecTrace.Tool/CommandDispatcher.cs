using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace SpecTrace.Tool;

public static class CommandDispatcher
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0];
        var rootPath = Path.GetFullPath(GetOption(args, "--root") ?? Directory.GetCurrentDirectory());
        var inputPath = ResolveInputPath(rootPath, GetOption(args, "--input-path"));

        try
        {
            return command switch
            {
                "validate" => await RunValidateAsync(rootPath, inputPath, GetOption(args, "--profile") ?? "core", GetOption(args, "--json-report")),
                "validate-edition" => RunValidateEdition(rootPath, GetOption(args, "--manifest"), GetOption(args, "--json-report")),
                "build-catalog" => await RunBuildCatalogAsync(rootPath, inputPath, GetOption(args, "--json-out")),
                "validate-evidence" => await RunValidateEvidenceAsync(rootPath, GetOptionValues(args, "--evidence-path")),
                "resolve-topic-view" => await RunResolveTopicViewAsync(
                    rootPath,
                    inputPath,
                    GetOption(args, "--topic-view-path"),
                    GetOption(args, "--topic-view-json")),
                "generate-attestation" => await RunGenerateAttestationAsync(
                    rootPath,
                    inputPath,
                    GetOption(args, "--profile") ?? "core",
                    GetOption(args, "--emit") ?? "both",
                    GetOption(args, "--out-dir") ?? SpecTraceAttestationService.DefaultOutputDirectory,
                    GetOptionValues(args, "--evidence-path")),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static async Task<int> RunValidateAsync(string rootPath, string? inputPath, string profile, string? jsonReportPath)
    {
        var artifacts = await CanonicalJsonLoader.LoadArtifactsAsync(rootPath, inputPath);
        var retiredLedger = await CanonicalJsonLoader.LoadRetiredLedgerAsync(rootPath);
        var report = RepositoryValidator.Validate(rootPath, profile, artifacts, retiredLedger);

        foreach (var finding in report.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        if (!string.IsNullOrWhiteSpace(jsonReportPath))
        {
            var reportPath = Path.GetFullPath(jsonReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
            File.WriteAllText(reportPath, report.ToJson());
        }

        if (report.HasErrors)
        {
            var errorCount = report.Findings.Count(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase));
            Console.Error.WriteLine($"Validation failed with {errorCount} error(s).");
            return 1;
        }

        Console.WriteLine($"Validation passed for {report.ArtifactCount} artifact(s) and {report.RequirementCount} requirement(s).");
        return 0;
    }

    private static int RunValidateEdition(string rootPath, string? manifestPath, string? jsonReportPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            throw new InvalidOperationException("Provide --manifest <path>.");
        }

        var fullManifestPath = Path.GetFullPath(Path.IsPathRooted(manifestPath) ? manifestPath : Path.Combine(rootPath, manifestPath));
        var manifest = JsonSchemaValidator.Load(rootPath).LoadStandardEdition(rootPath, fullManifestPath);
        var conformanceCases = ValidateVersioningConformanceCorpus(rootPath);
        var subjectSha256 = ComputeValidationSubjectSha256(manifest);
        var report = JsonSerializer.Serialize(new
        {
            result = "passed",
            validator = "spec-trace-tool-json-schema",
            version = typeof(CommandDispatcher).Assembly.GetName().Version?.ToString() ?? "unknown",
            edition = manifest.GetProperty("edition").GetString(),
            specifications = manifest.GetProperty("specificationOrder").GetArrayLength(),
            requirements = manifest.GetProperty("requirementRevisions").GetArrayLength(),
            conformanceCases,
            validationSubjectSha256 = subjectSha256,
        }, new JsonSerializerOptions { WriteIndented = false }) + Environment.NewLine;

        if (!string.IsNullOrWhiteSpace(jsonReportPath))
        {
            var fullReportPath = Path.GetFullPath(jsonReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullReportPath)!);
            File.WriteAllText(fullReportPath, report, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        Console.Write(report);
        return 0;
    }

    private static int ValidateVersioningConformanceCorpus(string rootPath)
    {
        var corpusPath = Path.Combine(rootPath, "conformance", "versioning");
        var paths = Directory.GetFiles(corpusPath, "*.json").OrderBy(path => path, StringComparer.Ordinal).ToList();
        if (paths.Count == 0)
        {
            throw new InvalidOperationException("Versioning conformance corpus is empty.");
        }

        foreach (var path in paths)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            var kind = root.TryGetProperty("kind", out var kindValue) ? kindValue.GetString() : "versioning";
            if (!string.Equals(kind, "versioning", StringComparison.Ordinal))
            {
                var nonVersioningValid = kind switch
                {
                    "edition-pin-mutation" => ValidatePinMutationCase(root),
                    "interchange-roundtrip" => ValidateInterchangeCase(root),
                    "safe-mutation-protocol" => ValidateProtocolCase(root),
                    _ => throw new InvalidOperationException($"Unsupported conformance case kind '{kind}' in {Path.GetFileName(path)}."),
                };
                if (nonVersioningValid != root.GetProperty("valid").GetBoolean())
                {
                    throw new InvalidOperationException($"Versioning conformance case produced the wrong result: {Path.GetFileName(path)}");
                }
                continue;
            }

            var edition = root.GetProperty("edition").GetString() ?? string.Empty;
            var maturity = root.GetProperty("maturity").GetString() ?? string.Empty;
            var bootstrap = root.GetProperty("bootstrap").GetBoolean();
            var revision = root.GetProperty("requirement").GetProperty("revision").GetInt32();
            var match = System.Text.RegularExpressions.Regex.Match(edition, "^(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)(?:-(draft|rc)\\.([1-9][0-9]*))?$");
            var valid = match.Success;
            if (match.Success)
            {
                var prerelease = match.Groups[4].Value;
                var expectedMaturity = prerelease == "draft" ? "draft" : prerelease == "rc" ? "candidate" : "stable";
                valid = string.Equals(maturity, expectedMaturity, StringComparison.Ordinal);
            }

            valid = valid && revision >= 1;
            if (!bootstrap)
            {
                valid = valid && root.TryGetProperty("previousEdition", out var previous) && !string.IsNullOrWhiteSpace(previous.GetString())
                    && root.TryGetProperty("behavioralReview", out var review) && !string.IsNullOrWhiteSpace(review.GetString());
            }

            if (valid != root.GetProperty("valid").GetBoolean())
            {
                throw new InvalidOperationException($"Versioning conformance case produced the wrong result: {Path.GetFileName(path)}");
            }
        }

        return paths.Count;
    }

    private static bool ValidatePinMutationCase(JsonElement root)
    {
        var pin = root.GetProperty("trustedPin");
        var mutation = root.GetProperty("mutation");
        var field = mutation.GetProperty("field").GetString() ?? string.Empty;
        if (!pin.TryGetProperty(field, out var original) || mutation.GetProperty("operation").GetString() == "remove")
        {
            return false;
        }

        return mutation.TryGetProperty("value", out var replacement) && CanonicalEquals(original, replacement);
    }

    private static bool ValidateInterchangeCase(JsonElement root)
    {
        var input = root.GetProperty("inputRequirement");
        var output = root.GetProperty("outputRequirement");
        var revision = input.GetProperty("revision").GetInt32();
        var providerVersion = root.GetProperty("providerRecordVersion").GetInt32();
        var preserved = input.GetProperty("id").GetString() == output.GetProperty("id").GetString()
            && revision == output.GetProperty("revision").GetInt32()
            && CanonicalEquals(input.GetProperty("trace"), output.GetProperty("trace"));
        var canonicalEquivalent = !root.TryGetProperty("canonicalA", out var first)
            || root.TryGetProperty("canonicalB", out var second) && CanonicalEquals(first, second);
        return revision >= 1 && providerVersion >= 1 && revision != providerVersion && preserved && canonicalEquivalent;
    }

    private static bool ValidateProtocolCase(JsonElement root)
    {
        var scenario = root.GetProperty("scenario").GetString();
        var rest = root.GetProperty("restOutcome").GetString();
        var mcp = root.GetProperty("mcpOutcome").GetString();
        if (!string.Equals(rest, mcp, StringComparison.Ordinal))
        {
            return false;
        }

        return scenario switch
        {
            "stale-commit" or "atomic-failure" => rest == "rejected" && !root.GetProperty("partialMutation").GetBoolean(),
            "deterministic-replay" => root.GetProperty("serverDefaultsExcluded").GetBoolean()
                && CanonicalEquals(root.GetProperty("logicalRequest"), root.GetProperty("replayedLogicalRequest")),
            "cancel" => rest == "canceled" && !root.GetProperty("partialMutation").GetBoolean(),
            "resume" => rest == "committed" && root.GetProperty("atomicCommit").GetBoolean(),
            _ => false,
        };
    }

    private static bool CanonicalEquals(JsonElement left, JsonElement right)
    {
        using var leftStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(leftStream)) WriteCanonicalValue(writer, left);
        using var rightStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(rightStream)) WriteCanonicalValue(writer, right);
        return leftStream.ToArray().AsSpan().SequenceEqual(rightStream.ToArray());
    }

    private static string ComputeValidationSubjectSha256(JsonElement manifest)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteCanonicalObject(writer, manifest, new HashSet<string>(StringComparer.Ordinal) { "validatedBy", "approvalReference", "signature" });
        }

        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    private static void WriteCanonicalObject(Utf8JsonWriter writer, JsonElement element, ISet<string>? excludedProperties = null)
    {
        writer.WriteStartObject();
        foreach (var property in element.EnumerateObject().Where(property => excludedProperties is null || !excludedProperties.Contains(property.Name)).OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            writer.WritePropertyName(property.Name);
            WriteCanonicalValue(writer, property.Value);
        }
        writer.WriteEndObject();
    }

    private static void WriteCanonicalValue(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                WriteCanonicalObject(writer, element);
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray()) WriteCanonicalValue(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static async Task<int> RunValidateEvidenceAsync(string rootPath, IReadOnlyList<string> evidencePaths)
    {
        var artifacts = await CanonicalJsonLoader.LoadArtifactsAsync(rootPath, inputPath: null);
        var catalog = new ArtifactCatalog(artifacts);
        var discovery = EvidenceValidator.DiscoverEvidenceFiles(rootPath, evidencePaths);

        foreach (var finding in discovery.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        if (discovery.Findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase)))
        {
            Console.Error.WriteLine("Evidence discovery failed.");
            return 1;
        }

        if (discovery.Files.Count == 0)
        {
            Console.WriteLine("No evidence files found.");
            return 0;
        }

        var validation = await EvidenceValidator.ValidateAsync(rootPath, discovery.Files, catalog, CancellationToken.None);
        foreach (var finding in validation.Report.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        if (validation.Report.HasErrors)
        {
            Console.Error.WriteLine("Evidence validation failed.");
            return 1;
        }

        Console.WriteLine($"Evidence validation passed for {validation.Report.FileCount} file(s), {validation.Report.RequirementCount} requirement entr{(validation.Report.RequirementCount == 1 ? "y" : "ies")}, and {validation.Report.ObservationCount} observation(s).");
        return 0;
    }

    private static async Task<int> RunBuildCatalogAsync(string rootPath, string? inputPath, string? jsonOutputPath)
    {
        var artifacts = await CanonicalJsonLoader.LoadArtifactsAsync(rootPath, inputPath);
        var retiredLedger = await CanonicalJsonLoader.LoadRetiredLedgerAsync(rootPath);
        var report = RepositoryValidator.Validate(rootPath, "core", artifacts, retiredLedger);

        foreach (var finding in report.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        if (report.HasErrors)
        {
            Console.Error.WriteLine("Refusing to build the catalog because core validation failed.");
            return 1;
        }

        var catalog = new ArtifactCatalog(artifacts);
        var snapshot = catalog.CreateSnapshot(rootPath, artifacts);

        var resolvedJsonOutput = Path.GetFullPath(jsonOutputPath ?? Path.Combine(rootPath, "specs", "generated", "spec-trace-catalog.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(resolvedJsonOutput)!);
        File.WriteAllText(resolvedJsonOutput, snapshot.ToJson());

        Console.WriteLine($"Wrote {Path.GetRelativePath(rootPath, resolvedJsonOutput).Replace('\\', '/')}");
        return 0;
    }

    private static async Task<int> RunGenerateAttestationAsync(
        string rootPath,
        string? inputPath,
        string profile,
        string emit,
        string outDir,
        IReadOnlyList<string> evidencePaths)
    {
        var scopedArtifacts = await CanonicalJsonLoader.LoadArtifactsAsync(rootPath, inputPath);
        var fullArtifacts = string.IsNullOrWhiteSpace(inputPath)
            ? scopedArtifacts
            : await CanonicalJsonLoader.LoadArtifactsAsync(rootPath, inputPath: null);

        var evidenceCatalog = new ArtifactCatalog(fullArtifacts);
        var discovery = EvidenceValidator.DiscoverEvidenceFiles(rootPath, evidencePaths);
        foreach (var finding in discovery.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        if (discovery.Findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase)))
        {
            Console.Error.WriteLine("Evidence discovery failed.");
            return 1;
        }

        var evidenceValidation = await EvidenceValidator.ValidateAsync(rootPath, discovery.Files, evidenceCatalog, CancellationToken.None);
        foreach (var finding in evidenceValidation.Report.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        if (evidenceValidation.Report.HasErrors)
        {
            Console.Error.WriteLine("Refusing to generate the attestation report because evidence validation failed.");
            return 1;
        }

        var retiredLedger = await CanonicalJsonLoader.LoadRetiredLedgerAsync(rootPath);
        var report = RepositoryValidator.Validate(rootPath, profile, scopedArtifacts, retiredLedger);
        foreach (var finding in report.Findings)
        {
            Console.WriteLine($"{finding.Severity}: {finding.Code} {finding.Message}");
        }

        var catalog = new ArtifactCatalog(scopedArtifacts);
        var result = SpecTraceAttestationService.Generate(
            rootPath,
            inputPath,
            profile,
            emit,
            outDir,
            scopedArtifacts,
            catalog,
            report,
            evidenceValidation.Snapshots);

        if (!string.IsNullOrWhiteSpace(result.SummaryHtmlPath))
        {
            Console.WriteLine($"Summary HTML: {Path.GetRelativePath(rootPath, result.SummaryHtmlPath).Replace('\\', '/')}");
        }

        if (!string.IsNullOrWhiteSpace(result.DetailsHtmlPath))
        {
            Console.WriteLine($"Details HTML: {Path.GetRelativePath(rootPath, result.DetailsHtmlPath).Replace('\\', '/')}");
        }

        if (!string.IsNullOrWhiteSpace(result.JsonPath))
        {
            Console.WriteLine($"Attestation JSON: {Path.GetRelativePath(rootPath, result.JsonPath).Replace('\\', '/')}");
        }

        if (report.HasErrors)
        {
            Console.Error.WriteLine("Attestation generated with validation errors.");
            return 1;
        }

        Console.WriteLine($"Attestation generated for {result.Snapshot.Aggregates.SpecificationCount} specification(s) and {result.Snapshot.Aggregates.RequirementCount} requirement(s).");
        return 0;
    }

    private static async Task<int> RunResolveTopicViewAsync(string rootPath, string? inputPath, string? topicViewPath, string? topicViewJson)
    {
        var hasPath = !string.IsNullOrWhiteSpace(topicViewPath);
        var hasJson = !string.IsNullOrWhiteSpace(topicViewJson);

        if (hasPath == hasJson)
        {
            Console.Error.WriteLine("Provide exactly one of --topic-view-path or --topic-view-json.");
            return 1;
        }

        var artifacts = await CanonicalJsonLoader.LoadArtifactsAsync(rootPath, inputPath);
        var schemaValidator = JsonSchemaValidator.Load(rootPath);
        var resolvedTopicViewPath = hasPath
            ? ResolveTopicViewPath(rootPath, topicViewPath!)
            : null;
        var topicViewInput = hasPath
            ? new TopicViewInputModel
            {
                Kind = "path",
                Path = CanonicalJsonLoader.NormalizeRepoPath(rootPath, resolvedTopicViewPath!),
            }
            : new TopicViewInputModel
            {
                Kind = "inline",
            };

        var topicViewDefinition = hasPath
            ? schemaValidator.LoadTopicViewDefinition(rootPath, resolvedTopicViewPath!)
            : LoadTopicViewFromJson(rootPath, topicViewJson!);

        var result = TopicViewResolver.Resolve(rootPath, topicViewInput, topicViewDefinition, artifacts);
        Console.WriteLine(result.ToJson());
        return 0;
    }

    private static string? GetOption(string[] args, string optionName)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static IReadOnlyList<string> GetOptionValues(string[] args, string optionName)
    {
        var values = new List<string>();
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (!string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var candidate = args[index + 1];
            if (candidate.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            values.Add(candidate);
        }

        return values;
    }

    private static string? ResolveInputPath(string rootPath, string? inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return null;
        }

        return Path.GetFullPath(Path.IsPathRooted(inputPath) ? inputPath : Path.Combine(rootPath, inputPath));
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command '{command}'.");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- validate [--root <path>] [--input-path <path>] [--profile core|traceable|auditable] [--json-report <path>]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- validate-edition --manifest <path> [--root <path>] [--json-report <path>]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- build-catalog [--root <path>] [--input-path <path>] [--json-out <path>]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- validate-evidence [--root <path>] [--evidence-path <file-or-dir>]...");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- resolve-topic-view [--root <path>] [--input-path <path>] (--topic-view-path <path> | --topic-view-json <json>)");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- generate-attestation [--root <path>] [--input-path <path>] [--profile core|traceable|auditable] [--emit html|json|both] [--out-dir <path>] [--evidence-path <file-or-dir>]...");
    }

    private static JsonElement LoadTopicViewFromJson(string rootPath, string json)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"spec-trace-topic-view-{Guid.NewGuid():N}.json");
        File.WriteAllText(tempPath, json);

        try
        {
            return JsonSchemaValidator.Load(rootPath).LoadTopicViewDefinition(rootPath, tempPath);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static string ResolveTopicViewPath(string rootPath, string topicViewPath)
    {
        var fullPath = Path.IsPathRooted(topicViewPath)
            ? topicViewPath
            : Path.Combine(rootPath, topicViewPath);

        return Path.GetFullPath(fullPath);
    }
}
