# Recommended Repository Layout

This file describes the recommended layout for repositories that adopt the standard. The `incursa/spec-trace` repository is the public reference package, so it also keeps a small root reference layer for readability and copy convenience.

## Default Layout

```text
artifact-id-policy.json
/specs
  /requirements
    /<domain>/
      _index.md
      SPEC-<DOMAIN>[-<GROUPING>...].md
  /architecture
    /<domain>/
      <readable-file-name>.md
  /work-items
    /<domain>/
      <readable-file-name>.md
  /verification
    /<domain>/
      <readable-file-name>.md
  /generated
    requirements-index.json
    traceability-matrix.md
    orphan-report.md
    verification-coverage.md
  /templates
    spec-template.md
    architecture-template.md
    work-item-template.md
    verification-template.md
  /schemas
    artifact-frontmatter.schema.json
    artifact-id-policy.schema.json
    requirement-clause.schema.json
    requirement-trace-fields.schema.json
    work-item-trace-fields.schema.json
```

## Layout Rules

### Specifications

Place specifications under `/specs/requirements/`.

Organize them:

1. by domain or bounded area first
2. by capability, behavior area, interface, or narrow technical concern second

Good grouping dimensions are stable technical or business concepts. Dates, sprints, owners, and release numbers are not.

### Architecture or Design

Place architecture and design artifacts under `/specs/architecture/`.

These documents explain how requirements are satisfied. They are the default place for design rationale and decision tradeoffs. Decision records are not part of the core layout today, but a repository may add them through an optional local extension if needed.

### Work Items

Place work items under `/specs/work-items/`.

Work items describe implementation work and should trace back to requirement IDs and design artifacts.

### Verification

Place verification artifacts under `/specs/verification/`.

Verification artifacts prove requirements. They may summarize verification at a scenario level when all covered requirements share one outcome. Mixed outcomes belong in separate verification artifacts.

### Generated Outputs

Place generated indexes, matrices, and coverage reports under `/specs/generated/`.

Generated outputs are derived artifacts such as coverage matrices, source-coverage views, evidence rollups, attestation snapshots, and current-status views. They are useful, but they are not source of truth.

## One Specification Per File

Each specification Markdown file contains one specification and one or more related requirement clauses.

If a concern is narrow, the specification may be narrow. The standard uses one specification model for both broad and narrow concerns.

## File Names

File names should be stable and readable.

- Specification file names should include the full specification artifact ID.
- Avoid dates, sprint numbers, and owner names.
- Keep traceability anchored on `artifact_id` and `REQ-...` identifiers inside the file, not on the file name.

Recommended examples:

- [`/specs/requirements/payments/SPEC-PAY-ACH.md`](./specs/requirements/payments/SPEC-PAY-ACH.md)
- [`/specs/requirements/arithmetic/SPEC-MATH-DIV.md`](./specs/requirements/arithmetic/SPEC-MATH-DIV.md)
- [`/specs/requirements/spec-trace/SPEC-STD.md`](./specs/requirements/spec-trace/SPEC-STD.md)

## Index Files

[`_index.md`](./_index.md) files are optional. Use them for navigation only.

An index file may summarize a domain and link specifications, architecture, work items, verification artifacts, and generated outputs. It does not replace the underlying artifacts.

## Traceability Loop

The layout should make this path easy to see:

1. a specification groups the requirement clauses
2. architecture or design artifacts satisfy requirements
3. work items address requirements and use design inputs
4. verification artifacts prove requirements
5. tests, code, and prose may reference stable artifact IDs directly, with prose using inline backticks for lightweight links or relative Markdown links for repo-local files and folders

If the layout makes that chain hard to follow, the structure is working against the standard.
