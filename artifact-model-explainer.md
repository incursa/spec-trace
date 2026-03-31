# Artifact Model Explainer

This file is a non-authoritative reading guide for the `spec-trace` reference repository. The canonical standard lives in [`./specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) as JSON artifacts validated by JSON Schema.

Use this as the plain-language map for how the artifact families, trace labels, and conformance profiles work in practice. If it ever disagrees with the SPEC suite, the SPEC suite wins.

For the distinction between authored trace, dynamic attestation, and local repository policy terms, read [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md).

## Artifact Families

The standard uses four core artifact families:

- `specification` and nested `requirement` records mean what must be true
- `architecture` means how the requirement is intended to be satisfied
- `work_item` means what implementation work is or was done
- `verification` means how the requirement was checked and what shared outcome was recorded

Those are different roles on purpose. None of them replaces the requirement text itself.

## Profiles In Practice

The canonical profile names are `core`, `traceable`, and `auditable`.

Helpful shorthand:

- `core` means `spec-valid`
- `traceable` means `artifact-linked`
- `auditable` means `evidence-backed`

Those phrases are explanatory only. They do not replace the canonical profile names or change the profile definitions.

Practical reading:

- `core` means the requirements are structurally valid and trustworthy as requirements
- `traceable` means every requirement is connected to at least one downstream artifact
- `auditable` means every requirement has verification coverage and the trace graph is internally consistent

## Trace Labels At A Glance

The canonical downstream trace graph is:

- `Satisfied By` for architecture artifacts
- `Implemented By` for work items
- `Verified By` for verification artifacts

That graph is different from provenance and from generated implementation evidence.

Use these distinctions:

- `Upstream Refs` are upstream provenance
- `Derived From` and `Supersedes` are lineage
- generated evidence snapshots record implementation observations such as `unit_test`, `code_ref`, `benchmark`, or `manual_test`

Only `Satisfied By`, `Implemented By`, and `Verified By` are canonical downstream trace edges.

## Canonical Semantics And Local Policy

The canonical standard defines the names and meanings of the profiles and trace labels.

A repository may define local policy terms such as:

- implemented
- verified
- release-ready

Those local terms can be useful for workflow and release gates, but they are repository policy, not canonical `spec-trace` semantics unless the repository explicitly standardizes them.

## Tiny Worked Example

If you want the smallest concrete chain, open [`examples/arithmetic/SPEC-MATH-DIV.json`](./examples/arithmetic/SPEC-MATH-DIV.json) and its linked architecture, work-item, and verification artifacts.
