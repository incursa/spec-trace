# Recommended Repository Layout

This file describes the recommended layout for repositories that adopt the standard.

## Default Layout

```text
model/
  model.schema.json
catalog/
  retired-requirements.json
specs/
  requirements/
    <domain>/
      _index.md
      SPEC-<DOMAIN>[-<GROUPING>...].json
  architecture/
    <domain>/
      <readable-file-name>.json
  work-items/
    <domain>/
      <readable-file-name>.json
  verification/
    <domain>/
      <readable-file-name>.json
  generated/
    spec-trace-catalog.json
    validation-report.json
examples/
  <domain>/
    *.json
    *.evidence.json
    generated/
      *.md
      *.json
```

## Layout Rules

### Canonical Source

Canonical authored artifacts live in JSON files. Support docs may live beside them for navigation or explanation, but contributors should edit the JSON file.

### Specifications

Place specifications under `/specs/requirements/<domain>/`.

- organize by stable domain first
- organize by capability, behavior area, interface, or narrow concern second
- keep one specification artifact per `.json` file
- include the full specification ID in the specification filename

### Architecture

Place architecture artifacts under `/specs/architecture/<domain>/`.

These artifacts explain how requirements are satisfied. They are the default place for design rationale and tradeoffs.

### Work Items

Place work items under `/specs/work-items/<domain>/`.

Work items describe implementation work and should trace back to requirements and design inputs.

### Verification

Place verification artifacts under `/specs/verification/<domain>/`.

Verification artifacts record how requirements were checked and what shared outcome was recorded.

### Generated Outputs

Place generated indexes, reports, and compatibility outputs under `/specs/generated/`.

Examples:

- repository catalogs
- validation reports
- attestation output
- coverage rollups

Those files are derived outputs, not canonical authored artifacts.

## File Names

File names should stay stable and readable.

- Specification `.json` files should include the full specification ID.
- Architecture, work-item, and verification artifact filenames may use readable stable names.
- Avoid dates, sprint numbers, and owner names in canonical filenames.
- Keep traceability anchored on stable IDs in the artifact content, not on the file name.

## Index Files

[`_index.md`](./specs/requirements/spec-trace/_index.md) files are optional navigation aids.

They may summarize a domain and link canonical JSON artifacts, but they do not replace the underlying source files.

## Traceability Loop

The layout should make this chain easy to follow:

1. a specification groups requirement records
2. architecture artifacts satisfy requirements
3. work items address requirements and use design inputs
4. verification artifacts prove requirements
5. generated evidence and reporting views summarize current repository state without becoming the authored source of truth
