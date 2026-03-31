namespace SpecTrace.Tool;

internal sealed class ArtifactCatalog
{
    private readonly Dictionary<string, CatalogItem> _artifacts;
    private readonly Dictionary<string, CatalogItem> _requirements;

    public ArtifactCatalog(IEnumerable<(string CuePath, ArtifactModel Artifact)> artifacts)
    {
        _artifacts = new Dictionary<string, CatalogItem>(StringComparer.OrdinalIgnoreCase);
        _requirements = new Dictionary<string, CatalogItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var (cuePath, artifact) in artifacts)
        {
            var markdownPath = Path.ChangeExtension(cuePath, ".md");
            _artifacts[artifact.ArtifactId] = new CatalogItem
            {
                Id = artifact.ArtifactId,
                Kind = "artifact",
                SourceCuePath = cuePath,
                SourceMarkdownPath = markdownPath,
                Title = artifact.Title,
                ArtifactType = artifact.ArtifactType,
                Domain = artifact.Domain,
            };

            foreach (var requirement in artifact.Requirements ?? [])
            {
                _requirements[requirement.Id] = new CatalogItem
                {
                    Id = requirement.Id,
                    Kind = "requirement",
                    SourceCuePath = cuePath,
                    SourceMarkdownPath = markdownPath,
                    Title = requirement.Title,
                    ArtifactType = "requirement",
                    ParentArtifactId = artifact.ArtifactId,
                    Domain = artifact.Domain,
                };
            }
        }
    }

    public IReadOnlyDictionary<string, CatalogItem> ArtifactItems => _artifacts;

    public IReadOnlyDictionary<string, CatalogItem> RequirementItems => _requirements;

    public bool TryGetArtifact(string id, out CatalogItem item) => _artifacts.TryGetValue(id, out item!);

    public bool TryGetRequirement(string id, out CatalogItem item) => _requirements.TryGetValue(id, out item!);

    public CatalogSnapshot CreateSnapshot(IEnumerable<(string CuePath, ArtifactModel Artifact)> artifacts)
    {
        var entries = new Dictionary<string, CatalogItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _artifacts.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            entries[pair.Key] = pair.Value;
        }

        foreach (var pair in _requirements.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            entries[pair.Key] = pair.Value;
        }

        var references = new List<CatalogReference>();
        foreach (var (_, artifact) in artifacts.OrderBy(item => item.CuePath, StringComparer.OrdinalIgnoreCase))
        {
            AddReferences(references, artifact.ArtifactId, "related_artifacts", artifact.RelatedArtifacts, "artifact", null);

            switch (artifact.ArtifactType)
            {
                case "architecture":
                    AddReferences(references, artifact.ArtifactId, "satisfies", artifact.Satisfies, "requirement", "REQ");
                    break;
                case "work_item":
                    AddReferences(references, artifact.ArtifactId, "addresses", artifact.Addresses, "requirement", "REQ");
                    AddReferences(references, artifact.ArtifactId, "design_links", artifact.DesignLinks, "artifact", "ARC");
                    AddReferences(references, artifact.ArtifactId, "verification_links", artifact.VerificationLinks, "artifact", "VER");
                    break;
                case "verification":
                    AddReferences(references, artifact.ArtifactId, "verifies", artifact.Verifies, "requirement", "REQ");
                    break;
            }

            foreach (var requirement in artifact.Requirements ?? [])
            {
                AddReferences(references, requirement.Id, "satisfied_by", requirement.Trace?.SatisfiedBy, "artifact", "ARC");
                AddReferences(references, requirement.Id, "implemented_by", requirement.Trace?.ImplementedBy, "artifact", "WI");
                AddReferences(references, requirement.Id, "verified_by", requirement.Trace?.VerifiedBy, "artifact", "VER");
                AddReferences(references, requirement.Id, "derived_from", requirement.Trace?.DerivedFrom, "requirement", "REQ");
                AddReferences(references, requirement.Id, "supersedes", requirement.Trace?.Supersedes, "requirement", "REQ");
                AddMixedReferences(references, requirement.Id, "related", requirement.Trace?.Related);
            }
        }

        references = references
            .OrderBy(reference => reference.SourceId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(reference => reference.Field, StringComparer.OrdinalIgnoreCase)
            .ThenBy(reference => reference.TargetId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new CatalogSnapshot
        {
            Entries = entries,
            References = references,
        };
    }

    public string RenderReference(string id, string sourceMarkdownPath)
    {
        if (TryGetArtifact(id, out var artifact))
        {
            var relativePath = Path.GetRelativePath(Path.GetDirectoryName(sourceMarkdownPath)!, artifact.SourceMarkdownPath).Replace('\\', '/');
            if (!relativePath.StartsWith('.'))
            {
                relativePath = $"./{relativePath}";
            }

            return $"[{id}]({relativePath})";
        }

        if (TryGetRequirement(id, out var requirement))
        {
            var relativePath = Path.GetRelativePath(Path.GetDirectoryName(sourceMarkdownPath)!, requirement.SourceMarkdownPath).Replace('\\', '/');
            if (!relativePath.StartsWith('.'))
            {
                relativePath = $"./{relativePath}";
            }

            var anchor = MarkdownAnchor.CreateRequirementAnchor(requirement.Id, requirement.Title);
            return $"[`{id}`]({relativePath}#{anchor})";
        }

        return $"`{id}`";
    }

    private static void AddReferences(
        ICollection<CatalogReference> references,
        string sourceId,
        string field,
        IEnumerable<string>? targetIds,
        string expectedKind,
        string? expectedPrefix)
    {
        foreach (var targetId in targetIds ?? [])
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                continue;
            }

            references.Add(new CatalogReference
            {
                SourceId = sourceId,
                Field = field,
                TargetId = targetId,
                ExpectedKind = expectedKind,
                ExpectedPrefix = expectedPrefix,
            });
        }
    }

    private static void AddMixedReferences(
        ICollection<CatalogReference> references,
        string sourceId,
        string field,
        IEnumerable<string>? targetIds)
    {
        foreach (var targetId in targetIds ?? [])
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                continue;
            }

            var trimmedTargetId = targetId.Trim();
            var isRequirement = trimmedTargetId.StartsWith("REQ-", StringComparison.OrdinalIgnoreCase);
            references.Add(new CatalogReference
            {
                SourceId = sourceId,
                Field = field,
                TargetId = trimmedTargetId,
                ExpectedKind = isRequirement ? "requirement" : "artifact",
                ExpectedPrefix = isRequirement ? "REQ" : null,
            });
        }
    }
}
