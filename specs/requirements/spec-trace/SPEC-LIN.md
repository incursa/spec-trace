---
artifact_id: SPEC-LIN
artifact_type: specification
title: Trace Lineage and Requirement Evolution
domain: spec-trace
capability: trace-lineage
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - lineage
  - traceability
  - evolution
---

# [`SPEC-LIN`](./SPEC-LIN.md) - Trace Lineage and Requirement Evolution

## Purpose

Define how requirement IDs stay stable while requirements evolve, and how the standard records lightweight upstream lineage without workflow states or mandatory tombstone records.

## Scope

This specification covers editorial clarifications, semantic changes, split and merge cases, moved requirements, ID reuse, withdrawn requirements, and the optional lineage and upstream-reference fields in requirement Trace blocks.

## Context

Requirements change over time, but traceability only stays useful if the repository can distinguish wording edits from new obligations.

## [`REQ-LIN-0001`](./SPEC-LIN.md#req-lin-0001-keep-editorial-clarifications-on-the-same-requirement-id) Keep editorial clarifications on the same requirement ID
Editorial clarifications MUST keep the same `REQ-...` identifier when the obligation does not change.

Notes:
- Clarifications include wording fixes, formatting changes, and other non-semantic edits.
- A moved requirement may keep the same identifier if its semantics do not change.

## [`REQ-LIN-0002`](./SPEC-LIN.md#req-lin-0002-assign-a-new-requirement-id-for-semantic-changes) Assign a new requirement ID for semantic changes
A semantic change MUST use a new `REQ-...` identifier.

Notes:
- New obligations, altered acceptance criteria, and changed invariants are semantic changes.

## [`REQ-LIN-0003`](./SPEC-LIN.md#req-lin-0003-assign-new-requirement-ids-for-split-and-merge-outcomes) Assign new requirement IDs for split and merge outcomes
Split and merge scenarios MUST produce new `REQ-...` identifiers for the resulting requirements.

Notes:
- Use lineage fields to preserve the relationship to the source requirement IDs.
- The active repository does not need tombstone requirement records for retired IDs.

## [`REQ-LIN-0004`](./SPEC-LIN.md#req-lin-0004-preserve-identifiers-when-a-requirement-moves-without-semantic-change) Preserve identifiers when a requirement moves without semantic change
A requirement MAY keep the same `REQ-...` identifier when it moves to another file or section and its semantics do not change.

Notes:
- File moves and section moves are editorial changes when the obligation stays the same.

## [`REQ-LIN-0005`](./SPEC-LIN.md#req-lin-0005-never-reuse-retired-requirement-ids) Never reuse retired requirement IDs
A retired `REQ-...` identifier MUST NOT be reused for a different obligation.

Notes:
- Retirement covers superseded and withdrawn identifiers.

## [`REQ-LIN-0006`](./SPEC-LIN.md#req-lin-0006-keep-upstream-trace-lightweight) Keep upstream trace lightweight
A requirement MAY use `Derived From`, `Supersedes`, and `Upstream Refs` in its Trace block to record lineage and upstream material without adding per-requirement workflow states.

Notes:
- `Derived From` records refinement lineage.
- `Supersedes` records forward replacement lineage.
- `Upstream Refs` records free-form external references such as laws, contracts, tickets, incidents, customer asks, and policies.
- `Upstream Refs` may cite whole documents or stable sub-document anchors such as sections, headings, paragraph markers, or other repository-specific source locations when local tooling supports them.
- The repository does not require tombstone requirement records for old identifiers.

## [`REQ-LIN-0007`](./SPEC-LIN.md#req-lin-0007-keep-inline-references-separate-from-trace) Keep inline references separate from Trace
Inline references MUST NOT be treated as `Derived From`, `Supersedes`, or `Upstream Refs` entries.

Notes:
- Requirements can still mention other artifact identifiers in prose using backtick-delimited inline references.
- Such mentions are lightweight links and do not establish lineage or upstream material on their own.
- Use Trace fields when the relationship needs typed, toolable semantics.
- Inline references do not imply copying, inheritance, or replacement.

## [`REQ-LIN-0008`](./SPEC-LIN.md#req-lin-0008-allow-precise-source-locators-when-coverage-reporting-needs-them) Allow precise source locators when coverage reporting needs them
A `Upstream Refs` entry MAY include document names, section labels, anchors, ranges, or other locators precise enough for source-coverage reporting.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)

Notes:
- Useful locators may point at RFC sections, paragraph ranges, sentence ranges, ticket subsections, policy clauses, or incident timelines.
- The field remains free-form so repositories can choose their own locator syntax.

## [`REQ-LIN-0009`](./SPEC-LIN.md#req-lin-0009-retire-withdrawn-requirements-without-reusing-their-identifiers) Retire withdrawn requirements without reusing their identifiers
A requirement that is withdrawn without a successor MUST retire its `REQ-...`
identifier rather than silently repurpose or reuse it.

Notes:
- Withdrawal without replacement is distinct from supersedence.
- A withdrawn identifier remains unavailable for future reuse.

## [`REQ-LIN-0010`](./SPEC-LIN.md#req-lin-0010-allow-retired-id-ledgers-without-requiring-tombstone-requirements) Allow retired-ID ledgers without requiring tombstone requirements
A repository MAY keep a retired-identifier ledger, changelog, or generated
history surface for withdrawn or superseded requirements without keeping a
mandatory tombstone requirement artifact.

Notes:
- This preserves history when teams want visibility into retired IDs.
- The core standard still does not require inactive requirement sections to
remain in the active specification files.
