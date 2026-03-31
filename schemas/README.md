# Schemas

These files are compatibility or slice schemas aligned to the authoritative model in [`../model/model.schema.json`](../model/model.schema.json).

## Canonical Source

- [`../model/model.schema.json`](../model/model.schema.json) is the authoritative JSON Schema for canonical artifacts.
- [`../specs/requirements/spec-trace/SPEC-SCH.json`](../specs/requirements/spec-trace/SPEC-SCH.json) defines the normative validation rules.

## Included Schemas

- [`artifact-frontmatter.schema.json`](./artifact-frontmatter.schema.json)
- [`artifact-id-policy.schema.json`](./artifact-id-policy.schema.json)
- [`requirement-clause.schema.json`](./requirement-clause.schema.json)
- [`requirement-trace-fields.schema.json`](./requirement-trace-fields.schema.json)
- [`work-item-trace-fields.schema.json`](./work-item-trace-fields.schema.json)
- [`evidence-snapshot.schema.json`](./evidence-snapshot.schema.json)
- [`retired-requirement-ledger.schema.json`](./retired-requirement-ledger.schema.json)

## Notes

- These files do not override the authoritative schema.
- Repository-wide reference resolution, duplicate detection, reciprocal-link checks, and profile enforcement happen in repository validation on top of JSON Schema.
