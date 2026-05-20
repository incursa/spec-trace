using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Tests;

public sealed class SpecAssemblerTests
{
    [Fact]
    public void AssembleFromCandidatesAddsCanonicalIdsAndUpstreamRefs()
    {
        var ledger = new[]
        {
            new SourceUnit
            {
                SourceUnitId = "RFC9114-S4P1-B3-P2-S1",
                SourceId = "RFC9114",
                Section = "4.1",
                SectionTitle = "HTTP Message Exchanges",
                BlockIndex = 3,
                ParagraphIndex = 2,
                SentenceIndex = 1,
                BlockKind = "paragraph",
                Text = "A client MUST send requests on a bidirectional stream.",
                SourceUrl = "https://www.rfc-editor.org/rfc/rfc9114.html#section-4.1",
                TextHash = "sha256:1111111111111111111111111111111111111111111111111111111111111111",
            },
        }.ToDictionary(unit => unit.SourceUnitId, StringComparer.Ordinal);

        var candidate = new CandidateDecision
        {
            SourceUnitId = "RFC9114-S4P1-B3-P2-S1",
            Decision = "emit",
            Requirements =
            [
                new CandidateRequirement
                {
                    Title = "Send requests on bidirectional streams",
                    Statement = "A client MUST send requests on a bidirectional stream.",
                    Coverage = new RequirementCoverage
                    {
                        Positive = "required",
                        Negative = "required",
                        Edge = "optional",
                        Fuzz = "deferred",
                    },
                },
            ],
        };

        var artifact = SpecAssembler.AssembleFromCandidates(
            [candidate],
            ledger,
            new SpecAssemblyOptions
            {
                SpecId = "SPEC-HTTP3-RFC9114",
                Domain = "http3",
                Capability = "http3-rfc9114",
                Title = "HTTP/3 RFC 9114 Requirements",
                Owner = "protocol-team",
                Purpose = "Capture HTTP/3 RFC 9114 requirements.",
            });

        var requirement = Assert.Single(artifact.Requirements);
        Assert.Equal("REQ-HTTP3-RFC9114-S4P1-0001", requirement.Id);
        Assert.Equal("Send requests on bidirectional streams", requirement.Title);
        Assert.NotNull(requirement.Coverage);
        Assert.NotNull(requirement.Trace);
        Assert.Contains("RFC 9114 §4.1 RFC9114-S4P1-B3-P2-S1", requirement.Trace.UpstreamRefs!);
        Assert.Contains("https://www.rfc-editor.org/rfc/rfc9114.html#section-4.1", requirement.Trace.UpstreamRefs!);
    }

    [Fact]
    public void AssembleCanUseNamespaceOnlyIdsForStrictValidators()
    {
        var ledger = new[]
        {
            new SourceUnit
            {
                SourceUnitId = "RFC9114-S4P1-B3-P2-S1",
                SourceId = "RFC9114",
                Section = "4.1",
                SectionTitle = "HTTP Message Exchanges",
                BlockIndex = 3,
                ParagraphIndex = 2,
                SentenceIndex = 1,
                BlockKind = "paragraph",
                Text = "A client MUST send requests on a bidirectional stream.",
                SourceUrl = "https://www.rfc-editor.org/rfc/rfc9114.html#section-4.1",
                TextHash = "sha256:1111111111111111111111111111111111111111111111111111111111111111",
            },
        }.ToDictionary(unit => unit.SourceUnitId, StringComparer.Ordinal);

        var artifact = SpecAssembler.AssembleFromCandidates(
            [
                new CandidateDecision
                {
                    SourceUnitId = "RFC9114-S4P1-B3-P2-S1",
                    Decision = "emit",
                    Requirements =
                    [
                        new CandidateRequirement
                        {
                            ProposedIdHint = "REQ-HTTP3-RFC9114-S4P1-0001",
                            Title = "Send requests on bidirectional streams",
                            Statement = "A client MUST send requests on a bidirectional stream.",
                        },
                    ],
                },
            ],
            ledger,
            new SpecAssemblyOptions
            {
                SpecId = "SPEC-HTTP3-RFC9114",
                Domain = "http3",
                Capability = "http3-rfc9114",
                Title = "HTTP/3 RFC 9114 Requirements",
                Owner = "protocol-team",
                Purpose = "Capture HTTP/3 RFC 9114 requirements.",
                IdStyle = "namespace",
            });

        Assert.Equal("REQ-HTTP3-RFC9114-0001", Assert.Single(artifact.Requirements).Id);
    }

    [Fact]
    public void AssembleRejectsStatementsWithMultipleNormativeKeywords()
    {
        var ledger = new Dictionary<string, SourceUnit>(StringComparer.Ordinal);
        var candidate = new CandidateDecision
        {
            SourceUnitId = "RFC1-S1-B1-P1-S1",
            Decision = "emit",
            Requirements =
            [
                new CandidateRequirement
                {
                    Title = "Bad statement",
                    Statement = "An endpoint MUST send data and MUST NOT send invalid data.",
                },
            ],
        };

        var exception = Assert.Throws<InvalidOperationException>(() => SpecAssembler.AssembleFromCandidates(
            [candidate],
            ledger,
            new SpecAssemblyOptions
            {
                SpecId = "SPEC-TEST-RFC1",
                Domain = "test",
                Capability = "test-rfc1",
                Title = "Test",
                Owner = "test",
                Purpose = "Test.",
            }));

        Assert.Contains("exactly one uppercase normative keyword", exception.Message, StringComparison.Ordinal);
    }
}
