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
        Assert.Equal(10, decision.Requirements.Count);
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The first bit of a QUIC long header packet MUST be set to 1.");
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The other seven bits in the first byte of a QUIC long header packet MUST be version-specific.");
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The Destination Connection ID field MUST follow its length byte.");
        Assert.Contains(decision.Requirements, requirement =>
            requirement.Statement == "The Destination Connection ID field MUST be between 0 and 255 bytes long.");
        Assert.All(decision.Requirements, requirement =>
            Assert.Equal(1, CandidateRules.CountNormativeKeywords(requirement.Statement)));
    }

    [Fact]
    public void ExtractedPacketStructureRequirementsKeepFieldPositionAndSizeSeparate()
    {
        var sourceUnit = Figure("""
Long Header Packet {
  Destination Connection ID Length (8),
  Destination Connection ID (0..2040),
}
""");

        var requirements = DeterministicCandidateExtractor.TryExtract(sourceUnit)!.Requirements;

        Assert.Contains(requirements, requirement =>
            requirement.Title == "Destination Connection ID Position" &&
            requirement.Statement == "The Destination Connection ID field MUST follow its length byte.");
        Assert.Contains(requirements, requirement =>
            requirement.Title == "Destination Connection ID Size" &&
            requirement.Statement == "The Destination Connection ID field MUST be between 0 and 255 bytes long.");
        Assert.DoesNotContain(requirements, requirement =>
            requirement.Statement.Contains("follow its length byte and be", StringComparison.Ordinal));
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
    [InlineData("paragraph", "This is explanatory.", true)]
    [InlineData("paragraph", "Short.", false)]
    [InlineData("paragraph", "An endpoint MUST validate the field.", true)]
    [InlineData("paragraph", "The default for this transport parameter is 0, which indicates that the endpoint does not support DATAGRAM frames.", true)]
    [InlineData("paragraph", "QUIC uses various frame types to transmit data within packets.", true)]
    [InlineData("table", "Field | Value", true)]
    [InlineData("figure", "Packet { Field (8) }", true)]
    public void FunctionalScopeSendsReviewableNonBoilerplateUnitsToAi(string blockKind, string text, bool expected)
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
    public void FunctionalScopeKeepsAbstractBehaviorForReview()
    {
        var sourceUnit = new SourceUnit
        {
            SourceUnitId = "RFC9000-S0-B4-P4-S2",
            SourceId = "RFC9000",
            Section = "0",
            SectionTitle = "",
            BlockIndex = 4,
            ParagraphIndex = 4,
            SentenceIndex = 2,
            BlockKind = "paragraph",
            Text = "QUIC provides applications with flow-controlled streams for structured communication.",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9000.html",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };

        Assert.True(DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit));
    }

    [Fact]
    public void FunctionalScopeSkipsLegalFrontMatter()
    {
        var sourceUnit = new SourceUnit
        {
            SourceUnitId = "RFC9000-S0-B11-P11-S1",
            SourceId = "RFC9000",
            Section = "0",
            SectionTitle = "Front Matter",
            BlockIndex = 11,
            ParagraphIndex = 11,
            SentenceIndex = 1,
            BlockKind = "paragraph",
            Text = "This document is subject to BCP 78 and the IETF Trust's Legal Provisions Relating to IETF Documents.",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9000.html",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };

        Assert.False(DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit));
    }

    [Fact]
    public void FunctionalScopeKeepsNotationForReview()
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

        Assert.True(DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit));
    }

    [Fact]
    public void FunctionalScopeSkipsPureCaptionsButNotFigureDescription()
    {
        var caption = new SourceUnit
        {
            SourceUnitId = "RFC9001-S4-B5-P5-S1",
            SourceId = "RFC9001",
            Section = "4",
            SectionTitle = "Carrying TLS Messages",
            BlockIndex = 5,
            ParagraphIndex = 5,
            SentenceIndex = 1,
            BlockKind = "paragraph",
            Text = "Table 1: Encryption Keys by Packet Type",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9001.html#section-4",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };
        var description = new SourceUnit
        {
            SourceUnitId = "RFC9001-S4-B5-P5-S2",
            SourceId = "RFC9001",
            Section = "4",
            SectionTitle = "Carrying TLS Messages",
            BlockIndex = 5,
            ParagraphIndex = 5,
            SentenceIndex = 2,
            BlockKind = "paragraph",
            Text = "Table 1 shows encryption keys by packet type.",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9001.html#section-4",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
        };

        Assert.False(DeterministicCandidateExtractor.ShouldSendToAi(caption));
        Assert.True(DeterministicCandidateExtractor.ShouldSendToAi(description));
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
