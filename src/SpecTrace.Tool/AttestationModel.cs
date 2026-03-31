using System.Text.Json.Serialization;

namespace SpecTrace.Tool;

public sealed class SpecTraceAttestationSnapshot
{
    [JsonPropertyName("schema_version")]
    public required int SchemaVersion { get; init; }

    [JsonPropertyName("generated_at")]
    public required string GeneratedAt { get; init; }

    [JsonPropertyName("repository")]
    public required SpecTraceAttestationRepository Repository { get; init; }

    [JsonPropertyName("selection")]
    public required SpecTraceAttestationSelection Selection { get; init; }

    [JsonPropertyName("validation")]
    public required SpecTraceAttestationValidationSummary Validation { get; init; }

    [JsonPropertyName("evidence")]
    public required SpecTraceAttestationEvidenceSummary Evidence { get; init; }

    [JsonPropertyName("aggregates")]
    public required SpecTraceAttestationAggregateSummary Aggregates { get; init; }

    [JsonPropertyName("specifications")]
    public required List<SpecTraceAttestationSpecification> Specifications { get; init; }

    [JsonPropertyName("warnings")]
    public List<string>? Warnings { get; init; }

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this, JsonOptions.Default);
}

public sealed class SpecTraceAttestationRepository
{
    [JsonPropertyName("root")]
    public required string Root { get; init; }

    [JsonPropertyName("display_name")]
    public required string DisplayName { get; init; }
}

public sealed class SpecTraceAttestationSelection
{
    [JsonPropertyName("profile")]
    public required string Profile { get; init; }

    [JsonPropertyName("input_path")]
    public string? InputPath { get; init; }

    [JsonPropertyName("emit")]
    public required string Emit { get; init; }

    [JsonPropertyName("output_directory")]
    public required string OutputDirectory { get; init; }
}

public sealed class SpecTraceAttestationValidationSummary
{
    [JsonPropertyName("error_count")]
    public required int ErrorCount { get; init; }

    [JsonPropertyName("warning_count")]
    public required int WarningCount { get; init; }

    [JsonPropertyName("findings")]
    public required List<Finding> Findings { get; init; }
}

public sealed class SpecTraceAttestationEvidenceSummary
{
    [JsonPropertyName("file_count")]
    public required int FileCount { get; init; }

    [JsonPropertyName("snapshot_count")]
    public required int SnapshotCount { get; init; }

    [JsonPropertyName("requirement_count")]
    public required int RequirementCount { get; init; }

    [JsonPropertyName("observation_count")]
    public required int ObservationCount { get; init; }

    [JsonPropertyName("files")]
    public required List<SpecTraceAttestationEvidenceFile> Files { get; init; }
}

public sealed class SpecTraceAttestationEvidenceFile
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("snapshot_id")]
    public required string SnapshotId { get; init; }

    [JsonPropertyName("generated_at")]
    public required string GeneratedAt { get; init; }

    [JsonPropertyName("producer")]
    public required string Producer { get; init; }

    [JsonPropertyName("requirement_count")]
    public required int RequirementCount { get; init; }

    [JsonPropertyName("observation_count")]
    public required int ObservationCount { get; init; }
}

public sealed class SpecTraceAttestationAggregateSummary
{
    [JsonPropertyName("specification_count")]
    public required int SpecificationCount { get; init; }

    [JsonPropertyName("requirement_count")]
    public required int RequirementCount { get; init; }

    [JsonPropertyName("requirements_with_downstream_trace")]
    public required int RequirementsWithDownstreamTrace { get; init; }

    [JsonPropertyName("requirements_with_implementation")]
    public required int RequirementsWithImplementation { get; init; }

    [JsonPropertyName("requirements_with_verification")]
    public required int RequirementsWithVerification { get; init; }

    [JsonPropertyName("requirements_with_evidence")]
    public required int RequirementsWithEvidence { get; init; }

    [JsonPropertyName("requirements_with_validation_errors")]
    public required int RequirementsWithValidationErrors { get; init; }

    [JsonPropertyName("requirements_with_validation_warnings")]
    public required int RequirementsWithValidationWarnings { get; init; }

    [JsonPropertyName("requirements_with_any_issue")]
    public required int RequirementsWithAnyIssue { get; init; }

    [JsonPropertyName("derived_status_counts")]
    public required Dictionary<string, int> DerivedStatusCounts { get; init; }

    [JsonPropertyName("evidence_kind_coverage")]
    public required Dictionary<string, int> EvidenceKindCoverage { get; init; }
}

public sealed class SpecTraceAttestationSpecification
{
    [JsonPropertyName("artifact_id")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("domain")]
    public required string Domain { get; init; }

    [JsonPropertyName("source_cue_path")]
    public required string SourceCuePath { get; init; }

    [JsonPropertyName("source_markdown_path")]
    public required string SourceMarkdownPath { get; init; }

    [JsonPropertyName("findings")]
    public required List<Finding> Findings { get; init; }

    [JsonPropertyName("requirements")]
    public required List<SpecTraceAttestationRequirement> Requirements { get; init; }
}

public sealed class SpecTraceAttestationRequirement
{
    [JsonPropertyName("requirement_id")]
    public required string RequirementId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("statement")]
    public required string Statement { get; init; }

    [JsonPropertyName("source_markdown_path")]
    public required string SourceMarkdownPath { get; init; }

    [JsonPropertyName("trace")]
    public required SpecTraceAttestationRequirementTrace Trace { get; init; }

    [JsonPropertyName("notes")]
    public List<string>? Notes { get; init; }

    [JsonPropertyName("evidence")]
    public required List<SpecTraceAttestationEvidenceKind> Evidence { get; init; }

    [JsonPropertyName("derived_status")]
    public required string DerivedStatus { get; init; }

    [JsonPropertyName("issues")]
    public required List<string> Issues { get; init; }

    [JsonPropertyName("findings")]
    public required List<Finding> Findings { get; init; }
}

public sealed class SpecTraceAttestationRequirementTrace
{
    [JsonPropertyName("satisfied_by")]
    public required List<string> SatisfiedBy { get; init; }

    [JsonPropertyName("implemented_by")]
    public required List<string> ImplementedBy { get; init; }

    [JsonPropertyName("verified_by")]
    public required List<string> VerifiedBy { get; init; }

    [JsonPropertyName("derived_from")]
    public required List<string> DerivedFrom { get; init; }

    [JsonPropertyName("supersedes")]
    public required List<string> Supersedes { get; init; }

    [JsonPropertyName("upstream_refs")]
    public required List<string> UpstreamRefs { get; init; }

    [JsonPropertyName("related")]
    public required List<string> Related { get; init; }
}

public sealed class SpecTraceAttestationEvidenceKind
{
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("refs")]
    public required List<string> Refs { get; init; }

    [JsonPropertyName("source_files")]
    public required List<string> SourceFiles { get; init; }

    [JsonPropertyName("summaries")]
    public required List<string> Summaries { get; init; }
}

public sealed class SpecTraceAttestationResult
{
    public required SpecTraceAttestationSnapshot Snapshot { get; init; }

    public string? SummaryHtmlPath { get; init; }

    public string? DetailsHtmlPath { get; init; }

    public string? JsonPath { get; init; }
}
