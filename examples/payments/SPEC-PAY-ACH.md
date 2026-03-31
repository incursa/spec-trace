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

# [`SPEC-PAY-ACH`](./SPEC-PAY-ACH.md) - ACH Duplicate Batch Handling

## Purpose

Define the duplicate-handling rules for ACH batch intake.

## Scope

This specification covers tenant-scoped duplicate detection and the point in the flow where the duplicate check must occur. It does not define settlement behavior or unrelated payment rails.

## Context

The same external batch identifier can appear in more than one tenant. The system needs a rule that blocks duplicates within a tenant without creating cross-tenant false positives or downstream side effects.

This specification builds on [`SPEC-STD`](../../specs/requirements/spec-trace/SPEC-STD.md) and [`SPEC-TPL`](../../specs/requirements/spec-trace/SPEC-TPL.md).

## [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md#req-pay-ach-0013-scope-duplicate-detection-to-the-tenant-and-batch-identifier) Scope duplicate detection to the tenant and batch identifier
The batch intake flow MUST treat duplicate detection as tenant-scoped and keyed by the external batch identifier.

Trace:
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Supersedes:
  - `REQ-PAY-ACH-0012`
- Upstream Refs:
  - ACH Operating Rules, duplicate batch handling
  - Payments platform intake policy

## [`REQ-PAY-ACH-0014`](./SPEC-PAY-ACH.md#req-pay-ach-0014-reject-duplicate-ach-batch-submission) Reject duplicate ACH batch submission
The batch intake flow MUST reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.

Trace:
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Derived From:
  - [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md#req-pay-ach-0013-scope-duplicate-detection-to-the-tenant-and-batch-identifier)

Notes:
- This rule depends on [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md).
- Duplicate means same tenant plus external batch identifier.

## [`REQ-PAY-ACH-0015`](./SPEC-PAY-ACH.md#req-pay-ach-0015-allow-the-same-external-batch-identifier-across-tenants) Allow the same external batch identifier across tenants
The batch intake flow MAY accept the same external batch identifier for a different tenant.

Trace:
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Derived From:
  - [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md#req-pay-ach-0013-scope-duplicate-detection-to-the-tenant-and-batch-identifier)

## [`REQ-PAY-ACH-0016`](./SPEC-PAY-ACH.md#req-pay-ach-0016-check-duplicates-before-downstream-side-effects) Check duplicates before downstream side effects
The duplicate check MUST complete before downstream processing begins for the submitted batch.

Trace:
- Satisfied By:
  - [ARC-PAY-ACH-0002](./sample-architecture.md)
- Implemented By:
  - [WI-PAY-ACH-0081](./sample-work-item.md)
- Verified By:
  - [VER-PAY-ACH-0021](./sample-verification.md)
- Derived From:
  - [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md#req-pay-ach-0013-scope-duplicate-detection-to-the-tenant-and-batch-identifier)
