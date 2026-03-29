# Examples

This folder contains worked examples for the standard.

The examples are intentionally cross-linked across specifications, architecture or design, work items, verification artifacts, tests, and code references. They exist to show the model in use. They are not mandatory prose.

The example set is also used as a validation target for [`scripts/Test-SpecTraceRepository.ps1`](../scripts/Test-SpecTraceRepository.ps1), including duplicate IDs, unresolved direct links, reciprocal trace consistency, namespace alignment, and profile checks.

Where it helps the narrative, the examples also show optional upstream lineage through `Derived From`, `Supersedes`, and `Source Refs`, plus inline identifier references in prose and notes.

Requirement titles in the examples are short descriptive labels for the obligation or concern; the clause carries the normative behavior and `Notes` carry the extras.

If you want the operational model explained in plain language first, read [`../artifact-model-explainer.md`](../artifact-model-explainer.md). It separates canonical downstream trace from provenance and direct implementation references.

## Inline Identifier References

Inline identifier references use backticks around stable IDs. They are lightweight links for prose when a full `Trace` block would be heavier than necessary.

Illustrative patterns:

```md
The button component MUST comply with [`REQ-UI-CONVENTIONS-0001`](./ui-design-system/SPEC-UI-CONVENTIONS.md).
Primary button styling MUST use [`REQ-UI-TOKENS-0001`](./ui-design-system/SPEC-UI-TOKENS.md).
This specification builds on [`SPEC-UI-TOKENS`](./ui-design-system/SPEC-UI-TOKENS.md).
```

This repository's worked examples use the same pattern with real IDs such as [`REQ-PAY-ACH-0013`](./payments/SPEC-PAY-ACH.md), [`REQ-TPL-0006`](../specs/requirements/spec-trace/SPEC-TPL.md), [`REQ-TPL-0007`](../specs/requirements/spec-trace/SPEC-TPL.md), and [`SPEC-TPL`](../specs/requirements/spec-trace/SPEC-TPL.md).

## Included Examples

### Payments

[`examples/payments/`](./payments/) is a product-style example built around ACH duplicate batch handling.

It demonstrates:

- a single specification file containing multiple related requirements
- compact requirement clauses
- traceability to architecture, work items, verification, tests, and code references
- optional upstream lineage and source references where the requirement history needs it
- inline identifier references in clauses, `Notes`, and descriptive prose
- verification artifacts with one shared status per artifact
- a product rule plus edge-case behavior

### Arithmetic

[`examples/arithmetic/`](./arithmetic/) is a narrow technical example built around a division operation.

It demonstrates:

- a single narrow specification file
- method-level and edge-case requirements
- stable requirement IDs that can be referenced directly from tests and code
- inline identifier references that keep shared rules visible without duplicating trace blocks
- verification artifacts with one shared status per artifact

It also shows the difference between:

- provenance in `Source Refs`
- canonical downstream trace in `Satisfied By`, `Implemented By`, and `Verified By`
- direct implementation references in `Test Refs` and `Code Refs`

That makes it the smallest worked example for the operational model.

### UI Design System

[`examples/ui-design-system/`](./ui-design-system/) is a compact design-system example built around shared conventions, tokens, and foundation rules for button and input components.

It demonstrates:

- shared specification files for naming, tokens, and interaction rules
- component specification files that reference shared requirements inline with backtick-delimited IDs instead of restating them
- inline requirement references that use backticks around stable IDs
- BEM block, element, and modifier usage in practice
- token identifiers such as `color-action-primary` and `control-height-medium`

### BEM CSS

[`examples/bem-css/`](./bem-css/) is a real BEM methodology specification set derived from the official BEM documentation.

It demonstrates:

- explicit shared definitions for blocks, elements, modifiers, and mixes
- a naming grammar with regex-like block constraints and negative rules
- class-based selectors and selector-specificity hygiene
- nested HTML elements without nested element-of-element names
- mixes for combining entities and replacing wrapper-heavy layouts
- downstream architecture and verification artifacts that keep the example fully traceable
- definition-like requirements that stay referenceable by stable IDs

## Entry Points

- [`../specs/requirements/spec-trace/`](../specs/requirements/spec-trace/) for the canonical SPEC suite
- [`artifact-id-policy.json`](../artifact-id-policy.json) for the shared identifier policy
- [`payments/SPEC-PAY-ACH.md`](./payments/SPEC-PAY-ACH.md)
- [`payments/sample-architecture.md`](./payments/sample-architecture.md)
- [`payments/sample-work-item.md`](./payments/sample-work-item.md)
- [`payments/sample-verification.md`](./payments/sample-verification.md)
- [`arithmetic/SPEC-MATH-DIV.md`](./arithmetic/SPEC-MATH-DIV.md)
- [`arithmetic/sample-architecture.md`](./arithmetic/sample-architecture.md)
- [`arithmetic/sample-work-item.md`](./arithmetic/sample-work-item.md)
- [`arithmetic/sample-verification.md`](./arithmetic/sample-verification.md)
- [`ui-design-system/README.md`](./ui-design-system/README.md)
- [`ui-design-system/SPEC-UI-CONVENTIONS.md`](./ui-design-system/SPEC-UI-CONVENTIONS.md)
- [`ui-design-system/SPEC-UI-TOKENS.md`](./ui-design-system/SPEC-UI-TOKENS.md)
- [`ui-design-system/SPEC-UI-FOUNDATION.md`](./ui-design-system/SPEC-UI-FOUNDATION.md)
- [`ui-design-system/SPEC-UI-BUTTON.md`](./ui-design-system/SPEC-UI-BUTTON.md)
- [`ui-design-system/SPEC-UI-INPUT.md`](./ui-design-system/SPEC-UI-INPUT.md)
- [`bem-css/README.md`](./bem-css/README.md)
- [`bem-css/SPEC-BEM-CONCEPTS.md`](./bem-css/SPEC-BEM-CONCEPTS.md)
- [`bem-css/SPEC-BEM-NAMING.md`](./bem-css/SPEC-BEM-NAMING.md)
- [`bem-css/SPEC-BEM-CSS.md`](./bem-css/SPEC-BEM-CSS.md)
- [`bem-css/sample-architecture.md`](./bem-css/sample-architecture.md)
- [`bem-css/sample-verification.md`](./bem-css/sample-verification.md)
