using SpecTrace.Rfc.Ai;
using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class CodexCliRequirementExtractorTests
{
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

    private static SourceUnit SourceUnit(string sourceUnitId)
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
            BlockKind = "paragraph",
            Text = "An endpoint SHOULD subtract such local delays from its RTT sample until the handshake is confirmed.",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9002.html#section-5.3",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };
    }
}
