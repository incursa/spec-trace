---
name: spec-trace-architecture-author
description: Draft or revise SpecTrace architecture or design artifacts. Use when explaining how requirements will be satisfied, linking design decisions back to `REQ-...` identifiers, or reshaping an `ARC-...` document without turning it into normative requirement text.
---

# Spec Trace Architecture Author

Use this skill to author design documents that explain how requirements are satisfied without redefining those requirements.

## Read First

- the relevant `SPEC-...` file
- `../../specs/requirements/spec-trace/SPEC-STD.md`
- `../../specs/requirements/spec-trace/SPEC-TPL.md`
- `../../architecture-template.md`
- the closest example architecture artifact under `../../examples/`

## Workflow

1. Start from the requirements being satisfied and keep those IDs visible throughout the document.
2. Use `../../architecture-template.md` as the base shape unless the repo already has a stronger local pattern.
3. Explain the chosen mechanism, state and data considerations, edge cases, constraints, rejected alternatives, and risks.
4. Keep the `satisfies` links accurate and update related artifacts when the design scope changes.
5. If the design introduces or changes normative behavior, update the specification instead of hiding the rule inside the architecture artifact.

## Guardrails

- Architecture explains satisfaction; it does not replace requirements.
- Prefer concrete design tradeoffs and invariants over management prose.
- If there is no requirement to satisfy, stop and create or locate the requirement first.
