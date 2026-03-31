package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Verification & {
    artifact_id: "VER-CALC-INT-0001"
    artifact_type: "verification"
    title: "Integer calculator verification"
    domain: "calculator-int"
    status: "passed"
    owner: "platform-core"
    related_artifacts: [
        "SPEC-CALC-INT",
        "ARC-CALC-INT-0001",
        "WI-CALC-INT-0001",
    ]
    scope: """
      Verify the full auditable contract for the signed 32-bit integer calculator
      example.
      """
    verifies: [
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
    verification_method: """
      Automated execution of requirement-linked tests plus boundary-case analysis of
      integer range behavior.
      """
    preconditions: [
        "the `calculator.int32.IntegerCalculator` implementation is available",
        "the requirement-linked test suite can execute in a checked-arithmetic mode",
        "the runtime exposes a divide-by-zero error path for rejected divisors",
    ]
    procedure: [
        "Confirm that one `IntegerCalculator` class exposes the five named methods.",
        "Execute representative addition cases and verify the returned sums.",
        "Swap the addition operands and confirm the result does not change.",
        "Execute addition overflow and underflow cases and confirm the calls are\nrejected.",
        "Execute representative subtraction cases and confirm the method uses the\ndeclared operand order.",
        "Execute subtraction overflow and underflow cases and confirm the calls are\nrejected.",
        "Execute representative multiplication cases and verify the returned\nproducts.",
        "Swap the multiplication operands and confirm the result does not change.",
        "Execute multiplication overflow and underflow cases and confirm the calls\nare rejected.",
        "Call `Divide` with `divisor = 0` and confirm the call is rejected.",
        "Execute evenly divisible division cases and confirm the exact quotient is\nreturned.",
        "Execute fractional division cases with positive and negative dividends and\nconfirm truncation toward `0`.",
        "Execute division cases with negative divisors and confirm the quotient\nsign follows the operand signs.",
        "Call `Modulus` with `divisor = 0` and confirm the call is rejected.",
        "Execute representative remainder cases with positive and negative\ndividends and confirm the signed remainder rule.",
        "Execute boundary-value cases that remain representable and confirm the\noperations succeed.",
        "Execute generic unrepresentable-result cases such as\n`-2147483648 / -1` and confirm the call is rejected instead of wrapping or\nsaturating.",
    ]
    expected_result: """
      The example exposes one integer calculator class with five methods, applies the
      named arithmetic rules for each method, accepts valid boundary operands,
      rejects divide-by-zero, handles negative divisors with the expected quotient
      sign, and rejects both overflow and underflow cases.
      """
    evidence: [
        "[`unit-tests.evidence.json`](./generated/unit-tests.evidence.json)",
        "[`implementation-map.evidence.json`](./generated/implementation-map.evidence.json)",
    ]
    status_summary: """
      This `passed` status applies to every requirement listed in `verifies`.
      
      passed
      """
}
