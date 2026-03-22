# Repository-Native Requirements and Traceability Standard

`incursa/spec-trace` is a public reference standard for precise, Markdown-first software specifications.

The standard is optimized for compact requirement clauses with stable IDs and direct traceability to:

- architecture and design artifacts
- work items
- verification artifacts
- tests
- code references

The canonical standard lives in `specs/requirements/spec-trace/`. The root documents are practical summaries and copy-ready templates. If a root document and the SPEC suite ever disagree, the SPEC suite wins.

## Core Model

- A specification is a document that groups related requirements for one capability, behavior area, interface, or narrow technical concern.
- A requirement is the smallest normative, testable statement in the system.
- The requirement clause is the normative content.
- Traceability is explicit. Related artifacts are linked by stable IDs or implementation-specific string references, not by loose prose.
- Verification proves a requirement. Tests may reference requirement IDs directly. Code may reference requirement IDs directly.
- Each specification Markdown file contains one specification and one or more related `REQ-...` clauses under it.

The standard is intentionally small. It does not require a requirements platform, a workflow tool, Gherkin, or a test-framework abstraction layer.

## Getting Started

1. Read `specs/requirements/spec-trace/` for the canonical self-specification suite.
2. Read [overview.md](overview.md) for the compact authoring model.
3. Read [layout.md](layout.md) for the recommended repository structure.
4. Read [authoring.md](authoring.md) for the task-oriented authoring workflow across specifications, requirements, design, work items, and verification artifacts.
5. Copy from the root templates if you want a starting point for your own repo.
6. Use [artifact-id-policy.json](artifact-id-policy.json) and the files under [schemas/](schemas/) for machine-readable validation targets.
7. Open [examples/README.md](examples/README.md) for worked examples, including a product-style payments example and a narrow arithmetic example.
8. If you use AI-assisted authoring, point the agent at [AGENTS.md](AGENTS.md), [LLMS.txt](LLMS.txt), and the repo-local collection under [skills/](skills/).

## Repository Contents

- `specs/requirements/spec-trace/` - canonical SPEC suite and proving ground for the standard
- [authoring.md](authoring.md) - task-oriented authoring guide that routes to the canonical suite, templates, and examples
- [overview.md](overview.md) - concise summary of the authoring model
- [layout.md](layout.md) - recommended repository layout and placement guidance
- [spec-template.md](spec-template.md) - copy-ready specification template
- [architecture-template.md](architecture-template.md) - copy-ready architecture or design template
- [work-item-template.md](work-item-template.md) - copy-ready work-item template
- [verification-template.md](verification-template.md) - copy-ready verification template
- [artifact-id-policy.json](artifact-id-policy.json) - shared identifier policy and grouping-key registry
- [schemas/](schemas/) - reference JSON Schemas for extracted metadata
- [examples/](examples/) - worked examples that apply the standard directly
- [scripts/Export-SpecTraceBundle.ps1](scripts/Export-SpecTraceBundle.ps1) - PowerShell utility that bundles discovered specification files into one Markdown output
- [AGENTS.md](AGENTS.md) - agent-oriented repository instructions that defer to the canonical SPEC suite
- [LLMS.txt](LLMS.txt) - plain-text AI bootstrap for llms.txt-style or prompt-bootstrap workflows
- [skills/](skills/) - repo-local authoring skills that help agents draft artifacts without re-implementing the standard

## AI-Assisted Authoring

This repository includes a small AI-facing convenience layer for teams that want repo-local agent instructions and reusable authoring skills.

- `AGENTS.md` gives repository-specific instructions to coding and documentation agents.
- `LLMS.txt` provides a lightweight plain-text bootstrap that points tools back to the canonical suite.
- `skills/` contains repo-local skills for drafting specifications, requirements, architecture artifacts, work items, verification artifacts, and cross-surface maintenance.

Those files are ergonomic helpers only. They must remain aligned with the SPEC suite and they must not become a second source of truth.

## Why This Exists

The standard is meant to answer practical software questions with minimal ceremony:

- Which requirements have tests?
- Which requirements have implementation references?
- Which requirements are missing verification?
- Which tests exist because of which requirements?
- Which code paths were introduced to satisfy which rules or edge cases?

That only works if requirement IDs are stable, requirement clauses are compact, and trace links are explicit.

## Self-Application

This repository uses the standard to specify itself under `specs/requirements/spec-trace/`. That recursive self-application is intentional. The SPEC suite is not an example pasted beside the standard; it is the standard expressed in its own form.

## Versioning

This repository uses a simple public-reference versioning approach.

- `0.1.x` tracks the early public reference package while the standard settles.
- Changes to canonical field names, identifier rules, or required structures are breaking changes and should be documented.
- `CHANGELOG.md` records package-level changes to the reference standard.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the rules for proposing changes and keeping the SPEC suite, templates, schemas, and examples aligned.

## License

This repository is licensed under Apache-2.0. See `LICENSE`.

## Security

This repository is primarily documentation, schemas, examples, and policy files. Most issues should go through the normal issue or pull-request flow.

If you believe you found a security-sensitive problem in the repository contents or release process, follow `SECURITY.md`.
