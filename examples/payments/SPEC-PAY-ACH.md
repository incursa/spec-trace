---
artifact_id: SPEC-PAY-ACH
artifact_type: specification
title: ACH Duplicate Batch Handling
domain: payments
capability: ach-duplicate-batch-handling
status: approved
owner: payments-platform
tags:
  - payments
  - ach
  - duplicate-detection
related_artifacts:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
  - [WI-PAY-ACH-0081](./sample-work-item.md)
  - [VER-PAY-ACH-0021](./sample-verification.md)
---

# [`SPEC-PAY-ACH`](./SPEC-PAY-ACH.md) - ACH Duplicate Batch Handling

## Purpose

Define the duplicate-handling rules for ACH batch intake.

## Scope

This specification covers tenant-scoped duplicate detection and the point in the flow where the duplicate check must occur. It does not define settlement behavior or unrelated payment rails.

## Context

The same external batch identifier can appear in more than one tenant. The system needs a rule that blocks duplicates within a tenant without creating cross-tenant false positives or downstream side effects.

This specification builds on [`SPEC-STD`](../../specs/requirements/spec-trace/SPEC-STD.md) and [`SPEC-TPL`](../../specs/requirements/spec-trace/SPEC-TPL.md).

## [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md) Scope duplicate detection to the tenant and batch identifier
The batch intake flow MUST treat duplicate detection as tenant-scoped and keyed by the external batch identifier.

Trace:
- Supersedes:
  - [`REQ-PAY-ACH-0012`](./SPEC-PAY-ACH.md)
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Source Refs:
  - ACH Operating Rules, duplicate batch handling
  - Payments platform intake policy

## [`REQ-PAY-ACH-0014`](./SPEC-PAY-ACH.md) Reject duplicate ACH batch submission
The batch intake flow MUST reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.

Trace:
- Derived From:
  - [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md)
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Test Refs:
  - tests/payments/ach/duplicate-batch.spec::rejects_second_submission_for_same_tenant
- Code Refs:
  - payments.ach.BatchIntakeGuard.rejectDuplicateBatch

Notes:
- This rule depends on [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md).
- Duplicate means same tenant plus external batch identifier.

## [`REQ-PAY-ACH-0015`](./SPEC-PAY-ACH.md) Allow the same external batch identifier across tenants
The batch intake flow MAY accept the same external batch identifier for a different tenant.

Trace:
- Derived From:
  - [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md)
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Test Refs:
  - tests/payments/ach/duplicate-batch.spec::allows_same_identifier_for_different_tenant
- Code Refs:
  - payments.ach.BatchIdentityKey

## [`REQ-PAY-ACH-0016`](./SPEC-PAY-ACH.md) Check duplicates before downstream side effects
The duplicate check MUST complete before downstream processing begins for the submitted batch.

Trace:
- Derived From:
  - [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md)
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Test Refs:
  - tests/payments/ach/duplicate-batch.spec::does_not_start_processing_for_rejected_duplicate
- Code Refs:
  - payments.ach.BatchSubmissionService.submit
