using System.Text.Json;

namespace SpecTrace.Rfc.Core;

public static class SpecAssembler
{
    public static SpecTraceSpecificationArtifact AssembleFromCandidates(
        IEnumerable<CandidateDecision> candidateDecisions,
        IReadOnlyDictionary<string, SourceUnit> ledger,
        SpecAssemblyOptions options)
    {
        var builder = new Builder(ledger, options);

        foreach (var decision in candidateDecisions)
        {
            CandidateRules.ValidateDecision(decision);
            if (!string.Equals(decision.Decision, "emit", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var requirement in decision.Requirements)
            {
                builder.AddRequirement(requirement, [decision.SourceUnitId]);
            }
        }

        return builder.Build();
    }

    public static SpecTraceSpecificationArtifact AssembleFromReviewDecisions(
        IEnumerable<ReviewDecision> reviewDecisions,
        IReadOnlyDictionary<string, SourceUnit> ledger,
        SpecAssemblyOptions options)
    {
        var builder = new Builder(ledger, options);

        foreach (var decision in reviewDecisions)
        {
            CandidateRules.ValidateReviewDecision(decision);
            if (decision.Action is not ("accept" or "accept_with_edit" or "split" or "merge"))
            {
                continue;
            }

            var sourceUnitIds = decision.SourceUnitIds.Count == 0
                ? [decision.SourceUnitId]
                : decision.SourceUnitIds;

            foreach (var requirement in decision.Requirements)
            {
                builder.AddRequirement(requirement, sourceUnitIds);
            }
        }

        return builder.Build();
    }

    public static async Task WriteAsync(string path, SpecTraceSpecificationArtifact artifact, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(artifact, RfcJson.Options), cancellationToken);
    }

    private sealed class Builder
    {
        private readonly IReadOnlyDictionary<string, SourceUnit> _ledger;
        private readonly SpecAssemblyOptions _options;
        private readonly List<SpecTraceRequirement> _requirements = [];
        private readonly Dictionary<string, int> _sectionCounters = new(StringComparer.Ordinal);
        private readonly HashSet<string> _usedIds = new(StringComparer.Ordinal);
        private readonly string _specificationNamespace;
        private int _nextRequirementSequence;

        public Builder(IReadOnlyDictionary<string, SourceUnit> ledger, SpecAssemblyOptions options)
        {
            _ledger = ledger;
            _options = options;
            _specificationNamespace = GetSpecificationNamespace(options.SpecId);
        }

        public void AddRequirement(CandidateRequirement candidate, IReadOnlyList<string> sourceUnitIds)
        {
            CandidateRules.ValidateRequirement(candidate, sourceUnitIds.FirstOrDefault() ?? "unknown");

            var id = ChooseRequirementId(candidate, sourceUnitIds);
            var upstreamRefs = BuildUpstreamRefs(candidate, sourceUnitIds);

            _requirements.Add(new SpecTraceRequirement
            {
                Id = id,
                Title = candidate.Title.Trim(),
                Statement = candidate.Statement.Trim(),
                Coverage = candidate.Coverage,
                Trace = upstreamRefs.Count == 0 ? null : new SpecTraceRequirementTrace { UpstreamRefs = upstreamRefs },
                Notes = NormalizeList(candidate.Notes),
            });
        }

        public SpecTraceSpecificationArtifact Build()
        {
            return new SpecTraceSpecificationArtifact
            {
                ArtifactId = _options.SpecId,
                Title = _options.Title,
                Domain = _options.Domain,
                Capability = _options.Capability,
                Status = _options.Status,
                Owner = _options.Owner,
                Purpose = _options.Purpose,
                Context = _options.Context,
                Requirements = _requirements,
            };
        }

