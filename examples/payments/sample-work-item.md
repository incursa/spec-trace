---
artifact_id: WI-PAY-ACH-0081
artifact_type: work_item
title: Add ACH duplicate batch guard
domain: payments
status: complete
owner: payments-platform
addresses:
  - REQ-PAY-ACH-0014
design_links:
  - ARC-PAY-ACH-0002
verification_links:
  - VER-PAY-ACH-0021
related_artifacts:
  - SPEC-PAY-ACH-0001
  - ARC-PAY-ACH-0002
  - VER-PAY-ACH-0021
---

# WI-PAY-ACH-0081 - Add ACH duplicate batch guard

The `status` field is implementation-specific. This work item uses the team's normal delivery vocabulary and stays traceable to the requirement, design, and verification artifacts.

## Summary

Implement the duplicate batch guard for ACH batch intake.

## Requirements Addressed

- REQ-PAY-ACH-0014

## Design Inputs

- ARC-PAY-ACH-0002

## Planned Changes

Add a tenant-scoped duplicate check at batch intake, persist accepted batch identifiers, and map duplicate detection to the approved rejection outcome.

## Out of Scope

- downstream settlement logic
- changes to unrelated payment rails
- changes to the requirement text itself

## Verification Plan

Use VER-PAY-ACH-0021 to confirm that a duplicate submission for the same tenant and external batch identifier is rejected after the initial acceptance path succeeds.

## Completion Notes

The implementation should preserve the same acceptance path for unique batch identifiers.

## Trace Links

Addresses:

- REQ-PAY-ACH-0014

Uses Design:

- ARC-PAY-ACH-0002

Verified By:

- VER-PAY-ACH-0021
