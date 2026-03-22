# SpecTrace Skills

This folder contains repo-local skills for AI-assisted authoring and maintenance. They are convenience helpers only. The authoritative requirements model remains the SPEC suite under `specs/requirements/spec-trace/`.

## How To Use Them

1. Start from `AGENTS.md` or `LLMS.txt`.
2. Pick the skill that matches the artifact you need to draft or revise.
3. Open the authoritative SPEC files, matching root template, and nearest example before writing.

If your AI tool only auto-discovers skills from a separate home directory, point it at these folders explicitly or mirror them into that environment.

For repository changes that affect traceability, run `scripts/Test-SpecTraceRepository.ps1` after editing the canonical files, schemas, or examples. Use `-Profile traceable` or `-Profile auditable` when you need the stricter repository policies.

## Included Skills

- `spec-trace-specification-author` drafts or revises `SPEC-...` files.
- `spec-trace-requirement-author` adds or tightens `REQ-...` clauses inside a specification.
- `spec-trace-architecture-author` drafts architecture or design artifacts linked to requirements.
- `spec-trace-work-item-author` drafts implementation-facing work items linked to requirements and design.
- `spec-trace-verification-author` drafts verification artifacts that record proof and outcome.
- `spec-trace-change-maintainer` keeps canonical and supporting surfaces aligned when the standard changes.

## Non-Authority Rule

Do not treat these skills as a second copy of the standard. When a skill, template, or root doc disagrees with `specs/requirements/spec-trace/`, the SPEC suite wins.
