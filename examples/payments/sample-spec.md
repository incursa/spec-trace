---
artifact_id: SPEC-PAY-ACH-0001
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

# SPEC-PAY-ACH-0001 - ACH Duplicate Batch Handling

## Purpose

Define the rule for handling duplicate ACH batch submissions for the payments capability.

## Scope

This specification covers duplicate detection for submitted ACH batches within a tenant. It does not define downstream settlement behavior or network-specific ACH processing rules.

## Context

Duplicate batch submissions can create double processing risk if the same external batch identifier is accepted more than once for the same tenant.

## REQ-PAY-ACH-0014 Reject duplicate ACH batch submission

Type: functional
Status: approved
Priority: high
Source: BR-PAY-0003
Verification: manual
Satisfied By: ARC-PAY-ACH-0002
Implemented By: WI-PAY-ACH-0081
Verified By: VER-PAY-ACH-0021

Requirement:
The system shall reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.

Rationale:
This prevents duplicate payment processing and makes the duplicate-handling rule explicit.

Notes:
A duplicate is determined by tenant and external batch identifier, not by submission timestamp.

## Open Questions

- Should duplicate detection keep historical accepted identifiers indefinitely or for a bounded retention window?
