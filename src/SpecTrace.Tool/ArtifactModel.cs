using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpecTrace.Tool;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ArtifactModel
{
    [JsonPropertyName("artifact_id")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("artifact_type")]
    public required string ArtifactType { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("domain")]
    public required string Domain { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("owner")]
    public required string Owner { get; init; }

    [JsonPropertyName("capability")]
    public string? Capability { get; init; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; init; }

    [JsonPropertyName("related_artifacts")]
    public List<string>? RelatedArtifacts { get; init; }

    [JsonPropertyName("purpose")]
    public string? Purpose { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    [JsonPropertyName("context")]
    public string? Context { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("open_questions")]
    public List<string>? OpenQuestions { get; init; }

    [JsonPropertyName("supplemental_sections")]
    public List<SupplementalSection>? SupplementalSections { get; init; }

    [JsonPropertyName("requirements")]
    public List<RequirementModel>? Requirements { get; init; }

    [JsonPropertyName("satisfies")]
    public List<string>? Satisfies { get; init; }

    [JsonPropertyName("design_summary")]
    public string? DesignSummary { get; init; }

    [JsonPropertyName("key_components")]
    public List<string>? KeyComponents { get; init; }

    [JsonPropertyName("data_and_state_considerations")]
    public string? DataAndStateConsiderations { get; init; }

    [JsonPropertyName("edge_cases_and_constraints")]
    public List<string>? EdgeCasesAndConstraints { get; init; }

    [JsonPropertyName("alternatives_considered")]
    public List<string>? AlternativesConsidered { get; init; }

    [JsonPropertyName("risks")]
    public List<string>? Risks { get; init; }

    [JsonPropertyName("addresses")]
    public List<string>? Addresses { get; init; }

    [JsonPropertyName("design_links")]
    public List<string>? DesignLinks { get; init; }

    [JsonPropertyName("verification_links")]
    public List<string>? VerificationLinks { get; init; }

    [JsonPropertyName("planned_changes")]
    public string? PlannedChanges { get; init; }

    [JsonPropertyName("out_of_scope")]
    public List<string>? OutOfScope { get; init; }

    [JsonPropertyName("verification_plan")]
    public string? VerificationPlan { get; init; }

    [JsonPropertyName("completion_notes")]
    public string? CompletionNotes { get; init; }

    [JsonPropertyName("verifies")]
    public List<string>? Verifies { get; init; }

    [JsonPropertyName("verification_method")]
    public string? VerificationMethod { get; init; }

    [JsonPropertyName("preconditions")]
    public List<string>? Preconditions { get; init; }

    [JsonPropertyName("procedure")]
    public List<string>? Procedure { get; init; }

    [JsonPropertyName("expected_result")]
    public string? ExpectedResult { get; init; }

    [JsonPropertyName("evidence")]
    public List<string>? Evidence { get; init; }

    [JsonPropertyName("status_summary")]
    public string? StatusSummary { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RequirementModel
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("statement")]
    public required string Statement { get; init; }

    [JsonPropertyName("trace")]
    public RequirementTraceModel? Trace { get; init; }

    [JsonPropertyName("notes")]
    public List<string>? Notes { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RequirementTraceModel
{
    [JsonPropertyName("satisfied_by")]
    public List<string>? SatisfiedBy { get; init; }

    [JsonPropertyName("implemented_by")]
    public List<string>? ImplementedBy { get; init; }

    [JsonPropertyName("verified_by")]
    public List<string>? VerifiedBy { get; init; }

    [JsonPropertyName("derived_from")]
    public List<string>? DerivedFrom { get; init; }

    [JsonPropertyName("supersedes")]
    public List<string>? Supersedes { get; init; }

    [JsonPropertyName("upstream_refs")]
    public List<string>? UpstreamRefs { get; init; }

    [JsonPropertyName("related")]
    public List<string>? Related { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SupplementalSection
{
    [JsonPropertyName("heading")]
    public required string Heading { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CatalogItem
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("source_path")]
    public required string SourcePath { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("artifact_type")]
    public string? ArtifactType { get; init; }

    [JsonPropertyName("parent_id")]
    public string? ParentArtifactId { get; init; }

    [JsonPropertyName("domain")]
    public required string Domain { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CatalogReference
{
    [JsonPropertyName("source_id")]
    public required string SourceId { get; init; }

    [JsonPropertyName("field")]
    public required string Field { get; init; }

    [JsonPropertyName("target_id")]
    public required string TargetId { get; init; }

    [JsonPropertyName("expected_kind")]
    public required string ExpectedKind { get; init; }

    [JsonPropertyName("expected_prefix")]
    public string? ExpectedPrefix { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CatalogSnapshot
{
    [JsonPropertyName("entries")]
    public required Dictionary<string, CatalogItem> Entries { get; init; }

    [JsonPropertyName("references")]
    public required List<CatalogReference> References { get; init; }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions.Default);
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RetiredRequirementLedger
{
    [JsonPropertyName("retired_requirements")]
    public required List<RetiredRequirementRecord> RetiredRequirements { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RetiredRequirementRecord
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("replaced_by")]
    public List<string>? ReplacedBy { get; init; }

    [JsonPropertyName("notes")]
    public List<string>? Notes { get; init; }
}
