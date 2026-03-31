using System.Net;
using System.Text;

namespace SpecTrace.Tool;

internal static class SpecTraceAttestationHtmlWriter
{
    public static void WriteSummary(string path, SpecTraceAttestationSnapshot snapshot, ArtifactCatalog catalog)
    {
        File.WriteAllText(path, BuildSummaryHtml(path, snapshot, catalog));
    }

    public static void WriteDetails(string path, SpecTraceAttestationSnapshot snapshot, ArtifactCatalog catalog)
    {
        File.WriteAllText(path, BuildDetailsHtml(path, snapshot, catalog));
        WriteSpecificationPages(path, snapshot, catalog);
    }

    private static string BuildSummaryHtml(string reportPath, SpecTraceAttestationSnapshot snapshot, ArtifactCatalog catalog)
    {
        var specifications = BuildSpecificationSummaries(reportPath, snapshot);
        var builder = new StringBuilder();
        AppendDocumentStart(builder, $"SpecTrace Attestation Summary - {snapshot.Repository.DisplayName}");
        AppendHeader(builder, "SpecTrace Attestation Summary", new[]
        {
            ("index.html", "index.html"),
            ("details.html", "details.html"),
            ("attestation.json", "attestation.json"),
        });
        AppendRepositorySection(builder, snapshot);
        AppendValidationSection(builder, snapshot);
        AppendAggregateSection(builder, snapshot);
        AppendEvidenceSection(builder, reportPath, snapshot);
        AppendSpecificationIndexSection(builder, reportPath, snapshot, specifications.Where(specification => specification.IssueBearingRequirements > 0 || specification.SpecificationFindings > 0).ToList(), "Specifications with issues");
        AppendFooter(builder);
        return builder.ToString();
    }

    private static string BuildDetailsHtml(string reportPath, SpecTraceAttestationSnapshot snapshot, ArtifactCatalog catalog)
    {
        var specifications = BuildSpecificationSummaries(reportPath, snapshot);
        var builder = new StringBuilder();
        AppendDocumentStart(builder, $"SpecTrace Attestation Details - {snapshot.Repository.DisplayName}");
        AppendHeader(builder, "SpecTrace Attestation Details", new[]
        {
            ("index.html", "index.html"),
            ("summary.html", "summary.html"),
            ("attestation.json", "attestation.json"),
        });
        AppendRepositorySection(builder, snapshot);
        AppendValidationSection(builder, snapshot);
        AppendAggregateSection(builder, snapshot);
        AppendEvidenceSection(builder, reportPath, snapshot);
        AppendSpecificationIndexSection(builder, reportPath, snapshot, specifications, "Specification breakdown");
        AppendFooter(builder);
        return builder.ToString();
    }

    private static void WriteSpecificationPages(string detailsPath, SpecTraceAttestationSnapshot snapshot, ArtifactCatalog catalog)
    {
        var reportRoot = Path.GetDirectoryName(detailsPath)!;
        foreach (var specification in snapshot.Specifications)
        {
            var pagePath = ResolveSpecificationPagePath(reportRoot, specification.SourcePath, specification.ArtifactId);
            Directory.CreateDirectory(Path.GetDirectoryName(pagePath)!);
            File.WriteAllText(pagePath, BuildSpecificationHtml(pagePath, reportRoot, snapshot, specification, catalog));
        }
    }

    private static string BuildSpecificationHtml(
        string pagePath,
        string reportRoot,
        SpecTraceAttestationSnapshot snapshot,
        SpecTraceAttestationSpecification specification,
        ArtifactCatalog catalog)
    {
        var builder = new StringBuilder();
        AppendDocumentStart(builder, $"SpecTrace Attestation - {specification.ArtifactId}");
        AppendHeader(builder, $"SpecTrace Attestation - {specification.ArtifactId}", new[]
        {
            ("index.html", RelativePath(pagePath, Path.Combine(reportRoot, "index.html"))),
            ("details.html", RelativePath(pagePath, Path.Combine(reportRoot, "details.html"))),
            ("attestation.json", RelativePath(pagePath, Path.Combine(reportRoot, "attestation.json"))),
        });

        AppendKeyValueTable(builder, "Specification", new[]
        {
            ("Specification", $"{specification.ArtifactId} - {specification.Title}"),
            ("Status", specification.Status),
            ("Domain", specification.Domain),
            ("Source", LinkToRepoPath(pagePath, snapshot.Repository.Root, specification.SourcePath)),
            ("Requirements", FormatInt(specification.Requirements.Count)),
            ("Specification findings", FormatInt(specification.Findings.Count)),
        });

        if (specification.Findings.Count > 0)
        {
            AppendFindingsSection(builder, "Specification findings", specification.Findings);
        }

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>Requirements</h2>");
        foreach (var requirement in specification.Requirements.OrderBy(requirement => requirement.RequirementId, StringComparer.OrdinalIgnoreCase))
        {
            AppendRequirementCard(builder, pagePath, snapshot, catalog, requirement);
        }

        builder.AppendLine("</section>");
        AppendFooter(builder);
        return builder.ToString();
    }

