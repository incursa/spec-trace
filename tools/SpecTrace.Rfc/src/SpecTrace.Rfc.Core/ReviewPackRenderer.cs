using System.Text;
using System.Text.Json;

namespace SpecTrace.Rfc.Core;

public static class ReviewPackRenderer
{
    public static async Task RenderAsync(
        string path,
        IReadOnlyList<SourceUnit> ledger,
        IReadOnlyDictionary<string, CandidateDecision> candidates,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var builder = new StringBuilder();

        builder.AppendLine("# RFC Requirement Candidate Review");
        builder.AppendLine();
        builder.AppendLine("Use this packet to decide which candidates become canonical SpecTrace requirements.");
        builder.AppendLine();
        builder.AppendLine("Review actions: `accept`, `accept_with_edit`, `split`, `merge`, `skip`, `gap`, `quarantine`.");
        builder.AppendLine();
        builder.AppendLine("Create a `review-decisions.jsonl` file with one JSON object per accepted or explicitly handled source unit. Accepted records must carry the final requirement payload.");
        builder.AppendLine();

        foreach (var unit in ledger)
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.AppendLine($"## {unit.SourceUnitId}");
            builder.AppendLine();
            builder.AppendLine($"- Section: `{unit.Section}` {unit.SectionTitle}");
            builder.AppendLine($"- Block kind: `{unit.BlockKind}`");
            builder.AppendLine($"- Hash: `{unit.TextHash}`");
            if (!string.IsNullOrWhiteSpace(unit.SourceUrl))
            {
                builder.AppendLine($"- Source URL: {unit.SourceUrl}");
            }

            builder.AppendLine();
            builder.AppendLine("> " + unit.Text.Replace("\n", "\n> ", StringComparison.Ordinal));
            builder.AppendLine();

            if (!candidates.TryGetValue(unit.SourceUnitId, out var candidate))
            {
                builder.AppendLine("No candidate decision was produced.");
                builder.AppendLine();
                continue;
            }

            builder.AppendLine($"Candidate decision: `{candidate.Decision}`");
            if (candidate.ReviewFlags.Count > 0)
            {
                builder.AppendLine($"Review flags: {string.Join(", ", candidate.ReviewFlags.Select(flag => $"`{flag}`"))}");
            }

            builder.AppendLine();
            builder.AppendLine("```json");
            builder.AppendLine(JsonSerializer.Serialize(candidate, RfcJson.Options));
            builder.AppendLine("```");
            builder.AppendLine();

            if (candidate.Requirements.Count > 0)
            {
                var reviewTemplate = new ReviewDecision
                {
                    SourceUnitId = unit.SourceUnitId,
                    SourceUnitIds = [unit.SourceUnitId],
                    Action = "accept",
                    Requirements = candidate.Requirements,
                };
                builder.AppendLine("Review JSONL template:");
                builder.AppendLine();
                builder.AppendLine("```json");
                builder.AppendLine(JsonSerializer.Serialize(reviewTemplate, RfcJson.JsonlOptions));
                builder.AppendLine("```");
                builder.AppendLine();
            }
        }

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
    }
}
