using System.Text.RegularExpressions;

namespace SpecTrace.Tool;

internal static class RepositoryValidator
{
    public static ValidationReport Validate(
        string rootPath,
        string profile,
        IReadOnlyList<(string SourcePath, ArtifactModel Artifact)> artifacts,
        RetiredRequirementLedger? retiredLedger)
    {
        var findings = new List<Finding>();
        var catalog = new ArtifactCatalog(artifacts);
        var artifactLookup = artifacts.ToDictionary(item => item.Artifact.ArtifactId, item => item.Artifact, StringComparer.OrdinalIgnoreCase);
        var retiredRequirementIds = new HashSet<string>(retiredLedger?.RetiredRequirements.Select(record => record.Id) ?? [], StringComparer.OrdinalIgnoreCase);
        var seenArtifactIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenRequirementIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (sourcePath, artifact) in artifacts)
        {
            ValidateArtifactPlacement(sourcePath, artifact, rootPath, findings);
            if (!seenArtifactIds.Add(artifact.ArtifactId))
            {
                AddError(findings, "duplicate-artifact-id", $"Duplicate artifact ID '{artifact.ArtifactId}'.", sourcePath, artifact.ArtifactId, null);
            }

            foreach (var requirement in artifact.Requirements ?? [])
            {
                if (!seenRequirementIds.Add(requirement.Id))
                {
                    AddError(findings, "duplicate-requirement-id", $"Duplicate requirement ID '{requirement.Id}'.", sourcePath, artifact.ArtifactId, requirement.Id);
                }
            }
            ValidateRequirementNamespace(sourcePath, artifact, findings);
        }

        foreach (var (sourcePath, artifact) in artifacts)
        {
            ValidateReferences(sourcePath, artifact, profile, catalog, artifactLookup, retiredRequirementIds, findings);
        }

        if (string.Equals(profile, "auditable", StringComparison.OrdinalIgnoreCase))
        {
            ValidateOrphans(artifacts, findings);
        }

        return new ValidationReport
        {
            Profile = profile,
            ArtifactCount = artifacts.Count,
            RequirementCount = artifacts.Sum(item => item.Artifact.Requirements?.Count ?? 0),
            Findings = findings
                .OrderBy(finding => finding.File, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.ArtifactId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.RequirementId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(finding => finding.Code, StringComparer.OrdinalIgnoreCase)
                .ToList(),
        };
    }

    private static void ValidateArtifactPlacement(string sourcePath, ArtifactModel artifact, string rootPath, List<Finding> findings)
    {
        var relativeSourcePath = Path.GetRelativePath(rootPath, sourcePath).Replace('\\', '/');
        var owningDirectory = GetOwningDirectory(relativeSourcePath);
        if (!string.IsNullOrWhiteSpace(owningDirectory) &&
            !string.Equals(artifact.Domain, owningDirectory, StringComparison.OrdinalIgnoreCase))
        {
            AddError(findings, "domain-path-mismatch", $"Domain '{artifact.Domain}' does not align with directory '{owningDirectory}'.", sourcePath, artifact.ArtifactId, null);
        }
    }

