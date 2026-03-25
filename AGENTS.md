# SpecTrace Agent Instructions

This repository contains the public reference standard for `spec-trace`.

The canonical standard is the SPEC suite under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). This file is an agent convenience layer and must not override that suite.

## Authority

Follow this order when working in the repository:

1. [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/)
2. root templates such as [`spec-template.md`](./spec-template.md), [`architecture-template.md`](./architecture-template.md), [`work-item-template.md`](./work-item-template.md), and [`verification-template.md`](./verification-template.md), plus schemas and examples
3. root summary documents such as [`README.md`](./README.md), [`overview.md`](./overview.md), [`layout.md`](./layout.md), and [`authoring.md`](./authoring.md)
4. AI convenience surfaces such as this file, [`LLMS.txt`](./LLMS.txt), and [`skills/`](./skills/)

If you find a mismatch, follow the higher-authority source and fix the lower-authority one in the same change when appropriate.

## Task Routing

- To understand the standard, read [`specs/requirements/spec-trace/_index.md`](./specs/requirements/spec-trace/_index.md) and the relevant `SPEC-...` files first.
- To handle a request that touches behavior but does not yet have explicit requirements, start with [`specs/requirements/spec-trace/REQUIREMENT-GAPS.md`](./specs/requirements/spec-trace/REQUIREMENT-GAPS.md) and the `spec_trace_gap_tracker` agent before writing code or implementation work items.
- To draft or revise repository-native spec, requirement, architecture, work-item, verification, or standard-maintenance artifacts, prefer the matching `spec_trace_*` subagent role rather than a generic agent when the task is clearly scoped.
- To draft or revise a specification, start from [`spec-template.md`](./spec-template.md) and use [`skills/spec-trace-specification-author/`](./skills/spec-trace-specification-author/).
- To add or tighten a requirement, work inside the owning `SPEC-...` file and use [`skills/spec-trace-requirement-author/`](./skills/spec-trace-requirement-author/).
- To change requirement evolution or upstream trace semantics, work inside [`specs/requirements/spec-trace/SPEC-LIN.md`](./specs/requirements/spec-trace/SPEC-LIN.md) and use [`skills/spec-trace-change-maintainer/`](./skills/spec-trace-change-maintainer/).
- To define or revise conformance profiles, work inside [`specs/requirements/spec-trace/SPEC-PRF.md`](./specs/requirements/spec-trace/SPEC-PRF.md) and use [`skills/spec-trace-change-maintainer/`](./skills/spec-trace-change-maintainer/).
- To draft architecture, work-item, or verification artifacts, use the matching root template plus the corresponding skill under [`skills/`](./skills/).
- To change canonical field names, identifier rules, templates, schemas, or example patterns, use [`skills/spec-trace-change-maintainer/`](./skills/spec-trace-change-maintainer/) and propagate the change across all affected surfaces.
- To run repository-wide validation, use [`scripts/Test-SpecTraceRepository.ps1`](./scripts/Test-SpecTraceRepository.ps1) with `-Profile core`, `-Profile traceable`, or `-Profile auditable`; use `-JsonReportPath` when you need machine-readable output.
- When writing Markdown in this repository, make repo-local references to files, folders, specifications, requirements, classes, interfaces, and other concrete local artifacts clickable with relative links. If the visible text should stay monospace, put backticks inside the link text, for example [`README.md`](./README.md) or [`SPEC-STD.md`](./specs/requirements/spec-trace/SPEC-STD.md). Use absolute URLs only for external targets that cannot be expressed relatively.
- Requirement clauses use the approved BCP 14-style uppercase keyword set only; lowercase spellings are plain English.
- Inline identifier references use backticks around stable artifact IDs and are allowed in clauses, Notes, and descriptive sections. They are prose mentions, not trace edges, so keep structured `Trace` fields for explicit machine-readable relationships.
- Requirement titles are short descriptive labels; the clause carries the normative behavior and `Notes` carry rationale and examples.
- Structured `Trace` fields have typed semantics by label family: downstream links, lineage, source citations, implementation-specific references, and loose associations.
- Verification artifacts use one artifact-scoped status for every requirement listed in `verifies`; if the requirements do not share one outcome, split the scope into separate artifacts.

## Working Rules

- Keep requirements inside specification documents.
- Keep architecture, work items, and verification artifacts distinct from requirements.
- Prefer the nearest template and worked example over inventing a new document shape.
- Do not re-implement the standard in this file or in `skills/` when a direct repository source can be referenced instead.
- Keep AI-facing docs and skills ergonomic, short, and explicitly subordinate to the SPEC suite.

## AI Assets In This Repo

- [`LLMS.txt`](./LLMS.txt) is the lightweight plain-text bootstrap.
- [`skills/README.md`](./skills/README.md) catalogs the repo-local authoring skills.
- [`authoring.md`](./authoring.md) is the task-oriented human guide that points back to the authoritative sources.
- [`scripts/Test-SpecTraceRepository.ps1`](./scripts/Test-SpecTraceRepository.ps1) is the repository-level validation entry point for duplicate IDs, unresolved direct links, reciprocal consistency, namespace alignment, and profile enforcement.
- [`scripts/Validate-SpecTrace.ps1`](./scripts/Validate-SpecTrace.ps1) remains a narrower compatibility check for the canonical bundle.
