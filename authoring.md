# Authoring Guide

This guide is a practical entry point for people and agents working with `spec-trace`. It is not normative. The canonical standard lives under `specs/requirements/spec-trace/`.

## Authority Model

Use the repository in this order:

1. `specs/requirements/spec-trace/` for the authoritative model.
2. Root templates, schemas, and examples for copy-ready support.
3. This guide, `README.md`, `AGENTS.md`, `LLMS.txt`, and `skills/` for ergonomic navigation.

If any convenience layer disagrees with the SPEC suite, the SPEC suite wins.

## Choose The Artifact

### Specification

Use a specification when you need to define one or more related requirements for a capability, behavior area, interface, or narrow technical concern.

Read first:

- `specs/requirements/spec-trace/_index.md`
- `specs/requirements/spec-trace/SPEC-STD.md`
- `specs/requirements/spec-trace/SPEC-TPL.md`
- `specs/requirements/spec-trace/SPEC-LAY.md`
- `spec-template.md`
- a nearby example specification in `examples/`

### Requirement

Use a requirement when you need one atomic normative statement. Requirements do not stand alone; they live inside a specification.
Requirement clauses use the approved BCP 14-style uppercase keyword set only. Lowercase spellings are plain English.
Requirement clauses may also use backtick-delimited inline identifier references to stable artifact IDs when a lightweight cross-link is helpful. Inline references are allowed in the clause, `Notes`, and other descriptive sections, but they do not replace `Trace` fields or lineage labels.
Common uses include requirement-to-requirement links, requirement-to-specification links, and inline references in architecture, work-item, or verification prose.

Use the title as a short descriptive label for the obligation or concern. Use the clause for the required behavior. Use `Notes` for rationale, clarifications, caveats, and examples.
Use `Trace` for typed downstream links, lineage, source citations, implementation-specific references, and loose associations.

Read first:

- the owning `SPEC-...` file
- `specs/requirements/spec-trace/SPEC-LIN.md` when the task changes requirement lineage, split/merge behavior, or upstream sources
- `specs/requirements/spec-trace/SPEC-STD.md`
- `specs/requirements/spec-trace/SPEC-TPL.md`
- `spec-template.md`

### Conformance Profile

Use the profiles spec when you need to decide how strict a repository's traceability policy should be.

Read first:

- `specs/requirements/spec-trace/SPEC-PRF.md`
- `specs/requirements/spec-trace/SPEC-STD.md`
- `specs/requirements/spec-trace/SPEC-SCH.md`

Core keeps the authoring burden low. `traceable` and `auditable` are stricter repository policies, not new artifact families.

### Architecture Or Design Artifact

Use an architecture artifact when you need to explain how requirements will be satisfied, including rationale and tradeoffs, without redefining them. Decision records are not part of the core standard today; if a repository adopts them, treat them as an optional local extension rather than a replacement for architecture artifacts.

Read first:

- the relevant `SPEC-...` file
- `architecture-template.md`
- a nearby example architecture artifact in `examples/`

### Work Item

Use a work item when you need to describe implementation work, delivery scope, and trace links back to requirements and design inputs.

Read first:

- the relevant `SPEC-...` file
- the relevant architecture artifact, if one exists
- `work-item-template.md`
- a nearby example work item in `examples/`

### Verification Artifact

Use a verification artifact when you need to record how requirements were proven and what the shared outcome was. If the requirements do not share one outcome, split the verification scope across multiple artifacts.

Read first:

- the relevant `SPEC-...` file
- the relevant architecture and work-item artifacts, if they exist
- `verification-template.md`
- a nearby example verification artifact in `examples/`

## Recommended Workflow

1. Start with the authoritative SPEC files for the task.
2. Open the matching template.
3. Open the closest example in `examples/`.
4. Draft or revise the artifact.
5. Run `scripts/Test-SpecTraceRepository.ps1` and check that trace links point at stable IDs rather than loose prose, with no duplicate IDs, unresolved references, reciprocal mismatches, or namespace drift. Use `-Profile traceable` or `-Profile auditable` when you want the stricter repository policies, and `-JsonReportPath` when you need a machine-readable report.
   Also check that any inline identifier references are backtick-delimited stable IDs rather than loose prose.

## When The Standard Changes

If a change affects canonical field names, identifier rules, template shape, schema contracts, or example patterns, update the affected surfaces together:

- the relevant files under `specs/requirements/spec-trace/`
- the root templates
- the schemas
- the examples
- root guidance such as `README.md`, `overview.md`, and `layout.md`
- repository validation such as `scripts/Test-SpecTraceRepository.ps1`
- AI convenience surfaces such as `AGENTS.md`, `LLMS.txt`, and `skills/`

If a repository needs extra front matter metadata, prefer namespaced `x_...` keys so the core field set stays stable.

Record notable package-level changes in `CHANGELOG.md`.

## AI Entry Points

If you want AI tooling to work directly from this repository:

- use `AGENTS.md` for repo-specific instructions
- use `LLMS.txt` for a lightweight bootstrap file
- use `skills/README.md` to choose a repo-local authoring skill
