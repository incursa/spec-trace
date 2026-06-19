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
        private readonly List<PendingRequirement> _pendingRequirements = [];
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
            _pendingRequirements.Add(new PendingRequirement(candidate, [.. sourceUnitIds]));
        }

        public SpecTraceSpecificationArtifact Build()
        {
            var orderedPendingRequirements = _pendingRequirements
                .OrderBy(item => item, Comparer<PendingRequirement>.Create(ComparePendingRequirements))
                .ToList();

            foreach (var pendingRequirement in orderedPendingRequirements)
            {
                var candidate = pendingRequirement.Candidate;
                var sourceUnitIds = pendingRequirement.SourceUnitIds;
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
            var prefix = _options.RequirementPrefix ?? $"REQ-{_specificationNamespace}";
            if (string.Equals(_options.IdStyle, "section", StringComparison.OrdinalIgnoreCase))
            {
                var sourceUnit = ResolvePrimarySourceUnit(sourceUnitIds);
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

            if (!_options.IgnoreIdHints &&
                !string.IsNullOrWhiteSpace(candidate.ProposedIdHint) &&
                CandidateRules.IsRequirementId(candidate.ProposedIdHint) &&
                NamespaceHintAllowed(candidate.ProposedIdHint) &&
                _usedIds.Add(candidate.ProposedIdHint))
            {
                return candidate.ProposedIdHint;
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

        private int ComparePendingRequirements(PendingRequirement left, PendingRequirement right)
        {
            var leftSourceUnit = ResolvePrimarySourceUnit(left.SourceUnitIds);
            var rightSourceUnit = ResolvePrimarySourceUnit(right.SourceUnitIds);

            var sourceComparison = ComparePrimarySourceUnits(leftSourceUnit, rightSourceUnit);
            if (sourceComparison != 0)
            {
                return sourceComparison;
            }

            var leftFingerprint = BuildRequirementFingerprint(left.Candidate, left.SourceUnitIds);
            var rightFingerprint = BuildRequirementFingerprint(right.Candidate, right.SourceUnitIds);

            var fingerprintComparison = StringComparer.Ordinal.Compare(leftFingerprint, rightFingerprint);
            if (fingerprintComparison != 0)
            {
                return fingerprintComparison;
            }

            return StringComparer.Ordinal.Compare(
                string.Join('\u001F', NormalizeSourceUnitIds(left.SourceUnitIds)),
                string.Join('\u001F', NormalizeSourceUnitIds(right.SourceUnitIds)));
        }

        private SourceUnit? ResolvePrimarySourceUnit(IReadOnlyList<string> sourceUnitIds)
        {
            SourceUnit? best = null;
            foreach (var sourceUnitId in sourceUnitIds)
            {
                if (_ledger.TryGetValue(sourceUnitId, out var sourceUnit))
                {
                    if (best is null || CompareSourceUnits(sourceUnit, best) < 0)
                    {
                        best = sourceUnit;
                    }
                }
            }

            return best;
        }

        private static int ComparePrimarySourceUnits(SourceUnit? left, SourceUnit? right)
        {
            if (left is null && right is null)
            {
                return 0;
            }

            if (left is null)
            {
                return 1;
            }

            if (right is null)
            {
                return -1;
            }

            return CompareSourceUnits(left, right);
        }

        private static int CompareSourceUnits(SourceUnit left, SourceUnit right)
        {
            var sectionComparison = CompareSections(left.Section, right.Section);
            if (sectionComparison != 0)
            {
                return sectionComparison;
            }

            var blockComparison = left.BlockIndex.CompareTo(right.BlockIndex);
            if (blockComparison != 0)
            {
                return blockComparison;
            }

            var paragraphComparison = left.ParagraphIndex.CompareTo(right.ParagraphIndex);
            if (paragraphComparison != 0)
            {
                return paragraphComparison;
            }

            var sentenceComparison = left.SentenceIndex.CompareTo(right.SentenceIndex);
            if (sentenceComparison != 0)
            {
                return sentenceComparison;
            }

            return StringComparer.Ordinal.Compare(left.SourceUnitId, right.SourceUnitId);
        }

        private static int CompareSections(string left, string right)
        {
            var leftSegments = SplitSectionSegments(left);
            var rightSegments = SplitSectionSegments(right);
            var segmentCount = Math.Min(leftSegments.Count, rightSegments.Count);

            for (var index = 0; index < segmentCount; index++)
            {
                var leftSegment = leftSegments[index];
                var rightSegment = rightSegments[index];
                if (leftSegment.IsNumeric && rightSegment.IsNumeric)
                {
                    var numericComparison = leftSegment.NumericValue.CompareTo(rightSegment.NumericValue);
                    if (numericComparison != 0)
                    {
                        return numericComparison;
                    }
                }
                else if (leftSegment.IsNumeric)
                {
                    return -1;
                }
                else if (rightSegment.IsNumeric)
                {
                    return 1;
                }
                else
                {
                    var textComparison = StringComparer.OrdinalIgnoreCase.Compare(leftSegment.Text, rightSegment.Text);
                    if (textComparison != 0)
                    {
                        return textComparison;
                    }
                }
            }

            return leftSegments.Count.CompareTo(rightSegments.Count);
        }

        private static List<SectionSegment> SplitSectionSegments(string section)
        {
            return section.Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => int.TryParse(segment, out var numericValue)
                    ? new SectionSegment(true, numericValue, string.Empty)
                    : new SectionSegment(false, 0, segment))
                .ToList();
        }

        private static string BuildRequirementFingerprint(CandidateRequirement candidate, IReadOnlyList<string> sourceUnitIds)
        {
            var normalizedUpstreamRefs = candidate.UpstreamRefs
                .Select(NormalizeSortText)
                .OrderBy(value => value, StringComparer.Ordinal);

            var normalizedNotes = candidate.Notes
                .Select(NormalizeSortText)
                .OrderBy(value => value, StringComparer.Ordinal);

            return string.Join('\u001E', [
                NormalizeSortText(candidate.Title),
                NormalizeSortText(candidate.Statement),
                candidate.Coverage is null
                    ? string.Empty
                    : string.Join('|', candidate.Coverage.Positive, candidate.Coverage.Negative, candidate.Coverage.Edge, candidate.Coverage.Fuzz),
                string.Join('\u001F', normalizedUpstreamRefs),
                string.Join('\u001F', normalizedNotes),
                string.Join('\u001F', NormalizeSourceUnitIds(sourceUnitIds)),
            ]);
        }

        private static string NormalizeSortText(string value)
        {
            return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                .ToUpperInvariant();
        }

        private static IEnumerable<string> NormalizeSourceUnitIds(IReadOnlyList<string> sourceUnitIds)
        {
            return sourceUnitIds
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal);
        }

        private bool NamespaceHintAllowed(string requirementId)
        {
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

        private sealed record PendingRequirement(CandidateRequirement Candidate, IReadOnlyList<string> SourceUnitIds);

        private readonly record struct SectionSegment(bool IsNumeric, int NumericValue, string Text);
    }
}
