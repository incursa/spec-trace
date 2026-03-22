---
name: spec-trace-change-maintainer
description: Keep SpecTrace canonical and supporting surfaces aligned. Use when changing field names, identifier rules, template shape, schema contracts, example patterns, or any repository-wide semantics that must be propagated across specs, templates, schemas, examples, docs, and AI convenience files.
---

# Spec Trace Change Maintainer

Use this skill when a request affects the standard itself or any surface that mirrors the standard.

## Read First

- `../../specs/requirements/spec-trace/_index.md`
- `../../specs/requirements/spec-trace/SPEC-STD.md`
- `../../specs/requirements/spec-trace/SPEC-TPL.md`
- `../../specs/requirements/spec-trace/SPEC-SCH.md`
- `../../specs/requirements/spec-trace/SPEC-EXM.md`
- `../../specs/requirements/spec-trace/SPEC-PRF.md`
- `../../CONTRIBUTING.md`
- `../../schemas/README.md`
- the affected root templates, examples, and summary docs

## Workflow

1. Classify the request as either a canonical change or a support-surface cleanup.
2. If the change is canonical, update the authoritative SPEC files first.
3. Propagate the result through templates, schemas, examples, root docs, changelog, and AI convenience surfaces such as `../../AGENTS.md`, `../../LLMS.txt`, and `../../skills/`.
4. Check for drift in terminology, field names, identifier shapes, trace labels, and example coverage.
5. Run `../../scripts/Test-SpecTraceRepository.ps1` and call out duplicate IDs, unresolved links, reciprocal mismatches, namespace drift, or profile failures explicitly.

## Guardrails

- Never let convenience docs outrun the canonical SPEC suite.
- If a source is ambiguous, tighten the canonical spec instead of smuggling the rule into a template or skill.
- Keep change sets coherent so reviewers can verify alignment across surfaces.
