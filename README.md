# Repository-Native Requirements and Traceability Standard

This repository is the public reference package for `incursa/spec-trace`, a lightweight, Markdown-first standard for requirements and traceability.

The files here define the standard, ship copy-ready templates, and provide strict reference schemas plus a worked example set. The shared artifact ID policy lives in [artifact-id-policy.json](artifact-id-policy.json). The `/specs/...` layout in [layout.md](layout.md) is the recommended structure for product repositories that adopt the standard; it is not the required layout of this reference package itself.

## Repository Status

This is an early public `0.1.x` reference release. The standard is intentionally small and practical, and the package is meant to be easy to read, copy, and adapt.

## What This Repo Solves

The repo gives you a simple way to:

- write requirements in Markdown
- keep design, work items, and verification linked by stable identifiers
- keep examples and schemas aligned with the standard
- publish a reference package that teams can copy into product repositories

Actual product requirements should live in the product repository. This repo is the reference standard package, not the live product spec.

## Getting Started

1. Read [overview.md](overview.md) for the standard rules and canonical field names.
2. Read [layout.md](layout.md) for the recommended repository structure and naming conventions for adopting product repos, including nested grouping levels.
3. Open [examples/README.md](examples/README.md) and [examples/payments/](examples/payments/) to see one complete worked example set.
4. Copy the templates into `/specs/templates/` and the [artifact-id-policy.json](artifact-id-policy.json) file into the repo root, or use them as the basis for your own internal standard.

## Contents

- [overview.md](overview.md) - the main standard document
- [artifact-id-policy.json](artifact-id-policy.json) - shared artifact ID policy metadata
- [layout.md](layout.md) - the recommended product-repository layout and naming conventions
- [spec-template.md](spec-template.md) - copy-ready specification template
- [architecture-template.md](architecture-template.md) - copy-ready architecture or design template
- [work-item-template.md](work-item-template.md) - copy-ready work-item template
- [verification-template.md](verification-template.md) - copy-ready verification template
- [examples/](examples/) - one worked example set based on the ACH duplicate batch scenario
- [examples/README.md](examples/README.md) - navigation for the example set
- [schemas/](schemas/) - strict reference JSON Schemas for front matter and trace fields
- [schemas/README.md](schemas/README.md) - navigation for the schema set and field mappings
- [.github/](.github/) - issue forms and pull request guidance for contributors
- [CHANGELOG.md](CHANGELOG.md) - package history for this reference repo
- [CONTRIBUTING.md](CONTRIBUTING.md) - how to propose changes to the standard
- [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) - project conduct expectations
- [SECURITY.md](SECURITY.md) - how to report security-related concerns
- [LICENSE](LICENSE) - Apache 2.0 license text

## How To Use The Standard

1. Use [overview.md](overview.md) as the canonical source for the standard.
2. Use [layout.md](layout.md) when organizing artifacts in a product repository.
3. Keep artifact IDs stable and link related artifacts by ID, not by prose alone.
4. Treat generated reports as derived outputs, not source-of-truth documents.

## How To Use The Templates

- Use [spec-template.md](spec-template.md) for capability-level specification documents.
- Use [architecture-template.md](architecture-template.md) for design artifacts that explain how requirements are satisfied.
- Use [work-item-template.md](work-item-template.md) for implementation work that is traceable back to requirements and design.
- Use [verification-template.md](verification-template.md) for lightweight verification records that show how a requirement was proven.

The templates are intentionally direct. They are meant to be copied and filled in, not wrapped in another process.

## How To Use The Examples

The [examples/payments/](examples/payments/) folder contains a complete worked example derived from the ACH duplicate batch scenario already referenced in the standard.

Use it as:

- a formatting reference
- a trace-link reference
- a starting point for your own product artifacts

The example artifacts cross-link the same core requirement across specification, architecture, work item, and verification records. Treat the wording as reference material, not as mandatory prose.

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
