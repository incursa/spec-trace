using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class CoverageAuditRendererTests
{
    [Fact]
    public async Task RenderAsyncGroupsSourceUnitsBySection()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "spec-trace-rfc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var outputPath = Path.Combine(tempDirectory, "coverage-audit.md");
            var ledger = new[]
            {
                SourceUnit("RFC8999-S1-B1-P1-S1", "1", "Introduction", "QUIC endpoints MUST use QUIC packets."),
                SourceUnit("RFC8999-S5P1-B3-P3-S1", "5.1", "Long Header", "The next four bytes include a 32-bit Version field."),
            };
            var candidates = new Dictionary<string, CandidateDecision>(StringComparer.Ordinal)
            {
                ["RFC8999-S1-B1-P1-S1"] = new CandidateDecision
                {
                    SourceUnitId = "RFC8999-S1-B1-P1-S1",
                    Decision = "emit",
                    Requirements =
                    [
                        new CandidateRequirement
                        {
                            Title = "Use QUIC packets to establish connections",
                            Statement = "QUIC endpoints MUST use QUIC packets to establish a QUIC connection.",
                            Coverage = new RequirementCoverage
                            {
                                Positive = "required",
                                Negative = "required",
                                Edge = "optional",
                                Fuzz = "deferred",
                            },
                        },
                    ],
                },
            };

            await CoverageAuditRenderer.RenderAsync(outputPath, ledger, candidates);

            var markdown = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("## Section 1 - Introduction", markdown, StringComparison.Ordinal);
            Assert.Contains("## Section 5.1 - Long Header", markdown, StringComparison.Ordinal);
            Assert.Contains("Candidate decision: `emit`", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static SourceUnit SourceUnit(string sourceUnitId, string section, string sectionTitle, string text)
    {
        return new SourceUnit
        {
            SourceUnitId = sourceUnitId,
            SourceId = "RFC8999",
            Section = section,
            SectionTitle = sectionTitle,
            BlockIndex = 1,
            ParagraphIndex = 1,
            SentenceIndex = 1,
            BlockKind = "paragraph",
            Text = text,
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc8999.html",
            TextHash = "sha256:1111111111111111111111111111111111111111111111111111111111111111",
        };
    }
}
