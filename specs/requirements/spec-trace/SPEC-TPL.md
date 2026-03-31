---
artifact_id: SPEC-TPL
artifact_type: specification
title: Canonical CUE Shape and Markdown Rendering Rules
domain: spec-trace
capability: templates-and-grammar
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - templates
  - grammar
  - cue
---

# [`SPEC-TPL`](./SPEC-TPL.md) - Canonical CUE Shape and Markdown Rendering Rules

## Purpose

Define the canonical CUE field model and the generated Markdown rendering rules used by the standard.

## Scope

This specification covers canonical CUE field names, requirement record
shape, normative keyword usage, optional trace data, requirement title
guidance, trace-field semantics, lineage and upstream-reference labels,
inline identifier reference syntax in generated Markdown, and the
rendering structure for architecture, work-item, and verification artifacts.

## Context

The standard needs predictable authored data, but the requirement itself should not be hidden under record-management boilerplate. Core CUE fields stay fixed; generated Markdown remains presentation-only and repositories may add optional local extension fields when they do not redefine the core shape.

## [`REQ-TPL-0001`](./SPEC-TPL.md#req-tpl-0001-keep-specification-cue-metadata-document-level-and-compact) Keep specification CUE metadata document-level and compact
Specification CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `capability`, `status`, and `owner`.

Notes:
- `tags` and `related_artifacts` are optional.

## [`REQ-TPL-0002`](./SPEC-TPL.md#req-tpl-0002-keep-architecture-cue-metadata-focused-on-satisfaction-links) Keep architecture CUE metadata focused on satisfaction links
Architecture CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, and `satisfies`.

Notes:
- `related_artifacts` is optional.

## [`REQ-TPL-0003`](./SPEC-TPL.md#req-tpl-0003-keep-work-item-cue-metadata-focused-on-implementation-links) Keep work-item CUE metadata focused on implementation links
Work-item CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, `addresses`, `design_links`, and `verification_links`.

Notes:
- `related_artifacts` is optional.