    private static void AppendRequirementCard(
        StringBuilder builder,
        string reportPath,
        SpecTraceAttestationSnapshot snapshot,
        ArtifactCatalog catalog,
        SpecTraceAttestationRequirement requirement)
    {
        builder.AppendLine($"<details class=\"requirement-card\" id=\"{Encode(requirement.RequirementId)}\">");
        builder.AppendLine("<summary>");
        builder.AppendLine($"<span class=\"requirement-id\">{Encode(requirement.RequirementId)}</span> ");
        builder.AppendLine($"<span class=\"requirement-title\">{Encode(requirement.Title)}</span> ");
        builder.AppendLine($"<span class=\"requirement-meta\">{Encode(requirement.DerivedStatus)}</span>");
        if (requirement.Issues.Count > 0)
        {
            builder.AppendLine($"<span class=\"requirement-tags\">{RenderTags(requirement.Issues)}</span>");
        }

        builder.AppendLine("</summary>");
        builder.AppendLine("<div class=\"requirement-body\">");
        builder.AppendLine($"<p>{Encode(requirement.Statement)}</p>");
        AppendKeyValueTable(builder, null, new[]
        {
            ("Source", LinkToRepoPath(reportPath, snapshot.Repository.Root, requirement.SourcePath)),
            ("Satisfied By", RenderReferenceList(reportPath, catalog, requirement.Trace.SatisfiedBy)),
            ("Implemented By", RenderReferenceList(reportPath, catalog, requirement.Trace.ImplementedBy)),
            ("Verified By", RenderReferenceList(reportPath, catalog, requirement.Trace.VerifiedBy)),
            ("Derived From", RenderReferenceList(reportPath, catalog, requirement.Trace.DerivedFrom)),
            ("Supersedes", RenderReferenceList(reportPath, catalog, requirement.Trace.Supersedes)),
            ("Upstream Refs", RenderList(requirement.Trace.UpstreamRefs)),
            ("Related", RenderReferenceList(reportPath, catalog, requirement.Trace.Related)),
            ("Evidence", requirement.Evidence.Count == 0 ? "none" : string.Join("<br />", requirement.Evidence.Select(RenderEvidenceKind))),
            ("Issues", requirement.Issues.Count == 0 ? "none" : Encode(string.Join(", ", requirement.Issues))),
        });

        if (requirement.Notes?.Count > 0)
        {
            builder.AppendLine("<h3>Notes</h3>");
            builder.AppendLine("<ul>");
            foreach (var note in requirement.Notes)
            {
                builder.AppendLine($"<li>{Encode(note)}</li>");
            }

            builder.AppendLine("</ul>");
        }

        if (requirement.Findings.Count > 0)
        {
            AppendFindingsSection(builder, "Validation findings", requirement.Findings);
        }

        builder.AppendLine("</div>");
        builder.AppendLine("</details>");
    }

    private static void AppendRepositorySection(StringBuilder builder, SpecTraceAttestationSnapshot snapshot)
    {
        AppendKeyValueTable(builder, "Repository", new[]
        {
            ("Repository", snapshot.Repository.DisplayName),
            ("Root", Encode(snapshot.Repository.Root)),
            ("Profile", snapshot.Selection.Profile),
            ("Input path", snapshot.Selection.InputPath ?? "entire repository"),
            ("Emit", snapshot.Selection.Emit),
            ("Output directory", snapshot.Selection.OutputDirectory),
            ("Generated", snapshot.GeneratedAt),
        });
    }

    private static void AppendValidationSection(StringBuilder builder, SpecTraceAttestationSnapshot snapshot)
    {
        AppendKeyValueTable(builder, "Validation", new[]
        {
            ("Errors", FormatInt(snapshot.Validation.ErrorCount)),
            ("Warnings", FormatInt(snapshot.Validation.WarningCount)),
            ("Findings", FormatInt(snapshot.Validation.Findings.Count)),
        });
    }

