# Maintainer Readiness

This document is the maintainer handoff for the `spec-trace` repository. It explains the local proof surface, release expectations, downstream adoption path, and known readiness gaps without replacing the canonical standard.

## Authority And Purpose

SpecTrace is a JSON-first standard for keeping requirements, trace links, validation rules, and derived evidence inside the repository they govern.

Authority order:

1. [`../specs/requirements/spec-trace/`](../specs/requirements/spec-trace/) is the canonical self-specification suite.
2. [`../model/model.schema.json`](../model/model.schema.json), root templates, examples, and [`../catalog/retired-requirements.json`](../catalog/retired-requirements.json) define the reusable artifact contract.
3. Tooling under [`../src/SpecTrace.Tool/`](../src/SpecTrace.Tool/) and compatibility schemas under [`../schemas/`](../schemas/) enforce and expose the model.
4. Root docs, `LLMS.txt`, and generated outputs are convenience surfaces.

If those layers disagree, fix the lower-authority layer or open a requirement gap before changing behavior.

## Core Concepts

- `specification`: canonical artifact that groups related requirements for a capability, interface, behavior area, or technical concern.
- `requirement`: stable `REQ-...` record inside a specification. Its `statement` is the normative, testable clause.
- `architecture`: design artifact that explains how requirements are intended to be satisfied.
- `work_item`: implementation-scope artifact connected to requirements, design inputs, and verification planning.
- `verification`: proof artifact that records how a requirement set was checked.
- `coverage`: authored expectation metadata on a requirement, using `positive`, `negative`, `edge`, and `fuzz`.
- `evidence snapshot`: generated `*.evidence.json` observation data. It does not replace canonical requirements or verification artifacts.
- `attestation`: generated HTML/JSON summary over canonical artifacts and evidence snapshots.
- `publish mirror`: curated reusable schema/template package under [`../publish/`](../publish/).

## Repository Layout

- [`../README.md`](../README.md): public front door.
- [`../authoring.md`](../authoring.md): task-oriented authoring workflow.
- [`../overview.md`](../overview.md): model summary.
- [`../layout.md`](../layout.md): downstream repository layout guidance.
- [`../specs/requirements/spec-trace/_index.md`](../specs/requirements/spec-trace/_index.md): canonical standard navigation.
- [`../examples/`](../examples/): worked examples for payments, arithmetic, calculator, UI design systems, and BEM CSS.
- [`../schemas/`](../schemas/): slice schemas and compatibility schemas derived from the authoritative model.
- [`../src/SpecTrace.Tool/`](../src/SpecTrace.Tool/): repository validator, catalog builder, evidence validator, topic-view resolver, and attestation renderer.
- [`../tools/SpecTrace.Rfc/`](../tools/SpecTrace.Rfc/): RFC intake and requirement-candidate workbench.
- [`../apps/spec-trace-mcp/`](../apps/spec-trace-mcp/): Markdown-first MCP documentation server.

## Command-Line Workflows

Use the PowerShell wrappers for normal local work:

```powershell
./scripts/Test-SpecTraceRepository.ps1 -Profile core
./scripts/Build-SpecTraceCatalog.ps1
./scripts/Validate-SpecTraceEvidence.ps1
./scripts/Render-SpecTraceAttestation.ps1 -Profile core -Emit both
```

The wrappers call the root .NET CLI:

```powershell
dotnet run --project src/SpecTrace.Tool -- validate --root . --profile core
dotnet run --project src/SpecTrace.Tool -- build-catalog --root .
dotnet run --project src/SpecTrace.Tool -- validate-evidence --root .
dotnet run --project src/SpecTrace.Tool -- generate-attestation --root . --profile core --emit both
```

Supported root commands:

- `validate`
- `build-catalog`
- `validate-evidence`
- `resolve-topic-view`
- `generate-attestation`

Use the RFC workbench wrapper for source-document conversion work:

