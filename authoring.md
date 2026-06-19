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
- optional `coverage`
- optional `trace`
- optional `notes`

Use the clause for the normative behavior. Use `notes` for rationale, examples, and clarifications.

### Coverage

Use `coverage` when you want to state which evidence dimensions you expect a requirement to have, not which evidence already exists.

If you use `coverage`, include all four keys. Use `not_applicable` or `deferred` when a dimension does not need active coverage instead of omitting that key.

- `positive` covers normal success cases.
- `negative` covers rejection, error, or invalid-input cases.
- `edge` covers boundary conditions and other edge cases.
- `fuzz` covers randomized, robustness, or adversarial checks.

Use these statuses inside the coverage object:

- `required` means the dimension is expected and should be provided.
- `optional` means the dimension is useful but not mandatory.
- `not_applicable` means the dimension does not make sense for the requirement.
- `deferred` means the dimension is expected later but is intentionally not required yet.

Coverage is authored expectation metadata. It does not report actual code coverage, test results, or runtime evidence. If a performance or other non-functional concern needs explicit treatment, model it as a separate requirement instead of implying it through the coverage settings on a functional requirement.

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
- Requirement coverage uses the canonical keys `positive`, `negative`, `edge`, and `fuzz` with the status values `required`, `optional`, `not_applicable`, and `deferred`.
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

6. Resolve a portable topic view when you want a machine-readable requirement slice:

```powershell
./scripts/Resolve-SpecTraceTopicView.ps1 -TopicViewPath ./specs/requirements/spec-trace/SPEC-TOP.json
```

7. Validate generated evidence snapshots when a tool emits `*.evidence.json`:

```powershell
./scripts/Validate-SpecTraceEvidence.ps1
./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/arithmetic/generated/division-evidence.evidence.json
```

8. Generate a derived repository attestation report when you want summary, detail, and per-spec HTML views:

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
- `./scripts/Resolve-SpecTraceTopicView.ps1 -TopicViewJson '{ "name": "sample", "include_requirements": ["REQ-SAMPLE-0001"] }'`
- `./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/calculator-int/generated`
- `./scripts/Render-SpecTraceAttestation.ps1 -InputPath ./examples/calculator-int -Emit both`

## Evidence Snapshots

- Evidence snapshots are generated JSON, not canonical authored artifacts.
- The authoritative shape is defined by [`model/model.schema.json`](./model/model.schema.json) and [`schemas/evidence-snapshot.schema.json`](./schemas/evidence-snapshot.schema.json).
- Snapshot `requirement_id` values must point at canonical `REQ-...` identifiers that exist in the repository.
- Multiple evidence files may overlap on the same requirement. Derived reporting merges them additively by evidence kind rather than treating one file's omission as a negative assertion.

### Source Context Notes

Source Context Notes are optional authoring aids. Use them when a future maintainer would reasonably ask why the code is shaped this way, where a value came from, what invariant must be preserved, or what external artifact explains the decision.

Use a short `CONTEXT` block with an optional `SEE` line. In source files, wrap the markers in the host language comment syntax.

Single-line note:

```text
CONTEXT: Default ACK delay exponent is the QUIC transport default until the peer overrides it.
SEE: spec:REQ-QUIC-RFC9000-TP-ACK-DELAY-EXPONENT-DEFAULT
```

Block note:

```text
CONTEXT: QUIC v1 packet protection labels
These literals are wire-protocol labels required by QUIC v1 packet protection.
They are intentionally separate from QUIC v2 labels because the versions use different label text.
SEE: spec:REQ-QUIC-RFC9001-S6P1-0009
END CONTEXT: QUIC v1 packet protection labels
```

Reference categories are guidance, not a closed list:

- `spec:` for SpecTrace requirement IDs or spec-backed artifacts
- `design:` for architecture or design docs
- `perf:` for benchmark, profiling, allocation, or performance artifacts
- `security:` for security notes, invariants, or threat-model references
- `interop:` for compatibility notes with other implementations
- `lifecycle:` for disposal, cancellation, ownership, or concurrency decisions
- `diag:` for diagnostics, logging, qlog, telemetry, or observability decisions
- `issue:` for issue, work item, or PR references
- `temporary:` for transitional behavior, ideally with an issue or expiry reference

Keep notes short, local, and human-readable. Do not copy long requirement text or design documents into source. If a note spans more than the immediately following declaration or statement, use an explicit end marker or equivalent exact range metadata so tooling can parse the region.

Collected notes may feed generated evidence or a source context index. They do not replace canonical requirements, trace links, verification artifacts, or test results.

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
