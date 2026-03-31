package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-CALC-INT"
    artifact_type: "specification"
    title: "Integer Calculator Contract"
    domain: "calculator-int"
    capability: "int32-calculator"
    status: "approved"
    owner: "platform-core"
    tags: [
        "calculator",
        "integer",
        "int32",
        "example",
    ]
    related_artifacts: [
        "ARC-CALC-INT-0001",
        "WI-CALC-INT-0001",
        "VER-CALC-INT-0001",
    ]
    purpose: "Define a complete worked example for a small integer calculator capability."
    scope: """
      This specification covers one calculator class with five two-operand methods:
      addition, subtraction, multiplication, division, and modulus for signed 32-bit
      integers.
      """
    context: """
      This example is intentionally small, but it is written to auditable depth. It
      shows direct method contracts, operand-order rules, division and remainder
      semantics, boundary handling, and overflow behavior in one traceable set.
      """
    requirements: [
        {
            id: "REQ-CALC-INT-0001"
            title: "Expose one integer calculator class"
            statement: """
              The calculator capability MUST be exposed through one `IntegerCalculator` class
              that defines `Add(int left, int right)`, `Subtract(int left, int right)`,
              `Multiply(int left, int right)`, `Divide(int dividend, int divisor)`, and
              `Modulus(int dividend, int divisor)`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § API Shape](./source-notes.md#api-shape)",
                ]
            }
        },
        {
            id: "REQ-CALC-INT-0002"
            title: "Return the mathematical sum"
            statement: """
              The `Add` method MUST return `left + right` when the exact mathematical sum is
              representable in the signed 32-bit integer range.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Addition](./source-notes.md#addition)",
                ]
            }
            notes: [
                "Unrepresentable sums are rejected according to [`REQ-CALC-INT-0014`](./SPEC-CALC-INT.md).",
            ]
        },
        {
            id: "REQ-CALC-INT-0003"
            title: "Keep addition operand-order invariant"
            statement: """
              The `Add` method MUST return the same result for `(left, right)` and
              `(right, left)`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Addition](./source-notes.md#addition)",
                ]
            }
            notes: [
                "The original request used the word transitive. This example interprets that\nintent as operand-order invariance for addition.",
            ]
        },
        {
            id: "REQ-CALC-INT-0004"
            title: "Use declared subtraction order"
            statement: """
              The `Subtract` method MUST return `left - right` using the declared operand
              order.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Subtraction](./source-notes.md#subtraction)",
                ]
            }
            notes: [
                "Swapping the operands intentionally changes the result unless both operands\nare equal.",
                "Unrepresentable differences are rejected according to [`REQ-CALC-INT-0015`](./SPEC-CALC-INT.md).",
            ]
        },
        {
            id: "REQ-CALC-INT-0005"
            title: "Return the mathematical product"
            statement: """
              The `Multiply` method MUST return `left * right` when the exact mathematical
              product is representable in the signed 32-bit integer range.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Multiplication](./source-notes.md#multiplication)",
                ]
            }
            notes: [
                "Unrepresentable products are rejected according to [`REQ-CALC-INT-0016`](./SPEC-CALC-INT.md).",
            ]
        },
        {
            id: "REQ-CALC-INT-0006"
            title: "Keep multiplication operand-order invariant"
            statement: """
              The `Multiply` method MUST return the same result for `(left, right)` and
              `(right, left)`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Multiplication](./source-notes.md#multiplication)",
                ]
            }
        },
        {
            id: "REQ-CALC-INT-0007"
            title: "Reject division by zero"
            statement: "The `Divide` method MUST reject `divisor = 0` with a divide-by-zero error."
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Division](./source-notes.md#division)",
                ]
            }
            notes: [
                "The exact exception type or error payload is implementation-specific unless a\nstricter requirement adds that constraint.",
            ]
        },
        {
            id: "REQ-CALC-INT-0008"
            title: "Return exact quotients when evenly divisible"
            statement: """
              The `Divide` method MUST return the exact integer quotient when `dividend` is
              evenly divisible by `divisor` and the quotient is representable in the signed
              32-bit integer range.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Division](./source-notes.md#division)",
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                ]
            }
        },
        {
            id: "REQ-CALC-INT-0009"
            title: "Truncate fractional quotients toward zero"
            statement: """
              The `Divide` method MUST truncate the mathematical quotient toward `0` when
              `dividend / divisor` is not an integer and the truncated quotient is
              representable in the signed 32-bit integer range.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Division](./source-notes.md#division)",
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                ]
            }
            notes: [
                "`7 / 2` returns `3`.",
                "`-7 / 2` returns `-3`.",
                "Negative divisors follow [`REQ-CALC-INT-0017`](./SPEC-CALC-INT.md).",
            ]
        },
        {
            id: "REQ-CALC-INT-0010"
            title: "Reject modulus by zero"
            statement: "The `Modulus` method MUST reject `divisor = 0` with a divide-by-zero error."
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Modulus](./source-notes.md#modulus)",
                ]
            }
        },
        {
            id: "REQ-CALC-INT-0011"
            title: "Return truncation-based signed remainders"
            statement: """
              The `Modulus` method MUST return the unique remainder `r` such that
              `dividend = (divisor * q) + r`, `|r| < |divisor|`, and `r` has the same sign as
              `dividend` or is `0`, where `q` is the mathematical quotient truncated toward
              `0`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Modulus](./source-notes.md#modulus)",
                    "[`source-notes.md` § Division](./source-notes.md#division)",
                ]
            }
            notes: [
                "`7 % 2` returns `1`.",
                "`-7 % 2` returns `-1`.",
            ]
        },
        {
            id: "REQ-CALC-INT-0012"
            title: "Accept boundary operands when results are representable"
            statement: """
              Each method MUST accept `Int32.MinValue` and `Int32.MaxValue` as operands when
              its specified result remains representable in the signed 32-bit integer range.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                    "calculator.int32.IntegerCalculator.Subtract",
                    "calculator.int32.IntegerCalculator.Multiply",
                    "calculator.int32.IntegerCalculator.Divide",
                    "calculator.int32.IntegerCalculator.Modulus",
                ]
            }
        },
        {
            id: "REQ-CALC-INT-0013"
            title: "Reject unrepresentable results as overflow"
            statement: """
              Each method MUST reject an operation when the exact mathematical result
              required by its contract falls outside the signed 32-bit integer range from
              `-2147483648` to `2147483647`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                    "calculator.int32.IntegerCalculator.Subtract",
                    "calculator.int32.IntegerCalculator.Multiply",
                    "calculator.int32.IntegerCalculator.Divide",
                    "calculator.int32.IntegerCalculator.Modulus",
                ]
            }
            notes: [
                "Representative overflow cases include `2147483647 + 1`,\n`-2147483648 - 1`, `50000 * 50000`, and `-2147483648 / -1`.",
                "Results below `-2147483648` are treated as underflow and are rejected by the\nsame rule.",
            ]
        },
        {
            id: "REQ-CALC-INT-0014"
            title: "Reject unrepresentable sums"
            statement: """
              The `Add` method MUST reject a call when `left + right` is greater than
              `2147483647` or less than `-2147483648`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Addition](./source-notes.md#addition)",
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                ]
            }
            notes: [
                "`2147483647 + 1` is rejected as overflow.",
                "`-2147483648 + -1` is rejected as underflow.",
            ]
        },
        {
            id: "REQ-CALC-INT-0015"
            title: "Reject unrepresentable differences"
            statement: """
              The `Subtract` method MUST reject a call when `left - right` is greater than
              `2147483647` or less than `-2147483648`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Subtraction](./source-notes.md#subtraction)",
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                ]
            }
            notes: [
                "`-2147483648 - 1` is rejected as underflow.",
                "`2147483647 - -1` is rejected as overflow.",
            ]
        },
        {
            id: "REQ-CALC-INT-0016"
            title: "Reject unrepresentable products"
            statement: """
              The `Multiply` method MUST reject a call when `left * right` is greater than
              `2147483647` or less than `-2147483648`.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Multiplication](./source-notes.md#multiplication)",
                    "[`source-notes.md` § Integer Range And Overflow](./source-notes.md#integer-range-and-overflow)",
                ]
            }
            notes: [
                "`50000 * 50000` is rejected as overflow.",
                "`50000 * -50000` is rejected as underflow.",
            ]
        },
        {
            id: "REQ-CALC-INT-0017"
            title: "Use signed quotients for negative divisors"
            statement: """
              The `Divide` method MUST allow a negative non-zero `divisor` and return a
              positive quotient when `dividend` and `divisor` have the same sign or a
              negative quotient when they have different signs.
              """
            trace: {
                satisfied_by: [
                    "ARC-CALC-INT-0001",
                ]
                implemented_by: [
                    "WI-CALC-INT-0001",
                ]
                verified_by: [
                    "VER-CALC-INT-0001",
                ]
                upstream_refs: [
                    "[`source-notes.md` § Division](./source-notes.md#division)",
                ]
            }
            notes: [
                "`7 / -2` returns `-3`.",
                "`-7 / -2` returns `3`.",
            ]
        },
    ]
}
