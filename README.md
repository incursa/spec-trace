# Repository-Native Requirements and Traceability Standard

`incursa/spec-trace` is a public reference standard for precise, Markdown-first software specifications.

The standard is optimized for compact requirement clauses with stable IDs and direct traceability to:

- architecture and design artifacts
- work items
- verification artifacts
- upstream source material
- generated evidence snapshots

The canonical standard lives in [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). The root documents are practical summaries and copy-ready templates. If a root document and the SPEC suite ever disagree, the SPEC suite wins.

The reference package also includes repository-wide validation through [`scripts/Test-SpecTraceRepository.ps1`](./scripts/Test-SpecTraceRepository.ps1), which checks identifier families, duplicate IDs, unresolved direct links, reciprocal trace consistency, namespace alignment, and profile-specific graph rules.

## Core Model

- The canonical static model uses four artifact families: specification, architecture, work item, and verification.
- Requirements are the smallest normative unit inside a specification.
- A specification groups related requirements for one capability, behavior area, interface, or narrow technical concern.
- A requirement is the smallest normative unit. It has a short descriptive title, a compact normative clause, and optional `Trace` and `Notes` sections.
- The title is a scan aid; the clause is the normative content.
- The clause should describe the required behavior, rule, or constraint. `Notes` carry rationale, clarification, and examples.
- The standard uses BCP 14-style uppercase requirement language inspired by RFC 2119 and RFC 8174. The approved set is `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`. Only uppercase forms are normative; lowercase spellings are plain English.
- Traceability is explicit. Related artifacts are linked by stable IDs in structured `Trace` fields, by inline backtick references in prose when a lightweight cross-link is enough, or by generated evidence snapshots when the link is tool-produced. Inline references are prose mentions, not trace edges.
- Structured `Trace` fields have typed semantics by family: `Satisfied By`, `Implemented By`, and `Verified By` are the canonical downstream trace; `Derived From` and `Supersedes` are lineage; `Upstream Refs` are upstream provenance; `Related` is a loose association.
- Generated evidence rollups, coverage views, and attestation snapshots are derived outputs. They answer current-status questions, but they are not canonical requirement text.
- Derived reporting can expose upstream, design, implementation, verification, and evidence-by-kind coverage dimensions without changing the canonical profile model.
- File-level front matter stays strict for the core keys, and repositories may add optional namespaced `x_...` keys for local extensions.
- Optional upstream lineage can record `Derived From`, `Supersedes`, and `Upstream Refs` when requirement history or external sources matter.
- The standard defines three repository-level conformance gates: `core`, `traceable`, and `auditable`. In practical terms, those read as `spec-valid`, `artifact-linked`, and `evidence-backed` respectively, but the canonical names remain `core`, `traceable`, and `auditable`.
- `core` is the low-burden baseline. `traceable` and `auditable` are stricter repository policies, not maturity labels or workflow stages.
- Richer coverage and attestation reporting lives in a separate derived-reporting layer described by [`SPEC-RPT.md`](./specs/requirements/spec-trace/SPEC-RPT.md).
- The core artifact families are specification, architecture, work item, and verification.
- Architecture artifacts are the default place for design rationale and decision tradeoffs.
- Work items describe implementation work, not requirement text.
- Verification artifacts record how requirements were checked and what shared outcome was recorded. A verification artifact has one status that applies to every requirement it lists. Generated evidence snapshots and local tooling may reference requirement IDs directly.
- `Verified By` means the requirement is covered by a verification artifact. It does not mean formal proof of correctness.
- Verification artifacts are proof-summary artifacts rather than hand-maintained duplicate test catalogs. The repository may use tests, manual QA, benchmarks, interoperability runs, security review, fuzzing, formal methods, or other evidence sources according to local policy.
- Decision records are not part of the core standard today; they may be added later as an optional local extension.
- The standard supports staged adoption without requiring fake historical work items or architecture artifacts that do not add repository truth.
- Each specification Markdown file contains one specification and one or more related `REQ-...` clauses under it.

For a compact operational explanation of the artifact model, read [`artifact-model-explainer.md`](./artifact-model-explainer.md). For the distinction between authored trace, dynamic attestation, and local repository policy, read [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md). The worked examples under [`examples/`](./examples/) show the same ideas in context.

The standard is intentionally small. It does not require a requirements platform, a workflow tool, Gherkin, or a test-framework abstraction layer.

## Getting Started

1. Read [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) for the canonical self-specification suite; if you are revising requirement evolution or upstream trace, also read [`specs/requirements/spec-trace/SPEC-LIN.md`](./specs/requirements/spec-trace/SPEC-LIN.md); if you are choosing a conformance profile, also read [`specs/requirements/spec-trace/SPEC-PRF.md`](./specs/requirements/spec-trace/SPEC-PRF.md); if you are shaping dashboards or attestation, also read [`specs/requirements/spec-trace/SPEC-RPT.md`](./specs/requirements/spec-trace/SPEC-RPT.md) and [`specs/requirements/spec-trace/SPEC-EVD.md`](./specs/requirements/spec-trace/SPEC-EVD.md).
2. Read [`overview.md`](./overview.md) for the compact authoring model.
3. Read [`layout.md`](./layout.md) for the recommended repository structure.
4. Read [`authoring.md`](./authoring.md) for the task-oriented authoring workflow across specifications, requirements, design, work items, and verification artifacts.
5. Read [`artifact-model-explainer.md`](./artifact-model-explainer.md) for a compact plain-language map of the artifact families and trace labels, then read [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) for the static-vs-dynamic and canonical-vs-local-policy distinction.
6. Copy from the root templates, such as [`spec-template.md`](./spec-template.md), [`architecture-template.md`](./architecture-template.md), [`work-item-template.md`](./work-item-template.md), and [`verification-template.md`](./verification-template.md), if you want a starting point for your own repo.
7. Use [`artifact-id-policy.json`](./artifact-id-policy.json) and the files under [`schemas/`](./schemas/) for machine-readable validation targets.
8. Open [`examples/README.md`](./examples/README.md) for worked examples, including a product-style payments example, a narrow arithmetic example, a tiny generated current-status rollup, and a dimension-oriented coverage rollup.
9. If you use AI-assisted authoring, point the agent at [`AGENTS.md`](./AGENTS.md), [`LLMS.txt`](./LLMS.txt), [`artifact-model-explainer.md`](./artifact-model-explainer.md), [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md), and the repo-local collection under [`skills/`](./skills/).