## [`REQ-TPL-0004`](./SPEC-TPL.md#req-tpl-0004-keep-verification-cue-metadata-focused-on-proof-links) Keep verification CUE metadata focused on proof links
Verification CUE artifacts MUST use the top-level fields `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, and `verifies`.

Notes:
- `related_artifacts` is optional.
- The `status` field is artifact-scoped; if the requirements in `verifies` do not share one outcome, split them into separate verification artifacts.

## [`REQ-TPL-0005`](./SPEC-TPL.md#req-tpl-0005-use-requirement-records-as-the-canonical-entry-point) Use requirement records as the canonical entry point
A canonical specification artifact MUST express requirements as records in its `requirements` collection, each with `id`, `title`, and `statement`.

## [`REQ-TPL-0006`](./SPEC-TPL.md#req-tpl-0006-require-bcp-14-style-uppercase-normative-keywords-in-every-clause) Require BCP 14-style uppercase normative keywords in every clause
The requirement clause MUST contain exactly one approved all-caps normative keyword.

Notes:
- The standard uses BCP 14-style uppercase requirement language, inspired by RFC 2119 and RFC 8174.
- Only uppercase forms carry the defined normative meaning; lowercase forms are plain English.
- The approved keywords are `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`.
- The keyword does not need to be the first word in the clause.

## [`REQ-TPL-0007`](./SPEC-TPL.md#req-tpl-0007-make-the-compact-clause-the-canonical-requirement-form) Make the compact clause the canonical requirement form
The canonical requirement form MUST place the normative clause in the requirement record's `statement` field.

Notes:
- Richer management metadata is allowed only as an optional local extension and should not obscure the clause itself.

## [`REQ-TPL-0008`](./SPEC-TPL.md#req-tpl-0008-keep-the-requirement-trace-block-small-and-explicit) Keep the requirement Trace block small and explicit
When a requirement includes structured trace data, it MUST use only the fields
`satisfied_by`, `implemented_by`, `verified_by`, `derived_from`,
`supersedes`, `upstream_refs`, and `related`.

Notes:
- `derived_from` and `supersedes` are optional lineage fields.
- `upstream_refs` is an optional free-form upstream source field.

## [`REQ-TPL-0017`](./SPEC-TPL.md#req-tpl-0017-keep-trace-and-notes-optional) Keep Trace and Notes optional
The canonical requirement form MAY add optional `trace` and `notes` fields after the normative `statement` field.

Notes:
- When lineage matters, prefer `Derived From`, `Supersedes`, and `Upstream Refs` over prose history.

## [`REQ-TPL-0027`](./SPEC-TPL.md#req-tpl-0027-use-backticks-for-inline-identifier-references) Use backticks for inline identifier references
Any inline reference to a canonical artifact identifier MUST use backticks.

Trace:
- Related:
  - [SPEC-STD](./SPEC-STD.md)
  - [SPEC-SCH](./SPEC-SCH.md)
  - [SPEC-EXM](./SPEC-EXM.md)

Notes:
- Inline identifier references can appear in requirement clauses, `Notes`, and other descriptive sections.
- Use them for lightweight, human-readable cross-links such as component conformance, token usage, and cross-spec relationships.
- Prefer structured trace fields when the relationship needs to be captured as explicit trace metadata.

## [`REQ-TPL-0028`](./SPEC-TPL.md#req-tpl-0028-constrain-inline-identifier-references-to-canonical-artifact-identifiers) Constrain inline identifier references to canonical artifact identifiers
An inline identifier reference MUST identify a valid artifact identifier defined by the standard.

Trace:
- Related:
  - [SPEC-ID](./SPEC-ID.md)
  - [SPEC-STD](./SPEC-STD.md)
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- The standard currently defines the `REQ-...`, `SPEC-...`, `ARC-...`, `WI-...`, and `VER-...` artifact families.
- The identifier begins with the known prefix for its family and otherwise satisfies the identifier policy.
- Inline identifier references are not file names, URLs, or loose prose labels.

## [`REQ-TPL-0029`](./SPEC-TPL.md#req-tpl-0029-give-requirement-titles-descriptive-meaning) Give requirement titles descriptive meaning
The requirement title SHOULD name the obligation or concern in a short human-readable phrase rather than repeat the full clause or encode implementation detail.

Notes:
- Requirement titles are scan aids, not the normative statement.
- A title should help a reader recognize the subject of the requirement quickly.

## [`REQ-TPL-0030`](./SPEC-TPL.md#req-tpl-0030-define-trace-label-semantics-by-family) Define trace label semantics by family
A requirement `trace` object MUST treat `satisfied_by`, `implemented_by`, and
`verified_by` as typed downstream trace links; `derived_from` and
`supersedes` as lineage; `upstream_refs` as upstream source citations; and
`related` as a loose association.

Notes:
- The trace block records explicit relationships, while the surrounding prose may add context.
- Inline identifier references stay separate from the trace block and do not change the meaning of its labels.
- `Upstream Refs` may point at whole source documents or stable sub-document locations when a repository needs finer lineage to RFCs, policies, contracts, or similar material.
- Generated evidence snapshots can carry tool-produced implementation
observations without turning those observations into canonical requirement
metadata.

## [`REQ-TPL-0009`](./SPEC-TPL.md#req-tpl-0009-keep-one-specification-and-its-requirements-in-the-same-file) Keep one specification and its requirements in the same file
A specification artifact MUST represent one specification in one canonical `.cue` file.

## [`REQ-TPL-0025`](./SPEC-TPL.md#req-tpl-0025-keep-related-requirements-under-their-specification) Keep related requirements under their specification
A specification artifact MUST place one or more related `REQ-...` requirement records under that specification.

## [`REQ-TPL-0010`](./SPEC-TPL.md#req-tpl-0010-keep-work-item-trace-labels-canonical) Keep work-item trace labels canonical
A canonical work-item artifact MUST use the fields `addresses`, `design_links`, and `verification_links`, and any generated Markdown `Trace Links` section renders the labels `Addresses`, `Uses Design`, and `Verified By`.

## [`REQ-TPL-0011`](./SPEC-TPL.md#req-tpl-0011-keep-the-supporting-artifact-templates-predictable) Keep the supporting artifact templates predictable
Architecture, work-item, and verification artifacts SHOULD follow the field order shown in the reference CUE templates and the section order shown in generated Markdown so readers and tooling see a consistent structure.

## [`REQ-TPL-0012`](./SPEC-TPL.md#req-tpl-0012-normalize-normative-keyword-meaning) Normalize normative keyword meaning
Each approved normative keyword MUST have one defined meaning in the standard.

## [`REQ-TPL-0018`](./SPEC-TPL.md#req-tpl-0018-define-meaning-of-must) Define meaning of MUST
This keyword MUST indicate a required condition.

## [`REQ-TPL-0019`](./SPEC-TPL.md#req-tpl-0019-define-meaning-of-must-not) Define meaning of MUST NOT
This keyword MUST indicate a prohibited condition.

## [`REQ-TPL-0020`](./SPEC-TPL.md#req-tpl-0020-define-meaning-of-shall) Define meaning of SHALL
This keyword MUST indicate a required condition.

## [`REQ-TPL-0021`](./SPEC-TPL.md#req-tpl-0021-define-meaning-of-shall-not) Define meaning of SHALL NOT
This keyword MUST indicate a prohibited condition.

## [`REQ-TPL-0022`](./SPEC-TPL.md#req-tpl-0022-define-meaning-of-should) Define meaning of SHOULD
This keyword MUST indicate a recommended but not strictly required condition.

## [`REQ-TPL-0023`](./SPEC-TPL.md#req-tpl-0023-define-meaning-of-may) Define meaning of MAY
This keyword MUST indicate a permitted or optional condition.

## [`REQ-TPL-0026`](./SPEC-TPL.md#req-tpl-0026-define-meaning-of-should-not) Define meaning of SHOULD NOT
This keyword MUST indicate a recommended-against condition.

## [`REQ-TPL-0013`](./SPEC-TPL.md#req-tpl-0013-keep-requirement-clauses-atomic-and-short) Keep requirement clauses atomic and short
A canonical requirement clause MUST express one obligation, rule, or constraint.

## [`REQ-TPL-0024`](./SPEC-TPL.md#req-tpl-0024-keep-requirement-clauses-concise-by-default) Keep requirement clauses concise by default
A canonical requirement clause SHOULD remain a single sentence unless a short paragraph is required for precision.

## [`REQ-TPL-0014`](./SPEC-TPL.md#req-tpl-0014-keep-extension-metadata-optional-and-behind-the-clause) Keep extension metadata optional and behind the clause
Any local extension metadata MUST remain optional and follow the normative clause.

## [`REQ-TPL-0015`](./SPEC-TPL.md#req-tpl-0015-keep-front-matter-at-document-scope) Keep front matter at document scope
Canonical top-level metadata MUST describe the artifact as a whole rather than carry per-requirement metadata.

Notes:
- Generated Markdown front matter is presentation only, so canonical IDs there should remain bare identifiers rather than Markdown links.

## [`REQ-TPL-0016`](./SPEC-TPL.md#req-tpl-0016-keep-generated-evidence-outside-the-canonical-requirement-trace) Keep generated evidence outside the canonical requirement trace
The standard MUST keep generated implementation evidence outside the canonical
requirement `Trace` block rather than embed tool-discovered test or code
observations directly in requirement text.

## [`REQ-TPL-0031`](./SPEC-TPL.md#req-tpl-0031-allow-clickable-canonical-identifiers-in-markdown-body-sections) Allow clickable canonical identifiers in Markdown body sections
A generated Markdown requirement heading or trace-bearing list entry MAY wrap a canonical identifier in a repo-local Markdown link when the visible link text is the identifier.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- Tooling should extract the visible identifier text rather than the surrounding Markdown syntax.
- This rule applies to generated Markdown body content, not to canonical CUE fields.

## [`REQ-TPL-0032`](./SPEC-TPL.md#req-tpl-0032-use-precise-anchors-for-linked-specification-targets) Use precise anchors for linked specification targets
When a generated repo-local Markdown link targets a specific headed requirement or other concrete subsection inside a specification artifact document, the link SHOULD include the relevant heading anchor or other repository-supported sub-document locator rather than point only at the containing file.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- Whole-document links remain appropriate when the target is the specification artifact as a whole.
- This rule governs navigation precision, not identifier extraction from the visible link text.
