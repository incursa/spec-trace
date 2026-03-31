package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Verification & {
    artifact_id: "VER-BEM-CSS-0001"
    artifact_type: "verification"
    title: "BEM Concepts, Naming, and CSS Review"
    domain: "bem-css"
    status: "passed"
    owner: "frontend-platform"
    related_artifacts: [
        "SPEC-BEM-CONCEPTS",
        "SPEC-BEM-NAMING",
        "SPEC-BEM-CSS",
        "ARC-BEM-CSS-0001",
    ]
    scope: "Verify the BEM example's shared terms, naming grammar, selector rules, and composition rules."
    verifies: [
        "REQ-BEM-CONCEPTS-0001",
        "REQ-BEM-CONCEPTS-0002",
        "REQ-BEM-CONCEPTS-0003",
        "REQ-BEM-CONCEPTS-0004",
        "REQ-BEM-CONCEPTS-0005",
        "REQ-BEM-CONCEPTS-0006",
        "REQ-BEM-CONCEPTS-0007",
        "REQ-BEM-NAMING-0001",
        "REQ-BEM-NAMING-0002",
        "REQ-BEM-NAMING-0003",
        "REQ-BEM-NAMING-0004",
        "REQ-BEM-NAMING-0005",
        "REQ-BEM-NAMING-0006",
        "REQ-BEM-NAMING-0007",
        "REQ-BEM-NAMING-0008",
        "REQ-BEM-NAMING-0009",
        "REQ-BEM-CSS-0001",
        "REQ-BEM-CSS-0002",
        "REQ-BEM-CSS-0003",
        "REQ-BEM-CSS-0004",
        "REQ-BEM-CSS-0005",
        "REQ-BEM-CSS-0006",
        "REQ-BEM-CSS-0007",
        "REQ-BEM-CSS-0008",
        "REQ-BEM-CSS-0009",
        "REQ-BEM-CSS-0010",
    ]
    verification_method: """
      Document inspection against the official BEM documentation, the three BEM specification files, and the reciprocal trace links in the example set.
      """
    preconditions: [
        "The three BEM specifications exist in the example folder.",
        "The official BEM documentation is available for comparison.",
        "The architecture artifact exists and lists the same downstream scope.",
    ]
    procedure: [
        "Check that each requirement is atomic and carries exactly one approved uppercase normative keyword.",
        "Confirm the concepts spec introduces the shared vocabulary before naming or CSS rules depend on it.",
        "Confirm the naming spec constrains block, element, and modifier class forms and excludes separator collisions.",
        "Confirm the CSS spec limits selectors, nested selectors, and wrapper-heavy layout behavior.",
        "Confirm each requirement carries `Satisfied By` and `Verified By` links to the shared architecture and verification artifacts.",
        "Confirm the front matter `satisfies` and `verifies` lists match the requirement scope.",
    ]
    expected_result: """
      The example reads as a coherent, reviewable BEM specification set with complete downstream trace and no ambiguous shared terms.
      """
    evidence: [
        "[`examples/bem-css/SPEC-BEM-CONCEPTS.md`](./SPEC-BEM-CONCEPTS.md)",
        "[`examples/bem-css/SPEC-BEM-NAMING.md`](./SPEC-BEM-NAMING.md)",
        "[`examples/bem-css/SPEC-BEM-CSS.md`](./SPEC-BEM-CSS.md)",
        "[`examples/bem-css/sample-architecture.md`](./sample-architecture.md)",
        "official BEM documentation:",
        "https://en.bem.info/methodology/quick-start/",
        "https://en.bem.info/methodology/naming-convention/",
        "https://en.bem.info/methodology/css/",
        "https://en.bem.info/methodology/html/",
        "https://en.bem.info/methodology/faq/?lang=en",
    ]
    status_summary: """
      This `passed` status applies to every requirement listed in `verifies`.
      
      passed
      """
}
