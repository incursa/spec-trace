namespace SpecTrace.Rfc.Ai;

public sealed class CodexExtractionOptions
{
    public required string LedgerPath { get; init; }

    public required string OutputPath { get; init; }

    public required string PromptPath { get; init; }

    public required string SchemaPath { get; init; }

    public string CodexCommand { get; init; } = "codex";

    public string Model { get; init; } = "gpt-5.4-mini";

    public string ReasoningEffort { get; init; } = "xhigh";

    public string WorkingDirectory { get; init; } = Directory.GetCurrentDirectory();

    public int BatchSize { get; init; } = 1;

    public int MaxBatchRetries { get; init; } = 2;

    public int BatchTimeoutSeconds { get; init; } = 300;

    public string ExtractionScope { get; init; } = "all";

    public string DeterministicExtractionMode { get; init; } = "off";

    public string AiMode { get; init; } = "codex";

    public string? RawOutputDirectory { get; init; }
}
