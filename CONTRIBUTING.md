# Contributing

This repository is the public reference standard for `incursa/spec-trace`. Changes should keep the standard lightweight, practical, and internally consistent.

## How To Propose Changes

1. Open an issue using the appropriate issue form.
2. Keep the proposal focused on one change or one coherent group of related changes.
3. Explain what is changing, why it is changing, and whether it affects canonical field names, artifact structure, examples, or schemas.

## Pull Request Expectations

- Keep pull requests small and readable.
- Update the affected standard docs, templates, examples, and schemas together when the change touches canonical content.
- Link to the related issue or discussion when there is one.
- Call out any breaking change to the standard explicitly.

## Coherence Rules

- Keep canonical field names stable unless the change is deliberate and documented.
- Keep examples aligned with the templates and the schemas.
- Keep terminology consistent across `overview.md`, `layout.md`, the templates, and the example artifacts.
- Prefer direct wording over process language that adds little value.

## Breaking Changes

Changes to artifact structure, required fields, canonical field labels, or trace-link conventions are breaking changes to the standard.

If you make one of those changes:

- document the reason in the pull request
- update the examples and schemas in the same change
- update the changelog

## What Good Contributions Look Like

- a wording fix that removes ambiguity without changing meaning
- a documentation fix that resolves an example mismatch
- a schema adjustment that matches the current templates
- a small improvement to public-facing guidance

## What To Avoid

- adding heavyweight governance
- introducing product-specific implementation details into the standard
- changing canonical names casually
- making the examples drift from the templates
