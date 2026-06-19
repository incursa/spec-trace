# SpecTrace

SpecTrace is a small, JSON-first standard for keeping software requirements inside the repository they govern.

It is for teams that want requirements to be precise, reviewable, and traceable without moving the source of truth into a heavyweight requirements system. A SpecTrace repository can start with a few stable requirement IDs and grow into design links, work tracking, verification evidence, generated catalogs, and attestation reports when the project needs them.

The canonical standard lives in [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). This README is the practical front door, not the authority.

## Documentation And Mirroring

The source documentation for this repository lives under [`docs/`](./docs/).
The docs site manifest in [`docs.site.json`](./docs.site.json) and the mirror
workflow in [`.github/workflows/sync-docs.yml`](./.github/workflows/sync-docs.yml)
copy that tree into the central `incursa-docs` repository and open a pull
request there.

Do not edit the mirrored `incursa-docs` copy directly. Make source changes in
this repository and let the sync workflow publish the mirror update.

The operational handoff page is [`docs/maintainer-readiness.md`](./docs/maintainer-readiness.md). It records the current validation floor, release expectations, and known gaps.

## Why Use It

- Put requirements next to code, schemas, docs, and examples.
- Give every requirement a stable `REQ-...` identifier that reviews, tests, source notes, and tools can reference.
- Keep the normative requirement clause short and testable.
- Separate requirements from design, delivery work, and verification evidence.
- Validate JSON shape, IDs, links, profiles, evidence snapshots, catalogs, and attestation output locally.
- Adopt it in stages: start at `core`, then move to `traceable` or `auditable` when stricter proof is worth the cost.

## The Basic Shape

A requirement is a record inside a specification artifact. The `statement` is the normative clause.

```json
{
  "id": "REQ-EXAMPLE-0001",
  "title": "Reject duplicate payment batches",
  "statement": "The payment importer MUST reject a batch when its upstream batch identifier was already accepted.",
  "coverage": {
    "positive": "required",
    "negative": "required",
    "edge": "optional",
    "fuzz": "not_applicable"
  },
  "trace": {
    "satisfied_by": ["ARC-EXAMPLE-0001"],
    "implemented_by": ["WI-EXAMPLE-0001"],
    "verified_by": ["VER-EXAMPLE-0001"]
  }
}
```

Keep the clause compact. Put rationale, examples, caveats, source references, and local policy in the supporting fields around it.

## Start A Spec

1. Copy [`spec-template.json`](./spec-template.json).
2. Give the specification a stable `SPEC-...` artifact ID.
3. Add one or more `REQ-...` requirement records.
4. Use exactly one approved uppercase keyword in each requirement statement: `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, or `MAY`.
5. Add optional `coverage` expectations when you want to declare what evidence dimensions matter.
6. Add optional `trace` links when architecture, work items, verification artifacts, lineage, or upstream sources are known.
7. Validate the repository:

```powershell
./scripts/Test-SpecTraceRepository.ps1 -Profile core
```

For a concrete starter, compare the template with [`examples/payments/SPEC-PAY-ACH.json`](./examples/payments/SPEC-PAY-ACH.json), [`examples/arithmetic/SPEC-MATH-DIV.json`](./examples/arithmetic/SPEC-MATH-DIV.json), or the self-specification index at [`specs/requirements/spec-trace/_index.md`](./specs/requirements/spec-trace/_index.md).

## What You Can Add Later

SpecTrace has four canonical authored artifact families:

| Artifact | Use It For | Template |
| --- | --- | --- |
| `specification` | Requirements for a capability, behavior area, interface, or technical concern. | [`spec-template.json`](./spec-template.json) |
| `architecture` | Design explanation, rationale, and satisfaction links. | [`architecture-template.json`](./architecture-template.json) |
| `work_item` | Implementation scope connected to requirements and verification planning. | [`work-item-template.json`](./work-item-template.json) |
| `verification` | Proof summaries for how a requirement set was checked. | [`verification-template.json`](./verification-template.json) |

Generated evidence snapshots, coverage rollups, catalogs, and attestation reports are useful derived outputs. They do not replace the canonical JSON artifacts.

## Conformance Profiles

SpecTrace keeps adoption lightweight by defining repository-level profiles:

| Profile | What It Proves |
| --- | --- |
| `core` | JSON shape, identifier correctness, approved keyword usage, duplicate detection, and broken-reference detection. |
| `traceable` | `core` plus at least one downstream trace link for every requirement. |
| `auditable` | `traceable` plus verification coverage, reciprocal trace agreement where applicable, and no orphan architecture, work-item, or verification artifacts. |

Most repositories should begin with `core`. Move up only when the extra trace burden answers a real review, release, compliance, or maintenance need.

## Common Commands

```powershell
# Validate canonical artifacts with the low-burden baseline.
./scripts/Test-SpecTraceRepository.ps1 -Profile core

