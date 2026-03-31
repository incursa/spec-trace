# Profiles And Attestation Explainer

This file is a non-authoritative reading guide for the `spec-trace` reference repository. The canonical standard lives in [`./specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) as JSON artifacts validated by JSON Schema. If this file disagrees with the SPEC suite, the SPEC suite wins.

Use this guide when you want the practical distinction between:

- the canonical static trace model
- generated evidence, coverage, and attestation reporting
- local repository policy terms such as implemented, verified, and release-ready

## Canonical Artifact Roles

The core artifact families have different jobs:

- `specification` and `requirement` say what must be true
- `architecture` says how the requirement is intended to be satisfied
- `work_item` says what implementation work is or was done
- `verification` says what proof activity covered the requirement set and what shared outcome was recorded

None of those artifact families replaces the requirement text itself.

## Canonical Trace And Evidence

The canonical downstream trace graph uses:

- `Satisfied By`
- `Implemented By`
- `Verified By`

The lineage and provenance fields are different:

- `Derived From` and `Supersedes` are lineage
- `Upstream Refs` are upstream provenance or source material

Generated evidence is different again. Evidence snapshots may record `unit_test`, `code_ref`, `manual_test`, `benchmark`, `fuzz`, or other repository-policy observations, but they are derived outputs rather than part of the canonical downstream trace graph.

## Profiles In Practice

The canonical profile names are `core`, `traceable`, and `auditable`.

Helpful shorthand:

- `core` means `spec-valid`
- `traceable` means `artifact-linked`
- `auditable` means `evidence-backed`

Those phrases are explanatory only. They do not replace the canonical profile names.

Practical reading:

- `core` means the repository has structurally valid requirements and valid identifiers
- `traceable` means the requirement graph has downstream artifact links and no unresolved graph references
- `auditable` means the requirement graph has verification coverage and internal consistency

## What Verified Means

`Verified By` means a requirement is covered by one or more verification artifacts in the authored trace graph.

That does not automatically mean:

- formal correctness
- a permanently green status
- a release gate used by every repository
- that every implementation detail has been hand-cataloged in the requirement

Live status comes from derived evidence reporting, not from the canonical requirement text.

## Incremental Adoption

Repositories can adopt the model incrementally.

Start with:

- well-formed requirements
- stable requirement IDs
- clear specification boundaries

Then add, as they become useful:

- `Satisfied By`, `Implemented By`, and `Verified By` links
- generated evidence snapshots
- architecture artifacts
- work items
- verification artifacts that actually add value

In this reference repository, the practical entry points are:

- [`./scripts/Validate-SpecTraceEvidence.ps1`](./scripts/Validate-SpecTraceEvidence.ps1) to vet one or more `*.evidence.json` snapshots against JSON Schema and the repository requirement catalog
- [`./scripts/Render-SpecTraceAttestation.ps1`](./scripts/Render-SpecTraceAttestation.ps1) to generate deterministic `index.html`, `summary.html`, `details.html`, per-spec pages, and `attestation.json`

## Smallest End-To-End Example

If you want the smallest concrete chain, read [`./examples/arithmetic/SPEC-MATH-DIV.json`](./examples/arithmetic/SPEC-MATH-DIV.json), its linked architecture, work-item, and verification artifacts, and the generated rollups under [`./examples/arithmetic/generated/`](./examples/arithmetic/generated/).
