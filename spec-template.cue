package templates

import model "github.com/incursa/spec-trace/model@v0"

// #SpecificationTemplate is the schema-backed authoring template for a
// specification artifact's top-level `artifact` value.
#SpecificationTemplate: model.#Specification & {
    artifact_type: "specification"
    status:        *"draft" | model.#ArtifactLifecycleStatus
}

// #SpecificationFile validates the full file shape used by canonical authored
// specification documents.
#SpecificationFile: {
    artifact: #SpecificationTemplate
}
