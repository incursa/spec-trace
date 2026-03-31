package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-SCH"
    artifact_type: "specification"
    title: "Schemas and Validation Rules"
    domain: "spec-trace"
    capability: "schema-validation"
    status: "draft"
    owner: "spec-trace-maintainers"
    tags: [
        "spec-trace",
        "schemas",
        "validation",
        "tooling",
    ]
    purpose: "Define the authoritative CUE contracts and repository validation rules that support the standard without prescribing one service or database."
    scope: """
      This specification covers the authoritative CUE schema packages, repository-wide
      catalog validation, generated evidence snapshot contracts, optional compatibility
      exports such as JSON Schema, the inline identifier reference convention in
      generated Markdown, and the validation capabilities expected from tooling.
      """
    context: """
      The standard depends on compact requirement records, explicit trace links, lightweight upstream lineage, and repository-level graph checks. Tooling needs an authoritative schema and a repository-wide catalog so references behave like foreign keys instead of loose prose.
      """
    requirements: [
        {
            id: "REQ-SCH-0001"
            title: "Provide schemas for the core extracted shapes"
            statement: """
              The reference package MUST provide importable CUE definitions for canonical artifact metadata, identifier policy metadata, compact requirement records, requirement trace fields, generated evidence snapshots, and work-item trace fields.
              """
        },
        {
            id: "REQ-SCH-0002"
            title: "Keep front matter validation strict by document family"
            statement: """
              The canonical CUE schema package MUST validate `artifact_type`, status vocabularies, and artifact identifier patterns for each document family.
              """
            notes: [
                "Optional local extension fields may appear in canonical CUE artifacts, but the core family-specific keys stay fixed.",
            ]
        },
        {
            id: "REQ-SCH-0003"
            title: "Validate requirement identifiers as part of the identifier catalog"
            statement: """
              The authoritative identifier definitions MUST validate both artifact identifier rules and `REQ-...` identifier rules.
              """
        },
        {
            id: "REQ-SCH-0004"
            title: "Validate the compact requirement clause shape"
            statement: """
              The canonical requirement definition MUST validate a `REQ-...` identifier, short title, normative clause, extracted normative keyword, and optional trace or notes data.
              """
            notes: [
                "The clause schema enforces the narrowed BCP 14-style uppercase keyword set used by the standard.",
            ]
        },
        {
            id: "REQ-SCH-0005"
            title: "Keep requirement trace labels canonical"
            statement: """
              The canonical requirement trace definition MUST allow only `satisfied_by`, `implemented_by`, `verified_by`, `derived_from`, `supersedes`, `upstream_refs`, and `related`.
              """
            notes: [
                "`satisfied_by` accepts ARC IDs, `implemented_by` accepts WI IDs, `verified_by` accepts VER IDs, `derived_from` and `supersedes` accept REQ IDs, `upstream_refs` remains a free-form string array, and `related` may mix requirement IDs and other core artifact IDs.",
            ]
        },
        {
            id: "REQ-SCH-0006"
            title: "Treat schemas as extracted-shape contracts"
            statement: """
              The authoritative schema layer MUST describe canonical CUE artifact shapes rather than require one specific Markdown parser implementation.
              """
        },
        {
            id: "REQ-SCH-0007"
            title: "Resolve explicit traceability links"
            statement: """
              Validation tooling MUST be able to resolve requirement links to design,
              work-item, verification, and lineage identifiers when those references are
              present in canonical CUE metadata.
              """
        },
        {
            id: "REQ-SCH-0008"
            title: "Report traceability gaps"
            statement: """
              Validation tooling SHOULD be able to report requirements missing design,
              implementation, verification, or requested reporting coverage dimensions.
              """
        },
        {
            id: "REQ-SCH-0009"
            title: "Keep evidence kinds extensible and schema-agnostic"
            statement: """
              The evidence snapshot schema MUST model evidence kinds as lowercase extensible
              tokens rather than as a language-specific or framework-specific grammar.
              """
        },
        {
            id: "REQ-SCH-0010"
            title: "Surface evidence coverage views"
            statement: """
              Validation tooling SHOULD be able to report which requirements have observed
              evidence for the well-known and custom evidence kinds present in evaluated
              evidence snapshots.
              """
        },
        {
            id: "REQ-SCH-0011"
            title: "Constrain trace-bearing identifier families"
            statement: """
              The canonical CUE definitions and any compatibility schema exports MUST constrain the direct trace-bearing lists in top-level artifact fields and requirement or work-item trace blocks to the identifier family each field is meant to carry rather than accept generic non-empty strings.
              """
        },
        {
            id: "REQ-SCH-0012"
            title: "Detect duplicate identifiers"
            statement: "Validation tooling MUST detect duplicate artifact IDs and duplicate requirement IDs in the repository."
        },
        {
            id: "REQ-SCH-0013"
            title: "Resolve explicit trace references"
            statement: """
              Validation tooling MUST resolve explicit trace references and report unresolved requirement or artifact references in direct trace-bearing fields.
              """
        },
        {
            id: "REQ-SCH-0014"
            title: "Enforce reciprocal trace consistency"
            statement: """
              Repository tooling MUST report when a requirement link is not reciprocated by the linked architecture, work-item, or verification artifact, or when generated Markdown diverges from canonical CUE trace fields.
              """
        },
        {
            id: "REQ-SCH-0015"
            title: "Keep namespaces aligned"
            statement: """
              Validation tooling MUST report when an artifact's canonical domain value or identifier namespace does not align with the directory namespace that owns the file.
              """
        },
        {
            id: "REQ-SCH-0016"
            title: "Model lineage and upstream refs explicitly"
            statement: """
              The canonical CUE trace definition and any compatibility schema export MUST model `Derived From` and `Supersedes` as requirement-reference arrays and `Upstream Refs` as a non-empty string array.
              """
        },
        {
            id: "REQ-SCH-0017"
            title: "Allow lineage references without tombstones"
            statement: """
              Validation tooling MUST accept `Derived From` and `Supersedes` references even when the referenced requirement IDs are not present as active documents in the repository.
              """
        },
        {
            id: "REQ-SCH-0018"
            title: "Document inline identifier references in schema contracts"
            statement: """
              The canonical schema package and compatibility guidance MUST document inline identifier references as backtick-delimited canonical artifact identifiers whose full resolution may require repository-level validation beyond shape checking alone.
              """
            trace: {
                related: [
                    "SPEC-STD",
                    "SPEC-TPL",
                    "SPEC-EXM",
                ]
            }
            notes: [
                "Inline identifier references are prose links, not `Trace`-block fields.",
                "Compatibility exports such as JSON Schema can describe the record shape, but they cannot fully resolve cross-file links by themselves.",
                "Repository-level tooling can extract and validate inline references against the repository's known artifact and retired-ID catalogs.",
            ]
        },
        {
            id: "REQ-SCH-0019"
            title: "Prevent trace inference from inline identifier references"
            statement: "Validation tooling MUST NOT infer typed trace edges from inline identifier references alone."
            notes: [
                "Inline references can be reported as mentions, but they are not a substitute for structured trace data.",
            ]
        },
        {
            id: "REQ-SCH-0020"
            title: "Extract inline identifier references when practical"
            statement: """
              Validation tooling SHOULD extract inline identifier references separately from structured trace fields when it can do so.
              """
            notes: [
                "Separate extraction supports lightweight mention reporting without promoting prose references into graph edges.",
            ]
        },
        {
            id: "REQ-SCH-0021"
            title: "Normalize linked identifiers in Markdown sections"
            statement: """
              Validation tooling MUST extract canonical identifiers from generated repo-local Markdown links, including same-document and cross-document anchor links, when the visible link text is the identifier in requirement headings and trace-bearing Markdown sections.
              """
            trace: {
                related: [
                    "SPEC-TPL",
                ]
            }
            notes: [
                "This keeps clickable Markdown authoring compatible with canonical identifier extraction.",
                "Anchor fragments may improve navigation precision without changing the extracted identifier.",
                "Generated file-level front matter may still be normalized by the extractor before schema validation when the source text contains decorated identifiers.",
            ]
        },
        {
            id: "REQ-SCH-0022"
            title: "Surface reporting dimensions for downstream tooling"
            statement: """
              Validation tooling SHOULD report per-repository and per-requirement coverage
              for `Upstream Refs`, `Satisfied By`, `Implemented By`, `Verified By`, and the
              evidence kinds present in evaluated evidence snapshots.
              """
            trace: {
                related: [
                    "SPEC-RPT",
                    "SPEC-EVD",
                ]
            }
            notes: [
                "These dimension reports complement the profile result instead of replacing it.",
                "Coverage views help teams distinguish greenfield gaps from brownfield trace debt.",
            ]
        },
        {
            id: "REQ-SCH-0023"
            title: "Emit machine-readable coverage summaries when requested"
            statement: """
              Validation tooling SHOULD be able to emit machine-readable summaries of coverage dimensions for dashboards, attestation tooling, and repository reporting.
              """
            trace: {
                related: [
                    "SPEC-RPT",
                ]
            }
        },
        {
            id: "REQ-SCH-0024"
            title: "Provide a schema for generated evidence snapshots"
            statement: """
              The evidence snapshot contract MUST validate generated evidence snapshots including producer metadata, requirement IDs, evidence kinds, statuses, and optional refs.
              """
            trace: {
                related: [
                    "SPEC-EVD",
                ]
            }
        },
        {
            id: "REQ-SCH-0025"
            title: "Allow partial evidence snapshots"
            statement: """
              Validation tooling MUST accept evidence snapshots that cover only a subset of
              requirements or only a subset of evidence kinds.
              """
            trace: {
                related: [
                    "SPEC-EVD",
                ]
            }
        },
        {
            id: "REQ-SCH-0026"
            title: "Merge overlapping evidence snapshots additively when reporting"
            statement: """
              Validation tooling SHOULD merge overlapping evidence snapshots additively when
              it produces repository-level or requirement-level evidence reports.
              """
            trace: {
                related: [
                    "SPEC-EVD",
                    "SPEC-RPT",
                ]
            }
        },
        {
            id: "REQ-SCH-0027"
            title: "Avoid negative inference from single-snapshot omission"
            statement: """
              Validation tooling MUST NOT infer that evidence is absent from the repository
              solely because one evaluated evidence snapshot omits a requirement or evidence
              kind.
              """
            trace: {
                related: [
                    "SPEC-EVD",
                ]
            }
        },
    ]
}
