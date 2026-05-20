namespace SpecTrace.Rfc.Core;

public sealed class SpecAssemblyOptions
{
    public required string SpecId { get; init; }

    public required string Domain { get; init; }

    public required string Capability { get; init; }

    public required string Title { get; init; }

    public required string Owner { get; init; }

    public required string Purpose { get; init; }

    public string Status { get; init; } = "draft";

    public string? Context { get; init; }

    public string? RequirementPrefix { get; init; }

    public string IdStyle { get; init; } = "section";

    public bool IgnoreIdHints { get; init; }
}
