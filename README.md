# Repository-Native Requirements and Traceability Standard

This is the public reference repository for `incursa/spec-trace`, a lightweight, Markdown-first standard for requirements and traceability.

It is for teams that want requirements, design, work items, and verification artifacts to live close to the codebase without adopting a heavyweight requirements platform.

## Repository Status

This is an early public reference release. The standard is intentionally small and practical, and the package is meant to be easy to read, copy, and adapt.

## What This Repo Solves

The repo gives you a simple way to:

- write requirements in Markdown
- keep design, work items, and verification linked by stable identifiers
- keep examples and schemas aligned with the standard
- publish a reference package that teams can copy into product repositories

Actual product requirements should live in the product repository. This repo is the reference standard, not the live product spec.

## Getting Started

1. Read `overview.md` for the standard rules and canonical field names.
2. Read `layout.md` for the recommended repository structure and naming conventions.
3. Open `examples/payments/` to see one complete worked example set.
4. Copy the templates into a product repo or use them as the basis for your own internal standard.

## Contents

- `overview.md` - the main standard document
- `layout.md` - the recommended repository layout and naming conventions
- `spec_template.md` - copy-ready specification template
- `architecture_template.md` - copy-ready architecture or design template
- `work_item_template.md` - copy-ready work-item template
- `verification_template.md` - copy-ready verification template
- `examples/` - one worked example set based on the ACH duplicate batch scenario
- `schemas/` - lightweight JSON Schema examples for front matter and trace links
- `.github/` - issue forms and pull request guidance for contributors
- `CHANGELOG.md` - package history for this reference repo
- `CONTRIBUTING.md` - how to propose changes to the standard
- `CODE_OF_CONDUCT.md` - project conduct expectations
- `SECURITY.md` - how to report security-related concerns
- `LICENSE` - Apache 2.0 license text

## How To Use The Standard

1. Use `overview.md` as the canonical source for the standard.
2. Use `layout.md` when organizing artifacts in a product repository.
3. Keep artifact IDs stable and link related artifacts by ID, not by prose alone.
4. Treat generated reports as derived outputs, not source-of-truth documents.

## How To Use The Templates

- Use `spec_template.md` for capability-level specification documents.
- Use `architecture_template.md` for design artifacts that explain how requirements are satisfied.
- Use `work_item_template.md` for implementation work that is traceable back to requirements and design.
- Use `verification_template.md` for lightweight verification records that show how a requirement was proven.

The templates are intentionally direct. They are meant to be copied and filled in, not wrapped in another process.

## How To Use The Examples

The `examples/payments/` folder contains a complete worked example derived from the ACH duplicate batch scenario already referenced in the standard.

Use it as:

- a formatting reference
- a trace-link reference
- a starting point for your own product artifacts

The example artifacts cross-link the same core requirement across specification, architecture, work item, and verification records.

## Versioning

This repository uses a simple public-reference versioning approach. The initial public package is `0.1.0`.

- `0.1.x` tracks the early public reference package while the standard settles.
- Changes to canonical field names or artifact structure should be treated as deliberate and documented.
- The changelog records package-level changes, not product-specific work.

## Contributing

See `CONTRIBUTING.md` for the practical rules for proposing changes, keeping examples and schemas aligned, and handling breaking changes to the standard.

## License

This repository is licensed under Apache-2.0. See `LICENSE`.

## Security

This repository is primarily documentation, templates, schemas, and examples. Most issues are specification or documentation issues and should go through normal issues or pull requests.

If you believe you found a security-sensitive problem in the repository contents or release process, follow `SECURITY.md`.
