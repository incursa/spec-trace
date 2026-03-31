# Authoring Guide

This guide is non-normative. The authoritative standard lives under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/).

## Authority Order

Use the repository in this order:

1. [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) for the canonical CUE-authored SPEC suite
2. [`model/`](./model/) and the root `.cue` templates for the canonical shape
3. generated Markdown siblings, examples, and compatibility schemas for browsing and integration help
4. root summaries and AI convenience surfaces for navigation

If any lower layer disagrees with the SPEC suite or the shared CUE model, the CUE-authored canonical sources win.

## Canonical Rules

- Author canonical artifacts in `.cue`, not Markdown, YAML, or JSON.
- Use the shared import path [`github.com/incursa/spec-trace/model@v0`](./model/README.md).
- Use the root module import path `github.com/incursa/spec-trace@v0:templates` when you want the published template definitions from the `templates` package.
- Keep concrete authored documents mostly data. Prefer straightforward field assignment over clever CUE metaprogramming.
- Treat generated Markdown as read-only presentation output.
- Use stable IDs for cross-file references. Do not use file paths as canonical trace identifiers when an artifact or requirement ID exists.
- If you rename or delete a referenced ID, fix every referrer in the same change.

## Templates

Start from the matching CUE template:

- [`spec-template.cue`](./spec-template.cue)
- [`architecture-template.cue`](./architecture-template.cue)
- [`work-item-template.cue`](./work-item-template.cue)
- [`verification-template.cue`](./verification-template.cue)

The root Markdown templates remain human-readable companion shapes, but the `.cue` templates are the canonical authoring entry points.
The root `.cue` files now export schema-backed definitions rather than concrete example artifacts, so authored documents should unify their top-level `artifact` value with the matching template definition or with the lower-level `model.#...` definition directly.

## Choose The Artifact

### Specification

Use a specification when you need to define one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Read first:

- [`specs/requirements/spec-trace/_index.md`](./specs/requirements/spec-trace/_index.md)
- [`specs/requirements/spec-trace/SPEC-STD.md`](./specs/requirements/spec-trace/SPEC-STD.md)
- [`specs/requirements/spec-trace/SPEC-TPL.md`](./specs/requirements/spec-trace/SPEC-TPL.md)
- [`specs/requirements/spec-trace/SPEC-LAY.md`](./specs/requirements/spec-trace/SPEC-LAY.md)

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
- Requirement-to-anything trace uses stable IDs in structured fields such as `satisfied_by`, `implemented_by`, `verified_by`, `derived_from`, `supersedes`, and `related`.
- Lineage references may point at retired requirement IDs when those IDs are present in the retired ledger at [`catalog/retired-requirements.cue`](./catalog/retired-requirements.cue).
- Generated Markdown may wrap IDs in repo-local links, but the visible identifier text remains the canonical reference token.

## Recommended Workflow

1. Start from the authoritative SPEC files for the behavior you are changing.
2. Copy the matching `.cue` template or a nearby `.cue` example.
3. Edit the canonical `.cue` artifact.
4. Validate the repository:

```powershell
./scripts/Test-SpecTraceRepository.ps1 -Profile core
```

5. Generate or verify Markdown output:

```powershell
./scripts/Render-SpecTraceMarkdown.ps1
./scripts/Render-SpecTraceMarkdown.ps1 -Check
```

6. Build a catalog when you need a machine-readable repository index:

```powershell
./scripts/Build-SpecTraceCatalog.ps1
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
- `./scripts/Build-SpecTraceCatalog.ps1 -JsonOutputPath ./specs/generated/spec-trace-catalog.json -CueOutputPath ./specs/generated/spec-trace-catalog.cue`
- `./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/calculator-int/generated`
- `./scripts/Render-SpecTraceAttestation.ps1 -InputPath ./examples/calculator-int -Emit both`

## Evidence Snapshots

- Evidence snapshots are generated JSON, not canonical authored artifacts.
- The authoritative shape is [`model.#EvidenceSnapshot`](./model/model.cue), and the validator uses CUE first before checking repository references.
- Snapshot `requirement_id` values must point at canonical `REQ-...` identifiers that exist in the repository.
- Multiple evidence files may overlap on the same requirement. Derived reporting merges them additively by evidence kind rather than treating one file's omission as a negative assertion.

## Markdown And Prose Links

When writing repository prose or browsing generated Markdown:

- prefer relative links for repo-local targets
- keep backticks inside the link text when monospace styling should remain
- include a heading anchor when linking to a specific generated requirement section

Those rules apply to prose and generated views. They do not make Markdown the source of truth again.

## When The Standard Changes

If a change affects canonical field names, identifier rules, template shape, schema contracts, generator behavior, or example patterns, update these surfaces together:

- the canonical `.cue` artifacts under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/)
- the shared model under [`model/`](./model/)
- root `.cue` templates
- generated Markdown outputs
- examples
- validation and generation tooling
- root guidance and AI convenience surfaces

Record notable reference-package changes in [`CHANGELOG.md`](./CHANGELOG.md).
