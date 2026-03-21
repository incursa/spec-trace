# Changelog

## [Unreleased]

### Added

- Public reference package for a Markdown-first requirements and traceability standard.
- `artifact-id-policy.json` for the shared identifier policy defaults.
- `examples/README.md` and `schemas/README.md` for lightweight navigation and field mapping.

### Changed

- Standardized the root template filenames to hyphenated names: `spec-template.md`, `architecture-template.md`, `work-item-template.md`, and `verification-template.md`.
- Clarified that `layout.md` describes the recommended layout for product repositories that adopt the standard, not the reference package's own root layout.
- Allowed multi-level grouping segments in artifact identifiers and repository paths.
- Raised the minimum terminal sequence width to four digits and updated the worked example IDs accordingly.
- Split the trace-field guidance into separate requirement and work-item schema files.
- Tightened the artifact front-matter schema for type-specific required fields and artifact-ID patterns.

## [0.1.0] - 2026-03-20

### Added

- Canonical overview, layout, and template documents for a repository-native requirements and traceability standard.
- Copy-ready templates for specifications, architecture or design, work items, and verification artifacts.
- A worked payments example set based on the ACH duplicate batch scenario.
- Lightweight JSON Schema examples for front matter and trace links.
- Repository guidance for contributors, issue reporting, and security reporting.
- GitHub collaboration templates and minimal repo hygiene files.

### Changed

- Normalized canonical field names, artifact naming, and traceability conventions across the standard package.
- Replaced the previous coverage-report naming convention with `verification-coverage.md` in the recommended layout.
