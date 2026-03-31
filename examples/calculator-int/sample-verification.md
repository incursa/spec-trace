---
artifact_id: VER-CALC-INT-0001
artifact_type: verification
title: Integer calculator verification
domain: calculator-int
status: passed
owner: platform-core
related_artifacts:
  - SPEC-CALC-INT
  - ARC-CALC-INT-0001
  - WI-CALC-INT-0001
verifies:
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
---

# [`VER-CALC-INT-0001`](./sample-verification.md) - Integer calculator verification

## Scope

Verify the full auditable contract for the signed 32-bit integer calculator
example.

## Requirements Verified

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

## Verification Method

Automated execution of requirement-linked tests plus boundary-case analysis of
integer range behavior.

## Preconditions

- the `calculator.int32.IntegerCalculator` implementation is available
- the requirement-linked test suite can execute in a checked-arithmetic mode
- the runtime exposes a divide-by-zero error path for rejected divisors

## Procedure or Approach

1. Confirm that one `IntegerCalculator` class exposes the five named methods.
2. Execute representative addition cases and verify the returned sums.
3. Swap the addition operands and confirm the result does not change.
4. Execute addition overflow and underflow cases and confirm the calls are
rejected.
5. Execute representative subtraction cases and confirm the method uses the
declared operand order.
6. Execute subtraction overflow and underflow cases and confirm the calls are
rejected.
7. Execute representative multiplication cases and verify the returned
products.
8. Swap the multiplication operands and confirm the result does not change.
9. Execute multiplication overflow and underflow cases and confirm the calls
are rejected.
10. Call `Divide` with `divisor = 0` and confirm the call is rejected.
11. Execute evenly divisible division cases and confirm the exact quotient is
returned.
12. Execute fractional division cases with positive and negative dividends and
confirm truncation toward `0`.
13. Execute division cases with negative divisors and confirm the quotient
sign follows the operand signs.
14. Call `Modulus` with `divisor = 0` and confirm the call is rejected.
15. Execute representative remainder cases with positive and negative
dividends and confirm the signed remainder rule.
16. Execute boundary-value cases that remain representable and confirm the
operations succeed.
17. Execute generic unrepresentable-result cases such as
`-2147483648 / -1` and confirm the call is rejected instead of wrapping or
saturating.

## Expected Result

The example exposes one integer calculator class with five methods, applies the
named arithmetic rules for each method, accepts valid boundary operands,
rejects divide-by-zero, handles negative divisors with the expected quotient
sign, and rejects both overflow and underflow cases.

## Evidence

- [`unit-tests.evidence.json`](./generated/unit-tests.evidence.json)
- [`implementation-map.evidence.json`](./generated/implementation-map.evidence.json)

## Status

This `passed` status applies to every requirement listed in `verifies`.

passed

## Related Artifacts

- [SPEC-CALC-INT](./SPEC-CALC-INT.md)
- [ARC-CALC-INT-0001](./sample-architecture.md)
- [WI-CALC-INT-0001](./sample-work-item.md)
