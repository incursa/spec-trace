# Repository-Native Requirements and Traceability Standard

## Purpose

This standard defines a lightweight, repository-native way to capture requirements in Markdown and keep them traceable to design, work items, implementation, and verification artifacts.

The goal is practical: make requirements easy to read in Git, easy to review, and easy for tools to parse. The standard is intentionally lightweight. It does not require a specialized requirements platform, a test framework convention, or a behavior-driven syntax.

Requirements are first-class artifacts. They live in the repository alongside the related design, work, and verification content that proves them out.

## Scope

This standard applies to:

- product requirements
- functional requirements
- non-functional requirements
- constraints and edge-case requirements
- architecture and design documents that satisfy requirements
- work items that implement requirements
- verification artifacts that prove requirements were fulfilled

This standard does not prescribe implementation language, test framework, or delivery tool. It only defines how artifacts are written, identified, and linked.

## Conformance

A repository conforms to this standard when it follows the required artifact structure and traceability rules below.

### Required For Conformance

- Use Markdown as the authoring format.
- Use file-level front matter for each artifact.
- Use the canonical field names defined in this document and the templates.
- Give every artifact a stable `artifact_id`.
- Keep requirement statements atomic and verifiable.
- Keep normative requirement text separate from explanatory text.
- Link requirements to design, work, and verification artifacts using explicit artifact identifiers.
- Preserve the meaning of an artifact when refining wording. If the meaning changes materially, create a new identifier.

### Normative Content

The following parts are normative in this standard:

- the required front matter fields
- the requirement section field names
- the requirement statement itself
- the work-item trace-link field names and directions
- the conformance rules above

### Explanatory Content

The following parts are explanatory only:

- examples
- rationale
- notes
- layout recommendations
- generated outputs
- implementation guidance that does not define a required field or rule

## Core Principles

This process is built on a small number of rules.

**Requirements must be human-readable.**
All requirements are stored in Markdown and reviewed like any other repository content.

**Requirements must be machine-parseable.**
Each requirement follows a predictable structure so tooling can extract identifiers, metadata, links, and status.

**Each requirement must be uniquely identifiable.**
Every requirement gets a stable identifier that does not change unless the requirement is materially replaced.

**Each requirement must be atomic.**
A requirement expresses one verifiable behavior, rule, or constraint. If a statement contains multiple independent obligations, split it.

**Traceability must be explicit.**
Links between requirements and other artifacts must be stored in metadata, not inferred from prose.

**Normative content must be separated from explanatory content.**
The requirement statement is the binding portion. Rationale, examples, notes, and commentary are useful, but they are not the requirement itself.

## Artifact Model

The repository may contain several artifact types. Each artifact has its own identifier and may link to other artifacts by identifier.

Common artifact types include:

- **Specification**: a Markdown document that groups related requirements
- **Requirement**: a defined need, rule, behavior, or constraint
- **Architecture or design**: explains how one or more requirements will be satisfied
- **Work item**: captures a unit of planned or completed work
- **Verification artifact**: identifies how a requirement was verified, whether by automated test, manual test, inspection, or another method
- **Decision record**: records a significant design or product decision

The exact artifact names can vary outside this package. What must stay stable is the artifact identifier, the required field names, and the explicit links between artifacts.

## Canonical Field Names

The following field names are canonical in compliant content.

### File-Level Front Matter

Specification documents use:

- `artifact_id`
- `artifact_type`
- `title`
- `domain`
- `capability`
- `status`
- `owner`
- `tags`
- `related_artifacts`

Architecture documents use:

- `artifact_id`
- `artifact_type`
- `title`
- `domain`
- `status`
- `owner`
- `satisfies`
- `related_artifacts`

Work items use:

- `artifact_id`
- `artifact_type`
- `title`
- `domain`
- `status`
- `owner`
- `addresses`
- `design_links`
- `verification_links`
- `related_artifacts`

Verification artifacts use:

- `artifact_id`
- `artifact_type`
- `title`
- `domain`
- `status`
- `owner`
- `verifies`
- `related_artifacts`

### Requirement Section Fields

Requirement sections use these exact field labels:

- `Type`
- `Status`
- `Priority`
- `Source`
- `Verification`
- `Derived From`
- `Satisfied By`
- `Implemented By`
- `Verified By`
- `Supersedes`
- `Related`
- `Requirement`
- `Rationale`
- `Notes`

The first five fields and `Requirement` are required. The remaining fields are optional.
Required fields must appear in every compliant requirement section. Optional fields may be omitted when they do not apply.

### Work Item Trace-Link Fields

Work item documents use these exact field labels in their `Trace Links` section:

- `Addresses`
- `Uses Design`
- `Verified By`

These labels are canonical in compliant work-item trace-link sections.

## Recommended Product Repository Structure

The structure below is the recommended layout for product repositories that adopt the standard. The public reference package may keep the same documents at the repository root for packaging convenience.

A recommended structure is:

```text
/specs
  /requirements
  /architecture
  /decisions
  /work-items
  /verification
  /generated
  /templates
```

The exact folder names may vary, but the model should remain the same:

- requirements define **what** is needed
- architecture and design define **how** it will be satisfied
- work items define **what work is being done**
- verification artifacts define **how fulfillment is proven**
- generated content contains indexes, traceability reports, and published documentation

Requirements should be organized by business domain, feature area, or product capability rather than by date.

## Requirement Document Structure

Each Markdown specification document should have file-level front matter. This front matter describes the document as a whole.

A typical file-level front matter block is:

