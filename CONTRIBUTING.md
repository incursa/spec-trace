# Contributing

This repository is the public reference standard for `incursa/spec-trace`. Changes should keep the standard lightweight, practical, and internally consistent.

## How To Propose Changes

1. Open an issue using the appropriate issue form.
2. Keep the proposal focused on one change or one coherent group of related changes.
3. Explain what is changing, why it is changing, and whether it affects canonical field names, identifier rules, templates, examples, or schemas.

## Pull Request Expectations

- Keep pull requests small and readable.
- Update the affected SPEC artifacts, examples, metadata catalog, and schemas together when the change touches canonical content.
- Link to the related issue or discussion when there is one.
- Call out any breaking change to the standard explicitly.

## Coherence Rules

- Keep canonical field names stable unless the change is deliberate and documented.
- Keep the distinction between specification, requirement, architecture, work item, and verification explicit.
- Keep the one-specification-per-file model aligned across the templates, schemas, and examples.
- Keep compact requirement clauses as the default authoring model.
- Keep the shared metadata catalog aligned with the identifier policy, including the requirement ID shape summary and grouping-key registry.
- Keep examples aligned with the schemas and the SPEC suite.
- Keep terminology consistent across the SPEC suite, README, metadata catalog, schemas, and the example artifacts.
- Keep AI-facing guidance such as [`AGENTS.md`](./AGENTS.md), [`LLMS.txt`](./LLMS.txt), and [`skills/`](./skills/) non-authoritative and aligned with the SPEC suite.
- Use relative links for repo-local references in Markdown. If the visible text should stay monospace, put backticks inside the link text. When the target is a specific requirement or other headed subsection inside a specification artifact document, include the relevant heading anchor or other supported sub-document locator. Use absolute URLs only for external destinations.
- Prefer direct wording over process language that adds little value.

## Breaking Changes

Changes to artifact structure, required fields, canonical field labels, identifier rules, or trace-link conventions are breaking changes to the standard.

If you make one of those changes:

- document the reason in the pull request
- update the examples and schemas in the same change
- update the changelog

## What Good Contributions Look Like

- a wording fix that removes ambiguity without changing meaning
- a tighter requirement clause that preserves behavior while improving testability
- a documentation fix that resolves an example mismatch
- a schema adjustment that matches the current templates
- a small improvement to public-facing guidance

## What To Avoid

- adding heavyweight governance
- introducing product-specific implementation details into the standard
- changing canonical names casually
- making the examples drift from the templates
