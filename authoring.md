# Authoring Guide

This guide is a practical entry point for people and agents working with `spec-trace`. It is not normative. The canonical standard lives under `specs/requirements/spec-trace/`.

## Authority Model

Use the repository in this order:

1. `specs/requirements/spec-trace/` for the authoritative model.
2. Root templates, schemas, and examples for copy-ready support.
3. This guide, `README.md`, `AGENTS.md`, `LOM.txt`, and `skills/` for ergonomic navigation.

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

Read first:

- the owning `SPEC-...` file
- `specs/requirements/spec-trace/SPEC-STD.md`
- `specs/requirements/spec-trace/SPEC-TPL.md`
- `spec-template.md`

### Architecture Or Design Artifact

Use an architecture artifact when you need to explain how requirements will be satisfied without redefining them.

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

Use a verification artifact when you need to record how requirements were proven and what the outcome was.

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
5. Check that trace links point at stable IDs rather than loose prose.

## When The Standard Changes

If a change affects canonical field names, identifier rules, template shape, schema contracts, or example patterns, update the affected surfaces together:

- the relevant files under `specs/requirements/spec-trace/`
- the root templates
- the schemas
- the examples
- root guidance such as `README.md`, `overview.md`, and `layout.md`
- AI convenience surfaces such as `AGENTS.md`, `LOM.txt`, and `skills/`

Record notable package-level changes in `CHANGELOG.md`.

## AI Entry Points

If you want AI tooling to work directly from this repository:

- use `AGENTS.md` for repo-specific instructions
- use `LOM.txt` for a lightweight bootstrap file
- use `skills/README.md` to choose a repo-local authoring skill
