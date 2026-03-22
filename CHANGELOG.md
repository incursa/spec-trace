# Changelog

## [Unreleased]

### Added

- Restored root reference guidance in `overview.md`, `layout.md`, and the copy-ready template files.
- Added `examples/arithmetic/` as a narrow technical example for method-level and edge-case requirements.
- Added `schemas/requirement-clause.schema.json` for extracted compact requirement clauses.
- Added `authoring.md`, `AGENTS.md`, `LLMS.txt`, and repo-local `skills/` to make human and AI authoring workflows easier without creating a second source of truth.

### Changed

- Changed specification artifact identifiers to use `SPEC-<DOMAIN>(-<GROUPING>...)` without a terminal sequence; this is a breaking identifier-policy change and the canonical suite plus aligned examples were updated to match.
- Reframed the standard around compact requirement clauses with stable `REQ-...` identifiers.
- Clarified that a specification groups requirements and that a requirement is the smallest normative, testable statement.
- Made direct traceability to design, work items, verification artifacts, tests, and code references a first-class goal.
- Replaced the old verbose per-requirement metadata pattern with an optional `Trace` block and optional `Notes` block.
- Formalized normative keyword usage around `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, and `MAY`.
- Refactored the canonical self-specification suite, worked examples, and templates to use the compact clause model.
- Extended the identifier policy to describe `REQ-...` identifiers alongside artifact identifiers.
- Removed the `specification_role` model and returned to one specification per Markdown file.
- Restored full specification IDs in specification filenames and kept related requirements grouped under that specification in the same file.
- Tightened the requirement-clause rule from “at least one approved keyword” to “exactly one approved keyword” and split the self-spec clauses accordingly.
- Renamed the AI bootstrap surface from `LOM.txt` to `LLMS.txt` and aligned the repo guidance to match.

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
