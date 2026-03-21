## Recommended Product Repository Layout and Conventions

This layout is for repositories that adopt the standard. The `incursa/spec-trace` repository is the public reference package; it keeps the standard documents at the repository root for packaging and copy convenience. Product repositories should treat the `/specs/...` tree below as the recommended layout for the live standard content. Nested grouping directories are valid between the top-level domain folder and the leaf artifact file.

The recommended default layout is:

```text
/specs
  /requirements
    /<domain>/
      _index.md
      <capability>.md
  /architecture
    /<domain>/
      ARC-<DOMAIN>[-<GROUPING>...]-###-<slug>.md
  /decisions
    ADR-###-<slug>.md
  /work-items
    /<domain>/
      WI-<DOMAIN>[-<GROUPING>...]-###-<slug>.md
  /verification
    /<domain>/
      VER-<DOMAIN>[-<GROUPING>...]-###-<slug>.md
  /templates
    spec-template.md
    architecture-template.md
    work-item-template.md
    verification-template.md
  /schemas
    artifact-frontmatter.schema.json
    requirement-trace-fields.schema.json
    work-item-trace-fields.schema.json
  /generated
    requirements-index.json
    traceability-matrix.md
    orphan-report.md
    verification-coverage.md
```

### Layout Rules

Requirements should be organized first by **domain or bounded area of the product**, and then by **capability or feature area**. In other words, organize by the shape of the system, not by sprint, release, assignee, or date.

A good default is:

- `/specs/requirements/payments/`
- `/specs/requirements/notary/`
- `/specs/requirements/auth/`
- `/specs/requirements/compliance/`

Within each domain folder, each Markdown file should represent a **capability-level specification**, not a single requirement and not an entire subsystem. A healthy file usually contains a coherent group of related requirements for one capability. As a general guideline, a specification file should usually contain somewhere between roughly five and twenty related requirements. If it becomes too large or starts to mix multiple capabilities, split it.

If a domain needs further organization, add nested grouping directories beneath it. Keep those groupings stable and capability-oriented rather than date-oriented.

Each domain folder may include an `_index.md` file that briefly explains what belongs in that domain and links to the relevant specification, architecture, decision, work-item, and verification artifacts.

### Artifact Placement

The repository should treat each artifact type differently.

Requirements belong under `/specs/requirements/` because they define what the system must do.

Architecture and design artifacts belong under `/specs/architecture/` because they explain how the requirements are being satisfied.

Decision records belong under `/specs/decisions/` because they capture cross-cutting or significant design choices that may affect many artifacts.

Work-item artifacts belong under `/specs/work-items/` because they capture planned or completed units of implementation work.

Verification artifacts belong under `/specs/verification/` because they describe how fulfillment was proven, whether by automated testing, manual testing, inspection, or some other means.

Generated reports, indexes, and matrices belong under `/specs/generated/` and should not be treated as source-of-truth documents.

### File Naming Conventions

File names should be stable, readable, and easy to scan in Git. Use hyphenated template filenames and human-readable slugs for artifact files. Artifact identifiers should live inside the file content and front matter, not depend on the file name.

Recommended examples:

- `/specs/requirements/payments/ach-batch-processing.md`
- `/specs/requirements/payments/ach/duplicate-batch-processing.md`
- `/specs/requirements/notary/template-generation.md`
- `/specs/architecture/payments/ach/ARC-PAY-ACH-002-duplicate-batch-detection.md`
- `/specs/work-items/payments/ach/WI-PAY-ACH-081-duplicate-batch-guard.md`
- `/specs/verification/payments/ach/VER-PAY-ACH-021-duplicate-batch-rejection.md`

Avoid using dates, sprint numbers, or developer names in file names.

### Identifier Conventions

Each artifact type should use a distinct identifier pattern so links are obvious and tooling can validate them.

A recommended convention is:

- `SPEC-<DOMAIN>[-<GROUPING>...]-###` for specification documents
- `REQ-<DOMAIN>[-<GROUPING>...]-###` for individual requirements
- `ARC-<DOMAIN>[-<GROUPING>...]-###` for architecture or design artifacts
- `ADR-###` for architectural decision records
- `WI-<DOMAIN>[-<GROUPING>...]-###` for work items
- `VER-<DOMAIN>[-<GROUPING>...]-###` for verification artifacts

Examples:

- `SPEC-PAY-001`
- `SPEC-PAY-ACH-001`
- `REQ-PAY-014`
- `ARC-PAY-002`
- `ARC-PAY-ACH-002`
- `ADR-012`
- `WI-PAY-081`
- `WI-PAY-ACH-081`
- `VER-PAY-021`
- `VER-PAY-ACH-021`

The optional grouping segments may be used to reflect multi-level organization. The identifier must still start with the artifact type prefix and domain code and end with a terminal sequence number.

The domain code should be short, stable, and reused consistently across the repository. Once an identifier is assigned, it should not be reused for a different artifact.

### Structure Within Markdown Files

Front matter should be used at the **file level** only. It describes the document as a whole.

Requirement-level metadata should be stored in the **body of the Markdown document** using fixed field labels under each requirement heading. This keeps the file readable in plain Markdown and avoids inventing nested front matter conventions that are harder to maintain.

The parser should assume:

- file-level front matter contains document metadata
- requirement sections begin with a level-two heading containing the requirement ID
- field labels inside each requirement section use exact names and exact order where practical
- trace links reference artifact identifiers, not file paths

### Organization Defaults

If no other structure has been chosen, the following defaults should be assumed:

- requirements are grouped by capability
- architecture documents are grouped by the domain they affect most directly
- work items are grouped by the domain they implement
- verification artifacts are grouped by the capability or scenario they prove
- decision records are shared and kept at a global level unless there is a strong reason to partition them further
- generated reports are fully derived and may be deleted and regenerated at any time

### Traceability Expectations

The repository layout should make it possible to generate, at minimum, the following views:

- all requirements by domain
- all requirements by status
- all requirements with no linked design
- all requirements with no linked work item
- all requirements with no linked verification
- all work items with no linked requirement
- all verification artifacts that reference retired or superseded requirements

That traceability should come from artifact identifiers and metadata links, not from loose prose.
