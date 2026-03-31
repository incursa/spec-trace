package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id: "SPEC-LAY"
    artifact_type: "specification"
    title: "Repository Layout and Artifact Placement"
    domain: "spec-trace"
    capability: "repository-layout"
    status: "draft"
    owner: "spec-trace-maintainers"
    tags: [
        "spec-trace",
        "layout",
        "placement",
    ]
    purpose: "Define how repositories should place canonical CUE source artifacts and derived outputs so the structure reinforces the traceability model."
    scope: """
      This specification covers the recommended `/specs` tree, placement of artifact families, canonical CUE sources, generated Markdown outputs, and the distinction between artifact role and file naming.
      """
    context: """
      Traceability weakens when requirements, design, work, and verification are scattered according to workflow noise instead of stable product structure.
      """
    requirements: [
        {
            id: "REQ-LAY-0001"
            title: "Keep live source artifacts under the /specs tree"
            statement: """
              A repository using the standard MUST keep live canonical source artifacts as CUE under [`/specs`](../../../specs) with dedicated areas for requirements, architecture, work-items, verification, generated outputs, templates, and schemas.
              """
            notes: [
                "Decision records are not part of the core layout today; repositories that need them may add an optional local extension.",
            ]
        },
        {
            id: "REQ-LAY-0002"
            title: "Organize specifications by stable domain and concern"
            statement: """
              Specifications MUST be organized by stable domain first and by capability, behavior area, interface, or technical concern second.
              """
        },
        {
            id: "REQ-LAY-0003"
            title: "Place artifact families according to their role"
            statement: """
              Architecture, work-item, and verification artifacts MUST live in their respective family areas with traceability back to requirement identifiers.
              """
        },
        {
            id: "REQ-LAY-0004"
            title: "Keep generated outputs separate from source artifacts"
            statement: """
              Generated indexes, matrices, coverage reports, and evidence snapshots MUST live
              under [`/specs/generated`](../../../specs/generated) when they are stored in
              the repository, while generated Markdown renderings of canonical artifacts may
              live beside their source `.cue` files for readability.
              """
        },
        {
            id: "REQ-LAY-0005"
            title: "Include the full specification ID in specification file names"
            statement: "Each canonical specification `.cue` file name MUST include the full specification artifact ID."
        },
        {
            id: "REQ-LAY-0006"
            title: "Keep one specification per canonical artifact file"
            statement: """
              Each specification `.cue` file MUST contain exactly one specification artifact and one or more related requirement records beneath it.
              """
        },
        {
            id: "REQ-LAY-0007"
            title: "Treat index files as navigation only"
            statement: "A domain or concern MAY include an [`_index.md`](./_index.md) file for navigation."
        },
        {
            id: "REQ-LAY-0008"
            title: "Keep the reference package packaging-friendly"
            statement: "The public reference package MUST keep its canonical suite under [`specs/requirements/spec-trace/`](./)."
        },
        {
            id: "REQ-LAY-0009"
            title: "Keep file names stable and readable"
            statement: "Specification file names SHOULD remain stable and readable without using dates, sprints, or owner names."
        },
        {
            id: "REQ-LAY-0010"
            title: "Keep indexes non-authoritative"
            statement: "An [`_index.md`](./_index.md) file MUST NOT replace the underlying artifacts."
        },
        {
            id: "REQ-LAY-0011"
            title: "Allow copy-friendly root guidance in the reference package"
            statement: "The public reference package MAY also publish root guidance and templates for copy convenience."
        },
    ]
}
