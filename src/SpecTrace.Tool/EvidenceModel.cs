using System.Text.Json.Serialization;

namespace SpecTrace.Tool;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EvidenceObservationModel
{
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("refs")]
    public List<string>? Refs { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RequirementEvidenceModel
{
    [JsonPropertyName("requirement_id")]
    public required string RequirementId { get; init; }

    [JsonPropertyName("observations")]
    public required List<EvidenceObservationModel> Observations { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EvidenceProducerModel
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EvidenceSnapshotModel
{
    [JsonPropertyName("snapshot_id")]
    public required string SnapshotId { get; init; }

    [JsonPropertyName("generated_at")]
    public required string GeneratedAt { get; init; }

    [JsonPropertyName("producer")]
    public required EvidenceProducerModel Producer { get; init; }

    [JsonPropertyName("requirements")]
    public required List<RequirementEvidenceModel> Requirements { get; init; }
}

public sealed class EvidenceValidationReport
{
    [JsonPropertyName("file_count")]
    public required int FileCount { get; init; }

    [JsonPropertyName("snapshot_count")]
    public required int SnapshotCount { get; init; }

    [JsonPropertyName("requirement_count")]
    public required int RequirementCount { get; init; }

    [JsonPropertyName("observation_count")]
    public required int ObservationCount { get; init; }

    [JsonPropertyName("findings")]
    public required List<Finding> Findings { get; init; }

    [JsonIgnore]
    public bool HasErrors => Findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase));

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this, JsonOptions.Default);
}

internal sealed class LoadedEvidenceSnapshot
{
    public required string SourcePath { get; init; }

    public required string RepoRelativePath { get; init; }

    public required EvidenceSnapshotModel Snapshot { get; init; }
}

internal sealed class EvidenceValidationResult
{
    public required EvidenceValidationReport Report { get; init; }

    public required IReadOnlyList<LoadedEvidenceSnapshot> Snapshots { get; init; }
}
