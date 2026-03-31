package templates

import model "github.com/incursa/spec-trace/model@v0"

// #ArchitectureTemplate is the schema-backed authoring template for an
// architecture artifact's top-level `artifact` value.
#ArchitectureTemplate: model.#Architecture & {
    artifact_type: "architecture"
    status:        *"draft" | model.#ArtifactLifecycleStatus
}

// #ArchitectureFile validates the full file shape used by canonical authored
// architecture documents.
#ArchitectureFile: {
    artifact: #ArchitectureTemplate
}
