package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-TPL"
    artifact_type: "specification"
    title: "Canonical CUE Shape and Markdown Rendering Rules"
    domain: "spec-trace"
    capability: "templates-and-grammar"
    status: "draft"
    owner: "spec-trace-maintainers"
    tags: [
        "spec-trace",
        "templates",
        "grammar",
        "cue",
    ]
    purpose: "Define the canonical CUE field model and the generated Markdown rendering rules used by the standard."
    scope: """
      This specification covers canonical CUE field names, requirement record
      shape, normative keyword usage, optional trace data, requirement title
      guidance, trace-field semantics, lineage and upstream-reference labels,
      inline identifier reference syntax in generated Markdown, and the
      rendering structure for architecture, work-item, and verification artifacts.
      """
    context: """
      The standard needs predictable authored data, but the requirement itself should not be hidden under record-management boilerplate. Core CUE fields stay fixed; generated Markdown remains presentation-only and repositories may add optional local extension fields when they do not redefine the core shape.
      """
    requirements: [
        {
            id: "REQ-TPL-0001"
            title: "Keep specification CUE metadata document-level and compact"
            statement: """
              Specification CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `capability`, `status`, and `owner`.
              """
            notes: [
                "`tags` and `related_artifacts` are optional.",
            ]
        },
        {
            id: "REQ-TPL-0002"
            title: "Keep architecture CUE metadata focused on satisfaction links"
            statement: """
              Architecture CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, and `satisfies`.
              """
            notes: [
                "`related_artifacts` is optional.",
            ]
        },
        {
            id: "REQ-TPL-0003"
            title: "Keep work-item CUE metadata focused on implementation links"
            statement: """
              Work-item CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, `addresses`, `design_links`, and `verification_links`.
              """
            notes: [
                "`related_artifacts` is optional.",
            ]
        },
        {
            id: "REQ-TPL-0004"
            title: "Keep verification CUE metadata focused on proof links"
            statement: """
              Verification CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, and `verifies`.
              """
            notes: [
                "`related_artifacts` is optional.",
                "The `status` field is artifact-scoped; if the requirements in `verifies` do not share one outcome, split them into separate verification artifacts.",
            ]
        },
        {
            id: "REQ-TPL-0005"
            title: "Use requirement records as the canonical entry point"
            statement: """
              A canonical specification artifact MUST express requirements as records in its `requirements` collection, each with `id`, `title`, and `statement`.
              """
        },
        {
            id: "REQ-TPL-0006"
            title: "Require BCP 14-style uppercase normative keywords in every clause"
            statement: "The requirement clause MUST contain exactly one approved all-caps normative keyword."
            notes: [
                "The standard uses BCP 14-style uppercase requirement language, inspired by RFC 2119 and RFC 8174.",
                "Only uppercase forms carry the defined normative meaning; lowercase forms are plain English.",
                "The approved keywords are `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`.",
                "The keyword does not need to be the first word in the clause.",
            ]
        },
        {
            id: "REQ-TPL-0007"
            title: "Make the compact clause the canonical requirement form"
            statement: "The canonical requirement form MUST place the normative clause in the requirement record's `statement` field."
            notes: [
                "Richer management metadata is allowed only as an optional local extension and should not obscure the clause itself.",
            ]
        },
        {
            id: "REQ-TPL-0008"
            title: "Keep the requirement Trace block small and explicit"
            statement: """
              When a requirement includes structured trace data, it MUST use only the fields
              `satisfied_by`, `implemented_by`, `verified_by`, `derived_from`,
              `supersedes`, `upstream_refs`, and `related`.
              """
            notes: [
                "`derived_from` and `supersedes` are optional lineage fields.",
                "`upstream_refs` is an optional free-form upstream source field.",
            ]
        },
        {
            id: "REQ-TPL-0017"
            title: "Keep Trace and Notes optional"
            statement: "The canonical requirement form MAY add optional `trace` and `notes` fields after the normative `statement` field."
            notes: [
                "When lineage matters, prefer `Derived From`, `Supersedes`, and `Upstream Refs` over prose history.",
            ]
        },
        {
            id: "REQ-TPL-0027"
            title: "Use backticks for inline identifier references"
            statement: "Any inline reference to a canonical artifact identifier MUST use backticks."
            trace: {
                related: [
                    "SPEC-STD",
                    "SPEC-SCH",
                    "SPEC-EXM",
                ]
            }
            notes: [
                "Inline identifier references can appear in requirement clauses, `Notes`, and other descriptive sections.",
                "Use them for lightweight, human-readable cross-links such as component conformance, token usage, and cross-spec relationships.",
                "Prefer structured trace fields when the relationship needs to be captured as explicit trace metadata.",
            ]
        },
        {
            id: "REQ-TPL-0028"
            title: "Constrain inline identifier references to canonical artifact identifiers"
            statement: "An inline identifier reference MUST identify a valid artifact identifier defined by the standard."
            trace: {
                related: [
                    "SPEC-ID",
                    "SPEC-STD",
                    "SPEC-SCH",
                ]
            }
            notes: [
                "The standard currently defines the `REQ-...`, `SPEC-...`, `ARC-...`, `WI-...`, and `VER-...` artifact families.",
                "The identifier begins with the known prefix for its family and otherwise satisfies the identifier policy.",
                "Inline identifier references are not file names, URLs, or loose prose labels.",
            ]
        },
        {
            id: "REQ-TPL-0029"
            title: "Give requirement titles descriptive meaning"
            statement: """
              The requirement title SHOULD name the obligation or concern in a short human-readable phrase rather than repeat the full clause or encode implementation detail.
              """
            notes: [
                "Requirement titles are scan aids, not the normative statement.",
                "A title should help a reader recognize the subject of the requirement quickly.",
            ]
        },
        {
            id: "REQ-TPL-0030"
            title: "Define trace label semantics by family"
            statement: """
              A requirement `trace` object MUST treat `satisfied_by`, `implemented_by`, and
              `verified_by` as typed downstream trace links; `derived_from` and
              `supersedes` as lineage; `upstream_refs` as upstream source citations; and
              `related` as a loose association.
              """
            notes: [
                "The trace block records explicit relationships, while the surrounding prose may add context.",
                "Inline identifier references stay separate from the trace block and do not change the meaning of its labels.",
                "`Upstream Refs` may point at whole source documents or stable sub-document locations when a repository needs finer lineage to RFCs, policies, contracts, or similar material.",
                "Generated evidence snapshots can carry tool-produced implementation\nobservations without turning those observations into canonical requirement\nmetadata.",
            ]
        },
        {
            id: "REQ-TPL-0009"
            title: "Keep one specification and its requirements in the same file"
            statement: "A specification artifact MUST represent one specification in one canonical `.cue` file."
        },
        {
            id: "REQ-TPL-0025"
            title: "Keep related requirements under their specification"
            statement: "A specification artifact MUST place one or more related `REQ-...` requirement records under that specification."
        },
        {
            id: "REQ-TPL-0010"
            title: "Keep work-item trace labels canonical"
            statement: "A canonical work-item artifact MUST use the fields `addresses`, `design_links`, and `verification_links`, and any generated Markdown `Trace Links` section renders the labels `Addresses`, `Uses Design`, and `Verified By`."
        },
        {
            id: "REQ-TPL-0011"
            title: "Keep the supporting artifact templates predictable"
            statement: """
              Architecture, work-item, and verification artifacts SHOULD follow the field order shown in the reference CUE templates and the section order shown in generated Markdown so readers and tooling see a consistent structure.
              """
        },
        {
            id: "REQ-TPL-0012"
            title: "Normalize normative keyword meaning"
            statement: "Each approved normative keyword MUST have one defined meaning in the standard."
        },
        {
            id: "REQ-TPL-0018"
            title: "Define meaning of MUST"
            statement: "This keyword MUST indicate a required condition."
        },
        {
            id: "REQ-TPL-0019"
            title: "Define meaning of MUST NOT"
            statement: "This keyword MUST indicate a prohibited condition."
        },
        {
            id: "REQ-TPL-0020"
            title: "Define meaning of SHALL"
            statement: "This keyword MUST indicate a required condition."
        },
        {
            id: "REQ-TPL-0021"
            title: "Define meaning of SHALL NOT"
            statement: "This keyword MUST indicate a prohibited condition."
        },
        {
            id: "REQ-TPL-0022"
            title: "Define meaning of SHOULD"
            statement: "This keyword MUST indicate a recommended but not strictly required condition."
        },
        {
            id: "REQ-TPL-0023"
            title: "Define meaning of MAY"
            statement: "This keyword MUST indicate a permitted or optional condition."
        },
        {
            id: "REQ-TPL-0026"
            title: "Define meaning of SHOULD NOT"
            statement: "This keyword MUST indicate a recommended-against condition."
        },
        {
            id: "REQ-TPL-0013"
            title: "Keep requirement clauses atomic and short"
            statement: "A canonical requirement clause MUST express one obligation, rule, or constraint."
        },
        {
            id: "REQ-TPL-0024"
            title: "Keep requirement clauses concise by default"
            statement: "A canonical requirement clause SHOULD remain a single sentence unless a short paragraph is required for precision."
        },
        {
            id: "REQ-TPL-0014"
            title: "Keep extension metadata optional and behind the clause"
            statement: "Any local extension metadata MUST remain optional and follow the normative clause."
        },
        {
            id: "REQ-TPL-0015"
            title: "Keep front matter at document scope"
            statement: "Canonical top-level metadata MUST describe the artifact as a whole rather than carry per-requirement metadata."
            notes: [
                "Generated Markdown front matter is presentation only, so canonical IDs there should remain bare identifiers rather than Markdown links.",
            ]
        },
        {
            id: "REQ-TPL-0016"
            title: "Keep generated evidence outside the canonical requirement trace"
            statement: """
              The standard MUST keep generated implementation evidence outside the canonical
              requirement `Trace` block rather than embed tool-discovered test or code
              observations directly in requirement text.
              """
        },
        {
            id: "REQ-TPL-0031"
            title: "Allow clickable canonical identifiers in Markdown body sections"
            statement: """
              A generated Markdown requirement heading or trace-bearing list entry MAY wrap a canonical identifier in a repo-local Markdown link when the visible link text is the identifier.
              """
            trace: {
                related: [
                    "SPEC-SCH",
                ]
            }
            notes: [
                "Tooling should extract the visible identifier text rather than the surrounding Markdown syntax.",
                "This rule applies to generated Markdown body content, not to canonical CUE fields.",
            ]
        },
        {
            id: "REQ-TPL-0032"
            title: "Use precise anchors for linked specification targets"
            statement: """
              When a generated repo-local Markdown link targets a specific headed requirement or other concrete subsection inside a specification artifact document, the link SHOULD include the relevant heading anchor or other repository-supported sub-document locator rather than point only at the containing file.
              """
            trace: {
                related: [
                    "SPEC-SCH",
                ]
            }
            notes: [
                "Whole-document links remain appropriate when the target is the specification artifact as a whole.",
                "This rule governs navigation precision, not identifier extraction from the visible link text.",
            ]
        },
    ]
}
