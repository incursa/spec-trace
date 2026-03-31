# Overview

This file summarizes the standard described in [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). It is a practical guide, not a second source of truth.

The reference repository is CUE-first. Canonical artifacts are authored in `.cue`; the Markdown siblings you browse under [`specs/`](./specs/) and [`examples/`](./examples/) are generated views.

## Core Vocabulary

### Specification

A specification is a canonical CUE artifact that groups one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Each specification artifact carries document-level fields such as `artifact_id`, `title`, `domain`, `capability`, `purpose`, `scope`, and `context`, plus a `requirements` collection.

### Requirement

A requirement is the smallest normative, testable statement in the system.

Each requirement:

- has a stable `REQ-...` identifier
- has a short descriptive title
- has a compact normative clause in `statement`
- may carry optional `trace`
- may carry optional `notes`

The clause is the normative content. The title is a scan aid. `notes` carry rationale, caveats, and examples.

### Architecture, Work Item, Verification

- `architecture` explains how requirements are intended to be satisfied
- `work_item` records implementation work and delivery scope
- `verification` records how a requirement set was checked and what shared outcome was recorded

Those roles stay distinct. None of them replaces the requirement text.

## Canonical Requirement Form

The canonical authored form is a CUE record:

```cue
{
    id:        "REQ-EXAMPLE-0001"
    title:     "Short label"
    statement: "The system MUST do something directly testable."
    trace: {
        satisfied_by: [
            "ARC-EXAMPLE-0001",
        ]
        implemented_by: [
            "WI-EXAMPLE-0001",
        ]
        verified_by: [
            "VER-EXAMPLE-0001",
        ]
    }
    notes: [
        "Optional rationale or examples.",
    ]
}
```

Generated Markdown renders that same requirement into a readable heading, clause, trace block, and notes block, but the CUE record remains authoritative.

## Normative Keywords

Requirement clauses use the approved uppercase keyword set:

- `MUST`
- `MUST NOT`
- `SHALL`
- `SHALL NOT`
- `SHOULD`
- `SHOULD NOT`
- `MAY`

Every canonical requirement clause must contain exactly one approved uppercase keyword.

## Trace Model

The canonical downstream trace graph uses:

- `satisfied_by` for architecture artifacts
- `implemented_by` for work items
- `verified_by` for verification artifacts

Lineage and provenance are separate:

- `derived_from` and `supersedes` are lineage
- `upstream_refs` is provenance
- `related` is a loose association

Inline identifier references in generated Markdown or prose are useful navigation aids, but they are not typed trace edges by themselves.

## Static Trace Versus Derived Reporting

The authored trace graph lives in canonical CUE artifacts.

Derived outputs can answer questions such as:

- which requirements currently have verification coverage
- which requirements have observed evidence by kind
- which requirements are missing downstream links
- which artifacts are orphaned under stricter profiles

Those outputs are useful, but they are not canonical requirement text.

## Conformance Profiles

The repository-level profiles are:

- `core`: structural validity, canonical IDs, approved keyword usage, duplicate detection, and broken-reference detection
- `traceable`: `core` plus at least one downstream trace link per requirement
- `auditable`: `traceable` plus verification coverage, reciprocal consistency, and no orphan ARC/WI/VER artifacts

Helpful shorthand:

- `core` means `spec-valid`
- `traceable` means `artifact-linked`
- `auditable` means `evidence-backed`

Those phrases are explanatory only. The canonical profile names remain `core`, `traceable`, and `auditable`.

## Validation Path

The repository validates canonical CUE inputs through a deterministic local toolchain:

- CUE checks structure, required fields, enums, and identifier regexes.
- The validator builds a repository-wide catalog of artifact IDs and nested requirement IDs.
- Validation fails on duplicates, broken references, and wrong target kinds.
- Markdown generation is deterministic and can be checked for drift in CI.

## Reading The Repo

Use this order:

1. canonical CUE artifacts under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/)
2. generated Markdown siblings for easier browsing
3. root guidance such as [`authoring.md`](./authoring.md) and [`layout.md`](./layout.md)
4. worked examples under [`examples/`](./examples/)