    private static void ValidateRequirementNamespace(string sourcePath, ArtifactModel artifact, List<Finding> findings)
    {
        if (!string.Equals(artifact.ArtifactType, "specification", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var specificationNamespace = GetNamespaceFromSpecificationId(artifact.ArtifactId);
        foreach (var requirement in artifact.Requirements ?? [])
        {
            if (!string.Equals(specificationNamespace, GetNamespaceFromRequirementId(requirement.Id), StringComparison.Ordinal))
            {
                AddError(findings, "requirement-namespace-mismatch", $"Requirement '{requirement.Id}' does not align with specification '{artifact.ArtifactId}'.", sourcePath, artifact.ArtifactId, requirement.Id);
            }
        }
    }

    private static void ValidateReferences(
        string sourcePath,
        ArtifactModel artifact,
        string profile,
        ArtifactCatalog catalog,
        IReadOnlyDictionary<string, ArtifactModel> artifactLookup,
        ISet<string> retiredRequirementIds,
        List<Finding> findings)
    {
        ValidateReferenceList(sourcePath, artifact.ArtifactId, null, "related_artifacts", artifact.RelatedArtifacts, catalog, ReferenceExpectation.Artifact, null, findings);

        switch (artifact.ArtifactType)
        {
                case "architecture":
                ValidateReferenceList(sourcePath, artifact.ArtifactId, null, "satisfies", artifact.Satisfies, catalog, ReferenceExpectation.Requirement, "REQ", findings);
                    break;
                case "work_item":
                ValidateReferenceList(sourcePath, artifact.ArtifactId, null, "addresses", artifact.Addresses, catalog, ReferenceExpectation.Requirement, "REQ", findings);
                ValidateReferenceList(sourcePath, artifact.ArtifactId, null, "design_links", artifact.DesignLinks, catalog, ReferenceExpectation.Artifact, "ARC", findings);
                ValidateReferenceList(sourcePath, artifact.ArtifactId, null, "verification_links", artifact.VerificationLinks, catalog, ReferenceExpectation.Artifact, "VER", findings);
                    break;
                case "verification":
                ValidateReferenceList(sourcePath, artifact.ArtifactId, null, "verifies", artifact.Verifies, catalog, ReferenceExpectation.Requirement, "REQ", findings);
                    break;
            }

        foreach (var requirement in artifact.Requirements ?? [])
        {
            ValidateReferenceList(sourcePath, artifact.ArtifactId, requirement.Id, "satisfied_by", requirement.Trace?.SatisfiedBy, catalog, ReferenceExpectation.Artifact, "ARC", findings);
            ValidateReferenceList(sourcePath, artifact.ArtifactId, requirement.Id, "implemented_by", requirement.Trace?.ImplementedBy, catalog, ReferenceExpectation.Artifact, "WI", findings);
            ValidateReferenceList(sourcePath, artifact.ArtifactId, requirement.Id, "verified_by", requirement.Trace?.VerifiedBy, catalog, ReferenceExpectation.Artifact, "VER", findings);
            ValidateReferenceList(sourcePath, artifact.ArtifactId, requirement.Id, "derived_from", requirement.Trace?.DerivedFrom, catalog, ReferenceExpectation.Requirement, "REQ", findings, retiredRequirementIds);
            ValidateReferenceList(sourcePath, artifact.ArtifactId, requirement.Id, "supersedes", requirement.Trace?.Supersedes, catalog, ReferenceExpectation.Requirement, "REQ", findings, retiredRequirementIds);
            ValidateReferenceList(sourcePath, artifact.ArtifactId, requirement.Id, "related", requirement.Trace?.Related, catalog, ReferenceExpectation.Mixed, null, findings);

            if (RequiresTraceableCoverage(profile))
            {
                var downstreamCount =
                    (requirement.Trace?.SatisfiedBy?.Count ?? 0) +
                    (requirement.Trace?.ImplementedBy?.Count ?? 0) +
                    (requirement.Trace?.VerifiedBy?.Count ?? 0);

                if (downstreamCount == 0)
                {
                    AddError(findings, "missing-downstream-trace", $"Requirement '{requirement.Id}' must have at least one downstream trace link in profile '{profile}'.", sourcePath, artifact.ArtifactId, requirement.Id);
                }
            }

            if (RequiresAuditableCoverage(profile) && (requirement.Trace?.VerifiedBy?.Count ?? 0) == 0)
            {
                AddError(findings, "missing-verification-coverage", $"Requirement '{requirement.Id}' must have at least one verified_by link in profile 'auditable'.", sourcePath, artifact.ArtifactId, requirement.Id);
            }

            if (RequiresAuditableCoverage(profile))
            {
                ValidateReciprocalLinks(sourcePath, artifact.ArtifactId, requirement, artifactLookup, findings);
            }
        }
    }

    private static void ValidateReciprocalLinks(
        string sourcePath,
        string artifactId,
        RequirementModel requirement,
        IReadOnlyDictionary<string, ArtifactModel> artifactLookup,
        List<Finding> findings)
    {
        foreach (var architectureId in requirement.Trace?.SatisfiedBy ?? [])
        {
            if (!artifactLookup.TryGetValue(architectureId, out var targetArtifact))
            {
                continue;
            }

            if (!(targetArtifact.Satisfies?.Contains(requirement.Id, StringComparer.OrdinalIgnoreCase) ?? false))
            {
                AddError(findings, "missing-reciprocal-link", $"Requirement '{requirement.Id}' is satisfied_by '{architectureId}', but the architecture artifact does not list the requirement in 'satisfies'.", sourcePath, artifactId, requirement.Id);
            }
        }

        foreach (var workItemId in requirement.Trace?.ImplementedBy ?? [])
        {
            if (!artifactLookup.TryGetValue(workItemId, out var targetArtifact))
            {
                continue;
            }

            if (!(targetArtifact.Addresses?.Contains(requirement.Id, StringComparer.OrdinalIgnoreCase) ?? false))
            {
                AddError(findings, "missing-reciprocal-link", $"Requirement '{requirement.Id}' is implemented_by '{workItemId}', but the work item does not list the requirement in 'addresses'.", sourcePath, artifactId, requirement.Id);
            }
        }

        foreach (var verificationId in requirement.Trace?.VerifiedBy ?? [])
        {
            if (!artifactLookup.TryGetValue(verificationId, out var targetArtifact))
            {
                continue;
            }

            if (!(targetArtifact.Verifies?.Contains(requirement.Id, StringComparer.OrdinalIgnoreCase) ?? false))
            {
                AddError(findings, "missing-reciprocal-link", $"Requirement '{requirement.Id}' is verified_by '{verificationId}', but the verification artifact does not list the requirement in 'verifies'.", sourcePath, artifactId, requirement.Id);
            }
        }
    }

    private static void ValidateOrphans(IReadOnlyList<(string SourcePath, ArtifactModel Artifact)> artifacts, List<Finding> findings)
    {
        var targetedArtifacts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, artifact) in artifacts)
        {
            foreach (var requirement in artifact.Requirements ?? [])
            {
                foreach (var id in requirement.Trace?.SatisfiedBy ?? [])
                {
                    targetedArtifacts.Add(id);
                }

                foreach (var id in requirement.Trace?.ImplementedBy ?? [])
                {
                    targetedArtifacts.Add(id);
                }

                foreach (var id in requirement.Trace?.VerifiedBy ?? [])
                {
                    targetedArtifacts.Add(id);
                }
            }
        }

        foreach (var (sourcePath, artifact) in artifacts)
        {
            if (artifact.ArtifactType == "specification")
            {
                continue;
            }

            if (!targetedArtifacts.Contains(artifact.ArtifactId))
            {
                AddError(findings, "orphan-artifact", $"Artifact '{artifact.ArtifactId}' is not targeted by any requirement downstream trace.", sourcePath, artifact.ArtifactId, null);
            }
        }
    }

