---
artifact_id: WI-CALC-INT-0001
artifact_type: work_item
title: Implement integer calculator operations and evidence-linked tests
domain: calculator-int
status: complete
owner: platform-core
addresses:
  - REQ-CALC-INT-0001
  - REQ-CALC-INT-0002
  - REQ-CALC-INT-0003
  - REQ-CALC-INT-0004
  - REQ-CALC-INT-0005
  - REQ-CALC-INT-0006
  - REQ-CALC-INT-0007
  - REQ-CALC-INT-0008
  - REQ-CALC-INT-0009
  - REQ-CALC-INT-0010
  - REQ-CALC-INT-0011
  - REQ-CALC-INT-0012
  - REQ-CALC-INT-0013
  - REQ-CALC-INT-0014
  - REQ-CALC-INT-0015
  - REQ-CALC-INT-0016
  - REQ-CALC-INT-0017
design_links:
  - ARC-CALC-INT-0001
verification_links:
  - VER-CALC-INT-0001
related_artifacts:
  - SPEC-CALC-INT
  - ARC-CALC-INT-0001
  - VER-CALC-INT-0001
---

# [`WI-CALC-INT-0001`](./sample-work-item.md) - Implement Integer Calculator Operations And Evidence-Linked Tests

## Summary

Implement the `IntegerCalculator` class, apply checked integer arithmetic, and
cover the full operation contract with direct requirement-linked tests.

## Requirements Addressed

- [REQ-CALC-INT-0001](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0002](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0003](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0004](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0005](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0006](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0007](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0008](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0009](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0010](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0011](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0012](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0013](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0014](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0015](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0016](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0017](./SPEC-CALC-INT.md)

## Design Inputs

- [ARC-CALC-INT-0001](./sample-architecture.md)

## Planned Changes

Create one `IntegerCalculator` class with five two-operand methods. Implement
checked addition, subtraction, and multiplication. Implement division and
modulus with explicit zero-divisor guards, truncation-toward-zero semantics,
negative-divisor sign handling, boundary tests, and explicit overflow and
underflow coverage for unrepresentable results.

## Out of Scope

- floating-point calculator behavior
- arbitrary-precision arithmetic
- localization of error text or exception payloads
- expression parsing, operator precedence, or user-interface concerns

## Verification Plan

Use [VER-CALC-INT-0001](./sample-verification.md),
[unit-tests.evidence.json](./generated/unit-tests.evidence.json), and
[implementation-map.evidence.json](./generated/implementation-map.evidence.json)
to prove every operation rule, boundary rule, signed-division rule, and
overflow or underflow rule.

## Completion Notes

The implementation keeps one stable class-level code reference at
`calculator.int32.IntegerCalculator` and method-level references under the same
namespace.

## Trace Links

Addresses:

- [REQ-CALC-INT-0001](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0002](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0003](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0004](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0005](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0006](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0007](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0008](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0009](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0010](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0011](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0012](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0013](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0014](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0015](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0016](./SPEC-CALC-INT.md)
- [REQ-CALC-INT-0017](./SPEC-CALC-INT.md)

Uses Design:

- [ARC-CALC-INT-0001](./sample-architecture.md)

Verified By:

- [VER-CALC-INT-0001](./sample-verification.md)
