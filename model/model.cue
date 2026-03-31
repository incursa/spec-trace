package model

#Domain:         string & =~#"^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$"#
#NonEmptyString: string & != ""
#Paragraph:      #NonEmptyString
#StringList:     [#NonEmptyString, ...#NonEmptyString]

#ArtifactLifecycleStatus:
    "draft" |
    "proposed" |
    "approved" |
    "implemented" |
    "verified" |
    "superseded" |
    "retired"

#WorkItemStatus:
    "planned" |
    "in_progress" |
    "blocked" |
    "complete" |
    "cancelled" |
    "superseded"

#VerificationStatus:
    "planned" |
    "passed" |
    "failed" |
    "blocked" |
    "waived" |
    "obsolete"

#EvidenceStatus:
    "observed" |
    "passed" |
    "failed" |
    "not_observed" |
    "not_collected" |
    "unsupported"

#SpecificationId: string & =~#"^SPEC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*$"#
#RequirementId:   string & =~#"^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$"#
#ArchitectureId:  string & =~#"^ARC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$"#
#WorkItemId:      string & =~#"^WI-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$"#
#VerificationId:  string & =~#"^VER-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$"#

#ArtifactId:   #SpecificationId | #ArchitectureId | #WorkItemId | #VerificationId
#CanonicalId:  #ArtifactId | #RequirementId
#ArtifactType: "specification" | "architecture" | "work_item" | "verification"
#EntryType:    #ArtifactType | "requirement"

#SpecificationRefList: [#SpecificationId, ...#SpecificationId]
#RequirementRefList:   [#RequirementId, ...#RequirementId]
#ArchitectureRefList:  [#ArchitectureId, ...#ArchitectureId]
#WorkItemRefList:      [#WorkItemId, ...#WorkItemId]
#VerificationRefList:  [#VerificationId, ...#VerificationId]
#ArtifactRefList:      [#ArtifactId, ...#ArtifactId]
#CanonicalRefList:     [#CanonicalId, ...#CanonicalId]

#RequirementStatement: #Paragraph & =~#"(?s).*\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b.*"#

#SupplementalSection: close({
    heading: #NonEmptyString
    content: #Paragraph
})

#RequirementTrace: close({
    satisfied_by?:   #ArchitectureRefList
    implemented_by?: #WorkItemRefList
    verified_by?:    #VerificationRefList
    derived_from?:   #RequirementRefList
    supersedes?:     #RequirementRefList
    upstream_refs?:  #StringList
    related?:        #CanonicalRefList
})

#Requirement: close({
    id:        #RequirementId
    title:     #NonEmptyString
    statement: #RequirementStatement
    trace?:    #RequirementTrace
    notes?:    #StringList
})

#RetiredRequirement: close({
    id:          #RequirementId
    title?:      #NonEmptyString
    replaced_by?: #RequirementRefList
    notes?:      #StringList
})

#RetiredRequirementLedger: close({
    retired_requirements: [#RetiredRequirement, ...#RetiredRequirement]
})

#Specification: close({
    artifact_id:       #SpecificationId
    artifact_type:     "specification"
    title:             #NonEmptyString
    domain:            #Domain
    capability:        #NonEmptyString
    status:            #ArtifactLifecycleStatus
    owner:             #NonEmptyString
    tags?:             #StringList
    related_artifacts?: #ArtifactRefList
    purpose:           #Paragraph
    scope?:            #Paragraph
    context?:          #Paragraph
    open_questions?:   #StringList
    supplemental_sections?: [#SupplementalSection, ...#SupplementalSection]
    requirements:      [#Requirement, ...#Requirement]
})

#Architecture: close({
    artifact_id:       #ArchitectureId
    artifact_type:     "architecture"
    title:             #NonEmptyString
    domain:            #Domain
    status:            #ArtifactLifecycleStatus
    owner:             #NonEmptyString
    related_artifacts?: #ArtifactRefList
    satisfies:         #RequirementRefList
    purpose:           #Paragraph
    design_summary:    #Paragraph
    key_components?:   #StringList
    data_and_state_considerations?: #Paragraph
    edge_cases_and_constraints?:     #StringList
    alternatives_considered?:        #StringList
    risks?:            #StringList
    open_questions?:   #StringList
    supplemental_sections?: [#SupplementalSection, ...#SupplementalSection]
})

#WorkItem: close({
    artifact_id:       #WorkItemId
    artifact_type:     "work_item"
    title:             #NonEmptyString
    domain:            #Domain
    status:            #WorkItemStatus
    owner:             #NonEmptyString
    related_artifacts?: #ArtifactRefList
    addresses:         #RequirementRefList
    design_links:      #ArchitectureRefList
    verification_links: #VerificationRefList
    summary:           #Paragraph
    planned_changes:   #Paragraph
    out_of_scope?:     #StringList
    verification_plan: #Paragraph
    completion_notes?: #Paragraph
    supplemental_sections?: [#SupplementalSection, ...#SupplementalSection]
})

#Verification: close({
    artifact_id:       #VerificationId
    artifact_type:     "verification"
    title:             #NonEmptyString
    domain:            #Domain
    status:            #VerificationStatus
    owner:             #NonEmptyString
    related_artifacts?: #ArtifactRefList
    verifies:          #RequirementRefList
    scope:             #Paragraph
    verification_method: #Paragraph
    preconditions?:    #StringList
    procedure:         #StringList
    expected_result:   #Paragraph
    evidence?:         #StringList
    status_summary?:   #Paragraph
    supplemental_sections?: [#SupplementalSection, ...#SupplementalSection]
})

#Artifact: #Specification | #Architecture | #WorkItem | #Verification

#EvidenceObservation: close({
    kind:     string & =~#"^[a-z][a-z0-9_]*$"#
    status:   #EvidenceStatus
    refs?:    #StringList
    summary?: #Paragraph
})

#RequirementEvidence: close({
    requirement_id: #RequirementId
    observations:   [#EvidenceObservation, ...#EvidenceObservation]
})

#EvidenceProducer: close({
    name:    #NonEmptyString
    version: #NonEmptyString
})

#EvidenceSnapshot: close({
    snapshot_id:  #NonEmptyString
    generated_at: #NonEmptyString
    producer:     #EvidenceProducer
    requirements: [#RequirementEvidence, ...#RequirementEvidence]
})

#CatalogReference: close({
    source_id:      #CanonicalId
    field:          #NonEmptyString
    target_id:      #CanonicalId
    expected_kind:  "artifact" | "requirement"
    expected_prefix?: "SPEC" | "ARC" | "WI" | "VER" | "REQ"
})

#CatalogEntry: close({
    id:            #CanonicalId
    kind:          "artifact" | "requirement"
    artifact_type: #EntryType
    parent_id?:    #SpecificationId
    source_path:   #NonEmptyString
    markdown_path: #NonEmptyString
    title:         #NonEmptyString
    domain:        #Domain
})

#Catalog: close({
    entries: {
        [string]: #CatalogEntry
    }
    references?: [...#CatalogReference]
})
