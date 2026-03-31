using System.Text;
using System.Text.Json;

namespace SpecTrace.Tool;

internal static class CueArtifactWriter
{
    public static string WriteArtifact(ArtifactModel artifact)
    {
        var writer = new CueTextWriter();
        writer.WriteLine("package artifacts");
        writer.WriteLine();
        writer.WriteLine(@"import model ""github.com/incursa/spec-trace/model@v0""");
        writer.WriteLine();
        writer.WriteLine($"artifact: {GetDefinitionName(artifact.ArtifactType)} & {{");
        writer.Indent();

        writer.WriteField("artifact_id", artifact.ArtifactId);
        writer.WriteField("artifact_type", artifact.ArtifactType);
        writer.WriteField("title", artifact.Title);
        writer.WriteField("domain", artifact.Domain);
        writer.WriteField("capability", artifact.Capability);
        writer.WriteField("status", artifact.Status);
        writer.WriteField("owner", artifact.Owner);
        writer.WriteStringList("tags", artifact.Tags);
        writer.WriteStringList("related_artifacts", artifact.RelatedArtifacts);
        writer.WriteBlockString("purpose", artifact.Purpose);
        writer.WriteBlockString("scope", artifact.Scope);
        writer.WriteBlockString("context", artifact.Context);
        writer.WriteBlockString("summary", artifact.Summary);
        writer.WriteStringList("open_questions", artifact.OpenQuestions);
        writer.WriteSupplementalSections("supplemental_sections", artifact.SupplementalSections);

        switch (artifact.ArtifactType)
        {
            case "specification":
                writer.WriteRequirements("requirements", artifact.Requirements);
                break;
            case "architecture":
                writer.WriteStringList("satisfies", artifact.Satisfies);
                writer.WriteBlockString("design_summary", artifact.DesignSummary);
                writer.WriteStringList("key_components", artifact.KeyComponents);
                writer.WriteBlockString("data_and_state_considerations", artifact.DataAndStateConsiderations);
                writer.WriteStringList("edge_cases_and_constraints", artifact.EdgeCasesAndConstraints);
                writer.WriteStringList("alternatives_considered", artifact.AlternativesConsidered);
                writer.WriteStringList("risks", artifact.Risks);
                break;
            case "work_item":
                writer.WriteStringList("addresses", artifact.Addresses);
                writer.WriteStringList("design_links", artifact.DesignLinks);
                writer.WriteStringList("verification_links", artifact.VerificationLinks);
                writer.WriteBlockString("planned_changes", artifact.PlannedChanges);
                writer.WriteStringList("out_of_scope", artifact.OutOfScope);
                writer.WriteBlockString("verification_plan", artifact.VerificationPlan);
                writer.WriteBlockString("completion_notes", artifact.CompletionNotes);
                break;
            case "verification":
                writer.WriteStringList("verifies", artifact.Verifies);
                writer.WriteBlockString("verification_method", artifact.VerificationMethod);
                writer.WriteStringList("preconditions", artifact.Preconditions);
                writer.WriteStringList("procedure", artifact.Procedure);
                writer.WriteBlockString("expected_result", artifact.ExpectedResult);
                writer.WriteStringList("evidence", artifact.Evidence);
                writer.WriteBlockString("status_summary", artifact.StatusSummary);
                break;
        }

        writer.Unindent();
        writer.WriteLine("}");
        return writer.ToString();
    }

    private static string GetDefinitionName(string artifactType)
    {
        return artifactType switch
        {
            "specification" => "model.#Specification",
            "architecture" => "model.#Architecture",
            "work_item" => "model.#WorkItem",
            "verification" => "model.#Verification",
            _ => throw new InvalidOperationException($"Unsupported artifact type '{artifactType}'."),
        };
    }

    private sealed class CueTextWriter
    {
        private readonly StringBuilder _builder = new();
        private int _indentLevel;

        public void Indent() => _indentLevel++;

        public void Unindent() => _indentLevel--;

        public void WriteLine(string value = "")
        {
            if (value.Length == 0)
            {
                _builder.AppendLine();
                return;
            }

            _builder.Append(new string(' ', _indentLevel * 4));
            _builder.AppendLine(value);
        }

        public void WriteField(string fieldName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            WriteLine($"{fieldName}: {ToCueString(value)}");
        }

        public void WriteBlockString(string fieldName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var normalized = value.Replace("\r\n", "\n");
            if (!normalized.Contains('\n') && normalized.Length < 120)
            {
                WriteField(fieldName, normalized);
                return;
            }

            WriteLine($"{fieldName}: \"\"\"");
            foreach (var line in normalized.Split('\n'))
            {
                WriteLine($"  {line}");
            }

            WriteLine("  \"\"\"");
        }

        public void WriteStringList(string fieldName, List<string>? values)
        {
            if (values is null || values.Count == 0)
            {
                return;
            }

            WriteLine($"{fieldName}: [");
            Indent();
            foreach (var value in values)
            {
                WriteLine($"{ToCueString(value)},");
            }

            Unindent();
            WriteLine("]");
        }

        public void WriteSupplementalSections(string fieldName, List<SupplementalSection>? sections)
        {
            if (sections is null || sections.Count == 0)
            {
                return;
            }

            WriteLine($"{fieldName}: [");
            Indent();
            foreach (var section in sections)
            {
                WriteLine("{");
                Indent();
                WriteField("heading", section.Heading);
                WriteBlockString("content", section.Content);
                Unindent();
                WriteLine("},");
            }

            Unindent();
            WriteLine("]");
        }

        public void WriteRequirements(string fieldName, List<RequirementModel>? requirements)
        {
            if (requirements is null || requirements.Count == 0)
            {
                return;
            }

            WriteLine($"{fieldName}: [");
            Indent();
            foreach (var requirement in requirements)
            {
                WriteLine("{");
                Indent();
                WriteField("id", requirement.Id);
                WriteField("title", requirement.Title);
                WriteBlockString("statement", requirement.Statement);
                WriteTrace("trace", requirement.Trace);
                WriteStringList("notes", requirement.Notes);
                Unindent();
                WriteLine("},");
            }

            Unindent();
            WriteLine("]");
        }

        public void WriteTrace(string fieldName, RequirementTraceModel? trace)
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

            WriteLine($"{fieldName}: {{");
            Indent();
            WriteStringList("satisfied_by", trace.SatisfiedBy);
            WriteStringList("implemented_by", trace.ImplementedBy);
            WriteStringList("verified_by", trace.VerifiedBy);
            WriteStringList("derived_from", trace.DerivedFrom);
            WriteStringList("supersedes", trace.Supersedes);
            WriteStringList("upstream_refs", trace.UpstreamRefs);
            WriteStringList("related", trace.Related);
            Unindent();
            WriteLine("}");
        }

        private static string ToCueString(string value) => JsonSerializer.Serialize(value, JsonOptions.Default);

        public override string ToString() => _builder.ToString();
    }
}
