# Artifact Model Explainer

This file is a non-authoritative reading guide for the `spec-trace` reference repository. The canonical standard lives in [`./specs/requirements/spec-trace/`](./specs/requirements/spec-trace/).

Use this as the plain-language map for how the artifact families, trace labels, and conformance profiles work in practice. If it ever disagrees with the SPEC suite, the SPEC suite wins.

For the distinction between authored trace, dynamic attestation, and local repository policy terms, read [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md).

## Artifact Families

The standard uses four core artifact families:

- `specification` / `requirement` means what must be true.
- `architecture` means how the requirement is intended to be satisfied.
- `work item` means what implementation work is or was done.
- `verification` means how the requirement was checked and what shared outcome was recorded.

Those are different roles on purpose. Architecture explains intent, work items explain delivery work, and verification records how the requirement was checked and what outcome was recorded. None of them replace the requirement text itself.

Architecture remains optional. When an RFC, policy, incident, or other upstream document is the real source of a requirement, capture that provenance in `Upstream Refs` and use derived reporting if you need source-coverage views.

## Profiles In Practice

The canonical profile names are `core`, `traceable`, and `auditable`.

Helpful shorthand:

- `core` means `spec-valid`.
- `traceable` means `artifact-linked`.
- `auditable` means `evidence-backed`.

Those phrases are explanatory only. They do not replace the canonical profile names or change the profile definitions.

Practical reading:

- `core` means the requirements are structurally valid and trustworthy as requirements.
- `traceable` means every requirement is connected to at least one downstream artifact.
- `auditable` means every requirement has verification coverage and the trace graph is internally consistent.

`auditable` does not mean formal proof of program correctness. It means the repository has recorded verification coverage according to its chosen practice.

If you need a dimension-style report instead of a gate result, use the derived-reporting model in [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) and [`SPEC-RPT.md`](./specs/requirements/spec-trace/SPEC-RPT.md).

## Trace Labels At A Glance

The canonical downstream trace graph is:

- `Satisfied By` for architecture artifacts
- `Implemented By` for work items
- `Verified By` for verification artifacts

That graph is different from provenance and from generated implementation
evidence.

Use these distinctions:

- `Upstream Refs` are upstream provenance. They point at laws, policies, tickets, incidents, decisions, or other source material that motivated the requirement.
- `Derived From` and `Supersedes` are lineage. They describe how one requirement evolved from another.
- Generated evidence snapshots record implementation observations such as
  `unit_test`, `code_ref`, `benchmark`, or `manual_test`. They can point at
  tests, file paths, symbols, or other local evidence refs without turning
  those refs into authored requirement trace.

Only `Satisfied By`, `Implemented By`, and `Verified By` are the canonical
downstream trace edges. Evidence snapshots are useful, but they are not
substitutes for that graph.

## Canonical Semantics And Local Policy

The canonical standard defines the names and meanings of the profiles and trace labels.

A repository may define local policy terms such as:

- implemented
- verified
- release-ready

Those local terms can be useful for workflow and release gates, but they are repository policy, not canonical `spec-trace` semantics unless the repository explicitly standardizes them.

In other words:

- canonical semantics answer what the standard means
- local policy answers how a repository chooses to run its process

## Tiny Worked Example

If you want the smallest concrete chain, open [`examples/arithmetic/SPEC-MATH-DIV.md`](./examples/arithmetic/SPEC-MATH-DIV.md) and its linked architecture, work-item, and verification artifacts.

That example shows:

- one requirement with `Upstream Refs` for provenance
- one architecture artifact in `Satisfied By`
- one work item in `Implemented By`
- one verification artifact in `Verified By`
- generated evidence snapshots that point straight at implementation details

Read the requirement first, then follow the downstream trace. Treat the
upstream refs as background provenance and the evidence snapshots as derived
implementation pointers.
