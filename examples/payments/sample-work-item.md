---
artifact_id: WI-PAY-ACH-0081
artifact_type: work_item
title: Add ACH duplicate batch guard
domain: payments
status: complete
owner: payments-platform
addresses:
  - REQ-PAY-ACH-0013
  - REQ-PAY-ACH-0014
  - REQ-PAY-ACH-0015
  - REQ-PAY-ACH-0016
design_links:
  - ARC-PAY-ACH-0002
verification_links:
  - VER-PAY-ACH-0021
related_artifacts:
  - SPEC-PAY-ACH
  - ARC-PAY-ACH-0002
  - VER-PAY-ACH-0021
---

# WI-PAY-ACH-0081 - Add ACH Duplicate Batch Guard

## Summary

Implement the tenant-scoped duplicate guard for ACH batch intake and prevent rejected duplicates from starting downstream work.

## Requirements Addressed

- REQ-PAY-ACH-0013
- REQ-PAY-ACH-0014
- REQ-PAY-ACH-0015
- REQ-PAY-ACH-0016

## Design Inputs

- ARC-PAY-ACH-0002

## Planned Changes

Add a tenant-scoped identity key, check the accepted-batch registry before dispatch, reject same-tenant duplicates, and preserve acceptance for the same external batch identifier when the tenant differs.

## Out of Scope

- settlement behavior
- unrelated payment rails
- changes to the requirement text

## Verification Plan

Use `VER-PAY-ACH-0021` to verify same-tenant rejection, cross-tenant acceptance, and absence of downstream side effects for rejected duplicates.

## Completion Notes

The implementation preserves the existing success path for unique batch identifiers.

## Trace Links

Addresses:

- REQ-PAY-ACH-0013
- REQ-PAY-ACH-0014
- REQ-PAY-ACH-0015
- REQ-PAY-ACH-0016

Uses Design:

- ARC-PAY-ACH-0002

Verified By:

- VER-PAY-ACH-0021
