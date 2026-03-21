# Schemas

These are reference schemas aligned to the SPEC suite under `../specs/requirements/spec-trace/`.

The schemas validate extracted metadata shapes, not raw Markdown parsing behavior.

The shared [artifact-id-policy.json](../artifact-id-policy.json) file defines the identifier contract for both artifact documents and requirement clauses.

## Included Schemas

- [artifact-id-policy.schema.json](artifact-id-policy.schema.json) validates the shared identifier and grouping-key metadata catalog
- [artifact-frontmatter.schema.json](artifact-frontmatter.schema.json) validates file-level front matter for specification, architecture, work-item, and verification documents
- [requirement-clause.schema.json](requirement-clause.schema.json) validates extracted compact requirement clauses, including the normative keyword and optional trace block
- [requirement-trace-fields.schema.json](requirement-trace-fields.schema.json) validates the canonical trace labels used inside a requirement `Trace` block
- [work-item-trace-fields.schema.json](work-item-trace-fields.schema.json) validates the canonical labels used in a work-item `Trace Links` section

## Mapping Notes

- `artifact-frontmatter.schema.json` is for document metadata.
- `requirement-clause.schema.json` is for the requirement itself.
- `requirement-trace-fields.schema.json` is referenced by the requirement-clause schema.
- `Test Refs` and `Code Refs` remain implementation-specific string references. The schema constrains the field names and value shape, not the local reference syntax.
