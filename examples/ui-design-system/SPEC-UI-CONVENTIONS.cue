package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-UI-CONVENTIONS"
    artifact_type: "specification"
    title: "UI Naming Conventions"
    domain: "ui-design-system"
    capability: "ui-naming-conventions"
    status: "approved"
    owner: "design-system"
    tags: [
        "ui",
        "design-system",
        "bem",
        "conventions",
    ]
    related_artifacts: [
        "SPEC-UI-BUTTON",
        "SPEC-UI-INPUT",
    ]
    purpose: "Define the shared BEM-style naming rules used by the UI design system."
    scope: """
      This specification covers the block, element, and modifier forms used by the button and input components in this example. It does not define token values or component behavior.
      """
    context: """
      The component specs reuse the same naming contract so the example can show shared requirements without repeating the class-name rules in each component file.
      """
    requirements: [
        {
            id: "REQ-UI-CONVENTIONS-0001"
            title: "Use a UI block namespace"
            statement: "Shared UI block classes MUST use a `ui-` namespace prefix."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0001",
                    "REQ-UI-INPUT-0001",
                ]
            }
        },
        {
            id: "REQ-UI-CONVENTIONS-0002"
            title: "Use BEM element classes"
            statement: "UI element classes MUST use the `block__element` form."
            trace: {
                related: [
                    "REQ-UI-INPUT-0002",
                ]
            }
        },
        {
            id: "REQ-UI-CONVENTIONS-0003"
            title: "Use BEM modifier classes"
            statement: "UI modifier classes MUST use the `block--modifier` or `block__element--modifier` form."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0002",
                    "REQ-UI-BUTTON-0003",
                    "REQ-UI-BUTTON-0004",
                    "REQ-UI-BUTTON-0005",
                    "REQ-UI-BUTTON-0006",
                    "REQ-UI-INPUT-0003",
                    "REQ-UI-INPUT-0004",
                    "REQ-UI-INPUT-0005",
                    "REQ-UI-INPUT-0006",
                ]
            }
            notes: [
                "Examples include `ui-button`, `ui-button--primary`, `ui-button--disabled`, `ui-input__field`, and `ui-input--invalid`.",
            ]
        },
    ]
}