        private string ChooseRequirementId(CandidateRequirement candidate, IReadOnlyList<string> sourceUnitIds)
        {
            if (!_options.IgnoreIdHints &&
                !string.IsNullOrWhiteSpace(candidate.ProposedIdHint) &&
                CandidateRules.IsRequirementId(candidate.ProposedIdHint) &&
                RequirementHintAllowed(candidate.ProposedIdHint) &&
                _usedIds.Add(candidate.ProposedIdHint))
            {
                return candidate.ProposedIdHint;
            }

            var prefix = _options.RequirementPrefix ?? $"REQ-{_specificationNamespace}";
            if (string.Equals(_options.IdStyle, "section", StringComparison.OrdinalIgnoreCase))
            {
                var sourceUnit = ResolveFirstSourceUnit(sourceUnitIds);
                var sectionKey = sourceUnit is null ? "S0" : RfcSegmenter.SectionKey(sourceUnit.Section);
                while (true)
                {
                    var next = _sectionCounters.GetValueOrDefault(sectionKey) + 1;
                    _sectionCounters[sectionKey] = next;
                    var id = $"{prefix}-{sectionKey}-{next:0000}";
                    if (_usedIds.Add(id))
                    {
                        return id;
                    }
                }
            }

            while (true)
            {
                _nextRequirementSequence++;
                var id = $"{prefix}-{_nextRequirementSequence:0000}";
                if (_usedIds.Add(id))
                {
                    return id;
                }
            }
        }

        private SourceUnit? ResolveFirstSourceUnit(IReadOnlyList<string> sourceUnitIds)
        {
            foreach (var sourceUnitId in sourceUnitIds)
            {
                if (_ledger.TryGetValue(sourceUnitId, out var sourceUnit))
                {
                    return sourceUnit;
                }
            }

            return null;
        }

        private bool RequirementHintAllowed(string requirementId)
        {
            if (string.Equals(_options.IdStyle, "section", StringComparison.OrdinalIgnoreCase))
            {
                var prefix = _options.RequirementPrefix ?? $"REQ-{_specificationNamespace}";
                return requirementId.StartsWith(prefix + "-", StringComparison.Ordinal);
            }

            if (!string.IsNullOrWhiteSpace(_options.RequirementPrefix))
            {
                return requirementId.StartsWith(_options.RequirementPrefix + "-", StringComparison.Ordinal);
            }

            var namespaceEnd = requirementId.LastIndexOf('-');
            if (namespaceEnd <= "REQ-".Length)
            {
                return false;
            }

            var requirementNamespace = requirementId["REQ-".Length..namespaceEnd];
            return string.Equals(requirementNamespace, _specificationNamespace, StringComparison.Ordinal);
        }

        private static string GetSpecificationNamespace(string specId)
        {
            return specId.StartsWith("SPEC-", StringComparison.Ordinal)
                ? specId["SPEC-".Length..]
                : throw new InvalidOperationException($"Specification id '{specId}' must start with SPEC-.");
        }

        private List<string> BuildUpstreamRefs(CandidateRequirement candidate, IReadOnlyList<string> sourceUnitIds)
        {
            var refs = new List<string>();
            AddDistinct(refs, candidate.UpstreamRefs);

            foreach (var sourceUnitId in sourceUnitIds)
            {
                if (!_ledger.TryGetValue(sourceUnitId, out var sourceUnit))
                {
                    AddDistinct(refs, [sourceUnitId]);
                    continue;
                }

                if (!refs.Any(item => item.Contains(sourceUnit.SourceUnitId, StringComparison.Ordinal)))
                {
                    var label = BuildSourceLabel(sourceUnit);
                    AddDistinct(refs, [label]);
                }

                if (!string.IsNullOrWhiteSpace(sourceUnit.SourceUrl))
                {
                    AddDistinct(refs, [sourceUnit.SourceUrl]);
                }
            }

            return refs;
        }

        private static string BuildSourceLabel(SourceUnit sourceUnit)
        {
            var source = sourceUnit.SourceId.StartsWith("RFC", StringComparison.OrdinalIgnoreCase) &&
                         sourceUnit.SourceId.Length > 3
                ? $"RFC {sourceUnit.SourceId[3..]}"
                : sourceUnit.SourceId;

            return string.Equals(sourceUnit.Section, "0", StringComparison.Ordinal)
                ? $"{source} {sourceUnit.SourceUnitId}"
                : $"{source} §{sourceUnit.Section} {sourceUnit.SourceUnitId}";
        }

        private static List<string>? NormalizeList(IEnumerable<string> values)
        {
            var result = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return result.Count == 0 ? null : result;
        }

        private static void AddDistinct(List<string> target, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var trimmed = value.Trim();
                if (!target.Contains(trimmed, StringComparer.Ordinal))
                {
                    target.Add(trimmed);
                }
            }
        }
    }
}
