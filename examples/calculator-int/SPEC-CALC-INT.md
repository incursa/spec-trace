---
artifact_id: SPEC-CALC-INT
artifact_type: specification
title: Integer Calculator Contract
domain: calculator-int
capability: int32-calculator
status: approved
owner: platform-core
tags:
  - calculator
  - integer
  - int32
  - example
related_artifacts:
  - ARC-CALC-INT-0001
  - WI-CALC-INT-0001
  - VER-CALC-INT-0001
---

# [`SPEC-CALC-INT`](./SPEC-CALC-INT.md) - Integer Calculator Contract

## Purpose

Define a complete worked example for a small integer calculator capability.

## Scope

This specification covers one calculator class with five two-operand methods:
addition, subtraction, multiplication, division, and modulus for signed 32-bit
integers.

## Context

This example is intentionally small, but it is written to auditable depth. It
shows direct method contracts, operand-order rules, division and remainder
semantics, boundary handling, and overflow behavior in one traceable set.

## [`REQ-CALC-INT-0001`](./SPEC-CALC-INT.md) Expose one integer calculator class
The calculator capability MUST be exposed through one `IntegerCalculator` class
that defines `Add(int left, int right)`, `Subtract(int left, int right)`,
`Multiply(int left, int right)`, `Divide(int dividend, int divisor)`, and
`Modulus(int dividend, int divisor)`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § API Shape](./source-notes.md#api-shape)

## [`REQ-CALC-INT-0002`](./SPEC-CALC-INT.md) Return the mathematical sum
The `Add` method MUST return `left + right` when the exact mathematical sum is
representable in the signed 32-bit integer range.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Addition](./source-notes.md#addition)

Notes:
- Unrepresentable sums are rejected according to [`REQ-CALC-INT-0014`](./SPEC-CALC-INT.md).

## [`REQ-CALC-INT-0003`](./SPEC-CALC-INT.md) Keep addition operand-order invariant
The `Add` method MUST return the same result for `(left, right)` and
`(right, left)`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Addition](./source-notes.md#addition)

Notes:
- The original request used the word transitive. This example interprets that
  intent as operand-order invariance for addition.

## [`REQ-CALC-INT-0004`](./SPEC-CALC-INT.md) Use declared subtraction order
The `Subtract` method MUST return `left - right` using the declared operand
order.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Subtraction](./source-notes.md#subtraction)

Notes:
- Swapping the operands intentionally changes the result unless both operands
  are equal.
- Unrepresentable differences are rejected according to [`REQ-CALC-INT-0015`](./SPEC-CALC-INT.md).

## [`REQ-CALC-INT-0005`](./SPEC-CALC-INT.md) Return the mathematical product
The `Multiply` method MUST return `left * right` when the exact mathematical
product is representable in the signed 32-bit integer range.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Multiplication](./source-notes.md#multiplication)

Notes:
- Unrepresentable products are rejected according to [`REQ-CALC-INT-0016`](./SPEC-CALC-INT.md).

## [`REQ-CALC-INT-0006`](./SPEC-CALC-INT.md) Keep multiplication operand-order invariant
The `Multiply` method MUST return the same result for `(left, right)` and
`(right, left)`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Multiplication](./source-notes.md#multiplication)

## [`REQ-CALC-INT-0007`](./SPEC-CALC-INT.md) Reject division by zero
The `Divide` method MUST reject `divisor = 0` with a divide-by-zero error.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Division](./source-notes.md#division)

Notes:
- The exact exception type or error payload is implementation-specific unless a
  stricter requirement adds that constraint.

## [`REQ-CALC-INT-0008`](./SPEC-CALC-INT.md) Return exact quotients when evenly divisible
The `Divide` method MUST return the exact integer quotient when `dividend` is
evenly divisible by `divisor` and the quotient is representable in the signed
32-bit integer range.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Division](./source-notes.md#division)
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)

## [`REQ-CALC-INT-0009`](./SPEC-CALC-INT.md) Truncate fractional quotients toward zero
The `Divide` method MUST truncate the mathematical quotient toward `0` when
`dividend / divisor` is not an integer and the truncated quotient is
representable in the signed 32-bit integer range.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Division](./source-notes.md#division)
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)

