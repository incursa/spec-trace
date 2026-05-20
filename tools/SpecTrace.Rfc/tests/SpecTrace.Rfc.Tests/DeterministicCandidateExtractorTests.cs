using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class DeterministicCandidateExtractorTests
{
    [Fact]
    public void ExtractsPacketStructureFigureIntoCandidateRequirements()
    {
        var sourceUnit = Figure("""
Long Header Packet {
  Header Form (1) = 1,
  Version-Specific Bits (7),
  Version (32),
  Destination Connection ID Length (8),
  Destination Connection ID (0..2040),
  Source Connection ID Length (8),
  Source Connection ID (0..2040),
  Version-Specific Data (..),
}
""");

        var decision = DeterministicCandidateExtractor.TryExtract(sourceUnit);

        Assert.NotNull(decision);
        Assert.Equal("emit", decision.Decision);
        Assert.Contains("deterministic_figure_extraction", decision.ReviewFlags);
        Assert.Equal(8, decision.Requirements.Count);
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The first bit of a QUIC long header packet MUST be set to 1.");
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The other seven bits in the first byte of a QUIC long header packet MUST be version-specific.");
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The Destination Connection ID field MUST follow its length byte and be between 0 and 255 bytes long.");
        Assert.All(decision.Requirements, requirement =>
            Assert.Equal(1, CandidateRules.CountNormativeKeywords(requirement.Statement)));
    }

    [Fact]
    public void SkipsExampleFigures()
    {
        var sourceUnit = Figure("""
Example Structure {
  Field (8),
}
""");

        Assert.Null(DeterministicCandidateExtractor.TryExtract(sourceUnit));
    }

    [Fact]
    public void KeepsUnparseableRangesForAiReview()
    {
        var sourceUnit = Figure("""
Transport Parameter {
  Unknown Length (a..b),
}
""");

        var decision = DeterministicCandidateExtractor.TryExtract(sourceUnit);

        Assert.NotNull(decision);
        Assert.Equal("The Unknown Length field in a Transport Parameter MUST be present.", Assert.Single(decision.Requirements).Statement);
    }

    [Theory]
    [InlineData("paragraph", "This is explanatory.", false)]
    [InlineData("paragraph", "An endpoint MUST validate the field.", true)]
    [InlineData("table", "Field | Value", true)]
    [InlineData("figure", "Packet { Field (8) }", true)]
    public void CandidateScopeOnlySendsLikelyRequirementUnitsToAi(string blockKind, string text, bool expected)
    {
        var sourceUnit = new SourceUnit
        {
            SourceUnitId = "RFC9000-S1-B1-P1-S1",
            SourceId = "RFC9000",
            Section = "1",
            SectionTitle = "Introduction",
            BlockIndex = 1,
            ParagraphIndex = 1,
            SentenceIndex = 1,
            BlockKind = blockKind,
            Text = text,
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9000.html#section-1",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };

        Assert.Equal(expected, DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit));
    }

    [Fact]
    public void CandidateScopeSkipsNonNormativeNotationFigures()
    {
        var sourceUnit = new SourceUnit
        {
            SourceUnitId = "RFC8999-S4-B6-P6-S1",
            SourceId = "RFC8999",
            Section = "4",
            SectionTitle = "Notational Conventions",
            BlockIndex = 6,
            ParagraphIndex = 6,
            SentenceIndex = 1,
            BlockKind = "figure",
            Text = "x (L) = C: Indicates that x has a fixed value of C.",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc8999.html#section-4",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };

        Assert.False(DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit));
    }

    private static SourceUnit Figure(string text)
    {
        return new SourceUnit
        {
            SourceUnitId = "RFC8999-S5P1-B5-P5-S1",
            SourceId = "RFC8999",
            Section = "5.1",
            SectionTitle = "Long Header",
            BlockIndex = 5,
            ParagraphIndex = 5,
            SentenceIndex = 1,
            BlockKind = "figure",
            Text = text,
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc8999.html#section-5.1",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };
    }
}
