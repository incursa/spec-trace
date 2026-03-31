using System.Text;
using System.Text.RegularExpressions;

namespace SpecTrace.Tool;

internal static class MarkdownArtifactWriter
{
    public static string WriteArtifact(ArtifactModel artifact, ArtifactCatalog catalog, string cuePath)
    {
        var markdownPath = Path.ChangeExtension(cuePath, ".md");
        var builder = new StringBuilder();

        WriteFrontMatter(builder, artifact);
        builder.AppendLine($"# [`{artifact.ArtifactId}`](./{Path.GetFileName(markdownPath)}) - {artifact.Title}");
        builder.AppendLine();

        switch (artifact.ArtifactType)
        {
            case "specification":
                WriteSpecification(builder, artifact, catalog, markdownPath);
                break;
            case "architecture":
                WriteArchitecture(builder, artifact, catalog, markdownPath);
                break;
            case "work_item":
                WriteWorkItem(builder, artifact, catalog, markdownPath);
                break;
            case "verification":
                WriteVerification(builder, artifact, catalog, markdownPath);
                break;
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void WriteFrontMatter(StringBuilder builder, ArtifactModel artifact)
    {
        builder.AppendLine("---");
        builder.AppendLine($"artifact_id: {artifact.ArtifactId}");
        builder.AppendLine($"artifact_type: {artifact.ArtifactType}");
        builder.AppendLine($"title: {artifact.Title}");
        builder.AppendLine($"domain: {artifact.Domain}");
        if (!string.IsNullOrWhiteSpace(artifact.Capability))
        {
            builder.AppendLine($"capability: {artifact.Capability}");
        }

        builder.AppendLine($"status: {artifact.Status}");
        builder.AppendLine($"owner: {artifact.Owner}");
        WriteYamlList(builder, "tags", artifact.Tags);
        WriteYamlList(builder, "related_artifacts", artifact.RelatedArtifacts);
        WriteYamlList(builder, "satisfies", artifact.Satisfies);
        WriteYamlList(builder, "addresses", artifact.Addresses);
        WriteYamlList(builder, "design_links", artifact.DesignLinks);
        WriteYamlList(builder, "verification_links", artifact.VerificationLinks);
        WriteYamlList(builder, "verifies", artifact.Verifies);
        builder.AppendLine("---");
        builder.AppendLine();
    }

    private static void WriteSpecification(StringBuilder builder, ArtifactModel artifact, ArtifactCatalog catalog, string markdownPath)
    {
        WriteSection(builder, "Purpose", artifact.Purpose);
        WriteSection(builder, "Scope", artifact.Scope);
        WriteSection(builder, "Context", artifact.Context);

        foreach (var section in artifact.SupplementalSections ?? [])
        {
            WriteSection(builder, section.Heading, section.Content);
        }

        foreach (var requirement in artifact.Requirements ?? [])
        {
            var anchor = MarkdownAnchor.CreateRequirementAnchor(requirement.Id, requirement.Title);
            builder.AppendLine($"## [`{requirement.Id}`](./{Path.GetFileName(markdownPath)}#{anchor}) {requirement.Title}");
            builder.AppendLine(NormalizeMarkdownText(requirement.Statement));
            builder.AppendLine();
            WriteRequirementTrace(builder, requirement.Trace, catalog, markdownPath);
            WriteNotes(builder, requirement.Notes);
        }

        WriteBulletedSection(builder, "Open Questions", artifact.OpenQuestions);
    }

    private static void WriteArchitecture(StringBuilder builder, ArtifactModel artifact, ArtifactCatalog catalog, string markdownPath)
    {
        WriteSection(builder, "Purpose", artifact.Purpose);
        WriteReferenceSection(builder, "Requirements Satisfied", artifact.Satisfies, catalog, markdownPath);
        WriteSection(builder, "Design Summary", artifact.DesignSummary);
        WriteBulletedSection(builder, "Key Components", artifact.KeyComponents);
        WriteSection(builder, "Data and State Considerations", artifact.DataAndStateConsiderations);
        WriteBulletedSection(builder, "Edge Cases and Constraints", artifact.EdgeCasesAndConstraints);
        WriteBulletedSection(builder, "Alternatives Considered", artifact.AlternativesConsidered);
        WriteBulletedSection(builder, "Risks", artifact.Risks);

        foreach (var section in artifact.SupplementalSections ?? [])
        {
            WriteSection(builder, section.Heading, section.Content);
        }

        WriteBulletedSection(builder, "Open Questions", artifact.OpenQuestions);
    }

    private static void WriteWorkItem(StringBuilder builder, ArtifactModel artifact, ArtifactCatalog catalog, string markdownPath)
    {
        WriteSection(builder, "Summary", artifact.Summary);
        WriteReferenceSection(builder, "Requirements Addressed", artifact.Addresses, catalog, markdownPath);
        WriteReferenceSection(builder, "Design Inputs", artifact.DesignLinks, catalog, markdownPath);
        WriteSection(builder, "Planned Changes", artifact.PlannedChanges);
        WriteBulletedSection(builder, "Out of Scope", artifact.OutOfScope);
        WriteSection(builder, "Verification Plan", artifact.VerificationPlan);
        WriteSection(builder, "Completion Notes", artifact.CompletionNotes);

        builder.AppendLine("## Trace Links");
        builder.AppendLine();
        WriteTraceLinkGroup(builder, "Addresses", artifact.Addresses, catalog, markdownPath);
        WriteTraceLinkGroup(builder, "Uses Design", artifact.DesignLinks, catalog, markdownPath);
        WriteTraceLinkGroup(builder, "Verified By", artifact.VerificationLinks, catalog, markdownPath);

        foreach (var section in artifact.SupplementalSections ?? [])
        {
            WriteSection(builder, section.Heading, section.Content);
        }
    }

    private static void WriteVerification(StringBuilder builder, ArtifactModel artifact, ArtifactCatalog catalog, string markdownPath)
    {
        WriteSection(builder, "Scope", artifact.Scope);
        WriteReferenceSection(builder, "Requirements Verified", artifact.Verifies, catalog, markdownPath);
        WriteSection(builder, "Verification Method", artifact.VerificationMethod);
        WriteBulletedSection(builder, "Preconditions", artifact.Preconditions);
        WriteNumberedSection(builder, "Procedure or Approach", artifact.Procedure);
        WriteSection(builder, "Expected Result", artifact.ExpectedResult);
        WriteBulletedSection(builder, "Evidence", artifact.Evidence);
        WriteSection(builder, "Status", !string.IsNullOrWhiteSpace(artifact.StatusSummary)
            ? artifact.StatusSummary
            : $"This `{artifact.Status}` status applies to every requirement listed in `verifies`.\n\n{artifact.Status}");
        WriteReferenceSection(builder, "Related Artifacts", artifact.RelatedArtifacts, catalog, markdownPath);

        foreach (var section in artifact.SupplementalSections ?? [])
        {
            WriteSection(builder, section.Heading, section.Content);
        }
    }

    private static void WriteRequirementTrace(StringBuilder builder, RequirementTraceModel? trace, ArtifactCatalog catalog, string markdownPath)
    {
        if (trace is null)
        {
            return;
        }

        if ((trace.SatisfiedBy?.Count ?? 0) == 0 &&
            (trace.ImplementedBy?.Count ?? 0) == 0 &&
            (trace.VerifiedBy?.Count ?? 0) == 0 &&
            (trace.DerivedFrom?.Count ?? 0) == 0 &&
            (trace.Supersedes?.Count ?? 0) == 0 &&
            (trace.UpstreamRefs?.Count ?? 0) == 0 &&
            (trace.Related?.Count ?? 0) == 0)
        {
            return;
        }

        builder.AppendLine("Trace:");
        WriteTraceLabel(builder, "Satisfied By", trace.SatisfiedBy, catalog, markdownPath);
        WriteTraceLabel(builder, "Implemented By", trace.ImplementedBy, catalog, markdownPath);
        WriteTraceLabel(builder, "Verified By", trace.VerifiedBy, catalog, markdownPath);
        WriteTraceLabel(builder, "Derived From", trace.DerivedFrom, catalog, markdownPath);
        WriteTraceLabel(builder, "Supersedes", trace.Supersedes, catalog, markdownPath);

        if (trace.UpstreamRefs?.Count > 0)
        {
            builder.AppendLine("- Upstream Refs:");
            foreach (var item in trace.UpstreamRefs)
            {
                builder.AppendLine($"  - {item}");
            }
        }

        WriteTraceLabel(builder, "Related", trace.Related, catalog, markdownPath);
        builder.AppendLine();
    }

    private static void WriteTraceLabel(StringBuilder builder, string label, IEnumerable<string>? ids, ArtifactCatalog catalog, string markdownPath)
    {
        var list = ids?.ToList();
        if (list is null || list.Count == 0)
        {
            return;
        }

        builder.AppendLine($"- {label}:");
        foreach (var id in list)
        {
            builder.AppendLine($"  - {catalog.RenderReference(id, markdownPath)}");
        }
    }

    private static void WriteTraceLinkGroup(StringBuilder builder, string label, IEnumerable<string>? ids, ArtifactCatalog catalog, string markdownPath)
    {
        builder.AppendLine($"{label}:");
        builder.AppendLine();
        foreach (var id in ids ?? [])
        {
            builder.AppendLine($"- {catalog.RenderReference(id, markdownPath)}");
        }

        builder.AppendLine();
    }

    private static void WriteSection(StringBuilder builder, string heading, string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        builder.AppendLine(NormalizeMarkdownText(content));
        builder.AppendLine();
    }

    private static void WriteBulletedSection(StringBuilder builder, string heading, IEnumerable<string>? items)
    {
        var list = items?.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        if (list is null || list.Count == 0)
        {
            return;
        }

        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        foreach (var item in list)
        {
            builder.AppendLine($"- {item}");
        }

        builder.AppendLine();
    }

    private static void WriteNumberedSection(StringBuilder builder, string heading, IEnumerable<string>? items)
    {
        var list = items?.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        if (list is null || list.Count == 0)
        {
            return;
        }

        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        var index = 1;
        foreach (var item in list)
        {
            builder.AppendLine($"{index}. {item}");
            index++;
        }

        builder.AppendLine();
    }

    private static void WriteReferenceSection(StringBuilder builder, string heading, IEnumerable<string>? ids, ArtifactCatalog catalog, string markdownPath)
    {
        var list = ids?.ToList();
        if (list is null || list.Count == 0)
        {
            return;
        }

        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        foreach (var id in list)
        {
            builder.AppendLine($"- {catalog.RenderReference(id, markdownPath)}");
        }

        builder.AppendLine();
    }

    private static void WriteNotes(StringBuilder builder, IEnumerable<string>? notes)
    {
        var list = notes?.Where(note => !string.IsNullOrWhiteSpace(note)).ToList();
        if (list is null || list.Count == 0)
        {
            return;
        }

        builder.AppendLine("Notes:");
        foreach (var note in list)
        {
            builder.AppendLine($"- {note}");
        }

        builder.AppendLine();
    }

    private static void WriteYamlList(StringBuilder builder, string key, IEnumerable<string>? values)
    {
        var list = values?.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        if (list is null || list.Count == 0)
        {
            return;
        }

        builder.AppendLine($"{key}:");
        foreach (var item in list)
        {
            builder.AppendLine($"  - {item}");
        }
    }

    private static string NormalizeMarkdownText(string content) => content.Replace("\r\n", "\n").Trim();
}

internal static class MarkdownAnchor
{
    public static string CreateRequirementAnchor(string requirementId, string title)
    {
        var raw = $"{requirementId} {title}".ToLowerInvariant();
        raw = Regex.Replace(raw, @"[`'\[\]\(\)\./]", string.Empty);
        raw = Regex.Replace(raw, @"[^a-z0-9 -]", string.Empty);
        raw = Regex.Replace(raw, @"\s+", "-");
        raw = Regex.Replace(raw, @"-+", "-");
        return raw.Trim('-');
    }
}
