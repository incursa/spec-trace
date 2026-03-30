# Requirement Gaps

This tracker records unresolved requirement questions before implementation work begins.
It is a convenience surface only and does not override the canonical SPEC suite under [`specs/requirements/spec-trace/`](./).

Use this file when a request touches behavior, acceptance criteria, or traceability and the requirements are incomplete, ambiguous, conflicting, or missing.

## How To Use

1. Record the gap before implementation work starts.
2. Link the gap to the relevant `SPEC-...` or `REQ-...` identifiers when they already exist.
3. Close the gap only after the missing requirement, clarification, or design decision is captured in canonical artifacts.

## Gap Types

- Missing requirement
- Ambiguous acceptance criteria
- Conflicting statements
- Missing edge case
- Missing validation rule
- Missing failure behavior
- Missing external constraint
- Unclear terminology
- Unresolved scope boundary
- Assumption that needs confirmation

## Open Gaps

| ID | Area | Question | Impact | Next Step | Status |
| --- | --- | --- | --- | --- | --- |

## Closed Gaps

| ID | Resolution | Closed By | Notes |
| --- | --- | --- | --- |
| GAP-2026-03-30-001 | Separated repository conformance gates from multidimensional derived reporting and attestation semantics. | [`SPEC-RPT`](./SPEC-RPT.md) | Profiles remain canonical gates; coverage and evidence views become derived reporting dimensions. |
| GAP-2026-03-30-002 | Clarified staged adoption semantics so greenfield and brownfield repositories can use the standard without synthetic history. | [`REQ-STD-0023`](./SPEC-STD.md#req-std-0023-support-staged-adoption-without-synthetic-history) | Missing work-item history no longer implies missing implementation. |
| GAP-2026-03-30-003 | Clarified that local states such as implemented, verified, and release-ready are derived repository-policy terms rather than canonical requirement states. | [`REQ-STD-0024`](./SPEC-STD.md#req-std-0024-keep-local-attestation-states-derived-rather-than-canonical) | Current-state attestation remains derived from trace, evidence, and policy. |
| GAP-2026-03-30-004 | Tightened source-material handling so `Upstream Refs` can carry precise locators for source-coverage reporting. | [`REQ-LIN-0008`](./SPEC-LIN.md#req-lin-0008-allow-precise-source-locators-when-coverage-reporting-needs-them) | Supports RFC- and source-document coverage views without adding a new artifact family. |
| GAP-2026-03-30-005 | Closed the extraction mismatch around clickable linked identifiers in Markdown headings and trace-bearing sections. | [`REQ-TPL-0031`](./SPEC-TPL.md#req-tpl-0031-allow-clickable-canonical-identifiers-in-markdown-body-sections), [`REQ-SCH-0021`](./SPEC-SCH.md#req-sch-0021-normalize-linked-identifiers-in-markdown-sections) | Tooling now normalizes repo-local Markdown links whose visible text is a canonical ID. |
| GAP-2026-03-30-006 | Renamed upstream source citations to `Upstream Refs` so the canonical lineage field is not confused with source-code references. | [`REQ-LIN-0006`](./SPEC-LIN.md#req-lin-0006-keep-upstream-trace-lightweight), [`REQ-TPL-0008`](./SPEC-TPL.md#req-tpl-0008-keep-the-requirement-trace-block-small-and-explicit) | Reduces ambiguity between upstream requirement origin and code-level evidence. |
| GAP-2026-03-30-007 | Moved test and code observations out of canonical requirement trace into generated evidence snapshots. | [`SPEC-EVD`](./SPEC-EVD.md), [`REQ-TPL-0016`](./SPEC-TPL.md#req-tpl-0016-keep-generated-evidence-outside-the-canonical-requirement-trace) | Canonical requirements stay authored; tool-produced implementation evidence becomes derived output. |
| GAP-2026-03-30-008 | Defined generated evidence snapshot shape, evidence-kind tokens, partial collection semantics, and additive merge behavior. | [`SPEC-EVD`](./SPEC-EVD.md), [`REQ-SCH-0024`](./SPEC-SCH.md#req-sch-0024-provide-a-schema-for-generated-evidence-snapshots) | Supports multiple overlapping evidence files without forcing one combined master file. |
| GAP-2026-03-30-009 | Tightened requirement retirement semantics for withdrawn-without-replacement IDs. | [`REQ-LIN-0009`](./SPEC-LIN.md#req-lin-0009-retire-withdrawn-requirements-without-reusing-their-identifiers), [`REQ-LIN-0010`](./SPEC-LIN.md#req-lin-0010-allow-retired-id-ledgers-without-requiring-tombstone-requirements) | Split, merge, replacement, and withdrawal now have clearer retired-ID handling. |
