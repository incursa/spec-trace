using SpecTrace.Rfc.Ai;
using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class CodexCliRequirementNormalizerTests
{
    [Fact]
    public async Task NormalizeAsyncDefaultsToDeterministicReviewDecisionsWhenAiIsOff()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "spec-trace-rfc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var ledgerPath = Path.Combine(tempDirectory, "ledger.jsonl");
            var reviewPath = Path.Combine(tempDirectory, "review.jsonl");
            var outputPath = Path.Combine(tempDirectory, "normalized.jsonl");
            var promptPath = Path.Combine(tempDirectory, "prompt.md");
            await File.WriteAllTextAsync(promptPath, "Normalize review decisions.");

            await Jsonl.WriteAsync(
                ledgerPath,
                new[]
                {
                    SourceUnit("RFC9002-S6-B1-P1-S1", "6", "Loss Detection", "An endpoint MUST arm a PTO timer."),
                    SourceUnit("RFC9002-S6-B1-P1-S2", "6", "Loss Detection", "That timer SHOULD be reset after an acknowledgment."),
                });

            await Jsonl.WriteAsync(
                reviewPath,
                new[]
                {
                    new ReviewDecision
                    {
                        SourceUnitId = "RFC9002-S6-B1-P1-S1",
                        SourceUnitIds = ["RFC9002-S6-B1-P1-S1", "RFC9002-S6-B1-P1-S2"],
                        Action = "merge",
                        Requirements =
                        [
                            new CandidateRequirement
                            {
                                ProposedIdHint = "REQ-QUIC-RFC9002-S6-0001",
                                Title = "Arm PTO timer",
                                Statement = "An endpoint MUST arm a PTO timer.",
                                Coverage = new RequirementCoverage
                                {
                                    Positive = "required",
                                    Negative = "required",
                                    Edge = "optional",
                                    Fuzz = "deferred",
                                },
                                UpstreamRefs =
                                [
                                    "RFC 9002 §6 RFC9002-S6-B1-P1-S1",
                                ],
                            },
                        ],
                        Notes = ["merged_overlapping_invariants"],
                    },
                    new ReviewDecision
                    {
                        SourceUnitId = "RFC9002-S6-B1-P1-S2",
                        SourceUnitIds = ["RFC9002-S6-B1-P1-S2"],
                        Action = "skip",
                        Notes = ["redundant_sibling"],
                    },
                });

            var count = await new CodexCliRequirementNormalizer().NormalizeAsync(new CodexNormalizeOptions
            {
                LedgerPath = ledgerPath,
                ReviewPath = reviewPath,
                OutputPath = outputPath,
                PromptPath = promptPath,
                SchemaPath = Path.Combine(tempDirectory, "schema.json"),
                AiMode = "off",
            });

            var decisions = await Jsonl.ReadAsync<ReviewDecision>(outputPath);

            Assert.Equal(2, count);
            Assert.Collection(
                decisions,
                merged =>
                {
                    Assert.Equal("merge", merged.Action);
                    Assert.Equal(["RFC9002-S6-B1-P1-S1", "RFC9002-S6-B1-P1-S2"], merged.SourceUnitIds);
                    Assert.Single(merged.Requirements);
                },
                skipped =>
                {
                    Assert.Equal("skip", skipped.Action);
                    Assert.Equal(["RFC9002-S6-B1-P1-S2"], skipped.SourceUnitIds);
                    Assert.Contains("redundant_sibling", skipped.Notes);
                });
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
            SourceId = "RFC9002",
            Section = section,
            SectionTitle = sectionTitle,
            BlockIndex = 1,
            ParagraphIndex = 1,
            SentenceIndex = 1,
            BlockKind = "paragraph",
            Text = text,
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9002.html",
            TextHash = "sha256:3333333333333333333333333333333333333333333333333333333333333333",
        };
    }
}
