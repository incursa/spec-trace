using SpecTrace.Rfc.Ai;
using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class CodexCliCoverageAuditorTests
{
    [Fact]
    public async Task AuditAsyncDefaultsToDeterministicReviewDecisionsWhenAiIsOff()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "spec-trace-rfc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var ledgerPath = Path.Combine(tempDirectory, "ledger.jsonl");
            var candidatesPath = Path.Combine(tempDirectory, "candidates.jsonl");
            var reviewPath = Path.Combine(tempDirectory, "review.jsonl");
            var reportPath = Path.Combine(tempDirectory, "coverage-audit.md");
            var promptPath = Path.Combine(tempDirectory, "prompt.md");
            await File.WriteAllTextAsync(promptPath, "Audit coverage.");

            await Jsonl.WriteAsync(
                ledgerPath,
                new[]
                {
                    SourceUnit("RFC8999-S1-B1-P1-S1", "1", "Introduction", "QUIC endpoints MUST use QUIC packets."),
                    SourceUnit("RFC8999-S1-B1-P1-S2", "1", "Introduction", "That sentence is explanatory."),
                    SourceUnit("RFC8999-S5P1-B3-P3-S1", "5.1", "Long Header", "The next four bytes include a 32-bit Version field."),
                });

            await Jsonl.WriteAsync(
                candidatesPath,
                new[]
                {
                    new CandidateDecision
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
                    new CandidateDecision
                    {
                        SourceUnitId = "RFC8999-S1-B1-P1-S2",
                        Decision = "skip_non_normative",
                        ReviewFlags = ["deterministic_scope_skip"],
                    },
                    new CandidateDecision
                    {
                        SourceUnitId = "RFC8999-S5P1-B3-P3-S1",
                        Decision = "gap",
                        ReviewFlags = ["needs_context"],
                    },
                });

            var count = await new CodexCliCoverageAuditor().AuditAsync(new CodexAuditOptions
            {
                LedgerPath = ledgerPath,
                CandidatePath = candidatesPath,
                OutputPath = reviewPath,
                ReportPath = reportPath,
                PromptPath = promptPath,
                SchemaPath = Path.Combine(tempDirectory, "schema.json"),
                AiMode = "off",
            });

            var decisions = await Jsonl.ReadAsync<ReviewDecision>(reviewPath);

            Assert.Equal(3, count);
            Assert.Collection(
                decisions,
                accept =>
                {
                    Assert.Equal("accept", accept.Action);
                    Assert.Equal(["RFC8999-S1-B1-P1-S1"], accept.SourceUnitIds);
                    Assert.Single(accept.Requirements);
                },
                skip =>
                {
                    Assert.Equal("skip", skip.Action);
                    Assert.Equal(["RFC8999-S1-B1-P1-S2"], skip.SourceUnitIds);
                    Assert.Contains("deterministic_scope_skip", skip.Notes);
                },
                gap =>
                {
                    Assert.Equal("gap", gap.Action);
                    Assert.Equal(["RFC8999-S5P1-B3-P3-S1"], gap.SourceUnitIds);
                    Assert.Contains("needs_context", gap.Notes);
                });

            var report = await File.ReadAllTextAsync(reportPath);
            Assert.Contains("## Section 1 - Introduction", report, StringComparison.Ordinal);
            Assert.Contains("## Section 5.1 - Long Header", report, StringComparison.Ordinal);
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
            TextHash = "sha256:2222222222222222222222222222222222222222222222222222222222222222",
        };
    }
}
