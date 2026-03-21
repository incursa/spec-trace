---
artifact_id: VER-<DOMAIN>[-<GROUPING>...]-###
artifact_type: verification
title: <Verification Title>
domain: <domain>
status: <implementation-specific>
owner: <team-or-role>
verifies:
  - REQ-<DOMAIN>[-<GROUPING>...]-###
related_artifacts:
  - SPEC-<DOMAIN>[-<GROUPING>...]-###
  - ARC-<DOMAIN>[-<GROUPING>...]-###
  - WI-<DOMAIN>[-<GROUPING>...]-###
---

# VER-<DOMAIN>[-<GROUPING>...]-### - <Verification Title>

Optional grouping segments may appear between the domain code and the terminal number.

The `status` field is implementation-specific. Use the repository's normal verification workflow vocabulary if one exists.

## Scope

State what is being verified and what is out of scope.

## Requirements Verified

- REQ-<DOMAIN>[-<GROUPING>...]-###

## Verification Method

Describe the method at a tooling-agnostic level, such as execution, inspection, analysis, or manual review.

## Preconditions

- <precondition>

## Procedure or Approach

Describe the steps or the approach used to verify the requirement.

## Expected Result

Describe the expected outcome in plain language.

## Evidence

- <evidence>

## Status

Record the verification outcome here using the repository's chosen workflow vocabulary.

## Related Artifacts

- SPEC-<DOMAIN>[-<GROUPING>...]-###
- ARC-<DOMAIN>[-<GROUPING>...]-###
- WI-<DOMAIN>[-<GROUPING>...]-###
