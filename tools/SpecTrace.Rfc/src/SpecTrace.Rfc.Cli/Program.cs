using System.Text.Json;
using System.Text.RegularExpressions;
using SpecTrace.Rfc.Ai;
using SpecTrace.Rfc.Core;

return await RfcCommandDispatcher.RunAsync(args);

internal static class RfcCommandDispatcher
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        var command = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();

        try
        {
            return command switch
            {
                "ingest" => await RunIngestAsync(rest),
                "segment" => await RunSegmentAsync(rest),
                "extract" => await RunExtractAsync(rest),
                "review-pack" => await RunReviewPackAsync(rest),
                "coverage-audit" => await RunCoverageAuditAsync(rest),
                "normalize" => await RunNormalizeAsync(rest),
                "assemble" => await RunAssembleAsync(rest),
                "validate" => await RunValidateAsync(rest),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static async Task<int> RunIngestAsync(string[] args)
    {
        var outPath = Required(args, "--out");
        var document = await new RfcIntakeService().IngestAsync(new RfcIngestOptions
        {
            RfcNumber = GetOption(args, "--rfc"),
            Source = GetOption(args, "--source"),
            SourceId = GetOption(args, "--source-id"),
            Title = GetOption(args, "--title"),
        });

        await RfcIntakeService.WriteAsync(outPath, document);
        Console.WriteLine($"Wrote {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static async Task<int> RunSegmentAsync(string[] args)
    {
        var sourcePath = Required(args, "--source");
        var outPath = Required(args, "--out");
        var document = await RfcIntakeService.ReadAsync(sourcePath);
        var units = RfcSegmenter.Segment(document);
        await Jsonl.WriteAsync(outPath, units);
        Console.WriteLine($"Wrote {units.Count} source unit(s) to {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static async Task<int> RunExtractAsync(string[] args)
    {
        var toolRoot = FindToolRoot();
        var ledgerPath = Required(args, "--ledger");
        var outPath = Required(args, "--out");
        var promptPath = GetOption(args, "--prompt") ?? Path.Combine(toolRoot, "prompts", "extract-requirements.md");
        var schemaPath = GetOption(args, "--schema") ?? Path.Combine(toolRoot, "schemas", "candidate-requirements.schema.json");
        var batchSize = int.Parse(GetOption(args, "--batch-size") ?? "25");

        var count = await new CodexCliRequirementExtractor().ExtractAsync(new CodexExtractionOptions
        {
            LedgerPath = ledgerPath,
            OutputPath = outPath,
            PromptPath = promptPath,
            SchemaPath = schemaPath,
            BatchSize = batchSize,
            MinBatchSize = int.Parse(GetOption(args, "--adaptive-min-batch-size") ?? "1"),
            MaxBatchRetries = int.Parse(GetOption(args, "--max-batch-retries") ?? "1"),
            BatchTimeoutSeconds = int.Parse(GetOption(args, "--batch-timeout-seconds") ?? "300"),
            ExtractionScope = GetOption(args, "--extraction-scope") ?? "candidate-units",
            DeterministicExtractionMode = GetOption(args, "--deterministic-extraction") ?? "off",
            AiMode = GetOption(args, "--ai-mode") ?? "codex",
            CodexCommand = GetOption(args, "--codex") ?? "codex",
            Model = GetOption(args, "--model") ?? "gpt-5.4-mini",
            ReasoningEffort = GetOption(args, "--reasoning-effort") ?? "high",
            RetryReasoningEffort = GetOption(args, "--retry-reasoning-effort") ?? "xhigh",
            WorkingDirectory = GetOption(args, "--workdir") ?? Directory.GetCurrentDirectory(),
            RawOutputDirectory = GetOption(args, "--raw-out-dir"),
            BatchOutputDirectory = GetOption(args, "--batch-out-dir"),
            Resume = HasSwitch(args, "--resume"),
        });

        Console.WriteLine($"Wrote {count} candidate decision(s) to {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static async Task<int> RunReviewPackAsync(string[] args)
    {
        var ledgerPath = Required(args, "--ledger");
        var candidatesPath = Required(args, "--candidates");
        var outPath = Required(args, "--out");

        var ledger = await Jsonl.ReadAsync<SourceUnit>(ledgerPath);
        var candidates = (await Jsonl.ReadAsync<CandidateDecision>(candidatesPath))
            .ToDictionary(candidate => candidate.SourceUnitId, StringComparer.Ordinal);

        await ReviewPackRenderer.RenderAsync(outPath, ledger, candidates);
        Console.WriteLine($"Wrote {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static async Task<int> RunCoverageAuditAsync(string[] args)
    {
        var toolRoot = FindToolRoot();
        var ledgerPath = Required(args, "--ledger");
        var candidatesPath = Required(args, "--candidates");
        var outPath = Required(args, "--out");
        var reportPath = GetOption(args, "--report-out");
        var promptPath = GetOption(args, "--prompt") ?? Path.Combine(toolRoot, "prompts", "coverage-audit.md");
        var schemaPath = GetOption(args, "--schema") ?? Path.Combine(toolRoot, "schemas", "review-batch.schema.json");
        var batchSize = int.Parse(GetOption(args, "--batch-size") ?? "25");

        var count = await new CodexCliCoverageAuditor().AuditAsync(new CodexAuditOptions
        {
            LedgerPath = ledgerPath,
            CandidatePath = candidatesPath,
            OutputPath = outPath,
            ReportPath = reportPath,
            PromptPath = promptPath,
            SchemaPath = schemaPath,
            BatchSize = batchSize,
            MinBatchSize = int.Parse(GetOption(args, "--adaptive-min-batch-size") ?? "1"),
            MaxBatchRetries = int.Parse(GetOption(args, "--max-batch-retries") ?? "1"),
            BatchTimeoutSeconds = int.Parse(GetOption(args, "--batch-timeout-seconds") ?? "300"),
            AiMode = GetOption(args, "--ai-mode") ?? "codex",
            CodexCommand = GetOption(args, "--codex") ?? "codex",
            Model = GetOption(args, "--model") ?? "gpt-5.4-mini",
            ReasoningEffort = GetOption(args, "--reasoning-effort") ?? "high",
            RetryReasoningEffort = GetOption(args, "--retry-reasoning-effort") ?? "xhigh",
            WorkingDirectory = GetOption(args, "--workdir") ?? Directory.GetCurrentDirectory(),
            RawOutputDirectory = GetOption(args, "--raw-out-dir"),
            BatchOutputDirectory = GetOption(args, "--batch-out-dir"),
            Resume = HasSwitch(args, "--resume"),
        });

        Console.WriteLine($"Wrote {count} review decision(s) to {Path.GetFullPath(outPath)}");
        if (!string.IsNullOrWhiteSpace(reportPath))
        {
            Console.WriteLine($"Wrote {Path.GetFullPath(reportPath)}");
        }

        return 0;
    }

    private static async Task<int> RunNormalizeAsync(string[] args)
    {
        var toolRoot = FindToolRoot();
        var ledgerPath = Required(args, "--ledger");
        var reviewPath = Required(args, "--review");
        var outPath = Required(args, "--out");
        var promptPath = GetOption(args, "--prompt") ?? Path.Combine(toolRoot, "prompts", "normalize-requirement.md");
        var schemaPath = GetOption(args, "--schema") ?? Path.Combine(toolRoot, "schemas", "review-batch.schema.json");
        var batchSize = int.Parse(GetOption(args, "--batch-size") ?? "25");

        var count = await new CodexCliRequirementNormalizer().NormalizeAsync(new CodexNormalizeOptions
        {
            LedgerPath = ledgerPath,
            ReviewPath = reviewPath,
            OutputPath = outPath,
            PromptPath = promptPath,
            SchemaPath = schemaPath,
            BatchSize = batchSize,
            MinBatchSize = int.Parse(GetOption(args, "--adaptive-min-batch-size") ?? "1"),
            MaxBatchRetries = int.Parse(GetOption(args, "--max-batch-retries") ?? "1"),
            BatchTimeoutSeconds = int.Parse(GetOption(args, "--batch-timeout-seconds") ?? "300"),
            AiMode = GetOption(args, "--ai-mode") ?? "codex",
            CodexCommand = GetOption(args, "--codex") ?? "codex",
            Model = GetOption(args, "--model") ?? "gpt-5.4-mini",
            ReasoningEffort = GetOption(args, "--reasoning-effort") ?? "high",
            RetryReasoningEffort = GetOption(args, "--retry-reasoning-effort") ?? "xhigh",
            WorkingDirectory = GetOption(args, "--workdir") ?? Directory.GetCurrentDirectory(),
            RawOutputDirectory = GetOption(args, "--raw-out-dir"),
            BatchOutputDirectory = GetOption(args, "--batch-out-dir"),
            Resume = HasSwitch(args, "--resume"),
        });

        Console.WriteLine($"Wrote {count} normalized review decision(s) to {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static async Task<int> RunAssembleAsync(string[] args)
    {
        var ledgerPath = Required(args, "--ledger");
        var outPath = Required(args, "--out");
        var specId = Required(args, "--spec-id");
        var candidatesPath = GetOption(args, "--candidates");
        var reviewPath = GetOption(args, "--review");

        if (string.IsNullOrWhiteSpace(candidatesPath) == string.IsNullOrWhiteSpace(reviewPath))
        {
            throw new InvalidOperationException("Provide exactly one of --candidates or --review.");
        }

        var ledger = (await Jsonl.ReadAsync<SourceUnit>(ledgerPath))
            .ToDictionary(unit => unit.SourceUnitId, StringComparer.Ordinal);
        var options = BuildAssemblyOptions(args, specId);
        var artifact = !string.IsNullOrWhiteSpace(reviewPath)
            ? SpecAssembler.AssembleFromReviewDecisions(await Jsonl.ReadAsync<ReviewDecision>(reviewPath), ledger, options)
            : SpecAssembler.AssembleFromCandidates(await Jsonl.ReadAsync<CandidateDecision>(candidatesPath!), ledger, options);

        await SpecAssembler.WriteAsync(outPath, artifact);
        Console.WriteLine($"Wrote {artifact.Requirements.Count} requirement(s) to {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static async Task<int> RunValidateAsync(string[] args)
    {
        var rootPath = GetOption(args, "--root") ?? Directory.GetCurrentDirectory();
        var inputPath = GetOption(args, "--input-path");
        var profile = GetOption(args, "--profile") ?? "core";
        return await SpecTraceValidationRunner.RunAsync(rootPath, inputPath, profile);
    }

    private static SpecAssemblyOptions BuildAssemblyOptions(string[] args, string specId)
    {
        var domain = GetOption(args, "--domain") ?? InferDomain(specId);
        return new SpecAssemblyOptions
        {
            SpecId = specId,
            Domain = domain,
            Capability = GetOption(args, "--capability") ?? domain,
            Title = GetOption(args, "--title") ?? $"{specId} Requirements",
            Owner = GetOption(args, "--owner") ?? "spec-maintainers",
            Purpose = GetOption(args, "--purpose") ?? "Capture requirements extracted from RFC source units.",
            Status = GetOption(args, "--status") ?? "draft",
            Context = GetOption(args, "--context"),
            RequirementPrefix = GetOption(args, "--requirement-prefix"),
            IdStyle = GetOption(args, "--id-style") ?? "section",
            IgnoreIdHints = HasSwitch(args, "--ignore-id-hints"),
        };
    }

    private static string InferDomain(string specId)
    {
        var value = specId.StartsWith("SPEC-", StringComparison.Ordinal) ? specId["SPEC-".Length..] : specId;
        value = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(value) ? "rfc" : value;
    }

    private static string FindToolRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var directPrompt = Path.Combine(directory.FullName, "prompts", "extract-requirements.md");
                if (File.Exists(directPrompt))
                {
                    return directory.FullName;
                }

                var nestedPrompt = Path.Combine(directory.FullName, "tools", "SpecTrace.Rfc", "prompts", "extract-requirements.md");
                if (File.Exists(nestedPrompt))
                {
                    return Path.Combine(directory.FullName, "tools", "SpecTrace.Rfc");
                }

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException("Could not locate tools/SpecTrace.Rfc prompts. Use --prompt and --schema explicitly.");
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

    private static string Required(string[] args, string optionName)
    {
        return GetOption(args, optionName)
            ?? throw new InvalidOperationException($"Missing required option {optionName}.");
    }

    private static bool HasSwitch(string[] args, string optionName)
    {
        return args.Any(arg => string.Equals(arg, optionName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsHelp(string value)
    {
        return value is "-h" or "--help" or "help";
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
        Console.WriteLine("  spec-rfc ingest --rfc <number> --out <source.json> [--source-id <id>] [--title <title>]");
        Console.WriteLine("  spec-rfc ingest --source <path-or-url> --out <source.json> [--source-id <id>] [--title <title>]");
        Console.WriteLine("  spec-rfc segment --source <source.json> --out <source-ledger.jsonl>");
        Console.WriteLine("  spec-rfc extract --ledger <source-ledger.jsonl> --out <candidates.jsonl> [--extraction-scope candidate-units|functional|normative|all] [--deterministic-extraction off|figures] [--ai-mode codex|off] [--batch-size 25] [--adaptive-min-batch-size 1] [--max-batch-retries 1] [--batch-timeout-seconds 300] [--model gpt-5.4-mini] [--reasoning-effort high] [--retry-reasoning-effort xhigh] [--raw-out-dir <dir>] [--batch-out-dir <dir>] [--resume]");
        Console.WriteLine("  spec-rfc review-pack --ledger <source-ledger.jsonl> --candidates <candidates.jsonl> --out <review.md>");
        Console.WriteLine("  spec-rfc coverage-audit --ledger <source-ledger.jsonl> --candidates <candidates.jsonl> --out <review-decisions.jsonl> [--report-out <coverage-audit.md>] [--ai-mode codex|off] [--batch-size 25] [--adaptive-min-batch-size 1] [--max-batch-retries 1] [--batch-timeout-seconds 300] [--model gpt-5.4-mini] [--reasoning-effort high] [--retry-reasoning-effort xhigh] [--raw-out-dir <dir>] [--batch-out-dir <dir>] [--resume]");
        Console.WriteLine("  spec-rfc normalize --ledger <source-ledger.jsonl> --review <review-decisions.jsonl> --out <normalized-review-decisions.jsonl> [--ai-mode codex|off] [--batch-size 25] [--adaptive-min-batch-size 1] [--max-batch-retries 1] [--batch-timeout-seconds 300] [--model gpt-5.4-mini] [--reasoning-effort high] [--retry-reasoning-effort xhigh] [--raw-out-dir <dir>] [--batch-out-dir <dir>] [--resume]");
        Console.WriteLine("  spec-rfc assemble --ledger <source-ledger.jsonl> (--candidates <candidates.jsonl> | --review <review-decisions.jsonl>) --spec-id <SPEC-ID> --out <SPEC-ID.json> [--domain <domain>] [--capability <capability>] [--id-style section|namespace]");
        Console.WriteLine("  spec-rfc validate [--root <repo>] [--input-path <path>] [--profile core|traceable|auditable]");
    }
}
