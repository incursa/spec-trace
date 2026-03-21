---
artifact_id: VER-<DOMAIN>-###
artifact_type: verification
title: <Verification Title>
domain: <domain>
status: <implementation-specific>
owner: <team-or-role>
verifies:
  - REQ-<DOMAIN>-###
related_artifacts:
  - SPEC-<DOMAIN>-###
  - ARC-<DOMAIN>-###
  - WI-<DOMAIN>-###
---

# VER-<DOMAIN>-### - <Verification Title>

The `status` field is implementation-specific. Use the repository's normal verification workflow vocabulary if one exists.

## Scope

State what is being verified and what is out of scope.

## Requirements Verified

- REQ-<DOMAIN>-###

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

- SPEC-<DOMAIN>-###
- ARC-<DOMAIN>-###
- WI-<DOMAIN>-###
