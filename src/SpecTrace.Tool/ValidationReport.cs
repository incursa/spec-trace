using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace SpecTrace.Tool;

public sealed class ValidationReport
{
    [JsonPropertyName("profile")]
    public required string Profile { get; init; }

    [JsonPropertyName("artifact_count")]
    public required int ArtifactCount { get; init; }

    [JsonPropertyName("requirement_count")]
    public required int RequirementCount { get; init; }

    [JsonPropertyName("findings")]
    public required List<Finding> Findings { get; init; }

    [JsonIgnore]
    public bool HasErrors => Findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase));

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions.Default);
}

public sealed class Finding
{
    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("file")]
    public string? File { get; init; }

    [JsonPropertyName("artifact_id")]
    public string? ArtifactId { get; init; }

    [JsonPropertyName("requirement_id")]
    public string? RequirementId { get; init; }
}

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