Notes:
- `7 / 2` returns `3`.
- `-7 / 2` returns `-3`.
- Negative divisors follow [`REQ-CALC-INT-0017`](./SPEC-CALC-INT.md).

## [`REQ-CALC-INT-0010`](./SPEC-CALC-INT.md) Reject modulus by zero
The `Modulus` method MUST reject `divisor = 0` with a divide-by-zero error.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Modulus](./source-notes.md#modulus)

## [`REQ-CALC-INT-0011`](./SPEC-CALC-INT.md) Return truncation-based signed remainders
The `Modulus` method MUST return the unique remainder `r` such that
`dividend = (divisor * q) + r`, `|r| < |divisor|`, and `r` has the same sign as
`dividend` or is `0`, where `q` is the mathematical quotient truncated toward
`0`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Modulus](./source-notes.md#modulus)
  - [`source-notes.md` § Division](./source-notes.md#division)

Notes:
- `7 % 2` returns `1`.
- `-7 % 2` returns `-1`.

## [`REQ-CALC-INT-0012`](./SPEC-CALC-INT.md) Accept boundary operands when results are representable
Each method MUST accept `Int32.MinValue` and `Int32.MaxValue` as operands when
its specified result remains representable in the signed 32-bit integer range.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)
  - calculator.int32.IntegerCalculator.Subtract
  - calculator.int32.IntegerCalculator.Multiply
  - calculator.int32.IntegerCalculator.Divide
  - calculator.int32.IntegerCalculator.Modulus

## [`REQ-CALC-INT-0013`](./SPEC-CALC-INT.md) Reject unrepresentable results as overflow
Each method MUST reject an operation when the exact mathematical result
required by its contract falls outside the signed 32-bit integer range from
`-2147483648` to `2147483647`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)
  - calculator.int32.IntegerCalculator.Subtract
  - calculator.int32.IntegerCalculator.Multiply
  - calculator.int32.IntegerCalculator.Divide
  - calculator.int32.IntegerCalculator.Modulus

Notes:
- Representative overflow cases include `2147483647 + 1`,
  `-2147483648 - 1`, `50000 * 50000`, and `-2147483648 / -1`.
- Results below `-2147483648` are treated as underflow and are rejected by the
  same rule.

## [`REQ-CALC-INT-0014`](./SPEC-CALC-INT.md) Reject unrepresentable sums
The `Add` method MUST reject a call when `left + right` is greater than
`2147483647` or less than `-2147483648`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Addition](./source-notes.md#addition)
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)

Notes:
- `2147483647 + 1` is rejected as overflow.
- `-2147483648 + -1` is rejected as underflow.

## [`REQ-CALC-INT-0015`](./SPEC-CALC-INT.md) Reject unrepresentable differences
The `Subtract` method MUST reject a call when `left - right` is greater than
`2147483647` or less than `-2147483648`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Subtraction](./source-notes.md#subtraction)
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)

Notes:
- `-2147483648 - 1` is rejected as underflow.
- `2147483647 - -1` is rejected as overflow.

## [`REQ-CALC-INT-0016`](./SPEC-CALC-INT.md) Reject unrepresentable products
The `Multiply` method MUST reject a call when `left * right` is greater than
`2147483647` or less than `-2147483648`.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Multiplication](./source-notes.md#multiplication)
  - [`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)

Notes:
- `50000 * 50000` is rejected as overflow.
- `50000 * -50000` is rejected as underflow.

## [`REQ-CALC-INT-0017`](./SPEC-CALC-INT.md) Use signed quotients for negative divisors
The `Divide` method MUST allow a negative non-zero `divisor` and return a
positive quotient when `dividend` and `divisor` have the same sign or a
negative quotient when they have different signs.

Trace:
- Satisfied By:
  - [ARC-CALC-INT-0001](./sample-architecture.md)
- Implemented By:
  - [WI-CALC-INT-0001](./sample-work-item.md)
- Verified By:
  - [VER-CALC-INT-0001](./sample-verification.md)
- Upstream Refs:
  - [`source-notes.md` § Division](./source-notes.md#division)

Notes:
- `7 / -2` returns `-3`.
- `-7 / -2` returns `3`.