    private static void AppendAggregateSection(StringBuilder builder, SpecTraceAttestationSnapshot snapshot)
    {
        var aggregates = snapshot.Aggregates;
        AppendKeyValueTable(builder, "Coverage", new[]
        {
            ("Specifications", FormatInt(aggregates.SpecificationCount)),
            ("Requirements", FormatInt(aggregates.RequirementCount)),
            ("With downstream trace", FormatInt(aggregates.RequirementsWithDownstreamTrace)),
            ("With implementation links", FormatInt(aggregates.RequirementsWithImplementation)),
            ("With verification links", FormatInt(aggregates.RequirementsWithVerification)),
            ("With evidence", FormatInt(aggregates.RequirementsWithEvidence)),
            ("With validation errors", FormatInt(aggregates.RequirementsWithValidationErrors)),
            ("With validation warnings", FormatInt(aggregates.RequirementsWithValidationWarnings)),
            ("With any issue", FormatInt(aggregates.RequirementsWithAnyIssue)),
            ("Derived statuses", RenderDictionary(aggregates.DerivedStatusCounts)),
            ("Evidence kinds", RenderDictionary(aggregates.EvidenceKindCoverage)),
        });
    }

    private static void AppendEvidenceSection(StringBuilder builder, string reportPath, SpecTraceAttestationSnapshot snapshot)
    {
        AppendKeyValueTable(builder, "Evidence", new[]
        {
            ("Files", FormatInt(snapshot.Evidence.FileCount)),
            ("Snapshots", FormatInt(snapshot.Evidence.SnapshotCount)),
            ("Requirement entries", FormatInt(snapshot.Evidence.RequirementCount)),
            ("Observations", FormatInt(snapshot.Evidence.ObservationCount)),
        });

        if (snapshot.Evidence.Files.Count == 0)
        {
            builder.AppendLine("<p class=\"muted\">No evidence snapshots were discovered.</p>");
            return;
        }

        builder.AppendLine("<section>");
        builder.AppendLine("<h2>Evidence Files</h2>");
        AppendGridTable(builder, new[]
        {
            "Path",
            "Snapshot ID",
            "Generated",
            "Producer",
            "Requirements",
            "Observations",
        }, snapshot.Evidence.Files.Select(file => new[]
        {
            LinkToRepoPath(reportPath, snapshot.Repository.Root, file.Path),
            Encode(file.SnapshotId),
            Encode(file.GeneratedAt),
            Encode(file.Producer),
            FormatInt(file.RequirementCount),
            FormatInt(file.ObservationCount),
        }));
        builder.AppendLine("</section>");
    }

    private static void AppendSpecificationIndexSection(
        StringBuilder builder,
        string reportPath,
        SpecTraceAttestationSnapshot snapshot,
        IReadOnlyList<SpecificationSummary> specifications,
        string title)
    {
        builder.AppendLine("<section>");
        builder.AppendLine($"<h2>{Encode(title)}</h2>");

        if (specifications.Count == 0)
        {
            builder.AppendLine("<p class=\"muted\">none</p>");
            builder.AppendLine("</section>");
            return;
        }

        AppendGridTable(builder, new[]
        {
            "Specification",
            "Status",
            "Requirements",
            "Issue-bearing",
            "Validation findings",
            "Page",
        }, specifications.Select(specification => new[]
        {
            $"{Encode(specification.SpecificationId)}<br /><span class=\"muted\">{LinkToRepoPath(reportPath, snapshot.Repository.Root, specification.SourcePath)}</span>",
            Encode(specification.Status),
            FormatInt(specification.Requirements),
            FormatInt(specification.IssueBearingRequirements),
            FormatInt(specification.SpecificationFindings + specification.RequirementFindings),
            specification.IssueBearingRequirements > 0 || specification.SpecificationFindings > 0
                ? $"<a href=\"{Encode(specification.PageHref)}\">Open</a>"
                : "<span class=\"muted\">clean</span>",
        }));
        builder.AppendLine("</section>");
    }

