using System.Text.Json;
using System.Text.RegularExpressions;

namespace SpecTrace.Tool;

internal static class TopicViewResolver
{
    public static TopicViewResolutionResult Resolve(
        string rootPath,
        TopicViewInputModel input,
        JsonElement topicViewDefinition,
        IReadOnlyList<(string SourcePath, ArtifactModel Artifact)> artifacts)
    {
        var includeRequirementIds = GetRequirementIdSet(topicViewDefinition, "include_requirements");
        var excludeRequirementIds = GetRequirementIdSet(topicViewDefinition, "exclude_requirements");
        var hasMatchPredicate = topicViewDefinition.TryGetProperty("match", out var matchPredicate);

        var findings = new List<TopicViewFindingModel>();
        var selectedRequirements = new List<TopicViewRequirementResultModel>();
        var explicitlyExcludedRequirements = new List<TopicViewRequirementResultModel>();
        var matchedRequirementCount = 0;
        var conflictCount = 0;

        var requirementContexts = artifacts
            .OrderBy(item => item.SourcePath, StringComparer.OrdinalIgnoreCase)
            .SelectMany(item => (item.Artifact.Requirements ?? []).Select(requirement => new RequirementContext(item.SourcePath, item.Artifact, requirement)))
            .OrderBy(item => item.Requirement.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var requirementLookup = requirementContexts
            .GroupBy(item => item.Requirement.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var knownRequirementIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var context in requirementContexts)
        {
            knownRequirementIds.Add(context.Requirement.Id);

            var matchEvaluation = hasMatchPredicate
                ? EvaluatePredicate(matchPredicate, context.Requirement, context.Artifact)
                : PredicateEvaluation.False("no match predicate");

            if (matchEvaluation.Matched)
            {
                matchedRequirementCount++;
            }

            var explicitInclude = includeRequirementIds.Contains(context.Requirement.Id);
            var explicitExclude = excludeRequirementIds.Contains(context.Requirement.Id);
            var selected = (matchEvaluation.Matched || explicitInclude) && !explicitExclude;

            var reasons = new List<TopicViewReasonModel>();
            if (matchEvaluation.Matched)
            {
                reasons.AddRange(matchEvaluation.Reasons);
            }

            if (explicitInclude)
            {
                reasons.Add(new TopicViewReasonModel
                {
                    Kind = "explicit_include",
                    Message = "Included explicitly via include_requirements.",
                });
            }

            if (explicitExclude)
            {
                reasons.Add(new TopicViewReasonModel
                {
                    Kind = "explicit_exclude",
                    Message = explicitInclude
                        ? "Excluded explicitly via exclude_requirements, overriding explicit inclusion."
                        : "Excluded explicitly via exclude_requirements.",
                });
            }

            var selection = new TopicViewSelectionStateModel
            {
                Selected = selected,
                Matched = matchEvaluation.Matched,
                ExplicitInclude = explicitInclude,
                ExplicitExclude = explicitExclude,
            };

            var result = BuildRequirementResult(rootPath, context, selection, reasons);

            if (selected)
            {
                selectedRequirements.Add(result);
            }

            if (explicitExclude)
            {
                explicitlyExcludedRequirements.Add(result);
            }

        }

        foreach (var requirementId in includeRequirementIds.Intersect(excludeRequirementIds, StringComparer.OrdinalIgnoreCase).OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            conflictCount++;
            requirementLookup.TryGetValue(requirementId, out var context);
            findings.Add(new TopicViewFindingModel
            {
                Severity = "warning",
                Code = "explicit-membership-conflict",
                Message = $"Requirement '{requirementId}' appears in both include_requirements and exclude_requirements; exclusion wins.",
                RequirementId = requirementId,
                ArtifactId = context?.Artifact.ArtifactId,
                SourcePath = context is null ? input.Path : CanonicalJsonLoader.NormalizeRepoPath(rootPath, context.SourcePath),
            });
        }

        foreach (var requirementId in includeRequirementIds.Where(id => !knownRequirementIds.Contains(id)).OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            findings.Add(new TopicViewFindingModel
            {
                Severity = "warning",
                Code = "unknown-explicit-include",
                Message = $"include_requirements references '{requirementId}', but the requirement was not found in the evaluated repository scope.",
                RequirementId = requirementId,
                SourcePath = input.Path,
            });
        }

        foreach (var requirementId in excludeRequirementIds.Where(id => !knownRequirementIds.Contains(id)).OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            findings.Add(new TopicViewFindingModel
            {
                Severity = "warning",
                Code = "unknown-explicit-exclude",
                Message = $"exclude_requirements references '{requirementId}', but the requirement was not found in the evaluated repository scope.",
                RequirementId = requirementId,
                SourcePath = input.Path,
            });
        }

        var warningCount = findings.Count(finding => string.Equals(finding.Severity, "warning", StringComparison.OrdinalIgnoreCase));

        return new TopicViewResolutionResult
        {
            Version = "1",
            RootPath = rootPath,
            Input = input,
            TopicView = topicViewDefinition,
            Summary = new TopicViewSummaryModel
            {
                ArtifactCount = artifacts.Count,
                RequirementCount = requirementContexts.Count,
                SelectedCount = selectedRequirements.Count,
                MatchedCount = matchedRequirementCount,
                ExplicitIncludeCount = includeRequirementIds.Count,
                ExplicitExcludeCount = excludeRequirementIds.Count,
                ConflictCount = conflictCount,
                WarningCount = warningCount,
            },
            Findings = findings
                .OrderBy(finding => finding.Severity, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.RequirementId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.ArtifactId, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            SelectedRequirements = selectedRequirements
                .OrderBy(result => result.RequirementId, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            ExplicitlyExcludedRequirements = explicitlyExcludedRequirements
                .OrderBy(result => result.RequirementId, StringComparer.OrdinalIgnoreCase)
                .ToList(),
        };
    }

    private static TopicViewRequirementResultModel BuildRequirementResult(
        string rootPath,
        RequirementContext context,
        TopicViewSelectionStateModel selection,
        List<TopicViewReasonModel> reasons)
    {
        return new TopicViewRequirementResultModel
        {
            RequirementId = context.Requirement.Id,
            Title = context.Requirement.Title,
            ArtifactId = context.Artifact.ArtifactId,
            ArtifactTitle = context.Artifact.Title,
            ArtifactType = context.Artifact.ArtifactType,
            Domain = context.Artifact.Domain,
            SourcePath = CanonicalJsonLoader.NormalizeRepoPath(rootPath, context.SourcePath),
            Selection = selection,
            Reasons = reasons.Count == 0
                ? [new TopicViewReasonModel
                {
                    Kind = "predicate",
                    Message = "No matching predicate or explicit include selected this requirement.",
                }]
                : reasons,
        };
    }

    private static PredicateEvaluation EvaluatePredicate(JsonElement predicate, RequirementModel requirement, ArtifactModel artifact)
    {
        if (predicate.TryGetProperty("all", out var allPredicate))
        {
            var reasons = new List<TopicViewReasonModel>();
            foreach (var child in allPredicate.EnumerateArray())
            {
                var childEvaluation = EvaluatePredicate(child, requirement, artifact);
                if (!childEvaluation.Matched)
                {
                    return PredicateEvaluation.False(DescribePredicate(predicate));
                }

                reasons.AddRange(childEvaluation.Reasons);
            }

            return PredicateEvaluation.True(DescribePredicate(predicate), reasons);
        }

        if (predicate.TryGetProperty("any", out var anyPredicate))
        {
            var reasons = new List<TopicViewReasonModel>();
            var matched = false;

            foreach (var child in anyPredicate.EnumerateArray())
            {
                var childEvaluation = EvaluatePredicate(child, requirement, artifact);
                if (!childEvaluation.Matched)
                {
                    continue;
                }

                matched = true;
                reasons.AddRange(childEvaluation.Reasons);
            }

            return matched
                ? PredicateEvaluation.True(DescribePredicate(predicate), reasons)
                : PredicateEvaluation.False(DescribePredicate(predicate));
        }

        if (predicate.TryGetProperty("not", out var notPredicate))
        {
            var childEvaluation = EvaluatePredicate(notPredicate, requirement, artifact);
            if (childEvaluation.Matched)
            {
                return PredicateEvaluation.False(DescribePredicate(predicate));
            }

            return PredicateEvaluation.True(DescribePredicate(predicate), [new TopicViewReasonModel
            {
                Kind = "predicate",
                Operator = "not",
                Message = DescribePredicate(predicate),
            }]);
        }

        if (predicate.TryGetProperty("literal", out var literalPredicate))
        {
            return EvaluateLiteralPredicate(literalPredicate, requirement, artifact);
        }

        if (predicate.TryGetProperty("regex", out var regexPredicate))
        {
            return EvaluateRegexPredicate(regexPredicate, requirement, artifact);
        }

        if (predicate.TryGetProperty("requirement_ids", out var requirementIdsPredicate))
        {
            return EvaluateRequirementIdsPredicate(requirementIdsPredicate, requirement, artifact);
        }

        throw new InvalidOperationException("Topic view predicate must contain one supported operator.");
    }

    private static PredicateEvaluation EvaluateLiteralPredicate(JsonElement literalPredicate, RequirementModel requirement, ArtifactModel artifact)
    {
        var fields = GetSelectors(literalPredicate, "fields");
        var value = GetRequiredString(literalPredicate, "value");
        var caseMode = GetRequiredString(literalPredicate, "case");
        var comparison = string.Equals(caseMode, "insensitive", StringComparison.OrdinalIgnoreCase)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var matchedValues = new List<string>();
        foreach (var field in fields)
        {
            foreach (var candidate in GetFieldValues(field, requirement, artifact))
            {
                if (candidate.IndexOf(value, comparison) >= 0)
                {
                    matchedValues.Add(candidate);
                }
            }
        }

        if (matchedValues.Count == 0)
        {
            return PredicateEvaluation.False(DescribeLiteralClause(fields, value, caseMode));
        }

        return PredicateEvaluation.True(
            DescribeLiteralClause(fields, value, caseMode),
            [new TopicViewReasonModel
            {
                Kind = "predicate",
                Operator = "literal",
                Message = DescribeLiteralClause(fields, value, caseMode),
                Fields = fields.ToList(),
                Value = value,
                Case = caseMode,
                MatchedValues = matchedValues.Distinct(StringComparer.Ordinal).ToList(),
            }]);
    }

    private static PredicateEvaluation EvaluateRegexPredicate(JsonElement regexPredicate, RequirementModel requirement, ArtifactModel artifact)
    {
        var fields = GetSelectors(regexPredicate, "fields");
        var pattern = GetRequiredString(regexPredicate, "pattern");
        var caseMode = GetRequiredString(regexPredicate, "case");
        var options = RegexOptions.CultureInvariant | RegexOptions.ECMAScript;
        if (string.Equals(caseMode, "insensitive", StringComparison.OrdinalIgnoreCase))
        {
            options |= RegexOptions.IgnoreCase;
        }

        var regex = CreateRegex(pattern, options);
        var matchedValues = new List<string>();
        foreach (var field in fields)
        {
            foreach (var candidate in GetFieldValues(field, requirement, artifact))
            {
                if (regex.IsMatch(candidate))
                {
                    matchedValues.Add(candidate);
                }
            }
        }

        if (matchedValues.Count == 0)
        {
            return PredicateEvaluation.False(DescribeRegexClause(fields, pattern, caseMode));
        }

        return PredicateEvaluation.True(
            DescribeRegexClause(fields, pattern, caseMode),
            [new TopicViewReasonModel
            {
                Kind = "predicate",
                Operator = "regex",
                Message = DescribeRegexClause(fields, pattern, caseMode),
                Fields = fields.ToList(),
                Pattern = pattern,
                Case = caseMode,
                MatchedValues = matchedValues.Distinct(StringComparer.Ordinal).ToList(),
            }]);
    }

    private static PredicateEvaluation EvaluateRequirementIdsPredicate(JsonElement requirementIdsPredicate, RequirementModel requirement, ArtifactModel artifact)
    {
        var requirementIds = GetRequirementIds(requirementIdsPredicate);
        var matched = requirementIds.Contains(requirement.Id, StringComparer.OrdinalIgnoreCase);

        if (!matched)
        {
            return PredicateEvaluation.False(DescribeRequirementIdsClause(requirementIds));
        }

        return PredicateEvaluation.True(
            DescribeRequirementIdsClause(requirementIds),
            [new TopicViewReasonModel
            {
                Kind = "predicate",
                Operator = "requirement_ids",
                Message = DescribeRequirementIdsClause(requirementIds),
                RequirementIds = requirementIds.ToList(),
                MatchedValues = [requirement.Id],
            }]);
    }

    private static string DescribePredicate(JsonElement predicate)
    {
        if (predicate.TryGetProperty("all", out var allPredicate))
        {
            return $"all({string.Join(", ", allPredicate.EnumerateArray().Select(DescribePredicate))})";
        }

        if (predicate.TryGetProperty("any", out var anyPredicate))
        {
            return $"any({string.Join(", ", anyPredicate.EnumerateArray().Select(DescribePredicate))})";
        }

        if (predicate.TryGetProperty("not", out var notPredicate))
        {
            return $"not({DescribePredicate(notPredicate)})";
        }

        if (predicate.TryGetProperty("literal", out var literalPredicate))
        {
            var fields = GetSelectors(literalPredicate, "fields");
            var value = GetRequiredString(literalPredicate, "value");
            var caseMode = GetRequiredString(literalPredicate, "case");
            return DescribeLiteralClause(fields, value, caseMode);
        }

        if (predicate.TryGetProperty("regex", out var regexPredicate))
        {
            var fields = GetSelectors(regexPredicate, "fields");
            var pattern = GetRequiredString(regexPredicate, "pattern");
            var caseMode = GetRequiredString(regexPredicate, "case");
            return DescribeRegexClause(fields, pattern, caseMode);
        }

        if (predicate.TryGetProperty("requirement_ids", out var requirementIdsPredicate))
        {
            return DescribeRequirementIdsClause(GetRequirementIds(requirementIdsPredicate));
        }

        return "unsupported predicate";
    }

    private static string DescribeLiteralClause(IEnumerable<string> fields, string value, string caseMode)
    {
        return $"literal(fields=[{string.Join(", ", fields)}], value={JsonSerializer.Serialize(value)}, case={caseMode})";
    }

    private static string DescribeRegexClause(IEnumerable<string> fields, string pattern, string caseMode)
    {
        return $"regex(fields=[{string.Join(", ", fields)}], pattern={JsonSerializer.Serialize(pattern)}, case={caseMode})";
    }

    private static string DescribeRequirementIdsClause(IEnumerable<string> requirementIds)
    {
        return $"requirement_ids(values=[{string.Join(", ", requirementIds)}])";
    }

    private static List<string> GetSelectors(JsonElement clause, string propertyName)
    {
        var selectors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var selector in clause.GetProperty(propertyName).EnumerateArray())
        {
            var value = selector.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var trimmed = value.Trim();
            if (seen.Add(trimmed))
            {
                selectors.Add(trimmed);
            }
        }

        return selectors;
    }

    private static List<string> GetRequirementIds(JsonElement clause)
    {
        var values = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var requirementId in clause.GetProperty("values").EnumerateArray())
        {
            var value = requirementId.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var trimmed = value.Trim();
            if (seen.Add(trimmed))
            {
                values.Add(trimmed);
            }
        }

        return values;
    }

    private static HashSet<string> GetRequirementIdSet(JsonElement topicViewDefinition, string propertyName)
    {
        if (!topicViewDefinition.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var values = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var requirementId in property.EnumerateArray())
        {
            var value = requirementId.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var trimmed = value.Trim();
            if (seen.Add(trimmed))
            {
                values.Add(trimmed);
            }
        }

        return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetRequiredString(JsonElement clause, string propertyName)
    {
        if (!clause.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"Topic view clause is missing required string property '{propertyName}'.");
        }

        return property.GetString() ?? throw new InvalidOperationException($"Topic view clause property '{propertyName}' cannot be null.");
    }

    private static IEnumerable<string> GetFieldValues(string selector, RequirementModel requirement, ArtifactModel artifact)
    {
        return selector switch
        {
            "requirement.id" => SingleValue(requirement.Id),
            "requirement.title" => SingleValue(requirement.Title),
            "requirement.statement" => SingleValue(requirement.Statement),
            "requirement.notes" => NormalizeValues(requirement.Notes),
            "requirement.trace.upstream_refs" => NormalizeValues(requirement.Trace?.UpstreamRefs),
            "requirement.trace.related" => NormalizeValues(requirement.Trace?.Related),
            "requirement.trace.derived_from" => NormalizeValues(requirement.Trace?.DerivedFrom),
            "requirement.trace.supersedes" => NormalizeValues(requirement.Trace?.Supersedes),
            "requirement.trace.satisfied_by" => NormalizeValues(requirement.Trace?.SatisfiedBy),
            "requirement.trace.implemented_by" => NormalizeValues(requirement.Trace?.ImplementedBy),
            "requirement.trace.verified_by" => NormalizeValues(requirement.Trace?.VerifiedBy),
            "artifact.artifact_id" => SingleValue(artifact.ArtifactId),
            "artifact.title" => SingleValue(artifact.Title),
            "artifact.capability" => artifact.Capability is null ? [] : SingleValue(artifact.Capability),
            "artifact.tags" => NormalizeValues(artifact.Tags),
            _ => throw new InvalidOperationException($"Unknown topic view selector '{selector}'."),
        };
    }

    private static IEnumerable<string> NormalizeValues(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        var normalized = new List<string>();
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                normalized.Add(value);
            }
        }

        return normalized;
    }

    private static IEnumerable<string> SingleValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return [value];
    }

    private static Regex CreateRegex(string pattern, RegexOptions options)
    {
        try
        {
            return new Regex(pattern, options);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException($"Invalid topic view regex pattern '{pattern}': {exception.Message}", exception);
        }
    }

    private sealed record RequirementContext(string SourcePath, ArtifactModel Artifact, RequirementModel Requirement);

    private sealed record PredicateEvaluation(bool Matched, string Description, List<TopicViewReasonModel> Reasons)
    {
        public static PredicateEvaluation True(string description, List<TopicViewReasonModel> reasons) => new(true, description, reasons);

        public static PredicateEvaluation False(string description) => new(false, description, []);
    }
}
