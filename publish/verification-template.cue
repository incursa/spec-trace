package templates

import model "github.com/incursa/spec-trace/model@v0"

// #VerificationTemplate is the schema-backed authoring template for a
// verification artifact's top-level `artifact` value.
#VerificationTemplate: model.#Verification & {
    artifact_type: "verification"
    status:        *"planned" | model.#VerificationStatus
}

// #VerificationFile validates the full file shape used by canonical authored
// verification documents.
#VerificationFile: {
    artifact: #VerificationTemplate
}
