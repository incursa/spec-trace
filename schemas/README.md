# Schemas

These are reference schemas aligned to the standard in [overview.md](../overview.md). They are intentionally strict enough to help tooling without turning into a heavyweight validation framework.

Useful entry points:

- [artifact-frontmatter.schema.json](artifact-frontmatter.schema.json) validates file-level front matter for specification, architecture, work-item, and verification artifacts
- [requirement-trace-fields.schema.json](requirement-trace-fields.schema.json) validates the canonical trace labels used inside requirement sections
- [work-item-trace-fields.schema.json](work-item-trace-fields.schema.json) validates the canonical trace labels used in the work-item `Trace Links` section

Use the schemas together with the templates and the canonical field names in [overview.md](../overview.md). They describe extracted metadata shape, not raw Markdown parsing behavior.