    private static List<SpecificationSummary> BuildSpecificationSummaries(string reportPath, SpecTraceAttestationSnapshot snapshot)
    {
        var reportRoot = Path.GetDirectoryName(reportPath)!;
        return snapshot.Specifications
            .Select(specification =>
            {
                var pagePath = ResolveSpecificationPagePath(reportRoot, specification.SourcePath, specification.ArtifactId);
                return new SpecificationSummary(
                    specification.ArtifactId,
                    specification.SourcePath,
                    specification.Status,
                    specification.Requirements.Count,
                    specification.Requirements.Count(requirement => requirement.Issues.Count > 0),
                    specification.Findings.Count,
                    specification.Requirements.Sum(requirement => requirement.Findings.Count),
                    RelativePath(reportPath, pagePath));
            })
            .OrderByDescending(summary => summary.IssueBearingRequirements)
            .ThenBy(summary => summary.SpecificationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ResolveSpecificationPagePath(string reportRoot, string sourcePath, string artifactId)
    {
        var normalized = string.IsNullOrWhiteSpace(sourcePath)
            ? artifactId
            : sourcePath.Replace('\\', '/');

        if (normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^5];
        }

        return Path.GetFullPath(Path.Combine(reportRoot, normalized.Replace('/', Path.DirectorySeparatorChar), "index.html"));
    }

    private static void AppendFindingsSection(StringBuilder builder, string title, IReadOnlyList<Finding> findings)
    {
        builder.AppendLine($"<h3>{Encode(title)}</h3>");
        builder.AppendLine("<ul class=\"validation-findings\">");
        foreach (var finding in findings)
        {
            var cssClass = string.Equals(finding.Severity, "error", StringComparison.OrdinalIgnoreCase) ? "error" : "warning";
            builder.AppendLine($"<li class=\"{cssClass}\"><strong>{Encode(finding.Code)}</strong>: {Encode(finding.Message)}</li>");
        }

        builder.AppendLine("</ul>");
    }

    private static void AppendKeyValueTable(StringBuilder builder, string? title, IEnumerable<(string Label, string Value)> rows)
    {
        builder.AppendLine("<section>");
        if (!string.IsNullOrWhiteSpace(title))
        {
            builder.AppendLine($"<h2>{Encode(title)}</h2>");
        }

        builder.AppendLine("<table><tbody>");
        foreach (var (label, value) in rows)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"<th>{Encode(label)}</th>");
            builder.AppendLine($"<td>{value}</td>");
            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</section>");
    }

