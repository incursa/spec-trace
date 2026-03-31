package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Verification & {
    artifact_id: "VER-PAY-ACH-0021"
    artifact_type: "verification"
    title: "Duplicate ACH batch rejection verification"
    domain: "payments"
    status: "passed"
    owner: "payments-platform"
    related_artifacts: [
        "SPEC-PAY-ACH",
        "ARC-PAY-ACH-0002",
        "WI-PAY-ACH-0081",
    ]
    scope: "Verify tenant-scoped duplicate handling for ACH batch intake."
    verifies: [
        "REQ-PAY-ACH-0013",
        "REQ-PAY-ACH-0014",
        "REQ-PAY-ACH-0015",
        "REQ-PAY-ACH-0016",
    ]
    verification_method: """
      Functional verification using an initial acceptance, a same-tenant repeat submission, and a cross-tenant submission that reuses the same external batch identifier.
      """
    preconditions: [
        "tenant `PAY-TENANT-01` exists",
        "tenant `PAY-TENANT-02` exists",
        "external batch identifier `ACH-2026-0319-01` is available for test use",
    ]
    procedure: [
        "Submit a batch for `PAY-TENANT-01` with external batch identifier `ACH-2026-0319-01`.",
        "Confirm the first submission is accepted.",
        "Submit the same batch again for `PAY-TENANT-01`.",
        "Confirm the second submission is rejected as a duplicate and does not start downstream processing.",
        "Submit the same external batch identifier for `PAY-TENANT-02`.",
        "Confirm the cross-tenant submission is accepted.",
    ]
    expected_result: """
      The first submission is accepted, the same-tenant repeat is rejected before downstream side effects, and the different-tenant submission is accepted.
      """
    evidence: [
        "[`duplicate-batch-evidence.evidence.json`](./generated/duplicate-batch-evidence.evidence.json)",
    ]
    status_summary: """
      This `passed` status applies to every requirement listed in `verifies`.
      
      passed
      """
}
