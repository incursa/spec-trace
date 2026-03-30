---
artifact_id: SPEC-SCH
artifact_type: specification
title: Schemas and Validation Rules
domain: spec-trace
capability: schema-validation
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - schemas
  - validation
  - tooling
---

# [`SPEC-SCH`](./SPEC-SCH.md) - Schemas and Validation Rules

## Purpose

Define the machine-readable contracts that support the standard without prescribing one parser or build system.

## Scope

This specification covers the reference schemas, the extracted metadata shapes
they validate, generated evidence snapshot shapes, the inline identifier
reference convention as documented schema guidance, and the validation
capabilities expected from tooling.

## Context

The standard depends on compact requirement clauses, file-level front matter, explicit trace links, lightweight upstream lineage, and repository-level graph checks. Tooling needs a machine-readable contract for those shapes and link families. The schema layer provides the `core` profile baseline, while `traceable` and `auditable` profiles add repository-level graph checks on top.

## [`REQ-SCH-0001`](./SPEC-SCH.md#req-sch-0001-provide-schemas-for-the-core-extracted-shapes) Provide schemas for the core extracted shapes
The reference package MUST provide schemas for artifact front matter,
identifier policy metadata, compact requirement clauses, requirement trace
fields, generated evidence snapshots, and work-item trace fields.

## [`REQ-SCH-0002`](./SPEC-SCH.md#req-sch-0002-keep-front-matter-validation-strict-by-document-family) Keep front matter validation strict by document family
[`artifact-frontmatter.schema.json`](../../../schemas/artifact-frontmatter.schema.json) MUST validate `artifact_type`, status vocabularies, and artifact identifier patterns for each document family.

Notes:
- Namespaced `x_...` keys may appear as optional local extensions in front matter, but the core family-specific keys stay fixed.

## [`REQ-SCH-0003`](./SPEC-SCH.md#req-sch-0003-validate-requirement-identifiers-as-part-of-the-identifier-catalog) Validate requirement identifiers as part of the identifier catalog
[`artifact-id-policy.schema.json`](../../../schemas/artifact-id-policy.schema.json) MUST validate both artifact identifier rules and `REQ-...` identifier rules.

## [`REQ-SCH-0004`](./SPEC-SCH.md#req-sch-0004-validate-the-compact-requirement-clause-shape) Validate the compact requirement clause shape
[`requirement-clause.schema.json`](../../../schemas/requirement-clause.schema.json) MUST validate a `REQ-...` identifier, short title, normative clause, extracted normative keyword, and optional trace or notes data.

Notes:
- The clause schema enforces the narrowed BCP 14-style uppercase keyword set used by the standard.

## [`REQ-SCH-0005`](./SPEC-SCH.md#req-sch-0005-keep-requirement-trace-labels-canonical) Keep requirement trace labels canonical
[`requirement-trace-fields.schema.json`](../../../schemas/requirement-trace-fields.schema.json) MUST allow only `Satisfied By`, `Implemented By`, `Verified By`, `Derived From`, `Supersedes`, `Upstream Refs`, and `Related`.

Notes:
- `Satisfied By` accepts ARC IDs, `Implemented By` accepts WI IDs, `Verified By`
  accepts VER IDs, `Derived From` and `Supersedes` accept REQ IDs,
  `Upstream Refs` remains a free-form string array, and `Related` may mix
  requirement IDs and other core artifact IDs.

## [`REQ-SCH-0006`](./SPEC-SCH.md#req-sch-0006-treat-schemas-as-extracted-shape-contracts) Treat schemas as extracted-shape contracts
The reference schemas MUST describe extracted metadata shapes rather than require one specific raw Markdown parser implementation.

## [`REQ-SCH-0007`](./SPEC-SCH.md#req-sch-0007-resolve-explicit-traceability-links) Resolve explicit traceability links
Validation tooling MUST be able to resolve requirement links to design,
work-item, verification, and lineage identifiers when those references are
present in extracted metadata.

