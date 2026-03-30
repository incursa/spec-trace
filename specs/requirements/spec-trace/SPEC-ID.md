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

# [`SPEC-ID`](./SPEC-ID.md) - Identifier Policy and Grouping-Key Registry

## Purpose

Define the shape, stability, and published metadata for identifiers used by the standard.

## Scope

This specification covers specification, architecture, work-item, verification, and requirement identifiers; domain and grouping tokens; terminal sequence rules for sequence-bearing identifiers; and the machine-readable grouping-key registry.

## Context

Stable identifiers are the backbone of traceability. The standard cannot answer coverage questions if requirement IDs and artifact IDs drift or collapse into file-name conventions.

## Identifier Shape Summary

The following EBNF-style summary describes the canonical identifier families defined by this specification.

```text
UPPER_LETTER ::= ? ASCII A through Z ?
DIGIT        ::= ? 0 through 9 ?
UPPER_ALNUM  ::= UPPER_LETTER | DIGIT

DOMAIN       ::= UPPER_LETTER , { UPPER_ALNUM }
GROUPING     ::= UPPER_LETTER , { UPPER_ALNUM }
SEQUENCE     ::= DIGIT , DIGIT , DIGIT , DIGIT , { DIGIT }

SPEC_ID      ::= "SPEC-" , DOMAIN , { "-" , GROUPING }
ARC_ID       ::= "ARC-" , DOMAIN , { "-" , GROUPING } , "-" , SEQUENCE
WI_ID        ::= "WI-" , DOMAIN , { "-" , GROUPING } , "-" , SEQUENCE
VER_ID       ::= "VER-" , DOMAIN , { "-" , GROUPING } , "-" , SEQUENCE
REQ_ID       ::= "REQ-" , DOMAIN , { "-" , GROUPING } , "-" , SEQUENCE
```

## Draft Evolution Note

Earlier drafts used retired identifier `REQ-ID-0002` for a combined domain-and-grouping token rule. The current draft splits that retired combined rule into `REQ-ID-0016` through `REQ-ID-0019` so domain versus grouping and character-set versus leading-character constraints stay atomic.

## [`REQ-ID-0001`](./SPEC-ID.md#req-id-0001-use-approved-artifact-identifier-prefixes) Use approved artifact identifier prefixes
Artifact documents MUST use `SPEC`, `ARC`, `WI`, or `VER` as applicable.

Notes:
- Decision records are not part of the core standard today; repositories that add them can do so through an optional local extension.

## [`REQ-ID-0010`](./SPEC-ID.md#req-id-0010-use-the-req-prefix-for-requirement-identifiers) Use the REQ prefix for requirement identifiers
Requirement identifiers MUST use the `REQ` prefix.

## [`REQ-ID-0016`](./SPEC-ID.md#req-id-0016-keep-domain-tokens-uppercase-alphanumeric) Keep domain tokens uppercase alphanumeric
A domain token MUST use only ASCII uppercase letters and digits.

## [`REQ-ID-0017`](./SPEC-ID.md#req-id-0017-keep-domain-tokens-letter-starting) Keep domain tokens letter-starting
A domain token MUST start with an ASCII uppercase letter.

## [`REQ-ID-0003`](./SPEC-ID.md#req-id-0003-allow-stable-optional-grouping-segments) Allow stable optional grouping segments
Identifiers MAY include zero or more grouping segments after the domain token.

Notes:
- Grouping segments work best when they reflect stable structure rather than dates or workflow state.

## [`REQ-ID-0018`](./SPEC-ID.md#req-id-0018-keep-grouping-tokens-uppercase-alphanumeric) Keep grouping tokens uppercase alphanumeric
A grouping token MUST use only ASCII uppercase letters and digits.

## [`REQ-ID-0019`](./SPEC-ID.md#req-id-0019-keep-grouping-tokens-letter-starting) Keep grouping tokens letter-starting
A grouping token MUST start with an ASCII uppercase letter.

## [`REQ-ID-0020`](./SPEC-ID.md#req-id-0020-keep-terminal-sequences-numeric) Keep terminal sequences numeric
A terminal sequence token MUST use only decimal digits.

## [`REQ-ID-0021`](./SPEC-ID.md#req-id-0021-keep-terminal-sequences-at-least-four-digits-long) Keep terminal sequences at least four digits long
A terminal sequence token MUST use at least four digits.

## [`REQ-ID-0004`](./SPEC-ID.md#req-id-0004-use-terminal-free-specification-identifiers) Use terminal-free specification identifiers
Specification artifact identifiers MUST use `SPEC-<DOMAIN>(-<GROUPING>...)`.

## [`REQ-ID-0013`](./SPEC-ID.md#req-id-0013-use-sequence-bearing-architecture-identifiers) Use sequence-bearing architecture identifiers
Architecture artifact identifiers MUST use `ARC-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`.

## [`REQ-ID-0014`](./SPEC-ID.md#req-id-0014-use-sequence-bearing-work-item-identifiers) Use sequence-bearing work-item identifiers
Work-item artifact identifiers MUST use `WI-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`.

## [`REQ-ID-0015`](./SPEC-ID.md#req-id-0015-use-sequence-bearing-verification-identifiers) Use sequence-bearing verification identifiers
Verification artifact identifiers MUST use `VER-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`.

## [`REQ-ID-0022`](./SPEC-ID.md#req-id-0022-use-sequence-bearing-requirement-identifiers) Use sequence-bearing requirement identifiers
Requirement identifiers MUST use `REQ-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`.

## [`REQ-ID-0005`](./SPEC-ID.md#req-id-0005-preserve-identifier-stability) Preserve identifier stability
An assigned identifier MUST remain stable.

## [`REQ-ID-0011`](./SPEC-ID.md#req-id-0011-prevent-identifier-reuse) Prevent identifier reuse
An assigned identifier MUST NOT be reused for a different artifact or requirement.

## [`REQ-ID-0006`](./SPEC-ID.md#req-id-0006-keep-identifiers-authoritative-inside-the-document) Keep identifiers authoritative inside the document
The identifier in front matter or a requirement heading MUST be authoritative.

## [`REQ-ID-0012`](./SPEC-ID.md#req-id-0012-keep-file-names-non-authoritative) Keep file names non-authoritative
The file name MUST NOT replace the document or requirement identifier in traceability links.

## [`REQ-ID-0007`](./SPEC-ID.md#req-id-0007-keep-requirement-ids-aligned-with-their-specification-context) Keep requirement IDs aligned with their specification context
A requirement identifier SHOULD reuse the domain and grouping segments of the specification that contains it.

Notes:
- This keeps related clauses easy to find and trace, especially from generated
  evidence snapshots and other repository tooling.

## [`REQ-ID-0008`](./SPEC-ID.md#req-id-0008-publish-machine-readable-grouping-metadata) Publish machine-readable grouping metadata
The reference package MUST publish a machine-readable grouping-key registry and identifier summary in [`artifact-id-policy.json`](../../../artifact-id-policy.json).

## [`REQ-ID-0009`](./SPEC-ID.md#req-id-0009-publish-identifier-family-shapes-in-the-catalog) Publish identifier family shapes in the catalog
The identifier catalog MUST define the `SPEC-<DOMAIN>(-<GROUPING>...)`, `ARC-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`, `WI-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`, `VER-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>`, and `REQ-<DOMAIN>(-<GROUPING>...)-<SEQUENCE>` shapes.