# Require every requirement to have at least one downstream trace link.
./scripts/Test-SpecTraceRepository.ps1 -Profile traceable

# Require verification coverage and stricter graph consistency.
./scripts/Test-SpecTraceRepository.ps1 -Profile auditable

# Emit a machine-readable repository catalog.
./scripts/Build-SpecTraceCatalog.ps1

# Resolve a portable topic view into machine-readable JSON.
./scripts/Resolve-SpecTraceTopicView.ps1 -TopicViewPath ./specs/requirements/spec-trace/SPEC-TOP.json

# Validate generated evidence snapshots.
./scripts/Validate-SpecTraceEvidence.ps1

# Render HTML and JSON attestation output.
./scripts/Render-SpecTraceAttestation.ps1 -Profile core -Emit both
```

The repository CI runs the core validation path, catalog build, evidence validation, attestation rendering, publish-mirror sync check, and tool tests.

## Release And Versioning

The reusable release surface is the curated JSON schema/template mirror under
[`publish/`](./publish/), not the full repository.

Release expectations:

- release tags use `v<major>.<minor>.<patch>` or a semver prerelease suffix
- run [`./scripts/Sync-PublishModule.ps1`](./scripts/Sync-PublishModule.ps1) before packaging or publishing
- run the local validation floor before tagging
- verify `git status --short --untracked-files=all -- publish` is empty after sync
- publish artifacts must contain only the curated `publish/` contents

The publish workflow creates a `spec-trace-publish-<version>.zip` archive from
`publish/`. Do not include local `.work*` directories, generated attestations,
source RFC extracts, or private work artifacts in release material.

## Key Rules

- Canonical authored artifacts are JSON documents.
- The authoritative JSON Schema is [`model/model.schema.json`](./model/model.schema.json).
- The authoritative standard is the SPEC suite under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/).
- Requirements live inside specification artifacts; they are not loose prose fragments.
- Requirement IDs and artifact IDs are the stable reference surface. Prefer IDs over file paths in trace links.
- `coverage` records expected evidence dimensions, not observed test results or code coverage.
- `trace` links record explicit relationships; backtick-delimited inline IDs are lightweight prose references.
- Generated reports, evidence snapshots, catalogs, and browsing views are derived material.
- Changes to field names, identifier rules, template shape, schema contracts, validator behavior, or example patterns need to update the affected specs, schemas, tooling, examples, and publish surfaces together.

## Repository Map

- [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) - canonical self-specification suite
- [`model/`](./model/) - authoritative JSON Schema and model notes
- [`examples/`](./examples/) - worked examples for payments, arithmetic, calculator, UI design systems, and BEM CSS
- [`schemas/`](./schemas/) - compatibility and slice schemas derived from the authoritative model
- [`catalog/retired-requirements.json`](./catalog/retired-requirements.json) - retired requirement ledger for lineage validation
- [`publish/`](./publish/) - curated reusable schema and template mirror
- [`src/SpecTrace.Tool/`](./src/SpecTrace.Tool/) - validation, catalog, evidence, and attestation tooling
- [`apps/spec-trace-mcp/`](./apps/spec-trace-mcp/) - deterministic Markdown-first MCP docs server

## Deeper Reading

- [`authoring.md`](./authoring.md) - task-oriented authoring workflow
- [`overview.md`](./overview.md) - practical model summary
- [`layout.md`](./layout.md) - repository layout guidance
- [`artifact-model-explainer.md`](./artifact-model-explainer.md) - plain-language artifact model
- [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) - profile and attestation details
- [`publish/README.md`](./publish/README.md) - reusable publish mirror
- [`apps/spec-trace-mcp/README.md`](./apps/spec-trace-mcp/README.md) - MCP docs server

## Readiness And Gaps

[`docs/maintainer-readiness.md`](./docs/maintainer-readiness.md) records the
current validation floor, downstream adoption path, and open gaps. Use it when
you need a concise handoff summary rather than the full standard.

## Contributing

Use [`CONTRIBUTING.md`](./CONTRIBUTING.md) for contribution expectations. In short: keep changes focused, keep the canonical SPEC suite and schema-aligned surfaces consistent, call out breaking changes to the standard, and validate the repository before review.

Security reports should follow [`SECURITY.md`](./SECURITY.md).