## [`REQ-SCH-0008`](./SPEC-SCH.md#req-sch-0008-report-traceability-gaps) Report traceability gaps
Validation tooling SHOULD be able to report requirements missing design,
implementation, verification, or requested reporting coverage dimensions.

## [`REQ-SCH-0009`](./SPEC-SCH.md#req-sch-0009-keep-evidence-kinds-extensible-and-schema-agnostic) Keep evidence kinds extensible and schema-agnostic
The evidence snapshot schema MUST model evidence kinds as lowercase extensible
tokens rather than as a language-specific or framework-specific grammar.

## [`REQ-SCH-0010`](./SPEC-SCH.md#req-sch-0010-surface-evidence-coverage-views) Surface evidence coverage views
Validation tooling SHOULD be able to report which requirements have observed
evidence for the well-known and custom evidence kinds present in evaluated
evidence snapshots.

## [`REQ-SCH-0011`](./SPEC-SCH.md#req-sch-0011-constrain-trace-bearing-identifier-families) Constrain trace-bearing identifier families
The published schemas MUST constrain the direct trace-bearing lists in front matter and requirement or work-item trace blocks to the identifier family each field is meant to carry rather than accept generic non-empty strings.

## [`REQ-SCH-0012`](./SPEC-SCH.md#req-sch-0012-detect-duplicate-identifiers) Detect duplicate identifiers
Validation tooling MUST detect duplicate artifact IDs and duplicate requirement IDs in the repository.

## [`REQ-SCH-0013`](./SPEC-SCH.md#req-sch-0013-resolve-explicit-trace-references) Resolve explicit trace references
Validation tooling MUST resolve explicit trace references and report unresolved requirement or artifact references in direct trace-bearing fields.

## [`REQ-SCH-0014`](./SPEC-SCH.md#req-sch-0014-enforce-reciprocal-trace-consistency) Enforce reciprocal trace consistency
Validation tooling MUST report when a requirement link is not reciprocated by the linked architecture, work-item, or verification artifact, or when a work-item or verification document body diverges from its front matter trace lists.

## [`REQ-SCH-0015`](./SPEC-SCH.md#req-sch-0015-keep-namespaces-aligned) Keep namespaces aligned
Validation tooling MUST report when a document's front matter domain value or identifier namespace does not align with the directory namespace that owns the file.

## [`REQ-SCH-0016`](./SPEC-SCH.md#req-sch-0016-model-lineage-and-source-refs-explicitly) Model lineage and upstream refs explicitly
[`requirement-trace-fields.schema.json`](../../../schemas/requirement-trace-fields.schema.json) MUST model `Derived From` and `Supersedes` as requirement-reference arrays and `Upstream Refs` as a non-empty string array.

## [`REQ-SCH-0017`](./SPEC-SCH.md#req-sch-0017-allow-lineage-references-without-tombstones) Allow lineage references without tombstones
Validation tooling MUST accept `Derived From` and `Supersedes` references even when the referenced requirement IDs are not present as active documents in the repository.

## [`REQ-SCH-0018`](./SPEC-SCH.md#req-sch-0018-document-inline-identifier-references-in-schema-contracts) Document inline identifier references in schema contracts
The reference schemas MUST document inline identifier references as backtick-delimited canonical artifact identifiers whose full resolution may require repository-level validation beyond JSON Schema.

Trace:
- Related:
  - [SPEC-STD](./SPEC-STD.md)
  - [SPEC-TPL](./SPEC-TPL.md)
  - [SPEC-EXM](./SPEC-EXM.md)

Notes:
- Inline identifier references are prose links, not `Trace`-block fields.
- JSON Schema can describe the extracted clause shape, but it cannot fully resolve cross-file links by itself.
- Repository-level tooling can extract and validate inline references against the repository's known artifact IDs.

## [`REQ-SCH-0019`](./SPEC-SCH.md#req-sch-0019-prevent-trace-inference-from-inline-identifier-references) Prevent trace inference from inline identifier references
Validation tooling MUST NOT infer typed trace edges from inline identifier references alone.

