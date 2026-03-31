package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#WorkItem & {
    artifact_id: "WI-CALC-INT-0001"
    artifact_type: "work_item"
    title: "Implement integer calculator operations and evidence-linked tests"
    domain: "calculator-int"
    status: "complete"
    owner: "platform-core"
    related_artifacts: [
        "SPEC-CALC-INT",
        "ARC-CALC-INT-0001",
        "VER-CALC-INT-0001",
    ]
    summary: """
      Implement the `IntegerCalculator` class, apply checked integer arithmetic, and
      cover the full operation contract with direct requirement-linked tests.
      """
    addresses: [
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
    design_links: [
        "ARC-CALC-INT-0001",
    ]
    verification_links: [
        "VER-CALC-INT-0001",
    ]
    planned_changes: """
      Create one `IntegerCalculator` class with five two-operand methods. Implement
      checked addition, subtraction, and multiplication. Implement division and
      modulus with explicit zero-divisor guards, truncation-toward-zero semantics,
      negative-divisor sign handling, boundary tests, and explicit overflow and
      underflow coverage for unrepresentable results.
      """
    out_of_scope: [
        "floating-point calculator behavior",
        "arbitrary-precision arithmetic",
        "localization of error text or exception payloads",
        "expression parsing, operator precedence, or user-interface concerns",
    ]
    verification_plan: """
      Use [VER-CALC-INT-0001](./sample-verification.md),
      [unit-tests.evidence.json](./generated/unit-tests.evidence.json), and
      [implementation-map.evidence.json](./generated/implementation-map.evidence.json)
      to prove every operation rule, boundary rule, signed-division rule, and
      overflow or underflow rule.
      """
    completion_notes: """
      The implementation keeps one stable class-level code reference at
      `calculator.int32.IntegerCalculator` and method-level references under the same
      namespace.
      """
}
