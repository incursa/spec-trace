---
artifact_id: WI-PAY-ACH-0081
artifact_type: work_item
title: Add ACH duplicate batch guard
domain: payments
status: complete
owner: payments-platform
addresses:
  - [REQ-PAY-ACH-0013](./SPEC-PAY-ACH.md)
  - [REQ-PAY-ACH-0014](./SPEC-PAY-ACH.md)
  - [REQ-PAY-ACH-0015](./SPEC-PAY-ACH.md)
  - [REQ-PAY-ACH-0016](./SPEC-PAY-ACH.md)
design_links:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
verification_links:
  - [VER-PAY-ACH-0021](./sample-verification.md)
related_artifacts:
  - [SPEC-PAY-ACH](./SPEC-PAY-ACH.md)
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
  - [VER-PAY-ACH-0021](./sample-verification.md)
---

# [`WI-PAY-ACH-0081`](./sample-work-item.md) - Add ACH Duplicate Batch Guard

## Summary

Implement the tenant-scoped duplicate guard for ACH batch intake and prevent rejected duplicates from starting downstream work.

## Requirements Addressed

- [REQ-PAY-ACH-0013](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0014](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0015](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0016](./SPEC-PAY-ACH.md)

## Design Inputs

- [ARC-PAY-ACH-0002](./sample-architecture.md)

## Planned Changes

Add a tenant-scoped identity key, check the accepted-batch registry before dispatch, reject same-tenant duplicates, and preserve acceptance for the same external batch identifier when the tenant differs.

## Out of Scope

- settlement behavior
- unrelated payment rails
- changes to the requirement text

## Verification Plan

Use [`VER-PAY-ACH-0021`](./sample-verification.md) to verify same-tenant rejection, cross-tenant acceptance, and absence of downstream side effects for rejected duplicates.

## Completion Notes

The implementation preserves the existing success path for unique batch identifiers.

## Trace Links

Addresses:

- [REQ-PAY-ACH-0013](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0014](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0015](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0016](./SPEC-PAY-ACH.md)

Uses Design:

- [ARC-PAY-ACH-0002](./sample-architecture.md)

Verified By:

- [VER-PAY-ACH-0021](./sample-verification.md)
