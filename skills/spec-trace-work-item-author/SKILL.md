---
name: spec-trace-work-item-author
description: Draft or revise SpecTrace work-item artifacts. Use when describing implementation work linked to requirements and design inputs, updating `WI-...` trace links, or shaping delivery-facing documents without turning them into requirements or design specs.
---

# Spec Trace Work Item Author

Use this skill to author implementation-facing work items that stay linked to requirements, design inputs, and verification artifacts.

## Read First

- the relevant `SPEC-...` file
- the relevant architecture artifact, if one exists
- `../../specs/requirements/spec-trace/SPEC-STD.md`
- `../../specs/requirements/spec-trace/SPEC-TPL.md`
- `../../work-item-template.md`
- the closest example work item under `../../examples/`

## Workflow

1. Start from the requirement IDs being addressed and the design inputs being used.
2. Use `../../work-item-template.md` as the base shape.
3. Describe the planned changes, out-of-scope items, and verification plan in implementation terms rather than normative requirement language.
4. Keep front matter and the `Trace Links` section aligned with the same IDs.
5. If the work item reveals a missing requirement or missing design decision, fix that upstream artifact instead of burying it here.

## Guardrails

- A work item describes work; it is not the requirement.
- Keep trace links explicit and stable.
- Do not let delivery status or implementation detail become the normative source of behavior.
