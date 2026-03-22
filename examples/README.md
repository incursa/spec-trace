# Examples

This folder contains worked examples for the standard.

The examples are intentionally cross-linked across specifications, architecture or design, work items, verification artifacts, tests, and code references. They exist to show the model in use. They are not mandatory prose.

The example set is also used as a validation target for `scripts/Test-SpecTraceRepository.ps1`, including duplicate IDs, unresolved direct links, reciprocal trace consistency, namespace alignment, and profile checks.

Where it helps the narrative, the examples also show optional upstream lineage through `Derived From`, `Supersedes`, and `Source Refs`.

## Included Examples

### Payments

`examples/payments/` is a product-style example built around ACH duplicate batch handling.

It demonstrates:

- a single specification file containing multiple related requirements
- compact requirement clauses
- traceability to architecture, work items, verification, tests, and code references
- optional upstream lineage and source references where the requirement history needs it
- verification artifacts with one shared status per artifact
- a product rule plus edge-case behavior

### Arithmetic

`examples/arithmetic/` is a narrow technical example built around a division operation.

It demonstrates:

- a single narrow specification file
- method-level and edge-case requirements
- stable requirement IDs that can be referenced directly from tests and code
- verification artifacts with one shared status per artifact

## Entry Points

- `../specs/requirements/spec-trace/` for the canonical SPEC suite
- [artifact-id-policy.json](../artifact-id-policy.json) for the shared identifier policy
- [payments/SPEC-PAY-ACH.md](payments/SPEC-PAY-ACH.md)
- [payments/sample-architecture.md](payments/sample-architecture.md)
- [payments/sample-work-item.md](payments/sample-work-item.md)
- [payments/sample-verification.md](payments/sample-verification.md)
- [arithmetic/SPEC-MATH-DIV.md](arithmetic/SPEC-MATH-DIV.md)
- [arithmetic/sample-architecture.md](arithmetic/sample-architecture.md)
- [arithmetic/sample-work-item.md](arithmetic/sample-work-item.md)
- [arithmetic/sample-verification.md](arithmetic/sample-verification.md)
