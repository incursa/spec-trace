package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-BEM-CONCEPTS"
    artifact_type: "specification"
    title: "BEM Concepts and Semantics"
    domain: "bem-css"
    capability: "bem-concepts"
    status: "approved"
    owner: "frontend-platform"
    tags: [
        "bem",
        "css",
        "methodology",
        "concepts",
        "semantics",
    ]
    related_artifacts: [
        "ARC-BEM-CSS-0001",
        "VER-BEM-CSS-0001",
        "SPEC-BEM-NAMING",
        "SPEC-BEM-CSS",
    ]
    purpose: "Define the core BEM terms that the naming, CSS, architecture, and verification rules rely on."
    scope: """
      This specification covers blocks, elements, modifiers, mixes, and the reuse boundaries that make BEM independent. It does not define class-name syntax or selector syntax.
      """
    context: """
      The naming rules only make sense if the underlying BEM terms are defined first. This file gives those terms concrete meaning before the contract moves on to syntax, CSS behavior, and downstream trace.
      
      In this example, shared meaning is captured as requirements so it can be referenced and verified by ID. That keeps the example traceable without adding a separate glossary artifact.
      """
    requirements: [
        {
            id: "REQ-BEM-CONCEPTS-0001"
            title: "Define a block as an independent reusable unit"
            statement: "A block MUST be a functionally independent reusable unit."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                    "https://en.bem.info/methodology/html/",
                ]
                related: [
                    "REQ-BEM-CONCEPTS-0002",
                    "REQ-BEM-NAMING-0002",
                    "REQ-BEM-CSS-0007",
                ]
            }
            notes: [
                "A block is the namespace root for its `block`, `block__element`, and `block_modifier` class families.",
            ]
        },
        {
            id: "REQ-BEM-CONCEPTS-0002"
            title: "Define an element as part of a block"
            statement: "An element MUST be a composite part of a block that cannot be used separately from it."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                    "https://en.bem.info/methodology/html/",
                ]
                related: [
                    "REQ-BEM-CONCEPTS-0001",
                    "REQ-BEM-NAMING-0003",
                    "REQ-BEM-NAMING-0004",
                    "REQ-BEM-CSS-0010",
                ]
            }
            notes: [
                "An element name stays local to its block and does not become a standalone namespace.",
            ]
        },
        {
            id: "REQ-BEM-CONCEPTS-0003"
            title: "Define a modifier as a variation of a block or element"
            statement: "A modifier MUST define the appearance, state, or behavior of a block or element."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                    "https://en.bem.info/methodology/block-modification/",
                ]
                related: [
                    "REQ-BEM-NAMING-0005",
                    "REQ-BEM-NAMING-0006",
                    "REQ-BEM-NAMING-0007",
                    "REQ-BEM-NAMING-0008",
                    "REQ-BEM-CSS-0005",
                    "REQ-BEM-CSS-0010",
                ]
            }
            notes: [
                "A modifier adds variation to the base entity instead of replacing the base entity.",
            ]
        },
        {
            id: "REQ-BEM-CONCEPTS-0004"
            title: "Keep block names semantic"
            statement: "A block name MUST describe purpose rather than appearance or state."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                    "https://en.bem.info/methodology/css/",
                ]
                related: [
                    "REQ-BEM-NAMING-0001",
                    "REQ-BEM-NAMING-0008",
                ]
            }
            notes: [
                "The block name is the semantic label that anchors the block namespace.",
            ]
        },
        {
            id: "REQ-BEM-CONCEPTS-0005"
            title: "Keep element names semantic"
            statement: "An element name MUST describe purpose rather than appearance or state."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                ]
                related: [
                    "REQ-BEM-NAMING-0003",
                    "REQ-BEM-NAMING-0004",
                ]
            }
            notes: [
                "Element names stay descriptive of the element's role inside the block.",
            ]
        },
        {
            id: "REQ-BEM-CONCEPTS-0006"
            title: "Allow mixes to combine BEM entities"
            statement: "A mix MAY combine multiple BEM entities on the same DOM node."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                    "https://en.bem.info/methodology/css/",
                    "https://en.bem.info/methodology/html/",
                ]
                related: [
                    "REQ-BEM-CSS-0007",
                    "REQ-BEM-CSS-0009",
                ]
            }
            notes: [
                "A mix combines existing entities; it does not create a new semantic entity.",
            ]
        },
        {
            id: "REQ-BEM-CONCEPTS-0007"
            title: "Keep blocks independent from external geometry"
            statement: "A block SHOULD NOT set its own external geometry or positioning."
            trace: {
                satisfied_by: [
                    "ARC-BEM-CSS-0001",
                ]
                verified_by: [
                    "VER-BEM-CSS-0001",
                ]
                upstream_refs: [
                    "https://en.bem.info/methodology/quick-start/",
                    "https://en.bem.info/methodology/css/",
                    "https://en.bem.info/methodology/html/",
                ]
                related: [
                    "REQ-BEM-CSS-0008",
                    "REQ-BEM-CSS-0009",
                ]
            }
            notes: [
                "External layout belongs to the parent context or to a mix, not to the block itself.",
            ]
        },
    ]
}
