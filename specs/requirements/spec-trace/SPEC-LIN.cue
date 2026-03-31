package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-LIN"
    artifact_type: "specification"
    title: "Trace Lineage and Requirement Evolution"
    domain: "spec-trace"
    capability: "trace-lineage"
    status: "draft"
    owner: "spec-trace-maintainers"
    tags: [
        "spec-trace",
        "lineage",
        "traceability",
        "evolution",
    ]
    purpose: """
      Define how requirement IDs stay stable while requirements evolve, and how the standard records lightweight upstream lineage without workflow states or mandatory tombstone records.
      """
    scope: """
      This specification covers editorial clarifications, semantic changes, split and merge cases, moved requirements, ID reuse, withdrawn requirements, and the optional lineage and upstream-reference fields in requirement Trace blocks.
      """
    context: """
      Requirements change over time, but traceability only stays useful if the repository can distinguish wording edits from new obligations.
      """
    requirements: [
        {
            id: "REQ-LIN-0001"
            title: "Keep editorial clarifications on the same requirement ID"
            statement: "Editorial clarifications MUST keep the same `REQ-...` identifier when the obligation does not change."
            notes: [
                "Clarifications include wording fixes, formatting changes, and other non-semantic edits.",
                "A moved requirement may keep the same identifier if its semantics do not change.",
            ]
        },
        {
            id: "REQ-LIN-0002"
            title: "Assign a new requirement ID for semantic changes"
            statement: "A semantic change MUST use a new `REQ-...` identifier."
            notes: [
                "New obligations, altered acceptance criteria, and changed invariants are semantic changes.",
            ]
        },
        {
            id: "REQ-LIN-0003"
            title: "Assign new requirement IDs for split and merge outcomes"
            statement: "Split and merge scenarios MUST produce new `REQ-...` identifiers for the resulting requirements."
            notes: [
                "Use lineage fields to preserve the relationship to the source requirement IDs.",
                "The active repository does not need tombstone requirement records for retired IDs.",
            ]
        },
        {
            id: "REQ-LIN-0004"
            title: "Preserve identifiers when a requirement moves without semantic change"
            statement: """
              A requirement MAY keep the same `REQ-...` identifier when it moves to another file or section and its semantics do not change.
              """
            notes: [
                "File moves and section moves are editorial changes when the obligation stays the same.",
            ]
        },
        {
            id: "REQ-LIN-0005"
            title: "Never reuse retired requirement IDs"
            statement: "A retired `REQ-...` identifier MUST NOT be reused for a different obligation."
            notes: [
                "Retirement covers superseded and withdrawn identifiers.",
            ]
        },
        {
            id: "REQ-LIN-0006"
            title: "Keep upstream trace lightweight"
            statement: """
              A requirement MAY use `Derived From`, `Supersedes`, and `Upstream Refs` in its Trace block to record lineage and upstream material without adding per-requirement workflow states.
              """
            notes: [
                "`Derived From` records refinement lineage.",
                "`Supersedes` records forward replacement lineage.",
                "`Upstream Refs` records free-form external references such as laws, contracts, tickets, incidents, customer asks, and policies.",
                "`Upstream Refs` may cite whole documents or stable sub-document anchors such as sections, headings, paragraph markers, or other repository-specific source locations when local tooling supports them.",
                "The repository does not require tombstone requirement records for old identifiers.",
            ]
        },
        {
            id: "REQ-LIN-0007"
            title: "Keep inline references separate from Trace"
            statement: "Inline references MUST NOT be treated as `Derived From`, `Supersedes`, or `Upstream Refs` entries."
            notes: [
                "Requirements can still mention other artifact identifiers in prose using backtick-delimited inline references.",
                "Such mentions are lightweight links and do not establish lineage or upstream material on their own.",
                "Use Trace fields when the relationship needs typed, toolable semantics.",
                "Inline references do not imply copying, inheritance, or replacement.",
            ]
        },
        {
            id: "REQ-LIN-0008"
            title: "Allow precise source locators when coverage reporting needs them"
            statement: """
              A `Upstream Refs` entry MAY include document names, section labels, anchors, ranges, or other locators precise enough for source-coverage reporting.
              """
            trace: {
                related: [
                    "SPEC-RPT",
                ]
            }
            notes: [
                "Useful locators may point at RFC sections, paragraph ranges, sentence ranges, ticket subsections, policy clauses, or incident timelines.",
                "The field remains free-form so repositories can choose their own locator syntax.",
            ]
        },
        {
            id: "REQ-LIN-0009"
            title: "Retire withdrawn requirements without reusing their identifiers"
            statement: """
              A requirement that is withdrawn without a successor MUST retire its `REQ-...`
              identifier rather than silently repurpose or reuse it.
              """
            notes: [
                "Withdrawal without replacement is distinct from supersedence.",
                "A withdrawn identifier remains unavailable for future reuse.",
            ]
        },
        {
            id: "REQ-LIN-0010"
            title: "Allow retired-ID ledgers without requiring tombstone requirements"
            statement: """
              A repository MAY keep a retired-identifier ledger, changelog, or generated
              history surface for withdrawn or superseded requirements without keeping a
              mandatory tombstone requirement artifact.
              """
            notes: [
                "This preserves history when teams want visibility into retired IDs.",
                "The core standard still does not require inactive requirement sections to\nremain in the active specification files.",
            ]
        },
    ]
}
