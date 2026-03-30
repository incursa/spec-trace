# Profiles And Attestation Explainer

This file is a non-authoritative reading guide for the `spec-trace` reference repository. The canonical standard lives in [`./specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). If this file disagrees with the SPEC suite, the SPEC suite wins.

Use this guide when you want the practical distinction between:

- the canonical static trace model
- generated evidence, coverage, and attestation reporting
- local repository policy terms such as implemented, verified, and release-ready

## Canonical Artifact Roles

The core artifact families have different jobs:

- `specification` and `requirement` say what must be true.
- `architecture` says how the requirement or stable technical concern is intended to be satisfied.
- `work item` says what implementation work is or was done.
- `verification` says what proof activity covered the requirement set and what shared outcome was recorded.

Those roles are intentionally distinct.

- A requirement is the normative obligation.
- An architecture artifact explains the intended satisfaction path.
- A work item records delivery work.
- A verification artifact summarizes proof coverage and the shared result.

None of those artifact families replaces the requirement text itself.

## Canonical Trace And Evidence

The canonical downstream trace graph uses:

- `Satisfied By`
- `Implemented By`
- `Verified By`

The lineage and provenance fields are different:

- `Derived From` and `Supersedes` are lineage.
- `Upstream Refs` are upstream provenance or source material.

Generated evidence is different again. Evidence snapshots may record
`unit_test`, `code_ref`, `manual_test`, `benchmark`, `fuzz`, or other
repository-policy observations, but they are derived outputs rather than part
of the canonical downstream trace graph. Inline identifier references in prose
are also not trace edges; they are lightweight mentions.

## Static Trace Versus Dynamic Attestation

The canonical standard defines the authored trace structure.

That structure is static in the sense that it is written into the repository as documents, fields, and links:

- requirements say what must be true
- architecture, work items, and verification artifacts link back to those requirements
- generated evidence snapshots may point at tests, code, or other observations
  that implement or exercise the requirement

A repository may also generate dynamic evidence or attestation reports from that static graph.

Those reports answer current-state questions such as:

- which requirements currently have passing tests
- which requirements currently have failing tests
- which requirements have benchmark regressions
- which requirements have recent or stale manual QA evidence
- which requirements have open or closed implementation work

These reports are useful, but they are derived outputs. They are not canonical requirement artifacts, and they do not create a fifth artifact family.

## Reporting Dimensions

Derived reporting can expose several dimensions at once instead of collapsing everything into one gate result.

Useful dimensions often include:

- source coverage through `Upstream Refs`
- design coverage through `Satisfied By`
- implementation-work coverage through `Implemented By`
- verification-artifact coverage through `Verified By`
- evidence-by-kind coverage such as `unit_test`, `code_ref`, `benchmark`, or
  `manual_test`

These dimensions help teams interpret the same repository differently:

- in greenfield work, missing implementation or test grounding may mean planned work
- in brownfield work, missing work-item history may just mean the implementation predates SpecTrace adoption

## Profiles In Practice

The canonical profile names are `core`, `traceable`, and `auditable`.

Helpful shorthand:

- `core` means `spec-valid`.
- `traceable` means `artifact-linked`.
- `auditable` means `evidence-backed`.

Those phrases are explanatory only. They do not replace the canonical profile names.

Practical reading:

- `core` means the repository has structurally valid requirements and valid identifiers.
- `traceable` means the requirement graph has downstream artifact links and no unresolved graph references.
- `auditable` means the requirement graph has verification coverage and internal consistency.

These profiles are repository-level conformance gates, not maturity scores and not workflow stages.

`auditable` does not mean formal proof of program correctness. It means the repository has recorded verification coverage and graph consistency according to its chosen policy.

## What Verified Means

`Verified By` means a requirement is covered by one or more verification artifacts in the authored trace graph.

That does not automatically mean:

- formal correctness
- a permanently green status
- a release gate used by every repository
- that every implementation detail has been hand-cataloged in the requirement

A verification artifact is usually best treated as a proof-summary artifact. It records the shared outcome and may reference evidence sources such as:

- unit tests
- integration tests
- manual QA
- benchmark runs
- interoperability runs
- security review
- fuzzing
- formal methods
- other repository-policy evidence sources

The repository decides which evidence sources matter for its own workflow. The canonical standard only fixes the trace model and the artifact roles.

That is why `Verified By` is not the same as a live green build. Live status comes from derived evidence reporting, not from the canonical requirement text.

## Local Repository Policy Terms

Repositories often want words for their own workflow gates:

- implemented
- verified
- release-ready

Those terms are local policy semantics. They are useful for teams, dashboards, and generated rollups, but they are not new canonical `spec-trace` artifact types or profile names unless the repository explicitly standardizes them.

In practice:

- canonical semantics answer what the standard means
- local policy semantics answer how a repository wants to label its own workflow state
- generated reports can use local policy labels without changing the canonical model

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

Legacy codebases do not need fake historical work items to become useful under the model.

During adoption, generated gap reports, coverage views, and attestation snapshots are valid and useful. They show where the graph is incomplete or where evidence is current, but they remain derived outputs.

This also means repositories do not need fake historical work items just to become useful under the model.

## Smallest End-To-End Example

If you want the smallest concrete chain, read [`./examples/arithmetic/SPEC-MATH-DIV.md`](./examples/arithmetic/SPEC-MATH-DIV.md), its linked architecture, work-item, and verification artifacts, the derived status rollup at [`./examples/arithmetic/generated/current-status-rollup.md`](./examples/arithmetic/generated/current-status-rollup.md), and the coverage-dimension rollup at [`./examples/arithmetic/generated/coverage-dimensions-rollup.md`](./examples/arithmetic/generated/coverage-dimensions-rollup.md).

That set shows:

- one requirement set with generated evidence snapshots
- one architecture artifact in `Satisfied By`
- one work item in `Implemented By`
- one verification artifact in `Verified By`
- one generated current-status view that summarizes the evidence without becoming canonical requirement text
- one generated coverage-dimension view that keeps partial coverage visible without inventing a new profile
