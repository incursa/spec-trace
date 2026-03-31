---
artifact_id: SPEC-EVD
artifact_type: specification
title: Generated Evidence Snapshots
domain: spec-trace
capability: generated-evidence
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - evidence
  - reporting
  - generated
---

# [`SPEC-EVD`](./SPEC-EVD.md) - Generated Evidence Snapshots

## Purpose

Define the generated evidence snapshot model used for point-in-time proof and
attestation reporting without turning that evidence into canonical requirement
text.

## Scope

This specification covers evidence snapshot shape, well-known evidence kinds,
evidence statuses, partial collection semantics, overlap and merge behavior,
and the rule that discovery conventions remain tool-specific rather than
canonical.

## Context

Repositories often need machine-produced evidence from multiple tools. Unit
tests, benchmarks, static analyzers, code-mapping tools, and manual evidence
imports may all contribute to the same requirement set. The standard therefore
needs a shared output contract for evidence without dictating how every
language, framework, or build system discovers it.

## [`REQ-EVD-0001`](./SPEC-EVD.md#req-evd-0001-keep-evidence-snapshots-derived-rather-than-canonical) Keep evidence snapshots derived rather than canonical
Generated evidence snapshots MUST be treated as derived outputs rather than as
canonical requirement artifacts or a fifth canonical artifact family.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)
  - [SPEC-STD](./SPEC-STD.md)

Notes:
- Requirements remain the canonical statement of intent.
- Evidence snapshots record point-in-time observations about repository truth.

## [`REQ-EVD-0002`](./SPEC-EVD.md#req-evd-0002-identify-evidence-snapshot-provenance) Identify evidence snapshot provenance
An evidence snapshot MUST record a stable snapshot identifier, generation
timestamp, and producer identity.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- Producer identity usually includes a tool name and version.
- Repositories may add commit hashes, branch names, workspace paths, or other
namespaced local metadata.

## [`REQ-EVD-0003`](./SPEC-EVD.md#req-evd-0003-record-requirement-scoped-observations) Record requirement-scoped observations
An evidence snapshot MUST record observations by `REQ-...` identifier and
evidence kind, with one status per observation entry.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- Observation entries may also carry free-form refs, summaries, or namespaced
local metadata.

## [`REQ-EVD-0004`](./SPEC-EVD.md#req-evd-0004-keep-evidence-kinds-extensible-and-machine-friendly) Keep evidence kinds extensible and machine-friendly
An evidence kind MUST use a lowercase token that starts with a letter and then
uses only lowercase letters, digits, or underscores.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- This keeps the kind easy to transport across tools and JSON outputs.

## [`REQ-EVD-0005`](./SPEC-EVD.md#req-evd-0005-define-well-known-evidence-kinds-without-closing-extension) Define well-known evidence kinds without closing extension
The standard MUST define well-known evidence kinds while allowing repositories
and tools to emit additional kinds.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- Well-known kinds include `unit_test`, `integration_test`, `manual_test`,
`benchmark`, `fuzz`, `security_scan`, `code_ref`, and `source_coverage`.
- Custom kinds remain valid when they follow the canonical token pattern.

## [`REQ-EVD-0006`](./SPEC-EVD.md#req-evd-0006-keep-evidence-statuses-canonical-and-finite) Keep evidence statuses canonical and finite
An evidence observation status MUST use one of `observed`, `passed`, `failed`,
`not_observed`, `not_collected`, or `unsupported`.

Trace:
- Related:
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- `not_observed` means the tool checked and did not find matching evidence.
- `not_collected` means the snapshot does not claim collection for that case.
- `unsupported` means the producer cannot collect that evidence kind in the
current environment or stack.

## [`REQ-EVD-0007`](./SPEC-EVD.md#req-evd-0007-allow-partial-evidence-collection) Allow partial evidence collection
An evidence snapshot MAY cover only a subset of requirements or only a subset
of evidence kinds.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)

Notes:
- Partial collection is normal when different tools own different evidence
kinds.

## [`REQ-EVD-0008`](./SPEC-EVD.md#req-evd-0008-merge-overlapping-evidence-snapshots-additively) Merge overlapping evidence snapshots additively
Derived reporting MUST treat multiple evidence snapshots as additive inputs
rather than require one combined master evidence file.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)

Notes:
- A repository may carry separate snapshots for unit tests, benchmarks, manual
QA, or other sources.
- Equivalent observations may be deduplicated during reporting, but the
standard does not require one universal deduplication formula.

## [`REQ-EVD-0009`](./SPEC-EVD.md#req-evd-0009-avoid-negative-inference-from-snapshot-absence) Avoid negative inference from snapshot absence
Reporting MUST NOT infer that evidence is absent from the repository solely
because one evidence snapshot omits a requirement or evidence kind.

Trace:
- Related:
  - [SPEC-RPT](./SPEC-RPT.md)

Notes:
- Omission from one snapshot means that snapshot made no claim.
- Reporting may still conclude that evidence is absent when all relevant
snapshots for the evaluated scope explicitly support that conclusion.

## [`REQ-EVD-0010`](./SPEC-EVD.md#req-evd-0010-keep-discovery-conventions-outside-the-core-standard) Keep discovery conventions outside the core standard
The core standard MUST NOT prescribe one universal discovery convention for how
tools extract evidence from programming languages, frameworks, or repositories.

Trace:
- Related:
  - [SPEC-STD](./SPEC-STD.md)

Notes:
- Tooling may use language-specific comments, attributes, manifests, naming
conventions, static analysis, metadata files, or other local mechanisms.
- The core standard defines the output contract, not the extraction algorithm.
