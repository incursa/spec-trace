# SpecTrace Agent Instructions

This repository contains the public reference standard for `spec-trace`.

Canonical authored artifacts are JSON documents. Their document shape is validated against the authoritative JSON Schema in [`model/model.schema.json`](./model/model.schema.json). Support docs and generated outputs are non-authoritative.

## Authority

Follow this order when working in the repository:

1. [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) for the canonical JSON-authored SPEC suite
2. [`model/model.schema.json`](./model/model.schema.json), the root `*.json` templates, worked examples, and [`catalog/retired-requirements.json`](./catalog/retired-requirements.json)
3. generated outputs and compatibility schemas
4. root summaries and AI convenience surfaces such as this file and [`LLMS.txt`](./LLMS.txt)

If you find a mismatch, follow the higher-authority source and fix the lower-authority one in the same change when appropriate.

## Task Routing

- To understand the standard, read [`specs/requirements/spec-trace/_index.md`](./specs/requirements/spec-trace/_index.md) and the relevant `SPEC-...json` artifacts first.
- To draft a new canonical artifact, start from the matching JSON template:
  - [`spec-template.json`](./spec-template.json)
  - [`architecture-template.json`](./architecture-template.json)
  - [`work-item-template.json`](./work-item-template.json)
  - [`verification-template.json`](./verification-template.json)
- To change canonical field names, identifier rules, templates, schemas, or example patterns, propagate the change across specs, schemas, tooling, examples, and publish surfaces in the same change set.
- To run repository-wide validation, use [`scripts/Test-SpecTraceRepository.ps1`](./scripts/Test-SpecTraceRepository.ps1).
- To emit a machine-readable repository catalog, use [`scripts/Build-SpecTraceCatalog.ps1`](./scripts/Build-SpecTraceCatalog.ps1).
- To validate generated evidence snapshots, use [`scripts/Validate-SpecTraceEvidence.ps1`](./scripts/Validate-SpecTraceEvidence.ps1).
- To generate attestation output, use [`scripts/Render-SpecTraceAttestation.ps1`](./scripts/Render-SpecTraceAttestation.ps1).
- To synchronize the reusable publish mirror, use [`scripts/Sync-PublishModule.ps1`](./scripts/Sync-PublishModule.ps1).

## Working Rules

- Keep requirements inside specification artifacts.
- Author canonical artifacts in JSON.
- Validate canonical document shape with [`model/model.schema.json`](./model/model.schema.json).
- Treat generated outputs as derived material.
- Use stable IDs for cross-file references. Prefer artifact IDs and requirement IDs over file paths.
- If you rename or delete a referenced ID, fix the referrers in the same change.
- Keep AI-facing docs short, ergonomic, and explicitly subordinate to the SPEC suite.

## AI Assets In This Repo

- [`LLMS.txt`](./LLMS.txt) is the lightweight bootstrap file.
- [`skills/README.md`](./skills/README.md) catalogs repo-local authoring skills.
- [`authoring.md`](./authoring.md) is the task-oriented human guide.
- [`artifact-model-explainer.md`](./artifact-model-explainer.md) and [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) explain the operational model in plain language.
