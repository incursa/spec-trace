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
                "migrate-markdown" => await RunMigrateMarkdownAsync(rootPath, inputPath),
                "generate-markdown" => await RunGenerateMarkdownAsync(rootPath, inputPath, HasFlag(args, "--check")),
                "validate" => await RunValidateAsync(rootPath, inputPath, GetOption(args, "--profile") ?? "core", GetOption(args, "--json-report")),
                "build-catalog" => await RunBuildCatalogAsync(rootPath, inputPath, GetOption(args, "--json-out"), GetOption(args, "--cue-out")),
                "validate-evidence" => await RunValidateEvidenceAsync(rootPath, GetOptionValues(args, "--evidence-path")),
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

    private static async Task<int> RunMigrateMarkdownAsync(string rootPath, string? inputPath)
    {
        foreach (var markdownFile in DiscoverArtifactMarkdownFiles(rootPath, inputPath))
        {
            var artifact = MarkdownArtifactParser.ParseFile(markdownFile);
            var cuePath = Path.ChangeExtension(markdownFile, ".cue");
            File.WriteAllText(cuePath, CueArtifactWriter.WriteArtifact(artifact));
            Console.WriteLine($"Wrote {Path.GetRelativePath(rootPath, cuePath)}");
        }

        return await Task.FromResult(0);
    }

    private static async Task<int> RunGenerateMarkdownAsync(string rootPath, string? inputPath, bool check)
    {
        var artifacts = await LoadCueArtifactsAsync(rootPath, inputPath);
        var catalog = new ArtifactCatalog(artifacts);
        var mismatches = new List<string>();

        foreach (var (cuePath, artifact) in artifacts)
        {
            var markdownPath = Path.ChangeExtension(cuePath, ".md");
            var generatedMarkdown = MarkdownArtifactWriter.WriteArtifact(artifact, catalog, cuePath);

            if (check)
            {
                var existingMarkdown = File.Exists(markdownPath) ? File.ReadAllText(markdownPath) : null;
                if (!string.Equals(existingMarkdown, generatedMarkdown, StringComparison.Ordinal))
                {
                    mismatches.Add(Path.GetRelativePath(rootPath, markdownPath).Replace('\\', '/'));
                }

                continue;
            }

            File.WriteAllText(markdownPath, generatedMarkdown);
            Console.WriteLine($"Generated {Path.GetRelativePath(rootPath, markdownPath)}");
        }

        if (check)
        {
            if (mismatches.Count > 0)
            {
                Console.Error.WriteLine("Generated Markdown is out of date:");
                foreach (var mismatch in mismatches)
                {
                    Console.Error.WriteLine($"  {mismatch}");
                }

                return 1;
            }

            Console.WriteLine("Generated Markdown is up to date.");
        }

        return 0;
    }

    private static async Task<int> RunValidateAsync(string rootPath, string? inputPath, string profile, string? jsonReportPath)
    {
        var artifacts = await LoadCueArtifactsAsync(rootPath, inputPath);
        var retiredLedger = await LoadRetiredLedgerAsync(rootPath);
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

    private static async Task<int> RunValidateEvidenceAsync(string rootPath, IReadOnlyList<string> evidencePaths)
    {
        var artifacts = await LoadCueArtifactsAsync(rootPath, inputPath: null);
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

    private static async Task<int> RunBuildCatalogAsync(string rootPath, string? inputPath, string? jsonOutputPath, string? cueOutputPath)
    {
        var artifacts = await LoadCueArtifactsAsync(rootPath, inputPath);
        var retiredLedger = await LoadRetiredLedgerAsync(rootPath);
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
        var snapshot = catalog.CreateSnapshot(artifacts);

        var resolvedJsonOutput = Path.GetFullPath(jsonOutputPath ?? Path.Combine(rootPath, "specs", "generated", "spec-trace-catalog.json"));
        var resolvedCueOutput = Path.GetFullPath(cueOutputPath ?? Path.Combine(rootPath, "specs", "generated", "spec-trace-catalog.cue"));

        Directory.CreateDirectory(Path.GetDirectoryName(resolvedJsonOutput)!);
        Directory.CreateDirectory(Path.GetDirectoryName(resolvedCueOutput)!);

        File.WriteAllText(resolvedJsonOutput, snapshot.ToJson());
        File.WriteAllText(resolvedCueOutput, CatalogSnapshotWriter.WriteCue(snapshot));

        Console.WriteLine($"Wrote {Path.GetRelativePath(rootPath, resolvedJsonOutput).Replace('\\', '/')}");
        Console.WriteLine($"Wrote {Path.GetRelativePath(rootPath, resolvedCueOutput).Replace('\\', '/')}");
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
        var scopedArtifacts = await LoadCueArtifactsAsync(rootPath, inputPath);
        var fullArtifacts = string.IsNullOrWhiteSpace(inputPath)
            ? scopedArtifacts
            : await LoadCueArtifactsAsync(rootPath, inputPath: null);

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

        var retiredLedger = await LoadRetiredLedgerAsync(rootPath);
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

    private static async Task<List<(string CuePath, ArtifactModel Artifact)>> LoadCueArtifactsAsync(string rootPath, string? inputPath)
    {
        var cueFiles = EnumerateCueFiles(rootPath, inputPath)
            .Select(Path.GetFullPath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var artifacts = new List<(string CuePath, ArtifactModel Artifact)>();
        foreach (var cueFile in cueFiles)
        {
            var relativeCuePath = Path.GetRelativePath(rootPath, cueFile).Replace('\\', '/');
            var artifact = await CueCli.ExportArtifactAsync(rootPath, relativeCuePath, CancellationToken.None);
            artifacts.Add((cueFile, artifact));
        }

        return artifacts;
    }

    private static IEnumerable<string> EnumerateCueFiles(string rootPath, string? inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return EnumerateCueFilesInDirectory(Path.Combine(rootPath, "specs"), rootPath)
                .Concat(EnumerateCueFilesInDirectory(Path.Combine(rootPath, "examples"), rootPath));
        }

        if (File.Exists(inputPath))
        {
            return ShouldIncludeCueFile(rootPath, inputPath) ? [inputPath] : [];
        }

        if (!Directory.Exists(inputPath))
        {
            return [];
        }

        return EnumerateCueFilesInDirectory(inputPath, rootPath);
    }

    private static IEnumerable<string> EnumerateCueFilesInDirectory(string directory, string rootPath)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateFiles(directory, "*.cue", SearchOption.AllDirectories)
            .Where(path => ShouldIncludeCueFile(rootPath, path));
    }

    private static bool ShouldIncludeCueFile(string rootPath, string path)
    {
        if (path.Contains($"{Path.DirectorySeparatorChar}generated{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = Path.GetRelativePath(rootPath, path).Replace('\\', '/');
        return relativePath.StartsWith("specs/", StringComparison.OrdinalIgnoreCase) ||
               relativePath.StartsWith("examples/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<RetiredRequirementLedger?> LoadRetiredLedgerAsync(string rootPath)
    {
        var ledgerPath = Path.Combine(rootPath, "catalog", "retired-requirements.cue");
        if (!File.Exists(ledgerPath))
        {
            return null;
        }

        var relativePath = Path.GetRelativePath(rootPath, ledgerPath).Replace('\\', '/');
        return await CueCli.ExportRetiredLedgerAsync(rootPath, relativePath, CancellationToken.None);
    }

    private static List<string> DiscoverArtifactMarkdownFiles(string rootPath, string? inputPath)
    {
        var markdownFiles = string.IsNullOrWhiteSpace(inputPath)
            ? EnumerateArtifactMarkdownFiles(Path.Combine(rootPath, "specs"), rootPath)
                .Concat(EnumerateArtifactMarkdownFiles(Path.Combine(rootPath, "examples"), rootPath))
            : EnumerateArtifactMarkdownFiles(inputPath, rootPath);

        return markdownFiles
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}generated{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => Path.GetFileName(path) is not "_index.md" and not "README.md" and not "source-notes.md" and not "REQUIREMENT-GAPS.md")
            .Where(IsArtifactMarkdownFile)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<string> EnumerateArtifactMarkdownFiles(string path, string rootPath)
    {
        if (File.Exists(path))
        {
            var relativePath = Path.GetRelativePath(rootPath, path).Replace('\\', '/');
            if (relativePath.StartsWith("specs/", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("examples/", StringComparison.OrdinalIgnoreCase))
            {
                return [Path.GetFullPath(path)];
            }

            return [];
        }

        if (!Directory.Exists(path))
        {
            return [];
        }

        return Directory.EnumerateFiles(path, "*.md", SearchOption.AllDirectories);
    }

    private static bool IsArtifactMarkdownFile(string path)
    {
        var lines = File.ReadLines(path).Take(12).ToList();
        return lines.Any(line => line.StartsWith("artifact_id:", StringComparison.Ordinal)) &&
               lines.Any(line => line.StartsWith("artifact_type:", StringComparison.Ordinal));
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

    private static bool HasFlag(string[] args, string flagName)
    {
        return args.Any(arg => string.Equals(arg, flagName, StringComparison.OrdinalIgnoreCase));
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
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- migrate-markdown [--root <path>] [--input-path <path>]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- generate-markdown [--root <path>] [--input-path <path>] [--check]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- validate [--root <path>] [--input-path <path>] [--profile core|traceable|auditable] [--json-report <path>]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- build-catalog [--root <path>] [--input-path <path>] [--json-out <path>] [--cue-out <path>]");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- validate-evidence [--root <path>] [--evidence-path <file-or-dir>]...");
        Console.WriteLine("  dotnet run --project src/SpecTrace.Tool -- generate-attestation [--root <path>] [--input-path <path>] [--profile core|traceable|auditable] [--emit html|json|both] [--out-dir <path>] [--evidence-path <file-or-dir>]...");
    }
}
