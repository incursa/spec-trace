using System.Text.RegularExpressions;

namespace SpecTrace.Rfc.Core;

public static class CandidateRules
{
    public static readonly string[] Decisions =
    [
        "emit",
        "skip_non_normative",
        "skip_duplicate",
        "merge_with_previous",
        "split_required",
        "needs_human_review",
        "gap",
    ];

    public static readonly string[] ReviewActions =
    [
        "accept",
        "accept_with_edit",
        "split",
        "merge",
        "skip",
        "gap",
        "quarantine",
    ];

    public static readonly string[] CoverageStatuses =
    [
        "required",
        "optional",
        "not_applicable",
        "deferred",
    ];

    private static readonly Regex NormativeKeyword = new(@"\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b", RegexOptions.Compiled);
    private static readonly Regex RequirementId = new(@"^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$", RegexOptions.Compiled);

    public static int CountNormativeKeywords(string statement)
    {
        return NormativeKeyword.Matches(statement).Count;
    }

    public static bool IsRequirementId(string id)
    {
        return RequirementId.IsMatch(id);
    }

    public static void ValidateDecision(CandidateDecision decision)
    {
        if (!Decisions.Contains(decision.Decision, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Candidate decision for '{decision.SourceUnitId}' has unsupported decision '{decision.Decision}'.");
        }

        if (decision.Decision == "emit" && decision.Requirements.Count == 0)
        {
            throw new InvalidOperationException($"Candidate decision for '{decision.SourceUnitId}' is emit but contains no requirements.");
        }

        foreach (var requirement in decision.Requirements)
        {
            ValidateRequirement(requirement, decision.SourceUnitId);
        }
    }

    public static void ValidateReviewDecision(ReviewDecision decision)
    {
        if (!ReviewActions.Contains(decision.Action, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Review decision for '{decision.SourceUnitId}' has unsupported action '{decision.Action}'.");
        }

        if (decision.SourceUnitIds.Count == 0)
        {
            throw new InvalidOperationException($"Review decision for '{decision.SourceUnitId}' is missing source_unit_ids.");
        }

        if (decision.SourceUnitIds.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException($"Review decision for '{decision.SourceUnitId}' contains an empty source_unit_ids entry.");
        }

        if (!string.Equals(decision.SourceUnitIds[0], decision.SourceUnitId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Review decision for '{decision.SourceUnitId}' must start source_unit_ids with the canonical source unit id.");
        }

        if (decision.Action is "accept" or "accept_with_edit" or "split" or "merge")
        {
            if (decision.Requirements.Count == 0)
            {
                throw new InvalidOperationException($"Review decision for '{decision.SourceUnitId}' action '{decision.Action}' requires at least one requirement payload.");
            }

            foreach (var requirement in decision.Requirements)
            {
                ValidateRequirement(requirement, decision.SourceUnitId);
            }
        }
    }

    public static void ValidateRequirement(CandidateRequirement requirement, string sourceUnitId)
    {
        if (string.IsNullOrWhiteSpace(requirement.Title))
        {
            throw new InvalidOperationException($"Requirement from '{sourceUnitId}' has an empty title.");
        }

        if (string.IsNullOrWhiteSpace(requirement.Statement))
        {
            throw new InvalidOperationException($"Requirement from '{sourceUnitId}' has an empty statement.");
        }

        var keywordCount = CountNormativeKeywords(requirement.Statement);
        if (keywordCount != 1)
        {
            throw new InvalidOperationException($"Requirement from '{sourceUnitId}' must contain exactly one uppercase normative keyword; found {keywordCount}: {requirement.Statement}");
        }

        if (requirement.Coverage is not null)
        {
            ValidateCoverage(requirement.Coverage, sourceUnitId);
        }
    }

    public static void ValidateCoverage(RequirementCoverage coverage, string sourceUnitId)
    {
        ValidateCoverageStatus(coverage.Positive, sourceUnitId, "positive");
        ValidateCoverageStatus(coverage.Negative, sourceUnitId, "negative");
        ValidateCoverageStatus(coverage.Edge, sourceUnitId, "edge");
        ValidateCoverageStatus(coverage.Fuzz, sourceUnitId, "fuzz");
    }

    private static void ValidateCoverageStatus(string value, string sourceUnitId, string field)
    {
        if (!CoverageStatuses.Contains(value, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Requirement from '{sourceUnitId}' has unsupported coverage.{field} value '{value}'.");
        }
    }
}