## Repository Contents

- [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/) - canonical SPEC suite and proving ground for the standard
- [`specs/requirements/spec-trace/SPEC-RPT.md`](./specs/requirements/spec-trace/SPEC-RPT.md) - canonical derived-reporting and attestation semantics
- [`specs/requirements/spec-trace/SPEC-EVD.md`](./specs/requirements/spec-trace/SPEC-EVD.md) - canonical generated-evidence snapshot semantics
- [`authoring.md`](./authoring.md) - task-oriented authoring guide that routes to the canonical suite, templates, and examples
- [`overview.md`](./overview.md) - concise summary of the authoring model
- [`layout.md`](./layout.md) - recommended repository layout and placement guidance
- [`spec-template.md`](./spec-template.md) - copy-ready specification template
- [`architecture-template.md`](./architecture-template.md) - copy-ready architecture or design template
- [`work-item-template.md`](./work-item-template.md) - copy-ready work-item template
- [`verification-template.md`](./verification-template.md) - copy-ready verification template
- [`artifact-id-policy.json`](./artifact-id-policy.json) - shared identifier policy and grouping-key registry
- [`schemas/`](./schemas/) - reference JSON Schemas for extracted metadata
- [`schemas/evidence-snapshot.schema.json`](./schemas/evidence-snapshot.schema.json) - reference JSON Schema for generated evidence snapshots
- [`examples/`](./examples/) - worked examples that apply the standard directly
- [`scripts/Export-SpecTraceBundle.ps1`](./scripts/Export-SpecTraceBundle.ps1) - PowerShell utility that bundles discovered specification files into one Markdown output
- [`scripts/Test-SpecTraceRepository.ps1`](./scripts/Test-SpecTraceRepository.ps1) - repository-wide validator for Markdown, schema, and cross-file trace rules; use `-Profile core|traceable|auditable`, `-InputPath`, and `-JsonReportPath`
- [`scripts/Validate-SpecTrace.ps1`](./scripts/Validate-SpecTrace.ps1) - narrower reference-package validator kept for compatibility and bundle-aligned checks
- [`AGENTS.md`](./AGENTS.md) - agent-oriented repository instructions that defer to the canonical SPEC suite
- [`LLMS.txt`](./LLMS.txt) - plain-text AI bootstrap for llms.txt-style or prompt-bootstrap workflows
- [`artifact-model-explainer.md`](./artifact-model-explainer.md) - compact plain-language map of artifact families and trace labels
- [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) - plain-language explanation of static trace, dynamic attestation, and local policy terms
- [`skills/`](./skills/) - repo-local authoring skills that help agents draft artifacts without re-implementing the standard

## AI-Assisted Authoring

This repository includes a small AI-facing convenience layer for teams that want repo-local agent instructions and reusable authoring skills.

- [`AGENTS.md`](./AGENTS.md) gives repository-specific instructions to coding and documentation agents.
- [`LLMS.txt`](./LLMS.txt) provides a lightweight plain-text bootstrap that points tools back to the canonical suite.
- [`skills/`](./skills/) contains repo-local skills for drafting specifications, requirements, architecture artifacts, work items, verification artifacts, and cross-surface maintenance.

Those files are ergonomic helpers only. They must remain aligned with the SPEC suite and they must not become a second source of truth.

## Why This Exists

The standard is meant to answer practical software questions with minimal ceremony:

- Which requirements have unit-test evidence?
- Which requirements have code evidence?
- Which requirements are missing verification?
- Which evidence observations exist because of which requirements?
- Which code paths were introduced to satisfy which rules or edge cases?
- Which source documents or source regions are covered by requirements?
- Which requirements are grounded only by evidence versus linked delivery artifacts?

That only works if requirement IDs are stable, requirement clauses are compact, trace links use the right identifier families, and repository-wide validation keeps the graph consistent.

## Self-Application

This repository uses the standard to specify itself under [`specs/requirements/spec-trace/`](./specs/requirements/spec-trace/). That recursive self-application is intentional. The SPEC suite is not an example pasted beside the standard; it is the standard expressed in its own form.

## Versioning

This repository uses a simple public-reference versioning approach.

- `0.1.x` tracks the early public reference package while the standard settles.
- Changes to canonical field names, identifier rules, or required structures are breaking changes and should be documented.
- [`CHANGELOG.md`](./CHANGELOG.md) records package-level changes to the reference standard.

## Contributing

See [`CONTRIBUTING.md`](./CONTRIBUTING.md) for the rules for proposing changes and keeping the SPEC suite, templates, schemas, and examples aligned.

## License

This repository is licensed under Apache-2.0. See [`LICENSE`](./LICENSE).

## Security

This repository is primarily documentation, schemas, examples, and policy files. Most issues should go through the normal issue or pull-request flow.

If you believe you found a security-sensitive problem in the repository contents or release process, follow [`SECURITY.md`](./SECURITY.md).
