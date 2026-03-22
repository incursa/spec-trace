# SpecTrace Agent Instructions

This repository contains the public reference standard for `spec-trace`.

The canonical standard is the SPEC suite under `specs/requirements/spec-trace/`. This file is an agent convenience layer and must not override that suite.

## Authority

Follow this order when working in the repository:

1. `specs/requirements/spec-trace/`
2. root templates, schemas, and examples
3. root summary documents such as `README.md`, `overview.md`, `layout.md`, and `authoring.md`
4. AI convenience surfaces such as this file, `LLMS.txt`, and `skills/`

If you find a mismatch, follow the higher-authority source and fix the lower-authority one in the same change when appropriate.

## Task Routing

- To understand the standard, read `specs/requirements/spec-trace/_index.md` and the relevant `SPEC-...` files first.
- To draft or revise a specification, start from `spec-template.md` and use `skills/spec-trace-specification-author/`.
- To add or tighten a requirement, work inside the owning `SPEC-...` file and use `skills/spec-trace-requirement-author/`.
- To draft architecture, work-item, or verification artifacts, use the matching root template plus the corresponding skill under `skills/`.
- To change canonical field names, identifier rules, templates, schemas, or example patterns, use `skills/spec-trace-change-maintainer/` and propagate the change across all affected surfaces.

## Working Rules

- Keep requirements inside specification documents.
- Keep architecture, work items, and verification artifacts distinct from requirements.
- Prefer the nearest template and worked example over inventing a new document shape.
- Do not re-implement the standard in this file or in `skills/` when a direct repository source can be referenced instead.
- Keep AI-facing docs and skills ergonomic, short, and explicitly subordinate to the SPEC suite.

## AI Assets In This Repo

- `LLMS.txt` is the lightweight plain-text bootstrap.
- `skills/README.md` catalogs the repo-local authoring skills.
- `authoring.md` is the task-oriented human guide that points back to the authoritative sources.
