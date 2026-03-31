---
artifact_id: SPEC-RPT
artifact_type: specification
title: Derived Reporting Dimensions and Attestation Views
domain: spec-trace
capability: derived-reporting
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - reporting
  - attestation
  - coverage
---

# [`SPEC-RPT`](./SPEC-RPT.md) - Derived Reporting Dimensions and Attestation Views

## Purpose

Define how repositories can generate useful coverage, progress, and attestation reporting from the canonical trace graph without turning that reporting into a new artifact family or overloading the conformance profiles.

## Scope

This specification covers reporting dimensions, staged adoption semantics,
source-coverage reporting, evidence-kind reporting, local policy attestation
states, and the rule that current-state evidence remains derived from
repository truth rather than hand-maintained canonical status fields.

## Context

Teams need to answer different questions at different stages of delivery. Greenfield repositories may have good requirements before they have implementation. Brownfield repositories may have tests and code before they have work-item history. The standard therefore needs a reporting layer that stays grounded in the canonical graph and real evidence without forcing every repository into the same maturity narrative.

## [`REQ-RPT-0001`](./SPEC-RPT.md#req-rpt-0001-keep-derived-reporting-separate-from-canonical-trace) Keep derived reporting separate from canonical trace
Coverage, attestation, and current-state status views MUST be treated as derived reporting over the canonical repository graph and evidence sources rather than as new canonical requirement metadata.

Notes:
- Derived reporting answers current-state questions without changing the requirement text.
- Derived reporting does not create a fifth artifact family.

## [`REQ-RPT-0002`](./SPEC-RPT.md#req-rpt-0002-report-coverage-dimensions-independently-of-profile-choice) Report coverage dimensions independently of profile choice
Reporting SHOULD be able to surface upstream, design, implementation,
verification, and evidence coverage by kind independently of the repository's
chosen conformance profile.

Notes:
- The same repository may run at `core` while still reporting rich coverage dimensions.
- These dimensions are reporting views, not new canonical profiles.

## [`REQ-RPT-0003`](./SPEC-RPT.md#req-rpt-0003-support-staged-adoption-across-repository-histories) Support staged adoption across repository histories
The standard MUST support staged adoption in both greenfield and brownfield repositories without requiring every reporting dimension to be complete from day one.

Notes:
- A new project may have valid requirements before it has code or tests.
- A legacy project may have code and tests before it has architecture artifacts or historical work items.

## [`REQ-RPT-0004`](./SPEC-RPT.md#req-rpt-0004-avoid-synthetic-work-history-as-an-implementation-proxy) Avoid synthetic work history as an implementation proxy
Reporting and repository policy MUST NOT infer that a requirement is unimplemented solely because it lacks an `Implemented By` work-item link.

Notes:
- Brownfield repositories often have real implementation with no historical work-item trail.
- `Implemented By` remains useful delivery trace, but it is not the only possible grounding signal.

## [`REQ-RPT-0005`](./SPEC-RPT.md#req-rpt-0005-keep-local-attestation-states-policy-derived) Keep local attestation states policy-derived
Repositories MAY derive local attestation states such as implemented, verified,
and release-ready from their own combination of trace coverage, verification
artifacts, evidence snapshots, freshness rules, and execution results.

Notes:
- These terms remain repository policy unless a repository standardizes them locally.
- The canonical standard defines the trace model, not one universal release formula.

## [`REQ-RPT-0006`](./SPEC-RPT.md#req-rpt-0006-keep-evidence-freshness-and-health-derived) Keep evidence freshness and health derived
Evidence freshness, stale manual QA, failing tests, benchmark regressions, and similar health signals MUST remain derived report states or local extension metadata rather than canonical requirement states.

Notes:
- Repositories may still record manual QA, benchmarks, fuzzing, or other evidence in verification artifacts.
- Freshness windows and status rollups are local policy concerns.

## [`REQ-RPT-0007`](./SPEC-RPT.md#req-rpt-0007-support-source-coverage-reporting-when-locators-are-precise) Support source-coverage reporting when locators are precise
Reporting SHOULD be able to surface which source materials or source regions are represented by requirements when `Upstream Refs` entries are precise enough to support that view.

Notes:
- Useful locators may include RFC sections, anchors, paragraph ranges, sentence ranges, ticket subsections, or policy clauses.
- Source-coverage reporting remains derived from the canonical requirements and their `Upstream Refs`.

## [`REQ-RPT-0008`](./SPEC-RPT.md#req-rpt-0008-ground-reporting-in-real-repository-evidence) Ground reporting in real repository evidence
Derived reporting MUST be able to use verification artifacts, generated
evidence snapshots, and other repository-policy evidence sources as grounding
signals for attestation views.

Notes:
- Verification artifacts summarize proof activity.
- Evidence snapshots provide tool-produced direct implementation or execution
grounding even when supporting documents are sparse.

## [`REQ-RPT-0009`](./SPEC-RPT.md#req-rpt-0009-distinguish-missing-evidence-from-uncollected-evidence) Distinguish missing evidence from uncollected evidence
Derived reporting SHOULD distinguish missing evidence from uncollected,
unsupported, or stale evidence rather than collapse those cases into one
undifferentiated gap state.

Notes:
- A requirement may lack `unit_test` evidence because no matching test exists.
- A requirement may also lack current `benchmark` evidence because no benchmark
snapshot was collected for the evaluated scope.
- These are different reporting outcomes even when neither one satisfies a
local policy gate.
