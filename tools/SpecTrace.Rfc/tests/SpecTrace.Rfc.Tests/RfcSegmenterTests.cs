using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class RfcSegmenterTests
{
    [Fact]
    public void HtmlExtractorKeepsRfcSectionsAndParagraphText()
    {
        const string html = """
<!DOCTYPE html>
<html><head><title>RFC 9221: An Unreliable Datagram Extension to QUIC</title><meta content="9221" name="rfc.number"><style>body { color: red; }</style></head>
<body><nav>Table of contents</nav><section id="section-4"><h2>4. Datagram Frame Types</h2><p>An endpoint MUST send DATAGRAM frames only when negotiated.</p></section></body></html>
""";

        var text = RfcHtmlTextExtractor.ToPlainText(html);

        Assert.Contains("4. Datagram Frame Types", text, StringComparison.Ordinal);
        Assert.Contains("An endpoint MUST send DATAGRAM frames only when negotiated.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("color: red", text, StringComparison.Ordinal);
        Assert.Equal("9221", RfcHtmlTextExtractor.TryGetRfcNumber(html));
    }

    [Fact]
    public void SegmentCreatesStableSourceUnitIdsAndSectionUrls()
    {
        var document = new RfcSourceDocument
        {
            SourceId = "RFC9114",
            RfcNumber = "9114",
            Title = "HTTP/3",
            SourceUrl = "https://www.rfc-editor.org/rfc/rfc9114.txt",
            CanonicalUrl = "https://www.rfc-editor.org/rfc/rfc9114.html",
            RetrievedAt = "2026-05-20T00:00:00.0000000Z",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
            Content = """
1. Introduction

A client MUST send requests on a bidirectional stream. This sentence is explanatory.

2. Frames

An endpoint MUST NOT send reserved frames.
""",
        };

        var units = RfcSegmenter.Segment(document);

        Assert.Collection(
            units,
            first =>
            {
                Assert.Equal("RFC9114-S1-B1-P1-S1", first.SourceUnitId);
                Assert.Equal("1", first.Section);
                Assert.Equal("Introduction", first.SectionTitle);
                Assert.Equal("A client MUST send requests on a bidirectional stream.", first.Text);
                Assert.Equal("https://www.rfc-editor.org/rfc/rfc9114.html#section-1", first.SourceUrl);
            },
            second =>
            {
                Assert.Equal("RFC9114-S1-B1-P1-S2", second.SourceUnitId);
                Assert.Equal("This sentence is explanatory.", second.Text);
            },
            third =>
            {
                Assert.Equal("RFC9114-S2-B1-P1-S1", third.SourceUnitId);
                Assert.Equal("An endpoint MUST NOT send reserved frames.", third.Text);
            });
    }

    [Fact]
    public void SegmentSkipsIndentedTableOfContentsEntries()
    {
        var document = new RfcSourceDocument
        {
            SourceId = "RFC8999",
            RfcNumber = "8999",
            Title = "QUIC Invariants",
            CanonicalUrl = "https://www.rfc-editor.org/rfc/rfc8999.html",
            RetrievedAt = "2026-05-20T00:00:00.0000000Z",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
            Content = """
Table of Contents

   1.  Introduction
     1.1.  Nested Entry
   Appendix A.  Incorrect Assumptions

1.  Introduction

A client MUST send packets.
""",
        };

        var units = RfcSegmenter.Segment(document);

        var unit = Assert.Single(units);
        Assert.Equal("RFC8999-S1-B1-P1-S1", unit.SourceUnitId);
        Assert.Equal("A client MUST send packets.", unit.Text);
        Assert.DoesNotContain(units, item => item.Text.Contains("Nested Entry", StringComparison.Ordinal));
    }

    [Fact]
    public void SegmentKeepsIdsUniqueWhenASectionHeadingRepeats()
    {
        var document = new RfcSourceDocument
        {
            SourceId = "RFC9002",
            RfcNumber = "9002",
            Title = "QUIC Recovery",
            CanonicalUrl = "https://www.rfc-editor.org/rfc/rfc9002.html",
            RetrievedAt = "2026-05-20T00:00:00.0000000Z",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
            Content = """
5.  Timers

An endpoint MUST arm a timer.

5.  Timers

An endpoint MUST cancel a timer.
""",
        };

        var units = RfcSegmenter.Segment(document);

        Assert.Collection(
            units,
            first => Assert.Equal("RFC9002-S5-B1-P1-S1", first.SourceUnitId),
            second => Assert.Equal("RFC9002-S5-B2-P2-S1", second.SourceUnitId));
        Assert.Equal(units.Count, units.Select(unit => unit.SourceUnitId).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void SegmentKeepsFiguresAsSingleSourceUnits()
    {
        var document = new RfcSourceDocument
        {
            SourceId = "RFC9000",
            RfcNumber = "9000",
            Title = "QUIC",
            CanonicalUrl = "https://www.rfc-editor.org/rfc/rfc9000.html",
            RetrievedAt = "2026-05-20T00:00:00.0000000Z",
            TextHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000",
            Content = """
7. Packetization

   field-a = 1
   field-b = 2
""",
        };

        var unit = Assert.Single(RfcSegmenter.Segment(document));
        Assert.Equal("figure", unit.BlockKind);
        Assert.Contains("field-a", unit.Text, StringComparison.Ordinal);
        Assert.Equal("RFC9000-S7-B1-P1-S1", unit.SourceUnitId);
    }
}
