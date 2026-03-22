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
  - ARC-PAY-ACH-0002
  - WI-PAY-ACH-0081
  - VER-PAY-ACH-0021
---

# SPEC-PAY-ACH - ACH Duplicate Batch Handling

## Purpose

Define the duplicate-handling rules for ACH batch intake.

## Scope

This specification covers tenant-scoped duplicate detection and the point in the flow where the duplicate check must occur. It does not define settlement behavior or unrelated payment rails.

## Context

The same external batch identifier can appear in more than one tenant. The system needs a rule that blocks duplicates within a tenant without creating cross-tenant false positives or downstream side effects.

## REQ-PAY-ACH-0013 Scope duplicate detection to the tenant and batch identifier
The batch intake flow MUST treat duplicate detection as tenant-scoped and keyed by the external batch identifier.

Trace:
- Supersedes:
  - REQ-PAY-ACH-0012
- Satisfied By:
  - ARC-PAY-ACH-0002
- Implemented By:
  - WI-PAY-ACH-0081
- Verified By:
  - VER-PAY-ACH-0021
- Source Refs:
  - ACH Operating Rules, duplicate batch handling
  - Payments platform intake policy

## REQ-PAY-ACH-0014 Reject duplicate ACH batch submission
The batch intake flow MUST reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.

Trace:
- Derived From:
  - REQ-PAY-ACH-0013
- Satisfied By:
  - ARC-PAY-ACH-0002
- Implemented By:
  - WI-PAY-ACH-0081
- Verified By:
  - VER-PAY-ACH-0021
- Test Refs:
  - tests/payments/ach/duplicate-batch.spec::rejects_second_submission_for_same_tenant
- Code Refs:
  - payments.ach.BatchIntakeGuard.rejectDuplicateBatch

Notes:
- Duplicate means same tenant plus external batch identifier.

## REQ-PAY-ACH-0015 Allow the same external batch identifier across tenants
The batch intake flow MAY accept the same external batch identifier for a different tenant.

Trace:
- Derived From:
  - REQ-PAY-ACH-0013
- Satisfied By:
  - ARC-PAY-ACH-0002
- Implemented By:
  - WI-PAY-ACH-0081
- Verified By:
  - VER-PAY-ACH-0021
- Test Refs:
  - tests/payments/ach/duplicate-batch.spec::allows_same_identifier_for_different_tenant
- Code Refs:
  - payments.ach.BatchIdentityKey

## REQ-PAY-ACH-0016 Check duplicates before downstream side effects
The duplicate check MUST complete before downstream processing begins for the submitted batch.

Trace:
- Derived From:
  - REQ-PAY-ACH-0013
- Satisfied By:
  - ARC-PAY-ACH-0002
- Implemented By:
  - WI-PAY-ACH-0081
- Verified By:
  - VER-PAY-ACH-0021
- Test Refs:
  - tests/payments/ach/duplicate-batch.spec::does_not_start_processing_for_rejected_duplicate
- Code Refs:
  - payments.ach.BatchSubmissionService.submit
