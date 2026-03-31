package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-PAY-ACH"
    artifact_type: "specification"
    title: "ACH Duplicate Batch Handling"
    domain: "payments"
    capability: "ach-duplicate-batch-handling"
    status: "approved"
    owner: "payments-platform"
    tags: [
        "payments",
        "ach",
        "duplicate-detection",
    ]
    related_artifacts: [
        "ARC-PAY-ACH-0002",
        "WI-PAY-ACH-0081",
        "VER-PAY-ACH-0021",
    ]
    purpose: "Define the duplicate-handling rules for ACH batch intake."
    scope: """
      This specification covers tenant-scoped duplicate detection and the point in the flow where the duplicate check must occur. It does not define settlement behavior or unrelated payment rails.
      """
    context: """
      The same external batch identifier can appear in more than one tenant. The system needs a rule that blocks duplicates within a tenant without creating cross-tenant false positives or downstream side effects.
      
      This specification builds on [`SPEC-STD`](../../specs/requirements/spec-trace/SPEC-STD.md) and [`SPEC-TPL`](../../specs/requirements/spec-trace/SPEC-TPL.md).
      """
    requirements: [
        {
            id: "REQ-PAY-ACH-0013"
            title: "Scope duplicate detection to the tenant and batch identifier"
            statement: "The batch intake flow MUST treat duplicate detection as tenant-scoped and keyed by the external batch identifier."
            trace: {
                satisfied_by: [
                    "ARC-PAY-ACH-0002",
                ]
                implemented_by: [
                    "WI-PAY-ACH-0081",
                ]
                verified_by: [
                    "VER-PAY-ACH-0021",
                ]
                supersedes: [
                    "REQ-PAY-ACH-0012",
                ]
                upstream_refs: [
                    "ACH Operating Rules, duplicate batch handling",
                    "Payments platform intake policy",
                ]
            }
        },
        {
            id: "REQ-PAY-ACH-0014"
            title: "Reject duplicate ACH batch submission"
            statement: """
              The batch intake flow MUST reject a submitted ACH batch when the same external batch identifier has already been accepted for the same tenant.
              """
            trace: {
                satisfied_by: [
                    "ARC-PAY-ACH-0002",
                ]
                implemented_by: [
                    "WI-PAY-ACH-0081",
                ]
                verified_by: [
                    "VER-PAY-ACH-0021",
                ]
                derived_from: [
                    "REQ-PAY-ACH-0013",
                ]
            }
            notes: [
                "This rule depends on [`REQ-PAY-ACH-0013`](./SPEC-PAY-ACH.md).",
                "Duplicate means same tenant plus external batch identifier.",
            ]
        },
        {
            id: "REQ-PAY-ACH-0015"
            title: "Allow the same external batch identifier across tenants"
            statement: "The batch intake flow MAY accept the same external batch identifier for a different tenant."
            trace: {
                satisfied_by: [
                    "ARC-PAY-ACH-0002",
                ]
                implemented_by: [
                    "WI-PAY-ACH-0081",
                ]
                verified_by: [
                    "VER-PAY-ACH-0021",
                ]
                derived_from: [
                    "REQ-PAY-ACH-0013",
                ]
            }
        },
        {
            id: "REQ-PAY-ACH-0016"
            title: "Check duplicates before downstream side effects"
            statement: "The duplicate check MUST complete before downstream processing begins for the submitted batch."
            trace: {
                satisfied_by: [
                    "ARC-PAY-ACH-0002",
                ]
                implemented_by: [
                    "WI-PAY-ACH-0081",
                ]
                verified_by: [
                    "VER-PAY-ACH-0021",
                ]
                derived_from: [
                    "REQ-PAY-ACH-0013",
                ]
            }
        },
    ]
}
