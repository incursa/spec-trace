# Artifact Model Explainer

This file is a non-authoritative reading guide for the `spec-trace` reference repository. The canonical standard lives in [`./specs/requirements/spec-trace/`](./specs/requirements/spec-trace/).

Use this as the plain-language map for how the artifact families, trace labels, and conformance profiles work in practice. If it ever disagrees with the SPEC suite, the SPEC suite wins.

## Artifact Families

The standard uses four core artifact families:

- `specification` / `requirement` means what must be true.
- `architecture` means how the requirement is intended to be satisfied.
- `work item` means what implementation work is or was done.
- `verification` means how the requirement was checked and what shared outcome was recorded.

Those are different roles on purpose. Architecture explains intent, work items explain delivery work, and verification records how the requirement was checked and what outcome was recorded. None of them replace the requirement text itself.

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

## Trace Labels At A Glance

The canonical downstream trace graph is:

- `Satisfied By` for architecture artifacts
- `Implemented By` for work items
- `Verified By` for verification artifacts

That graph is different from provenance and from implementation-specific direct references.

Use these distinctions:

- `Source Refs` are upstream provenance. They point at laws, policies, tickets, incidents, decisions, or other source material that motivated the requirement.
- `Derived From` and `Supersedes` are lineage. They describe how one requirement evolved from another.
- `Test Refs` and `Code Refs` are direct implementation references. They can point at tests, file paths, symbols, or other local references that help a reader find the implementation quickly.

Only `Satisfied By`, `Implemented By`, and `Verified By` are the canonical downstream trace edges. `Test Refs` and `Code Refs` are useful, but they are not substitutes for that graph.

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

- one requirement with `Source Refs` for provenance
- one architecture artifact in `Satisfied By`
- one work item in `Implemented By`
- one verification artifact in `Verified By`
- direct `Test Refs` and `Code Refs` that point straight at implementation details

Read the requirement first, then follow the downstream trace. Treat the source refs as background provenance and the test/code refs as direct implementation pointers.
