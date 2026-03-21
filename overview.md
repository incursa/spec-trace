# Overview

This file summarizes the standard described in `specs/requirements/spec-trace/`. It is a practical guide, not a second source of truth.

## Core Vocabulary

### Specification

A specification is a Markdown document that groups and organizes one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Each specification Markdown file contains exactly one specification. That specification carries document-level metadata and any shared framing prose, such as purpose, scope, or context.

### Requirement

A requirement is the smallest normative, testable statement in the system.

Each requirement:

- has a stable `REQ-...` identifier
- appears inside a specification
- is written as a compact normative clause
- may carry optional trace links and notes

A specification is not a requirement. The specification groups requirements. The requirement clause is the atomic obligation.

### Traceability

Traceability is the explicit link between a requirement and the artifacts that exist because of it.

The standard gives first-class weight to links from a requirement to:

- architecture or design artifacts
- work items
- verification artifacts
- tests
- code references

Tests are not the requirement. Code is not the requirement. They are artifacts that may exist because of the requirement and may reference the requirement ID directly.

### Verification Artifact

A verification artifact describes how one or more requirements were verified and records the outcome.

Verification artifacts may summarize verification at a higher level than individual tests. Tests may still reference requirement IDs directly.

### Work Item

A work item describes implementation work. It is not the requirement itself.

### Architecture or Design Artifact

An architecture or design artifact explains how one or more requirements will be satisfied. It is not the requirement itself.

## Normative Keywords

Requirement clauses use the following normative keywords:

- `MUST` or `SHALL`: required
- `MUST NOT` or `SHALL NOT`: prohibited
- `SHOULD`: recommended but not strictly required
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

## File-Level Metadata

File-level front matter remains important. It identifies the document as a whole. The schemas keep that metadata strict so tooling can classify artifacts reliably.

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

If those questions cannot be answered from stable IDs and explicit links, the traceability model is too weak.
