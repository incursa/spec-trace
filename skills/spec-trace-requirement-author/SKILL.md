---
name: spec-trace-requirement-author
description: Draft or revise SpecTrace requirement clauses inside specification files. Use when adding a new `REQ-...` clause, splitting or tightening an existing clause, or converting loose prose into a canonical requirement without creating a separate requirement document.
---

# Spec Trace Requirement Author

Use this skill for atomic requirement authoring inside a specification document.

## Read First

- [`../../specs/requirements/spec-trace/SPEC-STD.md`](../../specs/requirements/spec-trace/SPEC-STD.md)
- [`../../specs/requirements/spec-trace/SPEC-TPL.md`](../../specs/requirements/spec-trace/SPEC-TPL.md)
- the owning `SPEC-...` file
- [`../../spec-template.md`](../../spec-template.md)
- the closest example specification under [`../../examples/`](../../examples/)

## Workflow

1. Find the owning specification. If none exists, switch to `spec-trace-specification-author` and create or extend a `SPEC-...` file first.
2. Read neighboring requirements so the new clause matches the document's scope and abstraction level.
3. Write or revise the `REQ-...` heading and clause using the canonical grammar defined in [`SPEC-TPL.md`](../../specs/requirements/spec-trace/SPEC-TPL.md).
4. Add or update `Trace` only when design, implementation, verification, test, or code links materially help. Keep `Notes` clarifying rather than normative.
5. If the wording turns into implementation detail or proof procedure, move that material out to architecture, work-item, or verification artifacts and tighten the requirement.

## Guardrails

- Requirements live inside specifications.
- Keep the clause itself as the normative content.
- If the request actually changes the standard model, use `spec-trace-change-maintainer` instead of hiding the change in one requirement.