```yaml
---
artifact_id: SPEC-PAY-001
artifact_type: specification
title: ACH Duplicate Batch Handling
domain: payments
capability: ach-duplicate-batch-handling
status: draft
owner: payments-platform
tags:
  - payments
  - ach
related_artifacts:
  - ARC-PAY-002
  - WI-PAY-081
  - VER-PAY-021
---
```

This front matter is for the document, not for an individual requirement.

Inside the document, each requirement must appear in its own structured section. The section heading must begin with the requirement identifier.

A requirement section must contain the following elements in a fixed order:

1. Identifier and title
2. Metadata block
3. Requirement statement
4. Optional rationale
5. Optional notes

A recommended requirement section format is:

```md
## REQ-PAY-014 Reject duplicate ACH batch submission

Type: functional
Status: approved
Priority: high
Source: BR-PAY-003
Verification: manual
Derived From:
Satisfied By: ARC-PAY-002
Implemented By: WI-PAY-081
Verified By: VER-PAY-021
Supersedes:
Related:

Requirement:
The system shall reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.

Rationale:
This prevents duplicate payment processing and makes the duplicate-handling rule explicit.

Notes:
A duplicate is determined by tenant and external batch identifier, not by submission timestamp.
```

This format keeps the requirement readable in plain Markdown while remaining easy to parse.

## Requirement Writing Rules

A valid requirement must follow these writing rules.

**Use one requirement per section.**
Do not combine multiple separate obligations into one numbered item.

**Write the requirement as a single normative statement.**
The statement should be specific enough to verify. The clearest convention is to use “shall” for normative requirements.

**Keep the requirement technology-neutral unless the technology is itself a requirement.**
Requirements should describe expected behavior or constraints, not implementation details, unless the implementation choice is mandatory.

**Do not mix rationale into the requirement statement.**
If context is needed, place it under Rationale or Notes.

**Write requirements so they can be verified.**
A requirement that cannot be meaningfully tested, inspected, or otherwise verified is not ready.

**Use stable identifiers.**
An identifier should not change because wording was refined. A new identifier should be created only when the requirement meaning materially changes or is replaced.

## Traceability Model

Traceability is the core of this process. The requirement is the anchor artifact. Everything else points to or from it through explicit metadata.

At a minimum, the process should support these link types:

- **Derived From**: this requirement came from a higher-level business or product need
- **Satisfied By**: this design or architecture artifact explains how the requirement will be met
- **Implemented By**: this work item, change, or implementation artifact exists to fulfill the requirement
- **Verified By**: this verification artifact proves the requirement has been fulfilled
- **Supersedes / Superseded By**: this requirement replaced or was replaced by another requirement

The requirement-section field names above are canonical in this standard and should not be renamed in compliant artifacts. Work items use a separate `Trace Links` section with their own canonical labels.

A few rules matter here:

- links must reference artifact identifiers, not prose descriptions
- links must be explicit metadata, not implied in body text
- one requirement may be linked to many work items or verification artifacts
- one design document may satisfy many requirements
- one work item may address multiple requirements, but that should be deliberate rather than casual

Work items use their own trace-link labels to keep the implementation trail explicit:

- `Addresses`
- `Uses Design`
- `Verified By`

## Relationship to Work Items and Architecture

Requirements are not work items.

A requirement states what must be true. A work item states what someone plans to do. A design document explains how the requirement will be achieved.

This distinction matters because it keeps requirements stable even when implementation plans change.

A healthy flow looks like this:

1. A requirement is captured and approved.
2. A design or architecture artifact links back to the requirement through `Satisfied By`.
3. One or more work items link back through `Implemented By` and the work item's `Trace Links` section.
4. Verification artifacts link back through `Verified By`.
5. Generated traceability outputs show whether the requirement is approved, implemented, and verified.

This model lets the team answer questions such as:

- Which approved requirements have no design?
- Which implemented requirements have no verification?
- Which work items are not tied to a requirement?
- Which requirements were superseded?
- Which requirement introduced a given edge case?

## Lifecycle and Change Rules

Requirements should move through a simple lifecycle. A recommended set of statuses is:

- draft
- proposed
- approved
- implemented
- verified
- superseded
- retired

The workflow can be kept lightweight, but the meanings should be consistent.

A requirement should only be marked **implemented** when the intended behavior exists in the product.
A requirement should only be marked **verified** when there is an explicit verification artifact linked to it.
A requirement should be marked **superseded** rather than silently rewritten when it has been replaced by a newer requirement.

When editing a requirement, use this rule:

- if the intent stays the same and the wording is only clarified, keep the same ID
- if the expected behavior, constraint, or obligation materially changes, create a new ID and link the old one through a supersession relationship

## Tooling Expectations

The repository should support automation around this standard, but the standard itself should remain tooling-agnostic.

At minimum, tooling should be able to:

- parse Markdown files and front matter
- discover all requirement sections and identifiers
- validate identifier uniqueness
- validate required metadata fields
- resolve trace links across artifact types
- generate a traceability report
- generate human-readable documentation
- flag orphaned artifacts, such as work items with no requirement or approved requirements with no verification

Generated outputs should be treated as derived artifacts, not the source of truth. The source of truth remains the Markdown content in the repository.

## Summary

This hybrid process is a Markdown-first, repository-native requirements system. It is lightweight enough to live comfortably in Git, structured enough for tooling, and explicit enough to support real traceability.

The key design choices are simple:

- Markdown is the authoring format
- front matter describes the document
- each requirement is a structured section with a stable identifier
- only the requirement statement is normative
- architecture, work items, and verification artifacts link to requirements by ID
- tooling reads these links and generates traceability views

That gives you a practical requirements system without forcing the team into a heavyweight platform or a prescriptive test-writing style.
