---
artifact_id: SPEC-EXM
artifact_type: specification
title: Worked Examples and Traceability Coverage
domain: spec-trace
capability: worked-examples
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - examples
  - traceability
  - reference-material
---

# [`SPEC-EXM`](./SPEC-EXM.md) - Worked Examples and Traceability Coverage

## Purpose

Define what the worked examples must demonstrate so the standard feels concrete instead of ceremonial.

## Scope

This specification covers the example sets, the traceability chain they demonstrate, inline identifier reference examples, and the requirement that the examples stay aligned with the canonical templates and schemas.

## Context

Examples are where readers decide whether the standard is practical. The package therefore needs both a product-style example and a narrow technical example.

## [`REQ-EXM-0001`](./SPEC-EXM.md#req-exm-0001-provide-a-product-style-traceability-chain) Provide a product-style traceability chain
The repository MUST provide a product-style example set that includes linked specification, architecture, work-item, and verification artifacts.

## [`REQ-EXM-0002`](./SPEC-EXM.md#req-exm-0002-use-compact-requirement-clauses-in-example-specifications) Use compact requirement clauses in example specifications
Example specifications MUST use the compact requirement clause model and show
direct traceability to architecture, work-item, and verification artifacts.

## [`REQ-EXM-0003`](./SPEC-EXM.md#req-exm-0003-keep-the-payments-example-concrete-and-recognizable) Keep the payments example concrete and recognizable
The payments example MUST remain a recognizable duplicate-batch scenario that demonstrates both business-rule and edge-case requirements.

## [`REQ-EXM-0004`](./SPEC-EXM.md#req-exm-0004-include-a-narrow-technical-example) Include a narrow technical example
The repository MUST include a narrow technical example that demonstrates method-level and edge-case requirements in a single specification file.

## [`REQ-EXM-0005`](./SPEC-EXM.md#req-exm-0005-keep-examples-aligned-with-the-current-standard) Keep examples aligned with the current standard
Examples MUST stay aligned with the current templates, schemas, and identifier policy.

## [`REQ-EXM-0006`](./SPEC-EXM.md#req-exm-0006-keep-example-prose-illustrative-rather-than-canonical) Keep example prose illustrative rather than canonical
Example prose MUST remain illustrative rather than become hidden normative content outside the SPEC suite.

## [`REQ-EXM-0007`](./SPEC-EXM.md#req-exm-0007-demonstrate-upstream-trace-when-it-is-part-of-the-example) Demonstrate upstream trace when it is part of the example
Example specifications SHOULD use `Derived From`, `Supersedes`, or `Upstream Refs` when the worked example includes requirement evolution or upstream source material.

## [`REQ-EXM-0008`](./SPEC-EXM.md#req-exm-0008-demonstrate-inline-identifier-references) Demonstrate inline identifier references
Worked examples SHOULD show inline identifier references to a requirement, a specification, and a requirement that governs clause grammar or token usage.

Trace:
- Related:
  - [SPEC-STD](./SPEC-STD.md)
  - [SPEC-TPL](./SPEC-TPL.md)
  - [SPEC-SCH](./SPEC-SCH.md)

Notes:
- In this repository, concrete examples can use [`REQ-PAY-ACH-0013`](../../examples/payments/SPEC-PAY-ACH.md#req-pay-ach-0013-scope-duplicate-detection-to-the-tenant-and-batch-identifier), [`REQ-TPL-0006`](./SPEC-TPL.md#req-tpl-0006-require-bcp-14-style-uppercase-normative-keywords-in-every-clause), [`REQ-TPL-0007`](./SPEC-TPL.md#req-tpl-0007-make-the-compact-clause-the-canonical-requirement-form), and [`SPEC-TPL`](./SPEC-TPL.md).
- The examples demonstrate the pattern in requirement clauses, `Notes`, and descriptive prose without turning the example set into hidden normative content outside the SPEC suite.

## [`REQ-EXM-0009`](./SPEC-EXM.md#req-exm-0009-distinguish-derived-evidence-views-from-canonical-trace) Distinguish derived evidence views from canonical trace
Worked examples SHOULD make it obvious when a status, coverage, or attestation view is generated from the trace graph rather than authored as canonical requirement text.

Notes:
- Such views can summarize current passing tests, manual QA, benchmark health, or other repository-policy evidence sources.
- They are derived outputs, not a fifth artifact family, and they do not replace the example's specification, architecture, work-item, or verification artifacts.
- Worked examples that need direct implementation grounding SHOULD prefer
  generated evidence snapshots over hand-authored implementation-reference
  fields in requirement trace blocks.

## [`REQ-EXM-0010`](./SPEC-EXM.md#req-exm-0010-demonstrate-dimension-oriented-derived-reporting) Demonstrate dimension-oriented derived reporting
Worked examples SHOULD include at least one derived view that summarizes requirement coverage by dimension rather than only a single current-status label.

Notes:
- Useful dimensions include downstream trace, verification coverage,
  evidence-by-kind, and source coverage when the example includes source
  material.
- The view remains derived output and does not become a fifth artifact family.
