using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpecTrace.Tool;

public sealed class TopicViewResolutionResult
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("root_path")]
    public required string RootPath { get; init; }

    [JsonPropertyName("input")]
    public required TopicViewInputModel Input { get; init; }

    [JsonPropertyName("topic_view")]
    public required JsonElement TopicView { get; init; }

    [JsonPropertyName("summary")]
    public required TopicViewSummaryModel Summary { get; init; }

    [JsonPropertyName("findings")]
    public required List<TopicViewFindingModel> Findings { get; init; }

    [JsonPropertyName("selected_requirements")]
    public required List<TopicViewRequirementResultModel> SelectedRequirements { get; init; }

    [JsonPropertyName("explicitly_excluded_requirements")]
    public required List<TopicViewRequirementResultModel> ExplicitlyExcludedRequirements { get; init; }

    public string ToJson()
    {
        var options = new JsonSerializerOptions(JsonOptions.Default)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        return JsonSerializer.Serialize(this, options);
    }
}

public sealed class TopicViewInputModel
{
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("path")]
    public string? Path { get; init; }
}

public sealed class TopicViewSummaryModel
{
    [JsonPropertyName("artifact_count")]
    public required int ArtifactCount { get; init; }

    [JsonPropertyName("requirement_count")]
    public required int RequirementCount { get; init; }

    [JsonPropertyName("selected_count")]
    public required int SelectedCount { get; init; }

    [JsonPropertyName("matched_count")]
    public required int MatchedCount { get; init; }

    [JsonPropertyName("explicit_include_count")]
    public required int ExplicitIncludeCount { get; init; }

    [JsonPropertyName("explicit_exclude_count")]
    public required int ExplicitExcludeCount { get; init; }

    [JsonPropertyName("conflict_count")]
    public required int ConflictCount { get; init; }

    [JsonPropertyName("warning_count")]
    public required int WarningCount { get; init; }
}

public sealed class TopicViewFindingModel
{
    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("requirement_id")]
    public string? RequirementId { get; init; }

    [JsonPropertyName("artifact_id")]
    public string? ArtifactId { get; init; }

    [JsonPropertyName("source_path")]
    public string? SourcePath { get; init; }
}

public sealed class TopicViewSelectionStateModel
{
    [JsonPropertyName("selected")]
    public required bool Selected { get; init; }

    [JsonPropertyName("matched")]
    public required bool Matched { get; init; }

    [JsonPropertyName("explicit_include")]
    public required bool ExplicitInclude { get; init; }

    [JsonPropertyName("explicit_exclude")]
    public required bool ExplicitExclude { get; init; }
}

public sealed class TopicViewRequirementResultModel
{
    [JsonPropertyName("requirement_id")]
    public required string RequirementId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("artifact_id")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("artifact_title")]
    public required string ArtifactTitle { get; init; }

    [JsonPropertyName("artifact_type")]
    public required string ArtifactType { get; init; }

    [JsonPropertyName("domain")]
    public required string Domain { get; init; }

    [JsonPropertyName("source_path")]
    public required string SourcePath { get; init; }

    [JsonPropertyName("selection")]
    public required TopicViewSelectionStateModel Selection { get; init; }

    [JsonPropertyName("reasons")]
    public required List<TopicViewReasonModel> Reasons { get; init; }
}

public sealed class TopicViewReasonModel
{
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("operator")]
    public string? Operator { get; init; }

    [JsonPropertyName("fields")]
    public List<string>? Fields { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("pattern")]
    public string? Pattern { get; init; }

    [JsonPropertyName("case")]
    public string? Case { get; init; }

    [JsonPropertyName("matched_values")]
    public List<string>? MatchedValues { get; init; }

    [JsonPropertyName("requirement_ids")]
    public List<string>? RequirementIds { get; init; }
}
