---
artifact_id: VER-PAY-ACH-0021
artifact_type: verification
title: Duplicate ACH batch rejection verification
domain: payments
status: passed
owner: payments-platform
verifies:
  - [REQ-PAY-ACH-0013](./SPEC-PAY-ACH.md)
  - [REQ-PAY-ACH-0014](./SPEC-PAY-ACH.md)
  - [REQ-PAY-ACH-0015](./SPEC-PAY-ACH.md)
  - [REQ-PAY-ACH-0016](./SPEC-PAY-ACH.md)
related_artifacts:
  - [SPEC-PAY-ACH](./SPEC-PAY-ACH.md)
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
  - [WI-PAY-ACH-0081](./sample-work-item.md)
---

# [`VER-PAY-ACH-0021`](./sample-verification.md) - Duplicate ACH Batch Rejection Verification

## Scope

Verify tenant-scoped duplicate handling for ACH batch intake.

## Requirements Verified

- [REQ-PAY-ACH-0013](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0014](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0015](./SPEC-PAY-ACH.md)
- [REQ-PAY-ACH-0016](./SPEC-PAY-ACH.md)

## Verification Method

Functional verification using an initial acceptance, a same-tenant repeat submission, and a cross-tenant submission that reuses the same external batch identifier.

## Preconditions

- tenant `PAY-TENANT-01` exists
- tenant `PAY-TENANT-02` exists
- external batch identifier `ACH-2026-0319-01` is available for test use

## Procedure or Approach

1. Submit a batch for `PAY-TENANT-01` with external batch identifier `ACH-2026-0319-01`.
2. Confirm the first submission is accepted.
3. Submit the same batch again for `PAY-TENANT-01`.
4. Confirm the second submission is rejected as a duplicate and does not start downstream processing.
5. Submit the same external batch identifier for `PAY-TENANT-02`.
6. Confirm the cross-tenant submission is accepted.

## Expected Result

The first submission is accepted, the same-tenant repeat is rejected before downstream side effects, and the different-tenant submission is accepted.

## Evidence

- [`duplicate-batch-evidence.evidence.json`](./generated/duplicate-batch-evidence.evidence.json)

## Status

This `passed` status applies to every requirement listed in `verifies`.

passed

## Related Artifacts

- [SPEC-PAY-ACH](./SPEC-PAY-ACH.md)
- [ARC-PAY-ACH-0002](./sample-architecture.md)
- [WI-PAY-ACH-0081](./sample-work-item.md)
