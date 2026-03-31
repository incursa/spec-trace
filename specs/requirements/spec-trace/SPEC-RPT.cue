package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-RPT"
    artifact_type: "specification"
    title: "Derived Reporting Dimensions and Attestation Views"
    domain: "spec-trace"
    capability: "derived-reporting"
    status: "draft"
    owner: "spec-trace-maintainers"
    tags: [
        "spec-trace",
        "reporting",
        "attestation",
        "coverage",
    ]
    purpose: """
      Define how repositories can generate useful coverage, progress, and attestation reporting from the canonical trace graph without turning that reporting into a new artifact family or overloading the conformance profiles.
      """
    scope: """
      This specification covers reporting dimensions, staged adoption semantics,
      source-coverage reporting, evidence-kind reporting, local policy attestation
      states, and the rule that current-state evidence remains derived from
      repository truth rather than hand-maintained canonical status fields.
      """
    context: """
      Teams need to answer different questions at different stages of delivery. Greenfield repositories may have good requirements before they have implementation. Brownfield repositories may have tests and code before they have work-item history. The standard therefore needs a reporting layer that stays grounded in the canonical graph and real evidence without forcing every repository into the same maturity narrative.
      """
    requirements: [
        {
            id: "REQ-RPT-0001"
            title: "Keep derived reporting separate from canonical trace"
            statement: """
              Coverage, attestation, and current-state status views MUST be treated as derived reporting over the canonical repository graph and evidence sources rather than as new canonical requirement metadata.
              """
            notes: [
                "Derived reporting answers current-state questions without changing the requirement text.",
                "Derived reporting does not create a fifth artifact family.",
            ]
        },
        {
            id: "REQ-RPT-0002"
            title: "Report coverage dimensions independently of profile choice"
            statement: """
              Reporting SHOULD be able to surface upstream, design, implementation,
              verification, and evidence coverage by kind independently of the repository's
              chosen conformance profile.
              """
            notes: [
                "The same repository may run at `core` while still reporting rich coverage dimensions.",
                "These dimensions are reporting views, not new canonical profiles.",
            ]
        },
        {
            id: "REQ-RPT-0003"
            title: "Support staged adoption across repository histories"
            statement: """
              The standard MUST support staged adoption in both greenfield and brownfield repositories without requiring every reporting dimension to be complete from day one.
              """
            notes: [
                "A new project may have valid requirements before it has code or tests.",
                "A legacy project may have code and tests before it has architecture artifacts or historical work items.",
            ]
        },
        {
            id: "REQ-RPT-0004"
            title: "Avoid synthetic work history as an implementation proxy"
            statement: """
              Reporting and repository policy MUST NOT infer that a requirement is unimplemented solely because it lacks an `Implemented By` work-item link.
              """
            notes: [
                "Brownfield repositories often have real implementation with no historical work-item trail.",
                "`Implemented By` remains useful delivery trace, but it is not the only possible grounding signal.",
            ]
        },
        {
            id: "REQ-RPT-0005"
            title: "Keep local attestation states policy-derived"
            statement: """
              Repositories MAY derive local attestation states such as implemented, verified,
              and release-ready from their own combination of trace coverage, verification
              artifacts, evidence snapshots, freshness rules, and execution results.
              """
            notes: [
                "These terms remain repository policy unless a repository standardizes them locally.",
                "The canonical standard defines the trace model, not one universal release formula.",
            ]
        },
        {
            id: "REQ-RPT-0006"
            title: "Keep evidence freshness and health derived"
            statement: """
              Evidence freshness, stale manual QA, failing tests, benchmark regressions, and similar health signals MUST remain derived report states or local extension metadata rather than canonical requirement states.
              """
            notes: [
                "Repositories may still record manual QA, benchmarks, fuzzing, or other evidence in verification artifacts.",
                "Freshness windows and status rollups are local policy concerns.",
            ]
        },
        {
            id: "REQ-RPT-0007"
            title: "Support source-coverage reporting when locators are precise"
            statement: """
              Reporting SHOULD be able to surface which source materials or source regions are represented by requirements when `Upstream Refs` entries are precise enough to support that view.
              """
            notes: [
                "Useful locators may include RFC sections, anchors, paragraph ranges, sentence ranges, ticket subsections, or policy clauses.",
                "Source-coverage reporting remains derived from the canonical requirements and their `Upstream Refs`.",
            ]
        },
        {
            id: "REQ-RPT-0008"
            title: "Ground reporting in real repository evidence"
            statement: """
              Derived reporting MUST be able to use verification artifacts, generated
              evidence snapshots, and other repository-policy evidence sources as grounding
              signals for attestation views.
              """
            notes: [
                "Verification artifacts summarize proof activity.",
                "Evidence snapshots provide tool-produced direct implementation or execution\ngrounding even when supporting documents are sparse.",
            ]
        },
        {
            id: "REQ-RPT-0009"
            title: "Distinguish missing evidence from uncollected evidence"
            statement: """
              Derived reporting SHOULD distinguish missing evidence from uncollected,
              unsupported, or stale evidence rather than collapse those cases into one
              undifferentiated gap state.
              """
            notes: [
                "A requirement may lack `unit_test` evidence because no matching test exists.",
                "A requirement may also lack current `benchmark` evidence because no benchmark\nsnapshot was collected for the evaluated scope.",
                "These are different reporting outcomes even when neither one satisfies a\nlocal policy gate.",
            ]
        },
    ]
}
