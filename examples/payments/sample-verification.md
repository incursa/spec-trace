---
artifact_id: VER-PAY-ACH-0021
artifact_type: verification
title: Duplicate ACH batch rejection verification
domain: payments
status: passed
owner: payments-platform
verifies:
  - REQ-PAY-ACH-0014
related_artifacts:
  - SPEC-PAY-ACH-0001
  - ARC-PAY-ACH-0002
  - WI-PAY-ACH-0081
---

# VER-PAY-ACH-0021 - Duplicate ACH batch rejection verification

The `status` field is implementation-specific. This verification record uses the team's normal outcome vocabulary.

## Scope

Verify that a submitted ACH batch is rejected when the same external batch identifier has already been accepted for the same tenant.

## Requirements Verified

- REQ-PAY-ACH-0014

## Verification Method

Functional verification using two submissions of the same tenant-scoped batch identifier and review of the resulting acceptance and rejection outcomes.

## Preconditions

- tenant `PAY-TENANT-01` exists
- external batch identifier `ACH-2026-0319-01` is available for test use
- the first submission path is available and functioning

## Procedure or Approach

1. Submit the batch for tenant `PAY-TENANT-01` with external batch identifier `ACH-2026-0319-01`.
2. Confirm the first submission is accepted.
3. Submit the same batch again with the same tenant and external batch identifier.
4. Confirm the second submission is rejected as a duplicate.

## Expected Result

The first submission is accepted and the second submission is rejected without initiating duplicate downstream processing.

## Evidence

- acceptance record for the first submission
- duplicate rejection record for the second submission
- reviewer notes showing the tenant and external batch identifier match

## Status

passed

## Related Artifacts

- SPEC-PAY-ACH-0001
- ARC-PAY-ACH-0002
- WI-PAY-ACH-0081