    private static void ValidateReferenceList(
        string sourcePath,
        string artifactId,
        string? requirementId,
        string fieldName,
        IEnumerable<string>? references,
        ArtifactCatalog catalog,
        ReferenceExpectation expectation,
        string? expectedPrefix,
        List<Finding> findings,
        ISet<string>? retiredRequirementIds = null)
    {
        if (references is null)
        {
            return;
        }

        _ = expectedPrefix;

        foreach (var reference in references)
        {
            var resolved = expectation switch
            {
                ReferenceExpectation.Artifact => catalog.TryGetArtifact(reference, out _),
                ReferenceExpectation.Requirement => catalog.TryGetRequirement(reference, out _),
                ReferenceExpectation.Mixed => catalog.TryGetArtifact(reference, out _) || catalog.TryGetRequirement(reference, out _),
                _ => false,
            };

            if (!resolved &&
                expectation == ReferenceExpectation.Requirement &&
                retiredRequirementIds is not null &&
                retiredRequirementIds.Contains(reference))
            {
                continue;
            }

            if (!resolved)
            {
                AddError(findings, "broken-reference", $"Field '{fieldName}' references missing target '{reference}'.", sourcePath, artifactId, requirementId);
            }
        }
    }

    private static bool RequiresTraceableCoverage(string profile)
    {
        return string.Equals(profile, "traceable", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(profile, "auditable", StringComparison.OrdinalIgnoreCase);
    }

    private static bool RequiresAuditableCoverage(string profile)
    {
        return string.Equals(profile, "auditable", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetOwningDirectory(string relativeSourcePath)
    {
        var match = Regex.Match(relativeSourcePath, @"^(?:specs/requirements|examples)/(?<domain>[^/]+)/");
        return match.Success ? match.Groups["domain"].Value : null;
    }

    private static string? GetNamespaceFromSpecificationId(string specificationId)
    {
        var match = Regex.Match(specificationId, @"^SPEC-(?<ns>[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*)$");
        return match.Success ? match.Groups["ns"].Value : null;
    }

    private static string? GetNamespaceFromRequirementId(string requirementId)
    {
        var match = Regex.Match(requirementId, @"^REQ-(?<ns>[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*)-\d{4,}$");
        return match.Success ? match.Groups["ns"].Value : null;
    }

    private static void AddError(List<Finding> findings, string code, string message, string file, string? artifactId, string? requirementId)
    {
        findings.Add(new Finding
        {
            Severity = "error",
            Code = code,
            Message = message,
            File = file.Replace('\\', '/'),
            ArtifactId = artifactId,
            RequirementId = requirementId,
        });
    }

    private enum ReferenceExpectation
    {
        Artifact,
        Requirement,
        Mixed,
    }
}