    private static void AppendGridTable(StringBuilder builder, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        builder.AppendLine("<table>");
        builder.AppendLine("<thead><tr>");
        foreach (var header in headers)
        {
            builder.AppendLine($"<th>{Encode(header)}</th>");
        }

        builder.AppendLine("</tr></thead>");
        builder.AppendLine("<tbody>");
        foreach (var row in rows)
        {
            builder.AppendLine("<tr>");
            for (var index = 0; index < headers.Count; index++)
            {
                builder.AppendLine($"<td>{row.ElementAtOrDefault(index) ?? string.Empty}</td>");
            }

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table>");
    }

    private static void AppendHeader(StringBuilder builder, string heading, IReadOnlyList<(string Text, string Href)> links)
    {
        builder.AppendLine("<header>");
        builder.AppendLine($"<h1>{Encode(heading)}</h1>");
        builder.AppendLine("<p class=\"muted\">Derived attestation snapshot. This report is read-only and does not mutate canonical artifacts.</p>");
        builder.AppendLine("<nav class=\"nav-links\">");
        foreach (var (text, href) in links)
        {
            builder.AppendLine($"<a href=\"{Encode(href)}\">{Encode(text)}</a>");
        }

        builder.AppendLine("</nav>");
        builder.AppendLine("</header>");
    }

    private static void AppendDocumentStart(StringBuilder builder, string title)
    {
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"utf-8\" />");
        builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        builder.AppendLine($"<title>{Encode(title)}</title>");
        builder.AppendLine("<style>");
        builder.AppendLine("body{font-family:system-ui,-apple-system,Segoe UI,Roboto,sans-serif;line-height:1.45;margin:0 auto;max-width:1200px;padding:1.5rem;color:#111;background:#fff;}");
        builder.AppendLine("header,section,details{margin:0 0 1.25rem 0;}");
        builder.AppendLine("table{border-collapse:collapse;width:100%;margin:0.5rem 0 1rem 0;}");
        builder.AppendLine("th,td{border:1px solid #c8c8c8;padding:0.35rem 0.5rem;vertical-align:top;text-align:left;}");
        builder.AppendLine("th{background:#f5f5f5;}");
        builder.AppendLine("details{border:1px solid #ddd;padding:0.5rem 0.75rem;background:#fafafa;}");
        builder.AppendLine("summary{font-weight:600;cursor:pointer;}");
        builder.AppendLine("code,pre{font-family:ui-monospace,SFMono-Regular,Consolas,monospace;}");
        builder.AppendLine(".muted{color:#666;}");
        builder.AppendLine(".error{color:#8b0000;}");
        builder.AppendLine(".warning{color:#8a5a00;}");
        builder.AppendLine(".nav-links{display:flex;flex-wrap:wrap;gap:0.75rem;align-items:center;}");
        builder.AppendLine(".nav-links a{font-weight:600;}");
        builder.AppendLine(".requirement-card{margin:0 0 0.8rem 0;}");
        builder.AppendLine(".requirement-id{font-weight:700;}");
        builder.AppendLine(".requirement-title{font-weight:500;}");
        builder.AppendLine(".requirement-meta{color:#555;margin-left:0.5rem;}");
        builder.AppendLine(".requirement-tags{display:inline-flex;flex-wrap:wrap;gap:0.25rem;margin-left:0.75rem;}");
        builder.AppendLine(".requirement-body{margin-top:0.75rem;}");
        builder.AppendLine(".tag{display:inline-block;padding:0.1rem 0.45rem;border-radius:999px;border:1px solid transparent;font-size:0.82rem;line-height:1.2;white-space:nowrap;}");
        builder.AppendLine(".tag-error{background:#f8d7da;color:#7a0c12;border-color:#f1b3bb;}");
        builder.AppendLine(".tag-warning{background:#fff3cd;color:#6f5200;border-color:#ead28c;}");
        builder.AppendLine(".validation-findings{margin:0.25rem 0 0.75rem 1.25rem;padding:0;}");
        builder.AppendLine(".validation-findings li{margin:0.15rem 0;}");
        builder.AppendLine("</style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
    }

    private static void AppendFooter(StringBuilder builder)
    {
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
    }

    private static string RenderReferenceList(string reportPath, ArtifactCatalog catalog, IReadOnlyList<string> ids)
    {
        if (ids.Count == 0)
        {
            return "none";
        }

        return string.Join(", ", ids.Select(id => RenderCatalogReference(reportPath, catalog, id)));
    }

    private static string RenderCatalogReference(string reportPath, ArtifactCatalog catalog, string id)
    {
        if (catalog.TryGetArtifact(id, out var artifact))
        {
            return LinkToAbsolutePath(reportPath, artifact.SourcePath, id);
        }

        if (catalog.TryGetRequirement(id, out var requirement))
        {
            return LinkToAbsolutePath(reportPath, requirement.SourcePath, id);
        }

        return Encode(id);
    }

    private static string RenderEvidenceKind(SpecTraceAttestationEvidenceKind evidence)
    {
        var refs = evidence.Refs.Count == 0 ? string.Empty : $" [{Encode(string.Join(", ", evidence.Refs))}]";
        return $"<strong>{Encode(evidence.Kind)}</strong>: {Encode(evidence.Status)}{refs}";
    }

    private static string RenderList(IReadOnlyList<string> values)
    {
        return values.Count == 0
            ? "none"
            : Encode(string.Join(", ", values));
    }

    private static string RenderDictionary(IReadOnlyDictionary<string, int> values)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        return Encode(string.Join(", ", values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase).Select(pair => $"{pair.Key}: {pair.Value}")));
    }

    private static string RenderTags(IEnumerable<string> tags)
    {
        return string.Join(" ", tags.Select(tag =>
        {
            var cssClass = tag.Contains("warning", StringComparison.OrdinalIgnoreCase)
                ? "tag tag-warning"
                : "tag tag-error";
            return $"<span class=\"{cssClass}\">{Encode(tag)}</span>";
        }));
    }

    private static string LinkToRepoPath(string reportPath, string repoRoot, string? repoRelativePath, string? displayText = null)
    {
        if (string.IsNullOrWhiteSpace(repoRelativePath))
        {
            return "<span class=\"muted\">unavailable</span>";
        }

        var absolutePath = Path.Combine(repoRoot, repoRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return LinkToAbsolutePath(reportPath, absolutePath, displayText ?? repoRelativePath);
    }

    private static string LinkToAbsolutePath(string reportPath, string absoluteTarget, string? displayText = null)
    {
        var href = RelativePath(reportPath, absoluteTarget);
        return $"<a href=\"{Encode(href)}\">{Encode(displayText ?? Path.GetFileName(absoluteTarget))}</a>";
    }

    private static string RelativePath(string fromPath, string toPath)
    {
        var fromDirectory = Path.GetDirectoryName(fromPath) ?? Directory.GetCurrentDirectory();
        return Path.GetRelativePath(fromDirectory, toPath).Replace('\\', '/');
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string FormatInt(int value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private sealed record SpecificationSummary(
        string SpecificationId,
        string SourcePath,
        string Status,
        int Requirements,
        int IssueBearingRequirements,
        int SpecificationFindings,
        int RequirementFindings,
        string PageHref);
}
