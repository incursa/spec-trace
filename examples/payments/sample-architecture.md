---
artifact_id: ARC-PAY-ACH-0002
artifact_type: architecture
title: ACH Duplicate Batch Rejection Design
domain: payments
status: approved
owner: payments-platform
satisfies:
  - REQ-PAY-ACH-0014
related_artifacts:
  - SPEC-PAY-ACH-0001
  - WI-PAY-ACH-0081
  - VER-PAY-ACH-0021
---

# ARC-PAY-ACH-0002 - ACH Duplicate Batch Rejection Design

## Purpose

Describe the design used to satisfy REQ-PAY-ACH-0014 by rejecting duplicate ACH batch submissions for the same tenant and external batch identifier.

## Requirements Satisfied

- REQ-PAY-ACH-0014

## Design Summary

The intake path checks a tenant-scoped batch registry before accepting a batch for processing. If the tenant and external batch identifier already exist in the accepted registry, the batch is rejected with a duplicate error and no downstream processing is started.

## Key Components

- batch intake guard
- tenant-scoped accepted batch registry
- duplicate rejection response mapping
- acceptance persistence step

## Data and State Considerations

The design stores the tenant identifier and the external batch identifier for every accepted batch. The registry must support a deterministic existence check before the batch is admitted for processing.

## Edge Cases and Constraints

- A batch identifier that exists for a different tenant is not a duplicate.
- A retry of the same submitted batch after acceptance is a duplicate.
- The duplicate check must occur before any downstream processing that could create payment side effects.

## Alternatives Considered

- payload-hash matching alone was rejected because the duplicate rule is tenant plus external batch identifier, not payload equivalence
- deferring duplicate detection until later in the processing flow was rejected because it allows avoidable side effects

## Risks

- registry lag or stale state could permit duplicate acceptance if persistence is not atomic
- unclear retention rules could cause the accepted registry to grow without bound

## Open Questions

- What retention policy should govern accepted batch identifiers after settlement or archival?
