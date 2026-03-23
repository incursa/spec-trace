# Overview

This file summarizes the standard described in `specs/requirements/spec-trace/`. It is a practical guide, not a second source of truth.

The core artifact families are specification, architecture, work item, and verification.

## Core Vocabulary

### Specification

A specification is a Markdown document that groups and organizes one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Each specification Markdown file contains exactly one specification. That specification carries document-level metadata and any shared framing prose, such as purpose, scope, or context.

### Requirement

A requirement is the smallest normative, testable statement in the system.

Each requirement:

- has a short descriptive title
- has a stable `REQ-...` identifier
- appears inside a specification
- is written as a compact normative clause
- may carry optional trace links and notes

A specification is not a requirement. The specification groups requirements. The requirement clause is the atomic obligation.

The title is a scan aid for the obligation. The clause states the obligation, rule, or constraint. `Notes` carry rationale, caveats, and examples.

### Traceability

Traceability is the explicit link between a requirement and the artifacts that exist because of it.

The standard gives first-class weight to links from a requirement to:

- architecture or design artifacts
- work items
- verification artifacts
- tests
- code references

The published schemas constrain the direct link families, and repository-wide validation through `scripts/Test-SpecTraceRepository.ps1` checks duplicate IDs, unresolved direct links, reciprocal consistency, namespace alignment, and profile rules.
Inline identifier references are prose mentions, not trace edges. They live outside the structured `Trace` block even when they point at canonical artifact IDs.

### Inline Identifier References

An inline identifier reference is a backtick-delimited stable artifact ID inside prose.

Inline identifier references are lightweight, human-readable, machine-detectable links. They may indicate dependency, compliance, constraint, or another explicit relation, but they do not imply inheritance or copying.

Inline identifier references are allowed in requirement clauses, `Notes`, and other descriptive sections. Use them for component conformance, token usage, and cross-spec relationships. Avoid overuse when a structured `Trace` block is a better fit.

Requirements may reference other requirements and specifications inline; architecture, work-item, and verification prose may also use inline references when the relationship is lightweight and the stable ID is enough.

### Conformance Profiles

The standard defines three repository-level conformance profiles:

- `core`: shape, identifier, and approved keyword correctness only
- `traceable`: core plus no unresolved artifact or requirement refs, no duplicate IDs, and at least one downstream trace link for every requirement
- `auditable`: traceable plus verification coverage for every requirement, reciprocal trace agreement where reciprocal fields exist, and no orphan ARC, WI, or VER artifacts

Core is the default low-burden baseline. Stricter profiles are opt-in repository policy rather than per-artifact metadata.

Use `scripts/Test-SpecTraceRepository.ps1` with `-Profile core`, `-Profile traceable`, or `-Profile auditable` to enforce those levels. Add `-JsonReportPath` when you need a machine-readable report.

### Requirement Lineage And Evolution

A requirement may also carry optional upstream trace:

- `Derived From`: earlier `REQ-...` identifiers that the requirement refines or inherits from
- `Supersedes`: earlier `REQ-...` identifiers that the requirement replaces or closes forward
- `Source Refs`: free-form external references such as laws, contracts, tickets, incidents, customer asks, and policies

These fields stay lightweight. They do not add per-requirement workflow states, and the repository does not need tombstone requirement records for retired IDs.

Editorial clarifications keep the same `REQ-...` identifier. Semantic changes, split and merge outcomes, and other new obligations get new `REQ-...` identifiers. A moved requirement may keep the same `REQ-...` identifier if its semantics do not change. Old requirement IDs are never reused.

Tests are not the requirement. Code is not the requirement. They are artifacts that may exist because of the requirement and may reference the requirement ID directly.

### Verification Artifact

A verification artifact describes how one or more requirements were verified and records the shared outcome. When a single verification artifact lists multiple requirements, they must all share that outcome; otherwise, split the scope into separate verification artifacts.

Verification artifacts may summarize verification at a higher level than individual tests. Tests may still reference requirement IDs directly.

### Work Item

A work item describes implementation work. It is not the requirement itself.

### Architecture or Design Artifact

