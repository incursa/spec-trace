# Authoring Guide

This guide is non-normative. The authoritative standard lives under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/).

## Authority Order

Use the repository in this order:

1. [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) for the canonical JSON-authored SPEC suite
2. [`model/model.schema.json`](./model/model.schema.json), the root JSON templates, and [`catalog/retired-requirements.json`](./catalog/retired-requirements.json)
3. examples, generated outputs, and compatibility schemas
4. root summaries and AI convenience surfaces

If any lower layer disagrees with the SPEC suite or the authoritative schema, the SPEC suite and schema win.

## Canonical Rules

- Author canonical artifacts in JSON.
- Validate document shape with [`model/model.schema.json`](./model/model.schema.json).
- Keep requirements inside specification artifacts.
- Use stable IDs for cross-file references. Do not use file paths as canonical trace identifiers when an artifact or requirement ID exists.
- If you rename or delete a referenced ID, fix every referrer in the same change.
- Treat generated outputs and support docs as non-authoritative.

## Templates

Start from the matching JSON template:

- [`spec-template.json`](./spec-template.json)
- [`architecture-template.json`](./architecture-template.json)
- [`work-item-template.json`](./work-item-template.json)
- [`verification-template.json`](./verification-template.json)

The templates are valid starter artifacts, not schema definitions.

## Choose The Artifact

### Specification

Use a specification when you need to define one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Read first:

- [`specs/requirements/spec-trace/_index.md`](./specs/requirements/spec-trace/_index.md)
- [`specs/requirements/spec-trace/SPEC-STD.json`](./specs/requirements/spec-trace/SPEC-STD.json)
- [`specs/requirements/spec-trace/SPEC-TPL.json`](./specs/requirements/spec-trace/SPEC-TPL.json)
- [`specs/requirements/spec-trace/SPEC-LAY.json`](./specs/requirements/spec-trace/SPEC-LAY.json)

### Requirement

Requirements do not stand alone. They are nested records inside a specification artifact.

Each requirement should carry:

- `id`
- `title`
- `statement`
- optional `trace`
- optional `notes`

Use the clause for the normative behavior. Use `notes` for rationale, examples, and clarifications.

### Architecture

Use an architecture artifact when design explanation, rationale, or tradeoffs add value.

Architecture artifacts link back to requirements through `satisfies`. They do not restate the requirement text.

### Work Item

Use a work item when you need to describe implementation work and connect it to requirements, design inputs, and verification planning.

Work items link through `addresses`, `design_links`, and `verification_links`.

### Verification

Use a verification artifact when you need to record how a requirement set was checked and what shared outcome was recorded.

Verification artifacts link through `verifies`. If the listed requirements do not share one result, split the verification scope.

## Reference Rules

- Artifact-to-artifact references use stable artifact IDs such as `SPEC-...`, `ARC-...`, `WI-...`, and `VER-...`.
- Requirement trace uses stable IDs in structured fields such as `satisfied_by`, `implemented_by`, `verified_by`, `derived_from`, `supersedes`, and `related`.
- Lineage references may point at retired requirement IDs when those IDs are present in [`catalog/retired-requirements.json`](./catalog/retired-requirements.json).
- Inline identifier references use backticks around stable IDs in string fields. They are lightweight mentions, not structured trace edges.

## Recommended Workflow

1. Start from the authoritative SPEC files for the behavior you are changing.
2. Copy the matching JSON template or a nearby JSON example.
3. Edit the canonical JSON artifact.
4. Validate the repository:

```powershell
./scripts/Test-SpecTraceRepository.ps1 -Profile core
```

5. Build a catalog when you need a machine-readable repository index:

```powershell
./scripts/Build-SpecTraceCatalog.ps1
```

6. Validate generated evidence snapshots when a tool emits `*.evidence.json`:

```powershell
./scripts/Validate-SpecTraceEvidence.ps1
./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/arithmetic/generated/division-evidence.evidence.json
```

7. Generate a derived repository attestation report when you want summary, detail, and per-spec HTML views:

```powershell
./scripts/Render-SpecTraceAttestation.ps1
./scripts/Render-SpecTraceAttestation.ps1 -Profile core -OutDir ./artifacts/spec-trace/attestation
```

## Validation Commands

- `./scripts/Test-SpecTraceRepository.ps1 -Profile core`
- `./scripts/Test-SpecTraceRepository.ps1 -Profile traceable`
- `./scripts/Test-SpecTraceRepository.ps1 -Profile auditable`
- `./scripts/Test-SpecTraceRepository.ps1 -JsonReportPath ./specs/generated/validation-report.json`
- `./scripts/Build-SpecTraceCatalog.ps1 -JsonOutputPath ./specs/generated/spec-trace-catalog.json`
- `./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/calculator-int/generated`
- `./scripts/Render-SpecTraceAttestation.ps1 -InputPath ./examples/calculator-int -Emit both`

## Evidence Snapshots

- Evidence snapshots are generated JSON, not canonical authored artifacts.
- The authoritative shape is defined by [`model/model.schema.json`](./model/model.schema.json) and [`schemas/evidence-snapshot.schema.json`](./schemas/evidence-snapshot.schema.json).
- Snapshot `requirement_id` values must point at canonical `REQ-...` identifiers that exist in the repository.
- Multiple evidence files may overlap on the same requirement. Derived reporting merges them additively by evidence kind rather than treating one file's omission as a negative assertion.

## When The Standard Changes

If a change affects canonical field names, identifier rules, template shape, schema contracts, validator behavior, or example patterns, update these surfaces together:

- the canonical JSON artifacts under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/)
- the authoritative schema under [`model/`](./model/)
- the root JSON templates
- examples
- validation and reporting tooling
- root guidance and AI convenience surfaces
- the curated publish mirror under [`publish/`](./publish/)

Record notable reference-surface changes in [`CHANGELOG.md`](./CHANGELOG.md).
