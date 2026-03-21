---
artifact_id: SPEC-ID
artifact_type: specification
title: Identifier Policy and Grouping-Key Registry
domain: spec-trace
capability: identifier-policy
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - identifiers
  - policy
---

# SPEC-ID - Identifier Policy and Grouping-Key Registry

## Purpose

Define the shape, stability, and published metadata for identifiers used by the standard.

## Scope

This specification covers artifact identifiers, specification identifiers, requirement identifiers, grouping segments, terminal sequence rules for sequence-bearing identifiers, and the machine-readable grouping-key registry.

## Context

Stable identifiers are the backbone of traceability. The standard cannot answer coverage questions if requirement IDs and artifact IDs drift or collapse into file-name conventions.

## REQ-ID-0001 Use approved identifier prefixes
Artifact documents MUST use `SPEC`, `ARC`, `WI`, `VER`, or `ADR` as applicable.

## REQ-ID-0010 Use the REQ prefix for requirement clauses
Requirement clauses MUST use the `REQ` prefix.

## REQ-ID-0002 Keep domain and grouping tokens uppercase and letter-starting
Domain and grouping tokens MUST be uppercase alphanumeric text that starts with a letter.

## REQ-ID-0003 Allow stable optional grouping segments
Identifiers MAY include zero or more grouping segments after the domain token.

Notes:
- Grouping segments should reflect stable structure, not dates or workflow state.

## REQ-ID-0004 Use terminal-free specification identifiers
Specification artifact identifiers MUST use `SPEC-<DOMAIN>(-<GROUPING>...)`.

## REQ-ID-0005 Preserve identifier stability
An assigned identifier MUST remain stable.

## REQ-ID-0011 Prevent identifier reuse
An assigned identifier MUST NOT be reused for a different artifact or requirement.

## REQ-ID-0006 Keep identifiers authoritative inside the document
The identifier in front matter or a requirement heading MUST be authoritative.

## REQ-ID-0012 Keep file names non-authoritative
The file name MUST NOT replace the document or requirement identifier in traceability links.

## REQ-ID-0007 Keep requirement IDs aligned with their specification context
A requirement identifier SHOULD reuse the domain and grouping segments of the specification that contains it.

Notes:
- This keeps related clauses easy to find and trace, especially from tests and code references.

## REQ-ID-0008 Publish machine-readable grouping metadata
The reference package MUST publish a machine-readable grouping-key registry and identifier summary in `artifact-id-policy.json`.

## REQ-ID-0009 Describe requirement identifiers alongside artifact identifiers
The identifier catalog MUST define the `REQ-<DOMAIN>(-<GROUPING>...)-<SEQUENCE:4+>` identifier shape alongside the document artifact identifier shapes.
