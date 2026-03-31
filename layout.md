# Recommended Repository Layout

This file describes the recommended layout for repositories that adopt the standard.

## Default Layout

```text
cue.mod/
model/
catalog/
  retired-requirements.cue
/specs
  /requirements
    /<domain>/
      _index.md
      SPEC-<DOMAIN>[-<GROUPING>...].cue
      SPEC-<DOMAIN>[-<GROUPING>...].md
  /architecture
    /<domain>/
      <readable-file-name>.cue
      <readable-file-name>.md
  /work-items
    /<domain>/
      <readable-file-name>.cue
      <readable-file-name>.md
  /verification
    /<domain>/
      <readable-file-name>.cue
      <readable-file-name>.md
  /generated
    spec-trace-catalog.cue
    spec-trace-catalog.json
    traceability-matrix.md
    verification-coverage.md
/examples
  /<domain>/
    *.cue
    *.md
```

## Layout Rules

### Canonical Source

Canonical authored artifacts live in `.cue` files. Generated Markdown may sit beside the source `.cue` file for readability, but contributors should edit the `.cue` file.

### Specifications

Place specifications under `/specs/requirements/<domain>/`.

- organize by stable domain first
- organize by capability, behavior area, interface, or narrow concern second
- keep one specification artifact per `.cue` file
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
- traceability matrices
- coverage rollups
- bundled Markdown views

Those files are derived outputs, not canonical authored artifacts.

## File Names

File names should stay stable and readable.

- Specification `.cue` files should include the full specification ID.
- Architecture, work-item, and verification artifact filenames may use readable stable names.
- Avoid dates, sprint numbers, and owner names in canonical filenames.
- Keep traceability anchored on stable IDs in the artifact content, not on the file name.

## Index Files

[`_index.md`](./specs/requirements/spec-trace/_index.md) files are optional navigation aids.

They may summarize a domain and link the generated Markdown views, but they do not replace the underlying canonical `.cue` artifacts.

## Traceability Loop

The layout should make this chain easy to follow:

1. a specification groups requirement records
2. architecture artifacts satisfy requirements
3. work items address requirements and use design inputs
4. verification artifacts prove requirements
5. generated evidence and reporting views summarize current repository state without becoming the authored source of truth
