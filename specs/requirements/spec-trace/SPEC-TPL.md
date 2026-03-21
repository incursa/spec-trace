---
artifact_id: SPEC-TPL
artifact_type: specification
title: Templates and Compact Requirement Grammar
domain: spec-trace
capability: templates-and-grammar
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - templates
  - grammar
  - front-matter
---

# SPEC-TPL - Templates and Compact Requirement Grammar

## Purpose

Define the canonical file-level metadata and the compact requirement clause grammar used by the standard.

## Scope

This specification covers front matter keys, specification roles, requirement headings, normative keyword usage, optional trace blocks, and the reference structure for architecture, work-item, and verification documents.

## Context

The standard needs predictable documents, but the requirement itself should not be hidden under record-management boilerplate.

## REQ-TPL-0001 Keep specification front matter document-level and compact
Specification documents MUST use the file-level keys `artifact_id`, `artifact_type`, `title`, `domain`, `capability`, `status`, and `owner`.

Notes:
- `tags` and `related_artifacts` are optional.

## REQ-TPL-0002 Keep architecture front matter focused on satisfaction links
Architecture documents MUST use the file-level keys `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, and `satisfies`.

Notes:
- `related_artifacts` is optional.

## REQ-TPL-0003 Keep work-item front matter focused on implementation links
Work-item documents MUST use the file-level keys `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, `addresses`, `design_links`, and `verification_links`.

Notes:
- `related_artifacts` is optional.

## REQ-TPL-0004 Keep verification front matter focused on proof links
Verification documents MUST use the file-level keys `artifact_id`, `artifact_type`, `title`, `domain`, `status`, `owner`, and `verifies`.

Notes:
- `related_artifacts` is optional.

## REQ-TPL-0005 Use REQ headings as the requirement entry point
A canonical requirement section MUST use an H2 heading that begins with a `REQ-...` identifier followed by a short title.

## REQ-TPL-0006 Require an approved all-caps normative keyword in every clause
The requirement clause MUST contain exactly one approved all-caps normative keyword.

Notes:
- The keyword does not need to be the first word in the clause.
- The approved keywords are `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, and `MAY`.

## REQ-TPL-0007 Make the compact clause the canonical requirement form
The canonical requirement form MUST place the normative clause immediately after the requirement heading.

Notes:
- Richer management metadata is allowed only as an optional local extension and should not obscure the clause itself.

## REQ-TPL-0017 Keep Trace and Notes optional
The canonical requirement form MAY follow the normative clause with optional `Trace` and `Notes` blocks.

## REQ-TPL-0008 Keep the requirement Trace block small and explicit
When a requirement includes a `Trace` block, it MUST use only the labels `Satisfied By`, `Implemented By`, `Verified By`, `Test Refs`, `Code Refs`, and `Related`.

## REQ-TPL-0009 Keep one specification and its requirements in the same file
A specification document MUST represent one specification in one Markdown file.

## REQ-TPL-0025 Keep related requirements under their specification
A specification document MUST place one or more related `REQ-...` clauses under that specification.

## REQ-TPL-0010 Keep work-item trace labels canonical
A work-item `Trace Links` section MUST use the labels `Addresses`, `Uses Design`, and `Verified By`.

## REQ-TPL-0011 Keep the supporting artifact templates predictable
Architecture, work-item, and verification documents SHOULD follow the section order shown in the reference templates so readers and tooling see a consistent structure.

## REQ-TPL-0012 Normalize normative keyword meaning
Each approved normative keyword MUST have one defined meaning in the standard.

## REQ-TPL-0018 Define meaning of MUST
This keyword MUST indicate a required condition.

## REQ-TPL-0019 Define meaning of MUST NOT
This keyword MUST indicate a prohibited condition.

## REQ-TPL-0020 Define meaning of SHALL
This keyword MUST indicate a required condition.

## REQ-TPL-0021 Define meaning of SHALL NOT
This keyword MUST indicate a prohibited condition.

## REQ-TPL-0022 Define meaning of SHOULD
This keyword MUST indicate a recommended but not strictly required condition.

## REQ-TPL-0023 Define meaning of MAY
This keyword MUST indicate a permitted or optional condition.

## REQ-TPL-0013 Keep requirement clauses atomic and short
A canonical requirement clause MUST express one obligation, rule, or constraint.

## REQ-TPL-0024 Keep requirement clauses concise by default
A canonical requirement clause SHOULD remain a single sentence unless a short paragraph is required for precision.

## REQ-TPL-0014 Keep extension metadata optional and behind the clause
Any local extension metadata MUST remain optional and follow the normative clause.

## REQ-TPL-0015 Keep front matter at document scope
Front matter MUST describe the document as a whole rather than carry per-requirement metadata.

## REQ-TPL-0016 Keep test and code references implementation-specific
The standard MUST leave `Test Refs` and `Code Refs` implementation-specific rather than prescribe a test framework, code comment format, or language-specific syntax.
