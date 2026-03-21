---
artifact_id: SPEC-PAY-001
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
  - ARC-PAY-002
  - WI-PAY-081
  - VER-PAY-021
---

# SPEC-PAY-001 - ACH Duplicate Batch Handling

## Purpose

Define the rule for handling duplicate ACH batch submissions for the payments capability.

## Scope

This specification covers duplicate detection for submitted ACH batches within a tenant. It does not define downstream settlement behavior or network-specific ACH processing rules.

## Context

Duplicate batch submissions can create double processing risk if the same external batch identifier is accepted more than once for the same tenant.

## REQ-PAY-014 Reject duplicate ACH batch submission

Type: functional
Status: approved
Priority: high
Source: BR-PAY-003
Verification: manual
Satisfied By: ARC-PAY-002
Implemented By: WI-PAY-081
Verified By: VER-PAY-021

Requirement:
The system shall reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.

Rationale:
This prevents duplicate payment processing and makes the duplicate-handling rule explicit.

Notes:
A duplicate is determined by tenant and external batch identifier, not by submission timestamp.

## Open Questions

- Should duplicate detection keep historical accepted identifiers indefinitely or for a bounded retention window?
