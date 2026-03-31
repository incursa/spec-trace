using System.Globalization;

namespace SpecTrace.Tool;

internal static class SpecTraceAttestationService
{
    public const string DefaultOutputDirectory = "artifacts/spec-trace/attestation";

    public static SpecTraceAttestationResult Generate(
        string rootPath,
        string? inputPath,
        string profile,
        string emit,
        string outDir,
        IReadOnlyList<(string CuePath, ArtifactModel Artifact)> artifacts,
        ArtifactCatalog catalog,
        ValidationReport validationReport,
        IReadOnlyList<LoadedEvidenceSnapshot> evidenceSnapshots)
    {
        var normalizedEmit = NormalizeEmit(emit);
        var outputDirectory = Path.GetFullPath(Path.IsPathRooted(outDir) ? outDir : Path.Combine(rootPath, outDir));
        Directory.CreateDirectory(outputDirectory);

        var findingsByRequirement = validationReport.Findings
            .Where(finding => !string.IsNullOrWhiteSpace(finding.RequirementId))
            .GroupBy(finding => finding.RequirementId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderBy(finding => finding.Code, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

        var findingsBySpecification = validationReport.Findings
            .Where(finding => string.IsNullOrWhiteSpace(finding.RequirementId) && !string.IsNullOrWhiteSpace(finding.ArtifactId))
            .GroupBy(finding => finding.ArtifactId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderBy(finding => finding.Code, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

        var evidenceByRequirement = BuildEvidenceIndex(evidenceSnapshots);
        var specifications = artifacts
            .Where(item => string.Equals(item.Artifact.ArtifactType, "specification", StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.CuePath, StringComparer.OrdinalIgnoreCase)
            .Select(item => BuildSpecification(rootPath, item.CuePath, item.Artifact, findingsBySpecification, findingsByRequirement, evidenceByRequirement))
            .ToList();

        var requirements = specifications.SelectMany(specification => specification.Requirements).ToList();
        var snapshot = new SpecTraceAttestationSnapshot
        {
            SchemaVersion = 1,
            GeneratedAt = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Repository = new SpecTraceAttestationRepository
            {
                Root = rootPath.Replace('\\', '/'),
                DisplayName = Path.GetFileName(Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            },
            Selection = new SpecTraceAttestationSelection
            {
                Profile = profile,
                InputPath = string.IsNullOrWhiteSpace(inputPath) ? null : NormalizeRepoPath(rootPath, inputPath),
                Emit = normalizedEmit,
                OutputDirectory = NormalizeRepoPath(rootPath, outputDirectory),
            },
            Validation = new SpecTraceAttestationValidationSummary
            {
                ErrorCount = validationReport.Findings.Count(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase)),
                WarningCount = validationReport.Findings.Count(finding => string.Equals(finding.Severity, "warning", StringComparison.OrdinalIgnoreCase)),
                Findings = validationReport.Findings,
            },
            Evidence = new SpecTraceAttestationEvidenceSummary
            {
                FileCount = evidenceSnapshots.Count,
                SnapshotCount = evidenceSnapshots.Count,
                RequirementCount = evidenceSnapshots.Sum(snapshotItem => snapshotItem.Snapshot.Requirements.Count),
                ObservationCount = evidenceSnapshots.Sum(snapshotItem => snapshotItem.Snapshot.Requirements.Sum(requirement => requirement.Observations.Count)),
                Files = evidenceSnapshots
                    .OrderBy(snapshotItem => snapshotItem.RepoRelativePath, StringComparer.OrdinalIgnoreCase)
                    .Select(snapshotItem => new SpecTraceAttestationEvidenceFile
                    {
                        Path = snapshotItem.RepoRelativePath,
                        SnapshotId = snapshotItem.Snapshot.SnapshotId,
                        GeneratedAt = snapshotItem.Snapshot.GeneratedAt,
                        Producer = $"{snapshotItem.Snapshot.Producer.Name} {snapshotItem.Snapshot.Producer.Version}",
                        RequirementCount = snapshotItem.Snapshot.Requirements.Count,
                        ObservationCount = snapshotItem.Snapshot.Requirements.Sum(requirement => requirement.Observations.Count),
                    })
                    .ToList(),
            },
            Aggregates = BuildAggregateSummary(specifications),
            Specifications = specifications,
            Warnings = evidenceSnapshots.Count == 0
                ? ["No evidence snapshots were discovered for this report."]
                : null,
        };

        string? summaryPath = null;
        string? detailsPath = null;
        string? jsonPath = null;

        if (normalizedEmit is "html" or "both")
        {
            summaryPath = Path.Combine(outputDirectory, "summary.html");
            var indexPath = Path.Combine(outputDirectory, "index.html");
            detailsPath = Path.Combine(outputDirectory, "details.html");
            SpecTraceAttestationHtmlWriter.WriteSummary(summaryPath, snapshot, catalog);
            SpecTraceAttestationHtmlWriter.WriteSummary(indexPath, snapshot, catalog);
            SpecTraceAttestationHtmlWriter.WriteDetails(detailsPath, snapshot, catalog);
        }

        if (normalizedEmit is "json" or "both")
        {
            jsonPath = Path.Combine(outputDirectory, "attestation.json");
            File.WriteAllText(jsonPath, snapshot.ToJson());
        }

        return new SpecTraceAttestationResult
        {
            Snapshot = snapshot,
            SummaryHtmlPath = summaryPath,
            DetailsHtmlPath = detailsPath,
            JsonPath = jsonPath,
        };
    }

    private static SpecTraceAttestationSpecification BuildSpecification(
        string rootPath,
        string cuePath,
        ArtifactModel artifact,
        IReadOnlyDictionary<string, List<Finding>> findingsBySpecification,
        IReadOnlyDictionary<string, List<Finding>> findingsByRequirement,
        IReadOnlyDictionary<string, List<EvidenceObservationRecord>> evidenceByRequirement)
    {
        var markdownPath = Path.ChangeExtension(cuePath, ".md");
        return new SpecTraceAttestationSpecification
        {
            ArtifactId = artifact.ArtifactId,
            Title = artifact.Title,
            Status = artifact.Status,
            Domain = artifact.Domain,
            SourceCuePath = NormalizeRepoPath(rootPath, cuePath),
            SourceMarkdownPath = NormalizeRepoPath(rootPath, markdownPath),
            Findings = findingsBySpecification.TryGetValue(artifact.ArtifactId, out var specFindings) ? specFindings : [],
            Requirements = (artifact.Requirements ?? [])
                .Select(requirement => BuildRequirement(rootPath, markdownPath, requirement, findingsByRequirement, evidenceByRequirement))
                .ToList(),
        };
    }

    private static SpecTraceAttestationRequirement BuildRequirement(
        string rootPath,
        string markdownPath,
        RequirementModel requirement,
        IReadOnlyDictionary<string, List<Finding>> findingsByRequirement,
        IReadOnlyDictionary<string, List<EvidenceObservationRecord>> evidenceByRequirement)
    {
        var evidenceKinds = evidenceByRequirement.TryGetValue(requirement.Id, out var evidenceRecords)
            ? BuildEvidenceKinds(evidenceRecords)
            : [];
        var findings = findingsByRequirement.TryGetValue(requirement.Id, out var requirementFindings) ? requirementFindings : [];
        var issues = BuildRequirementIssues(requirement, evidenceKinds, findings);

        return new SpecTraceAttestationRequirement
        {
            RequirementId = requirement.Id,
            Title = requirement.Title,
            Statement = requirement.Statement,
            SourceMarkdownPath = NormalizeRepoPath(rootPath, markdownPath),
            Trace = new SpecTraceAttestationRequirementTrace
            {
                SatisfiedBy = Clone(requirement.Trace?.SatisfiedBy),
                ImplementedBy = Clone(requirement.Trace?.ImplementedBy),
                VerifiedBy = Clone(requirement.Trace?.VerifiedBy),
                DerivedFrom = Clone(requirement.Trace?.DerivedFrom),
                Supersedes = Clone(requirement.Trace?.Supersedes),
                UpstreamRefs = Clone(requirement.Trace?.UpstreamRefs),
                Related = Clone(requirement.Trace?.Related),
            },
            Notes = requirement.Notes is null ? null : [.. requirement.Notes],
            Evidence = evidenceKinds,
            DerivedStatus = DetermineDerivedStatus(requirement, evidenceKinds, findings),
            Issues = issues,
            Findings = findings,
        };
    }

    private static List<SpecTraceAttestationEvidenceKind> BuildEvidenceKinds(IEnumerable<EvidenceObservationRecord> evidenceRecords)
    {
        return evidenceRecords
            .GroupBy(record => record.Kind, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new SpecTraceAttestationEvidenceKind
            {
                Kind = group.Key,
                Status = AggregateEvidenceStatus(group.Select(record => record.Status)),
                Refs = group.SelectMany(record => record.Refs).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList(),
                SourceFiles = group.Select(record => record.SourceFile).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList(),
                Summaries = group.Select(record => record.Summary).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList()!,
            })
            .ToList();
    }

    private static Dictionary<string, List<EvidenceObservationRecord>> BuildEvidenceIndex(IReadOnlyList<LoadedEvidenceSnapshot> evidenceSnapshots)
    {
        var index = new Dictionary<string, List<EvidenceObservationRecord>>(StringComparer.OrdinalIgnoreCase);
        foreach (var snapshot in evidenceSnapshots)
        {
            foreach (var requirement in snapshot.Snapshot.Requirements)
            {
                if (!index.TryGetValue(requirement.RequirementId, out var records))
                {
                    records = [];
                    index[requirement.RequirementId] = records;
                }

                foreach (var observation in requirement.Observations)
                {
                    records.Add(new EvidenceObservationRecord(
                        observation.Kind,
                        observation.Status,
                        observation.Refs ?? [],
                        observation.Summary,
                        snapshot.RepoRelativePath));
                }
            }
        }

        return index;
    }

    private static SpecTraceAttestationAggregateSummary BuildAggregateSummary(IReadOnlyList<SpecTraceAttestationSpecification> specifications)
    {
        var requirements = specifications.SelectMany(specification => specification.Requirements).ToList();
        var evidenceKindCoverage = requirements
            .SelectMany(requirement => requirement.Evidence.Select(evidence => evidence.Kind))
            .GroupBy(kind => kind, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => requirements.Count(requirement => requirement.Evidence.Any(evidence => string.Equals(evidence.Kind, group.Key, StringComparison.OrdinalIgnoreCase))), StringComparer.OrdinalIgnoreCase);

        var derivedStatusCounts = requirements
            .GroupBy(requirement => requirement.DerivedStatus, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return new SpecTraceAttestationAggregateSummary
        {
            SpecificationCount = specifications.Count,
            RequirementCount = requirements.Count,
            RequirementsWithDownstreamTrace = requirements.Count(requirement => requirement.Trace.SatisfiedBy.Count > 0 || requirement.Trace.ImplementedBy.Count > 0 || requirement.Trace.VerifiedBy.Count > 0),
            RequirementsWithImplementation = requirements.Count(requirement => requirement.Trace.ImplementedBy.Count > 0),
            RequirementsWithVerification = requirements.Count(requirement => requirement.Trace.VerifiedBy.Count > 0),
            RequirementsWithEvidence = requirements.Count(requirement => requirement.Evidence.Count > 0),
            RequirementsWithValidationErrors = requirements.Count(requirement => requirement.Findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase))),
            RequirementsWithValidationWarnings = requirements.Count(requirement => requirement.Findings.Any(finding => string.Equals(finding.Severity, "warning", StringComparison.OrdinalIgnoreCase))),
            RequirementsWithAnyIssue = requirements.Count(requirement => requirement.Issues.Count > 0),
            DerivedStatusCounts = derivedStatusCounts,
            EvidenceKindCoverage = evidenceKindCoverage,
        };
    }

    private static List<string> BuildRequirementIssues(
        RequirementModel requirement,
        IReadOnlyList<SpecTraceAttestationEvidenceKind> evidenceKinds,
        IReadOnlyList<Finding> findings)
    {
        var issues = new List<string>();

        if ((requirement.Trace?.SatisfiedBy?.Count ?? 0) +
            (requirement.Trace?.ImplementedBy?.Count ?? 0) +
            (requirement.Trace?.VerifiedBy?.Count ?? 0) == 0)
        {
            issues.Add("downstream trace missing");
        }

        if ((requirement.Trace?.ImplementedBy?.Count ?? 0) == 0)
        {
            issues.Add("implementation missing");
        }

        if ((requirement.Trace?.VerifiedBy?.Count ?? 0) == 0)
        {
            issues.Add("verification missing");
        }

        foreach (var evidence in evidenceKinds)
        {
            if (string.Equals(evidence.Status, "failed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(evidence.Status, "not_collected", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(evidence.Status, "not_observed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(evidence.Status, "unsupported", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add($"{evidence.Kind}: {evidence.Status}");
            }
        }

        if (findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("validation error");
        }

        if (findings.Any(finding => string.Equals(finding.Severity, "warning", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add("validation warning");
        }

        return issues
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(issue => issue, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string DetermineDerivedStatus(
        RequirementModel requirement,
        IReadOnlyList<SpecTraceAttestationEvidenceKind> evidenceKinds,
        IReadOnlyList<Finding> findings)
    {
        if (findings.Any(finding => string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase)))
        {
            return "invalid";
        }

        if (evidenceKinds.Any(kind => string.Equals(kind.Status, "failed", StringComparison.OrdinalIgnoreCase)))
        {
            return "evidence-failing";
        }

        if ((requirement.Trace?.VerifiedBy?.Count ?? 0) > 0 &&
            evidenceKinds.Any(kind => string.Equals(kind.Status, "passed", StringComparison.OrdinalIgnoreCase) || string.Equals(kind.Status, "observed", StringComparison.OrdinalIgnoreCase)))
        {
            return "verified";
        }

        if ((requirement.Trace?.VerifiedBy?.Count ?? 0) > 0)
        {
            return "verification-linked";
        }

        if ((requirement.Trace?.ImplementedBy?.Count ?? 0) > 0)
        {
            return "implemented";
        }

        if ((requirement.Trace?.SatisfiedBy?.Count ?? 0) > 0)
        {
            return "designed";
        }

        return "specified";
    }

    private static string AggregateEvidenceStatus(IEnumerable<string> statuses)
    {
        var order = new[]
        {
            "failed",
            "not_collected",
            "not_observed",
            "unsupported",
            "passed",
            "observed",
        };

        var normalized = statuses
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .Select(status => status.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var status in order)
        {
            if (normalized.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return status;
            }
        }

        return normalized.FirstOrDefault() ?? "unknown";
    }

    private static string NormalizeEmit(string emit)
    {
        var normalized = string.IsNullOrWhiteSpace(emit) ? "both" : emit.Trim().ToLowerInvariant();
        return normalized is "html" or "json" or "both"
            ? normalized
            : throw new InvalidOperationException($"Unsupported attestation emit mode '{emit}'. Expected html, json, or both.");
    }

    private static List<string> Clone(IEnumerable<string>? values)
    {
        return values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList() ?? [];
    }

    private static string NormalizeRepoPath(string rootPath, string path)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath.Replace('\\', '/');
        }

        return Path.GetRelativePath(rootPath, fullPath).Replace('\\', '/');
    }

    private sealed record EvidenceObservationRecord(
        string Kind,
        string Status,
        IReadOnlyList<string> Refs,
        string? Summary,
        string SourceFile);
}