An architecture or design artifact explains how one or more requirements will be satisfied. It is the default place for design rationale and decision tradeoffs. It is not the requirement itself. Decision records are not part of the core standard today; a repository may add them later as an optional local extension.

## Normative Keywords

Requirement clauses use BCP 14-style uppercase normative language inspired by RFC 2119 and RFC 8174, but spec-trace intentionally narrows the approved set.

Only uppercase approved forms carry normative meaning; lowercase spellings are plain English.

Requirement clauses use the following approved keywords:

- `MUST` or `SHALL`: required
- `MUST NOT` or `SHALL NOT`: prohibited
- `SHOULD`: recommended but not strictly required
- `SHOULD NOT`: recommended against but not strictly required
- `MAY`: permitted or optional

Every canonical requirement clause must contain exactly one approved keyword in all caps.

The keyword does not need to be the first word in the sentence, but it must appear in the normative clause, it must be uppercase, and no second approved keyword may appear in the same clause.

## Canonical Requirement Form

The compact requirement clause is the canonical requirement form.

```md
## REQ-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+> <Short Title>
The system MUST <direct, testable behavior>.

Trace:
- Satisfied By: ARC-...
- Implemented By: WI-...
- Verified By: VER-...
- Derived From:
  - REQ-...
- Supersedes:
  - REQ-...
- Source Refs:
  - <external reference>
- Test Refs:
  - <test reference>
- Code Refs:
  - <code reference>
- Related:
  - <artifact or requirement ID>

Notes:
- <clarification>
```

Canonical rules:

- The heading begins with `REQ-...`.
- The requirement clause immediately follows the heading.
- The clause contains exactly one approved normative keyword.
- The clause is usually a single sentence.
- `Trace` is optional.
- `Notes` is optional.
- The requirement clause is the normative content.

The reference model intentionally does not require per-requirement fields such as `Type`, `Status`, `Priority`, `Source`, or `Verification` ahead of the clause.

Repositories may add richer management metadata if they choose, but that metadata is not the canonical requirement form and must not obscure the clause itself.

## One Specification Per File

Each specification file contains:

- one specification identified by the file's `artifact_id`
- one or more related `REQ-...` clauses beneath it

Narrow technical concerns are still modeled as specifications. The standard uses one specification model for both broad and narrow concerns.

## Trace Model

When a requirement includes a `Trace` block, the canonical labels are:

- `Satisfied By`
- `Implemented By`
- `Verified By`
- `Derived From`
- `Supersedes`
- `Source Refs`
- `Test Refs`
- `Code Refs`
- `Related`

These labels map to the extracted metadata validated by the schemas in `schemas/`.

`Test Refs` and `Code Refs` are implementation-specific string references. The standard does not prescribe one syntax. Valid references may include:

- test IDs
- fully qualified test names
- repository paths
- source-file locations
- code symbols
- manifest keys or metadata values used by local tooling

Inline identifier references are separate from the `Trace` block. They use backticks around stable artifact IDs in prose, while `Trace` captures the structured relationships that tooling can extract directly.

Within `Trace`, the label families have typed meanings: `Satisfied By`, `Implemented By`, and `Verified By` are downstream links; `Derived From` and `Supersedes` are lineage; `Source Refs` are upstream source citations; `Test Refs` and `Code Refs` are implementation-specific string references; `Related` is a loose association.

## File-Level Metadata

File-level front matter remains important. It identifies the document as a whole. The schemas keep that metadata strict so tooling can classify artifacts reliably.
Repositories may add optional namespaced `x_...` front-matter keys for local extensions without changing the core artifact family.

The key distinction is this:

- front matter describes the document
- requirement clauses describe the normative behavior
- one specification file groups the related requirement clauses for that specification

## Conformance Focus

A practical implementation of the standard should make these questions answerable:

- Which requirements exist for this capability?
- Which requirements have design coverage?
- Which requirements have work items?
- Which requirements have verification coverage?
- Which requirements are directly referenced by tests?
- Which requirements are directly referenced by code?
- Which trace links are duplicated, unresolved, or not reciprocated?

If those questions cannot be answered from stable IDs and explicit links, the traceability model is too weak.
