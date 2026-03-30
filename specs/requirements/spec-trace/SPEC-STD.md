---
artifact_id: SPEC-STD
artifact_type: specification
title: Core Standard Model and Publication Rules
domain: spec-trace
capability: core-standard-model
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - standard
  - traceability
  - governance
---

# [`SPEC-STD`](./SPEC-STD.md) - Core Standard Model and Publication Rules

## Purpose

Define the core vocabulary, authority model, and publication rules for the spec-trace standard.

## Scope

This specification covers what a specification is, what a requirement is,
where the canonical standard lives, how the reference package keeps its
support material aligned, how the canonical conformance profiles are named, how
derived reporting and generated evidence stay non-canonical, and how inline
identifier references and structured trace differ.

## Context

The repository exists to make precise software requirements easy to author, review, and trace in plain Markdown. That goal only works if the requirement itself stays short and direct, and the repository also needs a low-burden baseline so teams can adopt the standard without committing to a stricter traceability policy on day one.

## [`REQ-STD-0001`](./SPEC-STD.md#req-std-0001-distinguish-specifications-from-requirements) Distinguish specifications from requirements
A specification MUST group one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Notes:
- A specification carries document-level context. It is not the smallest normative unit.

## [`REQ-STD-0002`](./SPEC-STD.md#req-std-0002-treat-the-requirement-as-the-atomic-normative-unit) Treat the requirement as the atomic normative unit
A requirement MUST be the smallest normative, testable statement in the system.

Notes:
- The requirement clause is the normative content.
- The standard uses BCP 14-style uppercase requirement language inspired by RFC 2119 and RFC 8174, but only the approved uppercase keyword set in [`SPEC-TPL`](./SPEC-TPL.md) carries defined normative meaning.
- Lowercase spellings are plain English.

## [`REQ-STD-0014`](./SPEC-STD.md#req-std-0014-give-each-requirement-a-stable-identifier) Give each requirement a stable identifier
Each canonical requirement MUST have a stable `REQ-...` identifier.

## [`REQ-STD-0003`](./SPEC-STD.md#req-std-0003-keep-the-canonical-standard-in-the-spec-suite) Keep the canonical standard in the SPEC suite
The `spec-trace` standard MUST be expressed as the canonical SPEC suite under [`specs/requirements/spec-trace/`](./).

Trace:
- Related:
  - [SPEC-ID](./SPEC-ID.md)
  - [SPEC-LIN](./SPEC-LIN.md)
  - [SPEC-PRF](./SPEC-PRF.md)
  - [SPEC-RPT](./SPEC-RPT.md)
  - [SPEC-EVD](./SPEC-EVD.md)
  - [SPEC-LAY](./SPEC-LAY.md)
  - [SPEC-TPL](./SPEC-TPL.md)
  - [SPEC-SCH](./SPEC-SCH.md)
  - [SPEC-EXM](./SPEC-EXM.md)

## [`REQ-STD-0004`](./SPEC-STD.md#req-std-0004-keep-root-guidance-subordinate-to-the-spec-suite) Keep root guidance subordinate to the SPEC suite
[`README.md`](../../../README.md), [`overview.md`](../../../overview.md), [`layout.md`](../../../layout.md), the root templates, the schemas, and the examples MUST align with the SPEC suite.

## [`REQ-STD-0015`](./SPEC-STD.md#req-std-0015-keep-root-guidance-non-authoritative) Keep root guidance non-authoritative
[`README.md`](../../../README.md), [`overview.md`](../../../overview.md), [`layout.md`](../../../layout.md), the root templates, the schemas, and the examples MUST NOT override the SPEC suite.

## [`REQ-STD-0005`](./SPEC-STD.md#req-std-0005-make-traceability-a-first-class-goal) Make traceability a first-class goal
The standard MUST support explicit links from each requirement to relevant
design, work-item, verification, and upstream-origin material.

Notes:
- Direct implementation evidence may still be reported through generated
  evidence snapshots.

