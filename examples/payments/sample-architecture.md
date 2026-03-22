---
artifact_id: ARC-PAY-ACH-0002
artifact_type: architecture
title: ACH Duplicate Batch Rejection Design
domain: payments
status: approved
owner: payments-platform
satisfies:
  - REQ-PAY-ACH-0013
  - REQ-PAY-ACH-0014
  - REQ-PAY-ACH-0015
  - REQ-PAY-ACH-0016
related_artifacts:
  - SPEC-PAY-ACH
  - WI-PAY-ACH-0081
  - VER-PAY-ACH-0021
---

# ARC-PAY-ACH-0002 - ACH Duplicate Batch Rejection Design

## Purpose

Describe how ACH batch intake enforces the tenant-scoped duplicate rules defined in `SPEC-PAY-ACH`.

## Requirements Satisfied

- REQ-PAY-ACH-0013
- REQ-PAY-ACH-0014
- REQ-PAY-ACH-0015
- REQ-PAY-ACH-0016

## Design Summary

The intake path computes a tenant-scoped batch identity key and checks an accepted-batch registry before any downstream work is started. A same-tenant duplicate is rejected immediately. A different tenant may reuse the same external batch identifier because the tenant is part of the identity key.

## Key Components

- tenant-scoped batch identity key
- accepted-batch registry
- duplicate guard at intake
- downstream dispatch gate

## Data and State Considerations

The design persists the tenant identifier and external batch identifier for each accepted batch. The duplicate decision is made against that combined key before a batch is queued or processed further.

## Edge Cases and Constraints

- The same external batch identifier is not a duplicate when the tenant differs.
- A retry after acceptance is a duplicate when the tenant and external batch identifier are unchanged.
- Rejected duplicates must not reach downstream processing.

## Alternatives Considered

- Payload-hash matching alone was rejected because the rule is tenant plus external batch identifier.
- Late duplicate detection was rejected because it allows avoidable side effects.

## Risks

- Non-atomic writes to the accepted-batch registry could permit duplicate acceptance.