```powershell
./tools/SpecTrace.Rfc/spec-rfc.ps1 ingest --rfc 9114 --out ./.work/rfc9114/source.json
./tools/SpecTrace.Rfc/spec-rfc.ps1 segment --source ./.work/rfc9114/source.json --out ./.work/rfc9114/source-ledger.jsonl
./tools/SpecTrace.Rfc/spec-rfc.ps1 extract --ledger ./.work/rfc9114/source-ledger.jsonl --out ./.work/rfc9114/candidates.jsonl --ai-mode off
./tools/SpecTrace.Rfc/spec-rfc.ps1 coverage-audit --ledger ./.work/rfc9114/source-ledger.jsonl --candidates ./.work/rfc9114/candidates.jsonl --out ./.work/rfc9114/review-decisions.jsonl --ai-mode off
./tools/SpecTrace.Rfc/spec-rfc.ps1 normalize --ledger ./.work/rfc9114/source-ledger.jsonl --review ./.work/rfc9114/review-decisions.jsonl --out ./.work/rfc9114/review-decisions.normalized.jsonl --ai-mode off
./tools/SpecTrace.Rfc/spec-rfc.ps1 assemble --ledger ./.work/rfc9114/source-ledger.jsonl --review ./.work/rfc9114/review-decisions.normalized.jsonl --spec-id SPEC-HTTP3-RFC9114 --domain http3 --capability http3-rfc9114 --out ./specs/requirements/http3/SPEC-HTTP3-RFC9114.json
./tools/SpecTrace.Rfc/spec-rfc.ps1 validate --root . --input-path ./specs/requirements/http3/SPEC-HTTP3-RFC9114.json --profile core
```

Use `--ai-mode off` for deterministic local smoke checks. Codex-backed extraction and review are review aids, not canonical proof.

## Local Validation Floor

Use local commands as proof. Do not rely on GitHub Actions for this repository readiness check.

```powershell
dotnet test spec-trace.sln
dotnet test tools/SpecTrace.Rfc/SpecTrace.Rfc.sln
./scripts/Test-SpecTraceRepository.ps1 -Profile core
./scripts/Build-SpecTraceCatalog.ps1
./scripts/Validate-SpecTraceEvidence.ps1
./scripts/Render-SpecTraceAttestation.ps1 -Profile core -Emit both
./scripts/Sync-PublishModule.ps1
git diff --check
```

Run stricter profiles when the repository claims that level:

```powershell
./scripts/Test-SpecTraceRepository.ps1 -Profile traceable
./scripts/Test-SpecTraceRepository.ps1 -Profile auditable
```

Those stricter profiles are expected to fail until every requirement has downstream trace links, and the `auditable` profile also requires verification coverage.

## Downstream Adoption

Downstream repositories should start with the lowest useful proof burden:

1. Copy or vendor the schema and templates from [`../publish/`](../publish/).
2. Place canonical specifications under `specs/requirements/<domain>/`.
3. Keep requirements as JSON records inside `SPEC-...` artifacts.
4. Use stable IDs for trace links instead of path-dependent references.
5. Run `core` validation first.
6. Add architecture, work-item, verification, evidence, and attestation surfaces only when they answer a real review or release need.

Do not copy this repository's self-specification suite into a downstream repo as local project requirements. Use the publish mirror for reusable schema/template material and author local requirements for the downstream project.

## Release And Versioning

The publishable package is the curated JSON schema/template mirror under [`../publish/`](../publish/), not the full repository.

Release expectations:

- release tags use `v<major>.<minor>.<patch>` or a semver prerelease suffix;
- run `./scripts/Sync-PublishModule.ps1` before packaging or publishing;
- run the local validation floor before tagging;
- verify `git status --short --untracked-files=all -- publish` is empty after sync;
- publish artifacts must contain only the curated `publish/` contents.

The publish workflow creates a `spec-trace-publish-<version>.zip` archive from `publish/`. Do not include local `.work*` directories, generated attestations, source RFC extracts, or private work artifacts in release material.

## Current Readiness Status

This repository is locally ready at the `core` profile when the validation floor above passes.

Observed local status for this readiness pass:

- root tool tests passed;
- RFC workbench tests passed;
- `core` profile passed for 33 artifacts and 274 requirements;
- catalog generation passed;
- evidence validation passed for 4 evidence files, 41 requirement entries, and 47 observations;
- core attestation generation passed for 22 specifications and 274 requirements;
- publish mirror sync completed.

The repository is not yet `traceable` or `auditable` as a whole. The stricter profile checks currently fail because many requirements, including UI example requirements and canonical SpecTrace requirements, do not yet have downstream trace links. The `auditable` profile also reports missing verification coverage for those requirements.

## Known Gaps And Follow-Up Work

- Complete downstream trace links before claiming repository-wide `traceable` readiness.
- Complete verification coverage before claiming repository-wide `auditable` readiness.
- Keep the RFC workbench deterministic smoke path separate from Codex-assisted extraction and review.
- Add `CONTRIBUTORS.md` or `AUTHORS.md` only if the project wants a maintained contributor ledger beyond git history.
- Add `SUPPORT.md`, `CODEOWNERS`, and Dependabot if maintainers want explicit support routing, review ownership, or automated dependency upkeep.
- Add a `specs/README.md` index if the canonical `specs/requirements/spec-trace/_index.md` stops being sufficient as the primary standard navigation surface.