Notes:
- Inline references can be reported as mentions, but they are not a substitute for structured trace data.

## [`REQ-SCH-0020`](./SPEC-SCH.md#req-sch-0020-extract-inline-identifier-references-when-practical) Extract inline identifier references when practical
Validation tooling SHOULD extract inline identifier references separately from structured trace fields when it can do so.

Notes:
- Separate extraction supports lightweight mention reporting without promoting prose references into graph edges.

## [`REQ-SCH-0021`](./SPEC-SCH.md#req-sch-0021-normalize-linked-identifiers-in-markdown-sections) Normalize linked identifiers in Markdown sections
Validation tooling MUST extract canonical identifiers from repo-local Markdown links, including same-document and cross-document anchor links, when the visible link text is the identifier in requirement headings and trace-bearing Markdown sections.

Trace:
- Related:
  - [SPEC-TPL](./SPEC-TPL.md)

Notes:
- This keeps clickable Markdown authoring compatible with canonical identifier extraction.
- Anchor fragments may improve navigation precision without changing the extracted identifier.
- File-level front matter may still be normalized by the extractor before schema validation when the source text contains decorated identifiers.

## [`REQ-SCH-0022`](./SPEC-SCH.md#req-sch-0022-surface-reporting-dimensions-for-downstream-tooling) Surface reporting dimensions for downstream tooling
Validation tooling SHOULD report per-repository and per-requirement coverage
for `Upstream Refs`, `Satisfied By`, `Implemented By`, `Verified By`, and the
evidence kinds present in evaluated evidence snapshots.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)
  - [SPEC-EVD](./SPEC-EVD.md)

Notes:
- These dimension reports complement the profile result instead of replacing it.
- Coverage views help teams distinguish greenfield gaps from brownfield trace debt.

## [`REQ-SCH-0023`](./SPEC-SCH.md#req-sch-0023-emit-machine-readable-coverage-summaries-when-requested) Emit machine-readable coverage summaries when requested
Validation tooling SHOULD be able to emit machine-readable summaries of coverage dimensions for dashboards, attestation tooling, and repository reporting.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)

## [`REQ-SCH-0024`](./SPEC-SCH.md#req-sch-0024-provide-a-schema-for-generated-evidence-snapshots) Provide a schema for generated evidence snapshots
[`evidence-snapshot.schema.json`](../../../schemas/evidence-snapshot.schema.json)
MUST validate generated evidence snapshots including producer metadata,
requirement IDs, evidence kinds, statuses, and optional refs.

Trace:
- Related:
  - [SPEC-EVD](./SPEC-EVD.md)

## [`REQ-SCH-0025`](./SPEC-SCH.md#req-sch-0025-allow-partial-evidence-snapshots) Allow partial evidence snapshots
Validation tooling MUST accept evidence snapshots that cover only a subset of
requirements or only a subset of evidence kinds.

Trace:
- Related:
  - [SPEC-EVD](./SPEC-EVD.md)

## [`REQ-SCH-0026`](./SPEC-SCH.md#req-sch-0026-merge-overlapping-evidence-snapshots-additively-when-reporting) Merge overlapping evidence snapshots additively when reporting
Validation tooling SHOULD merge overlapping evidence snapshots additively when
it produces repository-level or requirement-level evidence reports.

Trace:
- Related:
  - [SPEC-EVD](./SPEC-EVD.md)
  - [SPEC-RPT](./SPEC-RPT.md)

## [`REQ-SCH-0027`](./SPEC-SCH.md#req-sch-0027-avoid-negative-inference-from-single-snapshot-omission) Avoid negative inference from single-snapshot omission
Validation tooling MUST NOT infer that evidence is absent from the repository
solely because one evaluated evidence snapshot omits a requirement or evidence
kind.

Trace:
- Related:
  - [SPEC-EVD](./SPEC-EVD.md)
