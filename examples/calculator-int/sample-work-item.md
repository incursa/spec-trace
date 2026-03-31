---
artifact_id: WI-CALC-INT-0001
artifact_type: work_item
title: Implement integer calculator operations and evidence-linked tests
domain: calculator-int
status: complete
owner: platform-core
related_artifacts:
  - SPEC-CALC-INT
  - ARC-CALC-INT-0001
  - VER-CALC-INT-0001
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
---

# [`WI-CALC-INT-0001`](./sample-work-item.md) - Implement integer calculator operations and evidence-linked tests

## Summary

Implement the `IntegerCalculator` class, apply checked integer arithmetic, and
cover the full operation contract with direct requirement-linked tests.

## Requirements Addressed

- [`REQ-CALC-INT-0001`](./SPEC-CALC-INT.md#req-calc-int-0001-expose-one-integer-calculator-class)
- [`REQ-CALC-INT-0002`](./SPEC-CALC-INT.md#req-calc-int-0002-return-the-mathematical-sum)
- [`REQ-CALC-INT-0003`](./SPEC-CALC-INT.md#req-calc-int-0003-keep-addition-operand-order-invariant)
- [`REQ-CALC-INT-0004`](./SPEC-CALC-INT.md#req-calc-int-0004-use-declared-subtraction-order)
- [`REQ-CALC-INT-0005`](./SPEC-CALC-INT.md#req-calc-int-0005-return-the-mathematical-product)
- [`REQ-CALC-INT-0006`](./SPEC-CALC-INT.md#req-calc-int-0006-keep-multiplication-operand-order-invariant)
- [`REQ-CALC-INT-0007`](./SPEC-CALC-INT.md#req-calc-int-0007-reject-division-by-zero)
- [`REQ-CALC-INT-0008`](./SPEC-CALC-INT.md#req-calc-int-0008-return-exact-quotients-when-evenly-divisible)
- [`REQ-CALC-INT-0009`](./SPEC-CALC-INT.md#req-calc-int-0009-truncate-fractional-quotients-toward-zero)
- [`REQ-CALC-INT-0010`](./SPEC-CALC-INT.md#req-calc-int-0010-reject-modulus-by-zero)
- [`REQ-CALC-INT-0011`](./SPEC-CALC-INT.md#req-calc-int-0011-return-truncation-based-signed-remainders)
- [`REQ-CALC-INT-0012`](./SPEC-CALC-INT.md#req-calc-int-0012-accept-boundary-operands-when-results-are-representable)
- [`REQ-CALC-INT-0013`](./SPEC-CALC-INT.md#req-calc-int-0013-reject-unrepresentable-results-as-overflow)
- [`REQ-CALC-INT-0014`](./SPEC-CALC-INT.md#req-calc-int-0014-reject-unrepresentable-sums)
- [`REQ-CALC-INT-0015`](./SPEC-CALC-INT.md#req-calc-int-0015-reject-unrepresentable-differences)
- [`REQ-CALC-INT-0016`](./SPEC-CALC-INT.md#req-calc-int-0016-reject-unrepresentable-products)
- [`REQ-CALC-INT-0017`](./SPEC-CALC-INT.md#req-calc-int-0017-use-signed-quotients-for-negative-divisors)

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

- [`REQ-CALC-INT-0001`](./SPEC-CALC-INT.md#req-calc-int-0001-expose-one-integer-calculator-class)
- [`REQ-CALC-INT-0002`](./SPEC-CALC-INT.md#req-calc-int-0002-return-the-mathematical-sum)
- [`REQ-CALC-INT-0003`](./SPEC-CALC-INT.md#req-calc-int-0003-keep-addition-operand-order-invariant)
- [`REQ-CALC-INT-0004`](./SPEC-CALC-INT.md#req-calc-int-0004-use-declared-subtraction-order)
- [`REQ-CALC-INT-0005`](./SPEC-CALC-INT.md#req-calc-int-0005-return-the-mathematical-product)
- [`REQ-CALC-INT-0006`](./SPEC-CALC-INT.md#req-calc-int-0006-keep-multiplication-operand-order-invariant)
- [`REQ-CALC-INT-0007`](./SPEC-CALC-INT.md#req-calc-int-0007-reject-division-by-zero)
- [`REQ-CALC-INT-0008`](./SPEC-CALC-INT.md#req-calc-int-0008-return-exact-quotients-when-evenly-divisible)
- [`REQ-CALC-INT-0009`](./SPEC-CALC-INT.md#req-calc-int-0009-truncate-fractional-quotients-toward-zero)
- [`REQ-CALC-INT-0010`](./SPEC-CALC-INT.md#req-calc-int-0010-reject-modulus-by-zero)
- [`REQ-CALC-INT-0011`](./SPEC-CALC-INT.md#req-calc-int-0011-return-truncation-based-signed-remainders)
- [`REQ-CALC-INT-0012`](./SPEC-CALC-INT.md#req-calc-int-0012-accept-boundary-operands-when-results-are-representable)
- [`REQ-CALC-INT-0013`](./SPEC-CALC-INT.md#req-calc-int-0013-reject-unrepresentable-results-as-overflow)
- [`REQ-CALC-INT-0014`](./SPEC-CALC-INT.md#req-calc-int-0014-reject-unrepresentable-sums)
- [`REQ-CALC-INT-0015`](./SPEC-CALC-INT.md#req-calc-int-0015-reject-unrepresentable-differences)
- [`REQ-CALC-INT-0016`](./SPEC-CALC-INT.md#req-calc-int-0016-reject-unrepresentable-products)
- [`REQ-CALC-INT-0017`](./SPEC-CALC-INT.md#req-calc-int-0017-use-signed-quotients-for-negative-divisors)

Uses Design:

- [ARC-CALC-INT-0001](./sample-architecture.md)

Verified By:

- [VER-CALC-INT-0001](./sample-verification.md)
