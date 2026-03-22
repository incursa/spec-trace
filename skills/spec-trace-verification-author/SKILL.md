---
name: spec-trace-verification-author
description: Draft or revise SpecTrace verification artifacts. Use when recording how requirements were proven, updating `VER-...` documents with evidence or outcome, or organizing verification scope without rewriting the underlying requirements.
---

# Spec Trace Verification Author

Use this skill to author proof-oriented artifacts that show how requirements were verified and what evidence exists.

## Read First

- the relevant `SPEC-...` file
- the relevant architecture and work-item artifacts, if they exist
- `../../specs/requirements/spec-trace/SPEC-STD.md`
- `../../specs/requirements/spec-trace/SPEC-TPL.md`
- `../../verification-template.md`
- the closest example verification artifact under `../../examples/`

## Workflow

1. Start from the requirement IDs being verified.
2. Use `../../verification-template.md` as the base shape.
3. Describe the verification method, preconditions, approach, expected result, evidence, and status in tooling-agnostic language unless local tooling names are the evidence itself.
4. Keep `verifies` and related-artifact links accurate as the verification scope changes. If covered requirements do not share one outcome, split the scope into separate verification artifacts.
5. If verification exposes a requirement gap or design ambiguity, fix the upstream artifact rather than encoding the missing rule here.

## Guardrails

- Verification records proof; it does not redefine the requirement.
- Keep evidence specific enough to audit later.
- Treat failing or blocked verification as a signal to tighten traceability, not as permission to drift from the requirement.
