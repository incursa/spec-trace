# Changelog

## [Unreleased]

### Added

- Restored root reference guidance in `overview.md`, `layout.md`, and the copy-ready template files.
- Formalized inline identifier references as backtick-delimited, human-readable links to stable artifact IDs, with cross-file resolution left to repository-level tooling.
- Added guidance that requirement titles are short descriptive labels, requirement clauses carry the normative behavior, and `Trace` blocks have typed semantics by label family.
- Added canonical conformance profiles (`core`, `traceable`, and `auditable`) and the [`SPEC-PRF`](./specs/requirements/spec-trace/SPEC-PRF.md) canonical spec.
- Added `examples/arithmetic/` as a narrow technical example for method-level and edge-case requirements.
- Added [`profiles-and-attestation-explainer.md`](./profiles-and-attestation-explainer.md) and a generated arithmetic current-status rollup to distinguish authored trace from derived evidence reporting.
- Added [`SPEC-RPT.md`](./specs/requirements/spec-trace/SPEC-RPT.md) to define derived reporting dimensions, attestation semantics, and staged-adoption guidance.
- Added `schemas/requirement-clause.schema.json` for extracted compact requirement clauses.
- Added `authoring.md`, `AGENTS.md`, `LLMS.txt`, and repo-local `skills/` to make human and AI authoring workflows easier without creating a second source of truth.

### Changed

- Switched Central Registry publishing from a long-lived token secret to GitHub OIDC via `cue-labs/registry-login-action`, and updated the release workflow to put the pinned CUE CLI on `PATH` before running `cue mod tidy` or `cue mod publish`.
- Clarified that consumers import the schema-backed root template package as `github.com/incursa/spec-trace@v0:templates`.
- Clarified repository documentation guidance so repo-local references use relative Markdown links, with backticks kept inside link text when monospace styling is desired.
- Clarified that repo-local Markdown links to specific requirements or other headed specification subsections should include anchors or equivalent sub-document locators when relevant.
- Clarified that `core`, `traceable`, and `auditable` are repository-level conformance gates rather than maturity labels, and that `Verified By` records verification coverage rather than formal correctness or live status.
- Clarified that generated evidence rollups, coverage views, and attestation snapshots are derived outputs rather than canonical requirement text.
- Clarified that source, design, implementation, verification, direct test, and direct code coverage are reporting dimensions that complement profiles instead of replacing them.
- Made the normative keyword model explicit: BCP 14-style uppercase requirement language inspired by RFC 2119 and RFC 8174 now uses the narrowed approved set `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`; lowercase forms are plain English.
- Tightened trace-bearing schemas to constrain identifier families instead of accepting generic non-empty strings, and added repository-level validation for duplicate IDs, unresolved references, reciprocal consistency, and namespace alignment. This is a breaking validation-contract change.
- Reworded [`REQ-STD-0019`](./specs/requirements/spec-trace/SPEC-STD.md#req-std-0019-keep-verification-outcomes-homogeneous-within-one-artifact) to keep the clause at exactly one approved normative keyword.
- Removed ADR/decision-record identifiers from the core artifact catalog and layout model; decision records are now treated as an optional local extension outside the core standard.
- Reserved `profile` for conformance levels and simplified decision-record wording to optional local extensions.
- Clarified that a verification artifact status applies to every requirement listed in `verifies`; mixed outcomes now belong in separate verification artifacts.
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
