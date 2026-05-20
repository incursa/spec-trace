using SpecTrace.Rfc.Ai;
using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class CodexCliRequirementExtractorTests
{
    [Fact]
    public async Task ExtractAsyncWithAiDisabledPreservesOneDecisionPerSourceUnit()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "spec-trace-rfc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var ledgerPath = Path.Combine(tempDirectory, "ledger.jsonl");
            var promptPath = Path.Combine(tempDirectory, "prompt.md");
            var candidatesPath = Path.Combine(tempDirectory, "candidates.jsonl");
            await File.WriteAllTextAsync(promptPath, "Extract requirements.");
            await Jsonl.WriteAsync(
                ledgerPath,
                new[]
                {
                    SourceUnit(
                        "RFC8999-S5P1-B5-P5-S1",
                        blockKind: "figure",
                        text: """
Long Header Packet {
  Header Form (1) = 1,
}
"""),
                    SourceUnit(
                        "RFC8999-S5P1-B6-P6-S1",
                        text: "The first bit of a long header packet MUST be set to 1."),
                    SourceUnit(
                        "RFC8999-S5P1-B7-P7-S1",
                        text: "This sentence is explanatory."),
                });

            var count = await new CodexCliRequirementExtractor().ExtractAsync(new CodexExtractionOptions
            {
                LedgerPath = ledgerPath,
                OutputPath = candidatesPath,
                PromptPath = promptPath,
                SchemaPath = Path.Combine(tempDirectory, "schema.json"),
                AiMode = "off",
            });

            var decisions = await Jsonl.ReadAsync<CandidateDecision>(candidatesPath);

            Assert.Equal(3, count);
            Assert.Collection(
                decisions,
                figure =>
                {
                    Assert.Equal("emit", figure.Decision);
                    Assert.Contains("deterministic_figure_extraction", figure.ReviewFlags);
                    Assert.Single(figure.Requirements);
                },
                normative =>
                {
                    Assert.Equal("needs_human_review", normative.Decision);
                    Assert.Contains("ai_disabled_candidate_unit", normative.ReviewFlags);
                },
                explanatory =>
                {
                    Assert.Equal("skip_non_normative", explanatory.Decision);
                    Assert.Contains("deterministic_scope_skip", explanatory.ReviewFlags);
                });
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void OrderAndValidateBatchRepairsSourceUnitIdWhenResultOrderIsCorrect()
    {
        var batch = new[]
        {
            SourceUnit("RFC9002-S5P3-B11-P11-S3"),
        };
        var results = new[]
        {
            new CandidateDecision
            {
                SourceUnitId = "RFC9002-S5P3-B11-S3",
                Decision = "emit",
                Requirements =
                [
                    new CandidateRequirement
                    {
                        ProposedIdHint = "REQ-QUIC-RFC9002-S5P3-0006",
                        Title = "Subtract local delays until handshake confirmation",
                        Statement = "An endpoint SHOULD subtract such local delays from its RTT sample until the handshake is confirmed.",
                        Coverage = new RequirementCoverage
                        {
                            Positive = "required",
                            Negative = "required",
                            Edge = "optional",
                            Fuzz = "deferred",
                        },
                        UpstreamRefs =
                        [
                            "RFC 9002 §5.3 RFC9002-S5P3-B11-S3",
                            "https://www.rfc-editor.org/rfc/rfc9002.html#section-5.3",
                        ],
                    },
                ],
            },
        };

        var ordered = CodexCliRequirementExtractor.OrderAndValidateBatch(batch, results, batchNumber: 2);

        var decision = Assert.Single(ordered);
        Assert.Equal("RFC9002-S5P3-B11-P11-S3", decision.SourceUnitId);
        Assert.Contains("source_unit_id_repaired_from:RFC9002-S5P3-B11-S3", decision.ReviewFlags);
        var requirement = Assert.Single(decision.Requirements);
        Assert.Contains("RFC9002-S5P3-B11-P11-S3", requirement.UpstreamRefs[0], StringComparison.Ordinal);
    }

    [Fact]
    public void OrderAndValidateBatchRejectsDroppedResults()
    {
        var batch = new[]
        {
            SourceUnit("RFC9002-S5P3-B10-P10-S1"),
            SourceUnit("RFC9002-S5P3-B11-P11-S3"),
        };
        var results = new[]
        {
            new CandidateDecision
            {
                SourceUnitId = "RFC9002-S5P3-B10-P10-S1",
                Decision = "skip_non_normative",
                Requirements = [],
            },
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CodexCliRequirementExtractor.OrderAndValidateBatch(batch, results, batchNumber: 2));

        Assert.Contains("RFC9002-S5P3-B11-P11-S3", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OrderAndValidateBatchRejectsShiftedUpstreamRefs()
    {
        var batch = new[]
        {
            SourceUnit("RFC8999-S4-B6-P6-S1"),
        };
        var results = new[]
        {
            new CandidateDecision
            {
                SourceUnitId = "RFC8999-S4-B6-P6-S1",
                Decision = "emit",
                Requirements =
                [
                    new CandidateRequirement
                    {
                        Title = "Ignore unused bits on receipt",
                        Statement = "When receiving a Version Negotiation packet, an endpoint MUST ignore the remaining 7 bits labeled \"Unused\".",
                        Coverage = new RequirementCoverage
                        {
                            Positive = "required",
                            Negative = "required",
                            Edge = "optional",
                            Fuzz = "deferred",
                        },
                        UpstreamRefs =
                        [
                            "RFC 8999 Appendix A RFC8999-SA-B13-P13-S2",
                            "https://www.rfc-editor.org/rfc/rfc8999.html#appendix-A",
                        ],
                    },
                ],
            },
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CodexCliRequirementExtractor.OrderAndValidateBatch(batch, results, batchNumber: 1));

        Assert.Contains("upstream_refs", exception.Message, StringComparison.Ordinal);
        Assert.Contains("RFC8999-S4-B6-P6-S1", exception.Message, StringComparison.Ordinal);
    }

    private static SourceUnit SourceUnit(
        string sourceUnitId,
        string blockKind = "paragraph",
        string text = "An endpoint SHOULD subtract such local delays from its RTT sample until the handshake is confirmed.")
    {
        return new SourceUnit
        {
            SourceUnitId = sourceUnitId,
            SourceId = "RFC9002",
            Section = "5.3",
            SectionTitle = "Estimating smoothed_rtt and rttvar",
            BlockIndex = 1,
            ParagraphIndex = 1,
            SentenceIndex = 1,
            BlockKind = blockKind,
            Text = text,
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9002.html#section-5.3",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };
    }
}
