# Schemas

These files are compatibility schemas aligned to the canonical CUE model under [`../model/`](../model/).

Authoritative constraints live in CUE. The JSON Schema files in this folder are export and integration surfaces for tools that need JSON-facing contracts.

## Canonical Source

- [`../model/model.cue`](../model/model.cue) is the authoritative shared schema package.
- [`../specs/requirements/spec-trace/SPEC-SCH.md`](../specs/requirements/spec-trace/SPEC-SCH.md) defines the normative validation rules.

## Included Compatibility Schemas

- [`artifact-frontmatter.schema.json`](./artifact-frontmatter.schema.json)
- [`artifact-id-policy.schema.json`](./artifact-id-policy.schema.json)
- [`requirement-clause.schema.json`](./requirement-clause.schema.json)
- [`requirement-trace-fields.schema.json`](./requirement-trace-fields.schema.json)
- [`work-item-trace-fields.schema.json`](./work-item-trace-fields.schema.json)
- [`evidence-snapshot.schema.json`](./evidence-snapshot.schema.json)

## Notes

- These files do not override the CUE model.
- Repository-wide reference resolution, duplicate detection, and wrong-target-kind checks happen in the repository validator, not in JSON Schema alone.
- Markdown is generated from canonical CUE, so any schema or extractor that consumes Markdown should treat it as a derived view.
