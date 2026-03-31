package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Architecture & {
    artifact_id: "ARC-CALC-INT-0001"
    artifact_type: "architecture"
    title: "Integer Calculator Design"
    domain: "calculator-int"
    status: "approved"
    owner: "platform-core"
    related_artifacts: [
        "SPEC-CALC-INT",
        "WI-CALC-INT-0001",
        "VER-CALC-INT-0001",
    ]
    purpose: """
      Describe the minimal but complete design used to satisfy the integer
      calculator contract.
      """
    satisfies: [
        "REQ-CALC-INT-0001",
        "REQ-CALC-INT-0002",
        "REQ-CALC-INT-0003",
        "REQ-CALC-INT-0004",
        "REQ-CALC-INT-0005",
        "REQ-CALC-INT-0006",
        "REQ-CALC-INT-0007",
        "REQ-CALC-INT-0008",
        "REQ-CALC-INT-0009",
        "REQ-CALC-INT-0010",
        "REQ-CALC-INT-0011",
        "REQ-CALC-INT-0012",
        "REQ-CALC-INT-0013",
        "REQ-CALC-INT-0014",
        "REQ-CALC-INT-0015",
        "REQ-CALC-INT-0016",
        "REQ-CALC-INT-0017",
    ]
    design_summary: """
      The design uses one stateless `IntegerCalculator` class with five public
      methods. Addition, subtraction, and multiplication use overflow-checked integer
      arithmetic. Division and modulus validate the divisor before evaluation, apply
      truncation-toward-zero integer semantics, preserve quotient signs for negative
      divisors, and reject unrepresentable results.
      """
    key_components: [
        "`IntegerCalculator` public API with `Add`, `Subtract`, `Multiply`, `Divide`,\nand `Modulus`",
        "operation-specific overflow and underflow guards for addition, subtraction,\nand multiplication",
        "shared zero-divisor guard for `Divide` and `Modulus`",
        "sign-aware integer division for negative divisors",
        "signed remainder rule aligned with truncation-toward-zero division",
    ]
    data_and_state_considerations: """
      The calculator is stateless. Every method consumes two signed 32-bit integer
      operands and returns one signed 32-bit integer result or an error. No method
      stores prior results, caches operands, or changes behavior based on call
      history.
      """
    edge_cases_and_constraints: [
        "`Int32.MinValue` and `Int32.MaxValue` are allowed as operands when the method\ncan still produce a representable result.",
        "Overflow cases are rejected rather than saturated or silently wrapped.",
        "Underflow cases are rejected rather than wrapped.",
        "`Divide` rejects `divisor = 0`.",
        "`Modulus` rejects `divisor = 0`.",
        "Fractional division results truncate toward `0`.",
        "Negative divisors are allowed and determine the quotient sign in the same way\nas the mathematical signs of the operands.",
        "Remainders keep the sign of the dividend or return `0`.",
        "`-2147483648 / -1` is rejected because the quotient is not representable in a\nsigned 32-bit integer.",
    ]
    alternatives_considered: [
        "Saturating arithmetic was rejected because it hides overflow.",
        "Wrapping arithmetic was rejected because it makes boundary errors difficult to\naudit.",
        "Floor division was rejected because it changes negative quotient and\nremainder behavior.",
        "Separate operation classes were rejected because the example is intentionally\nscoped to one small calculator class.",
    ]
    risks: [
        "Some languages treat integer overflow differently by default, so the example\ndepends on an explicit checked-arithmetic policy instead of ambient runtime\nbehavior.",
        "Teams that want arbitrary precision would need a different contract instead\nof stretching this one.",
    ]
}
