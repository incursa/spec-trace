package templates

import model "github.com/incursa/spec-trace/model@v0"

// #WorkItemTemplate is the schema-backed authoring template for a work-item
// artifact's top-level `artifact` value.
#WorkItemTemplate: model.#WorkItem & {
    artifact_type: "work_item"
    status:        *"planned" | model.#WorkItemStatus
}

// #WorkItemFile validates the full file shape used by canonical authored work
// item documents.
#WorkItemFile: {
    artifact: #WorkItemTemplate
}
