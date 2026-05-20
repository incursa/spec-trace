using System.Text.Json.Serialization;

namespace SpecTrace.Rfc.Core;

public sealed class RfcSourceDocument
{
    [JsonPropertyName("source_id")]
    public required string SourceId { get; init; }

    [JsonPropertyName("rfc_number")]
    public string? RfcNumber { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; init; }

    [JsonPropertyName("canonical_url")]
    public string? CanonicalUrl { get; init; }

    [JsonPropertyName("retrieved_at")]
    public required string RetrievedAt { get; init; }

    [JsonPropertyName("text_hash")]
    public required string TextHash { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}

public sealed class SourceUnit
{
    [JsonPropertyName("source_unit_id")]
    public required string SourceUnitId { get; init; }

    [JsonPropertyName("source_id")]
    public required string SourceId { get; init; }

    [JsonPropertyName("section")]
    public required string Section { get; init; }

    [JsonPropertyName("section_title")]
    public required string SectionTitle { get; init; }

    [JsonPropertyName("block_index")]
    public required int BlockIndex { get; init; }

    [JsonPropertyName("paragraph_index")]
    public required int ParagraphIndex { get; init; }

    [JsonPropertyName("sentence_index")]
    public required int SentenceIndex { get; init; }

    [JsonPropertyName("block_kind")]
    public required string BlockKind { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; init; }

    [JsonPropertyName("text_hash")]
    public required string TextHash { get; init; }
}

public sealed class CandidateBatchResponse
{
    [JsonPropertyName("results")]
    public required List<CandidateDecision> Results { get; init; }
}

public sealed class CandidateDecision
{
    [JsonPropertyName("source_unit_id")]
    public required string SourceUnitId { get; init; }

    [JsonPropertyName("decision")]
    public required string Decision { get; init; }

    [JsonPropertyName("requirements")]
    public List<CandidateRequirement> Requirements { get; init; } = [];

    [JsonPropertyName("review_flags")]
    public List<string> ReviewFlags { get; init; } = [];
}

public sealed class CandidateRequirement
{
    [JsonPropertyName("proposed_id_hint")]
    public string? ProposedIdHint { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("statement")]
    public required string Statement { get; init; }

    [JsonPropertyName("coverage")]
    public RequirementCoverage? Coverage { get; init; }

    [JsonPropertyName("upstream_refs")]
    public List<string> UpstreamRefs { get; init; } = [];

    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = [];
}

public sealed class RequirementCoverage
{
    [JsonPropertyName("positive")]
    public required string Positive { get; init; }

    [JsonPropertyName("negative")]
    public required string Negative { get; init; }

    [JsonPropertyName("edge")]
    public required string Edge { get; init; }

    [JsonPropertyName("fuzz")]
    public required string Fuzz { get; init; }
}

public sealed class ReviewDecision
{
    [JsonPropertyName("source_unit_id")]
    public required string SourceUnitId { get; init; }

    [JsonPropertyName("source_unit_ids")]
    public List<string> SourceUnitIds { get; init; } = [];

    [JsonPropertyName("action")]
    public required string Action { get; init; }

    [JsonPropertyName("requirements")]
    public List<CandidateRequirement> Requirements { get; init; } = [];

    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = [];
}

public sealed class SpecTraceSpecificationArtifact
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    [JsonPropertyName("artifact_id")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("artifact_type")]
    public string ArtifactType { get; init; } = "specification";

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("domain")]
    public required string Domain { get; init; }

    [JsonPropertyName("capability")]
    public required string Capability { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("owner")]
    public required string Owner { get; init; }

    [JsonPropertyName("purpose")]
    public required string Purpose { get; init; }

    [JsonPropertyName("context")]
    public string? Context { get; init; }

    [JsonPropertyName("requirements")]
    public required List<SpecTraceRequirement> Requirements { get; init; }
}

public sealed class SpecTraceRequirement
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("statement")]
    public required string Statement { get; init; }

    [JsonPropertyName("coverage")]
    public RequirementCoverage? Coverage { get; init; }

    [JsonPropertyName("trace")]
    public SpecTraceRequirementTrace? Trace { get; init; }

    [JsonPropertyName("notes")]
    public List<string>? Notes { get; init; }
}

public sealed class SpecTraceRequirementTrace
{
    [JsonPropertyName("upstream_refs")]
    public List<string>? UpstreamRefs { get; init; }
}
