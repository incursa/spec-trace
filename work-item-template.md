---
artifact_id: WI-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
artifact_type: work_item
title: <Work Item Title>
domain: <domain>
status: <implementation-specific>
owner: <team-or-role>
addresses:
  - REQ-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
design_links:
  - ARC-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
verification_links:
  - VER-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
related_artifacts:
  - ADR-<SEQUENCE:4+>
---

# WI-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+> - <Work Item Title>

Optional grouping segments may appear between the domain code and the terminal number, for example `WI-PAY-ACH-0081`.

The `status` field is implementation-specific. Use the work-item lifecycle that fits the team or repository, and keep that vocabulary documented in the repo if it matters for tooling or reporting.

## Summary

State the unit of work to be completed in plain language.

## Requirements Addressed

- REQ-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
- REQ-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>

## Design Inputs

- ARC-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
- ADR-<SEQUENCE:4+>

## Planned Changes

Describe the intended implementation work at a level appropriate for planning and review. This section should describe what will change without turning into low-level code notes.

## Out of Scope

Identify related items that are not part of this work item.

## Verification Plan

State how this work will be verified. Reference the verification artifact if one exists.

## Completion Notes

Optional section used after implementation to record any important deviations, follow-up work, or clarifications.

## Trace Links

Addresses:

- REQ-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>

Uses Design:

- ARC-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>

Verified By:

- VER-<DOMAIN>[-<GROUPING>...]-<SEQUENCE:4+>
