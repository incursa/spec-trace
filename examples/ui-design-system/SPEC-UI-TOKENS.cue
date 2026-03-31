package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-UI-TOKENS"
    artifact_type: "specification"
    title: "UI Token Set"
    domain: "ui-design-system"
    capability: "ui-token-set"
    status: "approved"
    owner: "design-system"
    tags: [
        "ui",
        "design-system",
        "tokens",
        "color",
        "spacing",
    ]
    related_artifacts: [
        "SPEC-UI-BUTTON",
        "SPEC-UI-INPUT",
    ]
    purpose: "Define the canonical token identifiers used by the UI design system example."
    scope: """
      This specification covers a small set of shared tokens that the button and input components consume. It does not prescribe token storage, export format, or raw CSS values.
      """
    context: "The token names stay abstract so the example can focus on traceable contracts rather than implementation details."
    requirements: [
        {
            id: "REQ-UI-TOKENS-0001"
            title: "Define the primary action token"
            statement: "The design system MUST define `color-action-primary` as the canonical token for primary action surfaces."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0002",
                    "REQ-UI-FOUNDATION-0001",
                ]
            }
        },
        {
            id: "REQ-UI-TOKENS-0002"
            title: "Define the secondary action token"
            statement: "The design system MUST define `color-action-secondary` as the canonical token for secondary action surfaces."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0003",
                ]
            }
        },
        {
            id: "REQ-UI-TOKENS-0003"
            title: "Define the standard spacing token"
            statement: "The design system MUST define `space-control-x-medium` as the canonical horizontal spacing token for controls."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0001",
                    "REQ-UI-INPUT-0001",
                ]
            }
        },
        {
            id: "REQ-UI-TOKENS-0004"
            title: "Define the compact control-height token"
            statement: "The design system MUST define `control-height-small` as the canonical compact control height token."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0004",
                    "REQ-UI-INPUT-0003",
                ]
            }
        },
        {
            id: "REQ-UI-TOKENS-0005"
            title: "Define the standard control-height token"
            statement: "The design system MUST define `control-height-medium` as the canonical standard control height token."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0004",
                    "REQ-UI-INPUT-0003",
                ]
            }
        },
        {
            id: "REQ-UI-TOKENS-0006"
            title: "Define the spacious control-height token"
            statement: "The design system MUST define `control-height-large` as the canonical spacious control height token."
            trace: {
                related: [
                    "REQ-UI-BUTTON-0004",
                    "REQ-UI-INPUT-0003",
                ]
            }
        },
    ]
}
