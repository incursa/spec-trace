# Repository-Native Requirements and Traceability Standard

`incursa/spec-trace` is a public reference standard for precise, repository-native software specifications.

The repository is JSON-first:

- canonical authored artifacts are `*.json`
- document shape is validated by [`model/model.schema.json`](./model/model.schema.json)
- repository tooling adds cross-file integrity, profile, evidence, catalog, and attestation checks on top of JSON Schema validation
- generated reports and support docs are derived or non-authoritative

The canonical standard lives under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/).

## Core Model

- The canonical static model uses four artifact families: `specification`, `architecture`, `work_item`, and `verification`.
- Requirements are the smallest normative unit and live inside a specification artifact.
- Requirement clauses use the approved uppercase keyword set: `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`.
- Structured trace uses stable IDs. `Satisfied By`, `Implemented By`, and `Verified By` are downstream trace. `Derived From` and `Supersedes` are lineage. `Upstream Refs` are provenance. `Related` is a loose association.
- Generated evidence snapshots, coverage rollups, attestation views, and catalogs are derived outputs.
- Repository-level conformance profiles are `core`, `traceable`, and `auditable`.

## Canonical Files

- [`model/model.schema.json`](./model/model.schema.json) - authoritative JSON Schema for canonical artifacts and shared supporting shapes
- [`catalog/retired-requirements.json`](./catalog/retired-requirements.json) - retired requirement ledger used during lineage validation
- [`spec-template.json`](./spec-template.json)
- [`architecture-template.json`](./architecture-template.json)
- [`work-item-template.json`](./work-item-template.json)
- [`verification-template.json`](./verification-template.json)

Concrete artifacts live under [`specs/`](./specs/) and [`examples/`](./examples/). Support docs may remain in Markdown, but contributors should edit the canonical JSON artifacts.

## Commands

```powershell
./scripts/Test-SpecTraceRepository.ps1 -Profile core
./scripts/Build-SpecTraceCatalog.ps1
./scripts/Validate-SpecTraceEvidence.ps1
./scripts/Render-SpecTraceAttestation.ps1
./scripts/Sync-PublishModule.ps1
```

Useful variants:

- `./scripts/Test-SpecTraceRepository.ps1 -Profile traceable`
- `./scripts/Test-SpecTraceRepository.ps1 -Profile auditable`
- `./scripts/Test-SpecTraceRepository.ps1 -JsonReportPath ./specs/generated/validation-report.json`
- `./scripts/Build-SpecTraceCatalog.ps1 -JsonOutputPath ./specs/generated/spec-trace-catalog.json`
- `./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/arithmetic/generated/division-evidence.evidence.json`
- `./scripts/Render-SpecTraceAttestation.ps1 -Profile core -OutDir ./artifacts/spec-trace/attestation`

## Repository Contents

- [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) - canonical self-specification suite
- [`model/`](./model/) - authoritative JSON Schema and supporting notes
- [`publish/`](./publish/) - curated publish mirror for the reusable schema and templates
- [`catalog/retired-requirements.json`](./catalog/retired-requirements.json) - retired requirement ledger
- [`schemas/`](./schemas/) - compatibility or slice schemas derived from the authoritative model
- [`examples/`](./examples/) - worked examples across all major artifact families
- [`src/SpecTrace.Tool/`](./src/SpecTrace.Tool/) - validation, catalog, evidence, and attestation tooling
- [`tests/SpecTrace.Tool.Tests/`](./tests/SpecTrace.Tool.Tests/) - automated tests

## Validation

The repository validation path is deterministic and local:

- JSON Schema validates artifact structure, required fields, allowed enums, and identifier patterns.
- Repository validation checks duplicate IDs, namespace alignment, reciprocal links, broken cross-file references, and profile-specific coverage rules.
- Evidence snapshots are validated against JSON Schema and then checked against the repository requirement catalog.
- Attestation reports merge overlapping evidence snapshots additively and emit deterministic `index.html`, `summary.html`, `details.html`, per-spec pages, and `attestation.json`.
- `build-catalog` emits the repository catalog as JSON.

## Publishing

[`publish/`](./publish/) is synchronized from the root schema and templates by [`scripts/Sync-PublishModule.ps1`](./scripts/Sync-PublishModule.ps1). It intentionally excludes the self-specification suite, examples, and repository tooling so downstream consumers can package or mirror just the reusable schema and template surface.
