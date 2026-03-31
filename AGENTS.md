# SpecTrace Agent Instructions

This repository contains the public reference standard for `spec-trace`.

Canonical authored artifacts are CUE. Generated Markdown exists for browsing and review, but it is not authoritative over the `.cue` source.

## Authority

Follow this order when working in the repository:

1. [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) for the canonical CUE-authored SPEC suite
2. [`model/`](./model/), root `.cue` templates, and worked examples
3. generated Markdown siblings and compatibility schemas
4. root summaries and AI convenience surfaces such as this file and [`LLMS.txt`](./LLMS.txt)

If you find a mismatch, follow the higher-authority source and fix the lower-authority one in the same change when appropriate.

## Task Routing

- To understand the standard, read [`specs/requirements/spec-trace/_index.md`](./specs/requirements/spec-trace/_index.md) and the relevant `SPEC-...` artifacts first.
- To revise repository-native specs, requirements, architecture, work items, verification artifacts, or standard-maintenance surfaces, prefer the matching `spec_trace_*` specialist when the task is clearly scoped.
- To draft a new canonical artifact, start from the matching `.cue` template:
  - [`spec-template.cue`](./spec-template.cue)
  - [`architecture-template.cue`](./architecture-template.cue)
  - [`work-item-template.cue`](./work-item-template.cue)
  - [`verification-template.cue`](./verification-template.cue)
  These files export schema-backed template definitions in the root `templates` package rather than concrete example artifacts.
- To change canonical field names, identifier rules, templates, schemas, or example patterns, use [`skills/spec-trace-change-maintainer/`](./skills/spec-trace-change-maintainer/) and propagate the change across affected surfaces.
- To run repository-wide validation, use [`scripts/Test-SpecTraceRepository.ps1`](./scripts/Test-SpecTraceRepository.ps1).
- To generate readable Markdown from canonical CUE, use [`scripts/Render-SpecTraceMarkdown.ps1`](./scripts/Render-SpecTraceMarkdown.ps1).
- To emit a machine-readable repository catalog, use [`scripts/Build-SpecTraceCatalog.ps1`](./scripts/Build-SpecTraceCatalog.ps1).

## Working Rules

- Keep requirements inside specification artifacts.
- Author canonical artifacts in `.cue`, not Markdown, YAML, or JSON.
- Treat generated Markdown as read-only output.
- Use stable IDs for cross-file references. Prefer artifact IDs and requirement IDs over file paths.
- If you rename or delete a referenced ID, fix the referrers in the same change.
- Keep AI-facing docs and skills short, ergonomic, and explicitly subordinate to the SPEC suite.

## AI Assets In This Repo

- [`LLMS.txt`](./LLMS.txt) is the lightweight bootstrap file.
- [`skills/README.md`](./skills/README.md) catalogs repo-local authoring skills.
- [`authoring.md`](./authoring.md) is the task-oriented human guide.
- [`artifact-model-explainer.md`](./artifact-model-explainer.md) and [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) explain the operational model in plain language.
