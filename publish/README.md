# SpecTrace Publish Mirror

This directory is the curated publish mirror for the reusable `spec-trace` JSON Schema and template surfaces.

It intentionally contains only:

- [`model/`](./model/) for the authoritative JSON Schema mirror
- root JSON templates:
  - [`spec-template.json`](./spec-template.json)
  - [`architecture-template.json`](./architecture-template.json)
  - [`work-item-template.json`](./work-item-template.json)
  - [`verification-template.json`](./verification-template.json)

It does not include the repository's canonical self-specification suite, worked examples, validation tooling, or generated artifacts.

Canonical authored sources still live at the repository root. This mirror is synchronized from those root files by [`../scripts/Sync-PublishModule.ps1`](../scripts/Sync-PublishModule.ps1), which also prunes files outside the curated publish set.
