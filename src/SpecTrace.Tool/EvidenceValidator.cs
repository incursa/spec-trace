using System.Text.Json;

namespace SpecTrace.Tool;

internal static class EvidenceValidator
{
    public static EvidenceDiscoveryResult DiscoverEvidenceFiles(string rootPath, IReadOnlyList<string> evidencePaths)
    {
        var findings = new List<Finding>();
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (evidencePaths.Count == 0)
        {
            foreach (var file in Directory.EnumerateFiles(rootPath, "*.evidence.json", SearchOption.AllDirectories)
                         .Where(path => !IsBuildArtifactPath(path)))
            {
                files.Add(Path.GetFullPath(file));
            }

            return new EvidenceDiscoveryResult
            {
                Files = files.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList(),
                Findings = findings,
            };
        }

        foreach (var candidate in evidencePaths)
        {
            var resolvedCandidate = Path.GetFullPath(Path.IsPathRooted(candidate) ? candidate : Path.Combine(rootPath, candidate));
            if (File.Exists(resolvedCandidate))
            {
                files.Add(resolvedCandidate);
                continue;
            }

            if (Directory.Exists(resolvedCandidate))
            {
                var matches = Directory.EnumerateFiles(resolvedCandidate, "*.evidence.json", SearchOption.AllDirectories)
                    .Where(path => !IsBuildArtifactPath(path))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (matches.Count == 0)
                {
                    findings.Add(CreateError(
                        "missing-evidence-files",
                        $"Path '{NormalizeRepoPath(rootPath, resolvedCandidate)}' does not contain any '*.evidence.json' files.",
                        resolvedCandidate,
                        null));
                    continue;
                }

                foreach (var match in matches)
                {
                    files.Add(Path.GetFullPath(match));
                }

                continue;
            }

            findings.Add(CreateError(
                "missing-evidence-path",
                $"Evidence path '{NormalizeRepoPath(rootPath, resolvedCandidate)}' does not exist.",
                resolvedCandidate,
                null));
        }

        return new EvidenceDiscoveryResult
        {
            Files = files.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList(),
            Findings = findings,
        };
    }

    public static async Task<EvidenceValidationResult> ValidateAsync(
        string rootPath,
        IReadOnlyList<string> evidenceFiles,
        ArtifactCatalog catalog,
        CancellationToken cancellationToken)
    {
        var findings = new List<Finding>();
        var snapshots = new List<LoadedEvidenceSnapshot>();
        var seenSnapshotIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var requirementCount = 0;
        var observationCount = 0;

        foreach (var evidenceFile in evidenceFiles)
        {
            var fullPath = Path.GetFullPath(evidenceFile);
            var repoRelativePath = NormalizeRepoPath(rootPath, fullPath);

            try
            {
                await CueCli.VetJsonFileAsync(
                    rootPath,
                    fullPath,
                    Path.Combine(rootPath, "model", "model.cue"),
                    "#EvidenceSnapshot",
                    cancellationToken);
            }
            catch (Exception exception)
            {
                findings.Add(CreateError(
                    "invalid-evidence-schema",
                    exception.Message,
                    fullPath,
                    null));
                continue;
            }

            EvidenceSnapshotModel snapshot;
            try
            {
                var json = await File.ReadAllTextAsync(fullPath, cancellationToken);
                snapshot = JsonSerializer.Deserialize<EvidenceSnapshotModel>(json, JsonOptions.Default)
                    ?? throw new InvalidOperationException($"JSON evidence file '{repoRelativePath}' could not be deserialized.");
            }
            catch (Exception exception)
            {
                findings.Add(CreateError(
                    "invalid-evidence-json",
                    $"Evidence file '{repoRelativePath}' could not be read as JSON: {exception.Message}",
                    fullPath,
                    null));
                continue;
            }

            if (!seenSnapshotIds.Add(snapshot.SnapshotId))
            {
                findings.Add(CreateError(
                    "duplicate-evidence-snapshot-id",
                    $"Duplicate evidence snapshot ID '{snapshot.SnapshotId}' was found.",
                    fullPath,
                    null));
            }

            var seenRequirementIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var requirement in snapshot.Requirements)
            {
                requirementCount++;

                if (!seenRequirementIds.Add(requirement.RequirementId))
                {
                    findings.Add(CreateError(
                        "duplicate-evidence-requirement",
                        $"Evidence snapshot '{snapshot.SnapshotId}' repeats requirement '{requirement.RequirementId}'.",
                        fullPath,
                        requirement.RequirementId));
                }

                if (!catalog.TryGetRequirement(requirement.RequirementId, out _))
                {
                    findings.Add(CreateError(
                        "broken-evidence-reference",
                        $"Evidence snapshot '{snapshot.SnapshotId}' references unknown requirement '{requirement.RequirementId}'.",
                        fullPath,
                        requirement.RequirementId));
                }

                var seenKinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var observation in requirement.Observations)
                {
                    observationCount++;
                    if (!seenKinds.Add(observation.Kind))
                    {
                        findings.Add(CreateError(
                            "duplicate-evidence-kind",
                            $"Evidence snapshot '{snapshot.SnapshotId}' repeats observation kind '{observation.Kind}' for requirement '{requirement.RequirementId}'.",
                            fullPath,
                            requirement.RequirementId));
                    }
                }
            }

            snapshots.Add(new LoadedEvidenceSnapshot
            {
                SourcePath = fullPath,
                RepoRelativePath = repoRelativePath,
                Snapshot = snapshot,
            });
        }

        return new EvidenceValidationResult
        {
            Report = new EvidenceValidationReport
            {
                FileCount = evidenceFiles.Count,
                SnapshotCount = snapshots.Count,
                RequirementCount = requirementCount,
                ObservationCount = observationCount,
                Findings = findings
                    .OrderBy(finding => finding.File, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(finding => finding.RequirementId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(finding => finding.Code, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            },
            Snapshots = snapshots
                .OrderBy(snapshot => snapshot.RepoRelativePath, StringComparer.OrdinalIgnoreCase)
                .ToList(),
        };
    }

    private static Finding CreateError(string code, string message, string? filePath, string? requirementId)
    {
        return new Finding
        {
            Severity = "error",
            Code = code,
            Message = message,
            File = filePath?.Replace('\\', '/'),
            RequirementId = requirementId,
        };
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

    private static bool IsBuildArtifactPath(string path)
    {
        var segments = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(segment =>
            string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(segment, ".git", StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed class EvidenceDiscoveryResult
{
    public required IReadOnlyList<string> Files { get; init; }

    public required IReadOnlyList<Finding> Findings { get; init; }
}
