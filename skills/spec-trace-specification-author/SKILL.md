---
name: spec-trace-specification-author
description: Draft or revise SpecTrace specification Markdown files. Use when creating a new `SPEC-...` document, regrouping related requirements inside a specification, or checking that a specification file follows the repository's authoritative requirements model without treating this skill as normative.
---

# Spec Trace Specification Author

Use this skill to draft or revise a specification file while keeping the repository's SPEC suite authoritative.

## Read First

- [`../../specs/requirements/spec-trace/_index.md`](../../specs/requirements/spec-trace/_index.md)
- [`../../specs/requirements/spec-trace/SPEC-STD.md`](../../specs/requirements/spec-trace/SPEC-STD.md)
- [`../../specs/requirements/spec-trace/SPEC-TPL.md`](../../specs/requirements/spec-trace/SPEC-TPL.md)
- [`../../specs/requirements/spec-trace/SPEC-LAY.md`](../../specs/requirements/spec-trace/SPEC-LAY.md)
- [`../../spec-template.md`](../../spec-template.md)
- the closest example specification under [`../../examples/`](../../examples/)

## Workflow

1. Decide whether the request belongs in an existing `SPEC-...` file. Extend an existing specification when the capability or concern already fits its purpose and scope.
2. If a new specification is warranted, start from [`../../spec-template.md`](../../spec-template.md) and place the file under the appropriate domain path.
3. Keep document-level framing in front matter plus `Purpose`, `Scope`, and `Context`. Put normative behavior in the `REQ-...` sections inside the file.
4. Keep related requirements together. If the request is really design, implementation, or proof, move that content into an architecture, work-item, or verification artifact instead of bloating the specification.
5. If the change affects the standard itself, update the impacted templates, schemas, examples, and root guidance in the same change set.

## Guardrails

- `../../specs/requirements/spec-trace/` is authoritative. If this skill or any root document disagrees, follow the SPEC suite and fix the subordinate surface.
- Do not create stand-alone requirement documents.
- Prefer the nearest worked example over inventing a new specification shape.
