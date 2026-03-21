---
artifact_id: SPEC-SCH-0001
artifact_type: specification
title: Schemas and Validation Rules
domain: spec-trace
capability: schema-validation
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - schemas
  - validation
  - tooling
---

# SPEC-SCH-0001 - Schemas and Validation Rules

## Purpose

Define the machine-readable contracts that support the standard without prescribing one parser or build system.

## Scope

This specification covers the reference schemas, the extracted metadata shapes they validate, and the validation capabilities expected from tooling.

## Context

The standard depends on compact requirement clauses, file-level front matter, and explicit trace links. Tooling needs a machine-readable contract for those shapes.

## REQ-SCH-0001 Provide schemas for the core extracted shapes
The reference package MUST provide schemas for artifact front matter, identifier policy metadata, compact requirement clauses, requirement trace fields, and work-item trace fields.

## REQ-SCH-0002 Keep front matter validation strict by document family
`artifact-frontmatter.schema.json` MUST validate `artifact_type`, status vocabularies, and artifact identifier patterns for each document family.

## REQ-SCH-0003 Validate requirement identifiers as part of the identifier catalog
`artifact-id-policy.schema.json` MUST validate both artifact identifier rules and `REQ-...` identifier rules.

## REQ-SCH-0004 Validate the compact requirement clause shape
`requirement-clause.schema.json` MUST validate a `REQ-...` identifier, short title, normative clause, extracted normative keyword, and optional trace or notes data.

## REQ-SCH-0005 Keep requirement trace labels canonical
`requirement-trace-fields.schema.json` MUST allow only `Satisfied By`, `Implemented By`, `Verified By`, `Test Refs`, `Code Refs`, and `Related`.

## REQ-SCH-0006 Treat schemas as extracted-shape contracts
The reference schemas MUST describe extracted metadata shapes rather than require one specific raw Markdown parser implementation.

## REQ-SCH-0007 Resolve explicit traceability links
Validation tooling MUST be able to resolve requirement links to design, work-item, verification, test, and code references when those references are present in extracted metadata.

## REQ-SCH-0008 Report traceability gaps
Validation tooling SHOULD be able to report requirements missing design, implementation, verification, test, or code references.

## REQ-SCH-0009 Keep test and code references schema-agnostic
The requirement trace and requirement clause schemas MUST model `Test Refs` and `Code Refs` as arrays of non-empty strings rather than as a framework-specific grammar.

## REQ-SCH-0010 Surface direct test and code coverage views
Validation tooling SHOULD be able to report which requirements have direct test references and which have direct code references.