## [`REQ-STD-0021`](./SPEC-STD.md#req-std-0021-treat-inline-identifier-references-as-lightweight-links) Treat inline identifier references as lightweight links
The standard MUST treat a backtick-delimited canonical artifact identifier in prose as a lightweight, human-readable, machine-detectable inline identifier reference.

Trace:
- Related:
  - [SPEC-TPL](./SPEC-TPL.md)
  - [SPEC-SCH](./SPEC-SCH.md)
  - [SPEC-EXM](./SPEC-EXM.md)

Notes:
- An inline identifier reference can indicate that the current artifact, requirement, or descriptive passage depends on, complies with, is constrained by, or otherwise explicitly relates to the referenced artifact.
- Inline identifier references do not imply inheritance or copying.
- Inline identifier references do not replace `Trace` fields or lineage labels such as `Derived From` and `Supersedes`.
- Inline identifier references can appear in requirement clauses, `Notes`, and other descriptive sections.
- They are useful for component conformance, token usage, and cross-spec relationships when a full trace block would be heavier than necessary.

## [`REQ-STD-0022`](./SPEC-STD.md#req-std-0022-keep-inline-identifier-references-separate-from-structured-trace) Keep inline identifier references separate from structured trace
The standard MUST treat an inline identifier reference as a prose reference rather than a structured trace edge, lineage record, or inferred relationship type.

Notes:
- The surrounding prose carries any additional meaning.
- A backtick-delimited identifier alone does not encode dependency, inheritance, satisfaction, implementation, or verification.
- Inline identifier references do not replace `Trace` fields.

## [`REQ-STD-0006`](./SPEC-STD.md#req-std-0006-treat-generated-outputs-as-derived-material) Treat generated outputs as derived material
Generated indexes, traceability matrices, and coverage reports MUST be treated as derived outputs rather than canonical requirements.

Notes:
- This includes current-status views, evidence rollups, coverage dashboards, and attestation snapshots that answer questions such as which requirements currently have passing tests, failing tests, benchmark regressions, stale manual QA, or open implementation work.
- These views are useful derived outputs, but they are not requirement text, and they do not become source artifacts just because they summarize the canonical graph.

## [`REQ-STD-0007`](./SPEC-STD.md#req-std-0007-update-canonical-surfaces-together) Update canonical surfaces together
A change to a canonical field, identifier rule, template shape, schema contract, or example pattern MUST update the affected specs, templates, schemas, and examples in the same change set.

## [`REQ-STD-0008`](./SPEC-STD.md#req-std-0008-use-the-reference-repository-as-a-proving-ground) Use the reference repository as a proving ground
The reference repository MUST use the standard to specify itself under [`specs/requirements/spec-trace/`](./).

## [`REQ-STD-0009`](./SPEC-STD.md#req-std-0009-keep-canonical-requirements-inside-specifications) Keep canonical requirements inside specifications
Each specification Markdown file MUST contain one specification.

## [`REQ-STD-0016`](./SPEC-STD.md#req-std-0016-keep-canonical-requirements-inside-specifications) Keep canonical requirements inside specifications
A canonical requirement MUST appear inside its specification document rather than stand alone as an unlabeled prose fragment.

## [`REQ-STD-0010`](./SPEC-STD.md#req-std-0010-distinguish-work-items-from-requirements) Distinguish work items from requirements
A work item MUST describe implementation work rather than normative requirement text.

Notes:
- A work item is a delivery record, not the requirement itself.

## [`REQ-STD-0011`](./SPEC-STD.md#req-std-0011-distinguish-architecture-from-requirements) Distinguish architecture from requirements
An architecture or design artifact MUST explain how one or more requirements are satisfied rather than replace those requirements.

Notes:
- Architecture is intent and design, not the requirement clause.
- Architecture artifacts are optional design explanations; upstream RFCs, tickets, and other source materials belong in `Upstream Refs` when they motivate the requirement.

