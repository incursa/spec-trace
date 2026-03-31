# Repository-Native Requirements and Traceability Standard

`incursa/spec-trace` is a public reference standard for precise, repository-native software specifications.

The repository is now CUE-first:

- canonical authored artifacts are `.cue`
- shared schemas and policies live in the importable [`model/`](./model/) package
- schema-backed authoring templates live in the module-root `templates` package
- generated Markdown is a readable projection, not the source of truth
- JSON and YAML are compatibility or integration formats, not primary authoring formats
- the pinned CUE CLI is bootstrapped from GitHub release assets by [`scripts/Resolve-Cue.ps1`](./scripts/Resolve-Cue.ps1), so the repo does not require a Go toolchain just to validate artifacts

The canonical standard lives under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). Each canonical spec artifact is authored as CUE and has a generated Markdown sibling for normal browsing and review.

## Core Model

- The canonical static model uses four artifact families: specification, architecture, work item, and verification.
- Requirements are the smallest normative unit and live inside a specification artifact.
- Requirement clauses use the approved uppercase keyword set: `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`.
- Structured trace uses stable IDs. `Satisfied By`, `Implemented By`, and `Verified By` are downstream trace. `Derived From` and `Supersedes` are lineage. `Upstream Refs` are provenance. `Related` is a loose association.
- Generated evidence snapshots, coverage rollups, attestation views, and Markdown renderings are derived outputs.
- The repository-level conformance profiles are `core`, `traceable`, and `auditable`.

## Canonical Authoring

Canonical authored artifacts are CUE values with a top-level `artifact` unified against the shared model:

- specifications: [`model.#Specification`](./model/model.cue)
- architecture artifacts: [`model.#Architecture`](./model/model.cue)
- work items: [`model.#WorkItem`](./model/model.cue)
- verification artifacts: [`model.#Verification`](./model/model.cue)

The published module path is `github.com/incursa/spec-trace@v0`. Consumers can import:

- `github.com/incursa/spec-trace/model@v0` for the canonical schema definitions
- `github.com/incursa/spec-trace@v0:templates` for the schema-backed root template definitions (`templates.#SpecificationTemplate`, `templates.#ArchitectureTemplate`, `templates.#WorkItemTemplate`, `templates.#VerificationTemplate`)

The Central Registry package is built from the dedicated [`publish/`](./publish/) submodule, so it contains only the reusable schema and template definitions plus package metadata. It does not publish the canonical SPEC suite, examples, generated artifacts, or repository tooling.

Concrete artifacts live under [`specs/`](./specs/) and [`examples/`](./examples/). Generated Markdown sits beside the `.cue` source for readability, but contributors should edit the `.cue` file.

## Commands

```powershell
./scripts/Resolve-Cue.ps1
./scripts/Test-SpecTraceRepository.ps1 -Profile core
./scripts/Render-SpecTraceMarkdown.ps1 -Check
./scripts/Build-SpecTraceCatalog.ps1
./scripts/Validate-SpecTraceEvidence.ps1
./scripts/Render-SpecTraceAttestation.ps1
```

Useful variants:

- `./scripts/Test-SpecTraceRepository.ps1 -Profile traceable`
- `./scripts/Test-SpecTraceRepository.ps1 -Profile auditable`
- `./scripts/Test-SpecTraceRepository.ps1 -JsonReportPath ./specs/generated/validation-report.json`
- `./scripts/Render-SpecTraceMarkdown.ps1`
- `./scripts/Convert-MarkdownArtifactsToCue.ps1 -InputPath ./examples/payments`
- `./scripts/Export-SpecTraceBundle.ps1 -InputPath ./specs/requirements/spec-trace -OutputPath ./specs/generated/spec-bundle.md`
- `./scripts/Validate-SpecTraceEvidence.ps1 -EvidencePath ./examples/arithmetic/generated/division-evidence.evidence.json`
- `./scripts/Render-SpecTraceAttestation.ps1 -Profile core -OutDir ./artifacts/spec-trace/attestation`

## Repository Contents

- [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) - canonical self-specification suite, authored in CUE with generated Markdown siblings
- [`model/`](./model/) - canonical shared CUE definitions for artifacts, requirements, evidence, retired-ID ledgers, and generated catalogs
- [`cue.mod/`](./cue.mod/) - root CUE module
- [`publish/`](./publish/) - dedicated publish submodule for the reusable Central Registry package
- [`catalog/retired-requirements.cue`](./catalog/retired-requirements.cue) - retired requirement ledger used during lineage validation
- [`spec-template.cue`](./spec-template.cue), [`architecture-template.cue`](./architecture-template.cue), [`work-item-template.cue`](./work-item-template.cue), [`verification-template.cue`](./verification-template.cue) - schema-backed template definitions for the published root `templates` package
- [`schemas/`](./schemas/) - compatibility JSON Schemas and related exports; helpful for integrations, not authoritative over CUE
- [`examples/`](./examples/) - worked examples across all major artifact families
- [`src/SpecTrace.Tool/`](./src/SpecTrace.Tool/) - migration, validation, catalog, evidence-validation, attestation, and Markdown-generation toolchain
- [`tests/SpecTrace.Tool.Tests/`](./tests/SpecTrace.Tool.Tests/) - automated tests covering valid and invalid repositories

## Reading Order

1. [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) for the canonical standard.
2. [`authoring.md`](./authoring.md) for the day-to-day workflow.
3. [`overview.md`](./overview.md) for a compact conceptual summary.
4. [`layout.md`](./layout.md) for placement and file-layout guidance.
5. [`examples/README.md`](./examples/README.md) for worked examples.

## Validation And Generation

The repository validation path is deterministic and local:

- CUE enforces artifact structure, required fields, allowed enums, and identifier regexes.
- the .NET validator builds a repository-wide in-memory catalog of artifact IDs and nested requirement IDs
- validation fails on invalid structure, invalid IDs, duplicate IDs, broken cross-file references, and wrong target kinds
- evidence snapshots are vetted against the canonical CUE schema and then checked against the repository requirement catalog
- attestation reports merge overlapping evidence snapshots additively and emit deterministic `index.html`, `summary.html`, `details.html`, per-spec pages, and `attestation.json`
- `build-catalog` can emit the catalog as JSON and CUE for diagnostics or downstream tooling
- Markdown is generated from canonical CUE and can be checked for drift in CI

## AI And Convenience Surfaces

- [`AGENTS.md`](./AGENTS.md) is the repo-local agent guide.
- [`LLMS.txt`](./LLMS.txt) is the lightweight bootstrap file.
- [`artifact-model-explainer.md`](./artifact-model-explainer.md) and [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) explain the model in plain language.
- [`skills/`](./skills/) contains repo-local authoring helpers.

Those files are convenience surfaces only. They must stay aligned with the canonical CUE-authored SPEC suite.

## Publishing

The repository is prepared for Central Registry publishing through [`.github/workflows/publish-module.yml`](./.github/workflows/publish-module.yml).

- Push a semver tag such as `v0.1.0`, or run the workflow manually with a version input.
- Add a Central Registry trust entry for this repository and workflow so GitHub Actions can exchange its OIDC token for a short-lived registry token.
- The workflow synchronizes [`publish/`](./publish/) from the canonical root model and templates, validates the repository, checks that the publish submodule stays tidy, performs a dry run from that submodule, and then publishes from `publish/`.
