# Schemas

These are reference schemas aligned to the SPEC suite under `../specs/requirements/spec-trace/`.

The schemas validate extracted metadata shapes and the identifier families used by direct trace-bearing lists. They do not prescribe one Markdown parser.

The shared [artifact-id-policy.json](../artifact-id-policy.json) file defines the identifier contract for both artifact documents and requirement clauses.

## Included Schemas

- [artifact-id-policy.schema.json](artifact-id-policy.schema.json) validates the shared identifier and grouping-key metadata catalog
- [artifact-frontmatter.schema.json](artifact-frontmatter.schema.json) validates file-level front matter for specification, architecture, work-item, and verification documents, including family-specific trace-link lists and optional namespaced `x_` extension keys
- [requirement-clause.schema.json](requirement-clause.schema.json) validates extracted compact requirement clauses, including the normative keyword phrase and optional trace block
- [requirement-trace-fields.schema.json](requirement-trace-fields.schema.json) validates the canonical trace labels used inside a requirement `Trace` block, including family-typed direct links and optional lineage or free-form source fields
- [work-item-trace-fields.schema.json](work-item-trace-fields.schema.json) validates the canonical labels used in a work-item `Trace Links` section and constrains requirement, design, and verification links to their expected identifier families

## Mapping Notes

- `artifact-frontmatter.schema.json` is for document metadata.
- `requirement-clause.schema.json` is for the requirement itself.
- `requirement-trace-fields.schema.json` is referenced by the requirement-clause schema.
- Verification front matter uses one artifact-scoped status for every requirement ID listed in `verifies`; mixed outcomes belong in separate artifacts.
- `Test Refs` and `Code Refs` remain implementation-specific string references. The schema constrains the field names and value shape, not the local reference syntax.
- `requirement-clause.schema.json` recognizes the narrowed BCP 14-style uppercase keyword set: `MUST`, `MUST NOT`, `SHALL`, `SHALL NOT`, `SHOULD`, `SHOULD NOT`, and `MAY`. Lowercase spellings are plain English.
- `Derived From` and `Supersedes` are lineage references; the repository-level validator accepts them without requiring tombstone requirement records.
- File-level front matter may use optional namespaced `x_` keys for local extensions, but the core artifact family fields remain fixed.
- The `core` profile maps to schema, identifier, and keyword checks; `traceable` and `auditable` add repository-level trace checks on top of those basics.
- Repository-level validation is expected to go beyond shape checks and report duplicate IDs, unresolved direct links, reciprocal mismatches, and namespace drift.