## [`REQ-STD-0012`](./SPEC-STD.md#req-std-0012-distinguish-verification-artifacts-and-tests-from-requirements) Distinguish verification artifacts and tests from requirements
A verification artifact MUST record how one or more requirements were verified.

Notes:
- `Verified By` means the requirement is covered by one or more verification artifacts.
- It does not by itself mean formal proof of program correctness.
- The rigor of verification depends on repository practice and local policy.
- Verification artifacts are usually more useful as proof-summary artifacts than as hand-maintained duplicate test catalogs.
- A repository may use tests, manual QA, benchmarks, interoperability runs, security review, fuzzing, formal methods, or other evidence sources according to local policy when it records verification.
- Verification artifacts may cite one or more generated evidence snapshots as
  proof inputs.

## [`REQ-STD-0019`](./SPEC-STD.md#req-std-0019-keep-verification-outcomes-homogeneous-within-one-artifact) Keep verification outcomes homogeneous within one artifact
A verification artifact MUST only list requirements in `verifies` when the artifact status applies to every listed requirement; mixed outcomes belong in separate verification artifacts.

## [`REQ-STD-0020`](./SPEC-STD.md#req-std-0020-define-canonical-conformance-profiles) Define canonical conformance profiles
The standard MUST define `core`, `traceable`, and `auditable` as its canonical conformance profiles.

Trace:
- Related:
  - [SPEC-PRF](./SPEC-PRF.md)

Notes:
- `core` is the low-burden baseline.
- `traceable` and `auditable` are stricter optional repository profiles.
- Profiles are repository-level enforcement targets, not per-artifact metadata.
- Explanatory shorthand such as spec-valid, artifact-linked, and evidence-backed does not replace the canonical profile names.

## [`REQ-STD-0013`](./SPEC-STD.md#req-std-0013-distinguish-code-references-from-requirements) Distinguish code references from requirements
Generated evidence snapshots MAY link discovered tests to requirement
identifiers without becoming the requirements themselves.

## [`REQ-STD-0017`](./SPEC-STD.md#req-std-0017-distinguish-code-references-from-requirements) Distinguish code references from requirements
Generated evidence snapshots MAY link discovered code locations to requirement
identifiers without becoming the requirements themselves.

## [`REQ-STD-0018`](./SPEC-STD.md#req-std-0018-keep-code-non-normative) Keep code non-normative
Source code and code comments MUST NOT be treated as the normative requirement text.

## [`REQ-STD-0023`](./SPEC-STD.md#req-std-0023-support-staged-adoption-without-synthetic-history) Support staged adoption without synthetic history
The standard MUST support staged adoption without requiring synthetic architecture artifacts, historical work items, or hand-maintained verification matrices that do not add repository truth.

Notes:
- Greenfield repositories may start with requirements and source lineage before implementation exists.
- Brownfield repositories may ground requirements directly through evidence
  snapshots and verification evidence without reconstructing delivery history.

## [`REQ-STD-0024`](./SPEC-STD.md#req-std-0024-keep-local-attestation-states-derived-rather-than-canonical) Keep local attestation states derived rather than canonical
Local states such as implemented, verified, and release-ready MUST be treated as derived repository-policy outputs rather than canonical requirement states.

Trace:
- Related:
  - [SPEC-PRF](./SPEC-PRF.md)
  - [SPEC-RPT](./SPEC-RPT.md)

Notes:
- Canonical requirement text says what must be true.
- Repository policy and evidence reporting say what appears true right now.

## [`REQ-STD-0025`](./SPEC-STD.md#req-std-0025-keep-generated-evidence-separate-from-canonical-artifact-families) Keep generated evidence separate from canonical artifact families
The standard MUST define generated evidence snapshots as derived outputs rather
than as a fifth canonical artifact family.

Trace:
- Related:
  - [SPEC-EVD](./SPEC-EVD.md)
  - [SPEC-RPT](./SPEC-RPT.md)

Notes:
- Evidence snapshots complement requirements and verification artifacts.
- Evidence snapshots do not replace the canonical specification,
  architecture, work-item, or verification artifact families.
