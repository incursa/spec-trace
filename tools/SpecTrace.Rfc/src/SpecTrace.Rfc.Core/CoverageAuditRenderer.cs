using System.Text;
using System.Text.Json;

namespace SpecTrace.Rfc.Core;

public static class CoverageAuditRenderer
{
    public static async Task RenderAsync(
        string path,
        IReadOnlyList<SourceUnit> ledger,
        IReadOnlyDictionary<string, CandidateDecision> candidates,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var builder = new StringBuilder();

        builder.AppendLine("# RFC Coverage Audit");
        builder.AppendLine();
        builder.AppendLine("Use this packet to close coverage gaps after extraction and before final assembly.");
        builder.AppendLine();
        builder.AppendLine("Audit actions: `accept`, `accept_with_edit`, `split`, `merge`, `skip`, `gap`, `quarantine`.");
        builder.AppendLine();
        builder.AppendLine("The extraction pass is advisory. Promote missing but testable invariants here, and do not drop source units.");
        builder.AppendLine();

        var sectionGroups = ledger
            .GroupBy(unit => new { unit.Section, unit.SectionTitle }, (key, units) => new { key.Section, key.SectionTitle, Units = units.ToList() })
            .ToList();

        builder.AppendLine($"- Source units: {ledger.Count}");
        builder.AppendLine($"- Candidate decisions: {candidates.Count}");
        builder.AppendLine($"- Sections: {sectionGroups.Count}");
        builder.AppendLine();

        foreach (var sectionGroup in sectionGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.AppendLine($"## Section {sectionGroup.Section} - {sectionGroup.SectionTitle}");
            builder.AppendLine();
            builder.AppendLine($"- Source units: {sectionGroup.Units.Count}");
            builder.AppendLine();

            foreach (var unit in sectionGroup.Units)
            {
                builder.AppendLine($"### {unit.SourceUnitId}");
                builder.AppendLine();
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
                    builder.AppendLine("Candidate decision: none");
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
            }
        }

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
    }
}
