# SpecTrace CUE Module

This submodule is the publishable Central Registry package for the reusable `spec-trace` CUE surfaces.

It intentionally contains only the reusable schema and template definitions:

- [`model/`](./model/) for canonical schema definitions
- root `templates` package definitions in:
  - [`spec-template.cue`](./spec-template.cue)
  - [`architecture-template.cue`](./architecture-template.cue)
  - [`work-item-template.cue`](./work-item-template.cue)
  - [`verification-template.cue`](./verification-template.cue)

It does not include the repository's canonical self-specification suite, worked examples, validation tooling, or generated artifacts.

## Import Paths

- `github.com/incursa/spec-trace/model@v0`
- `github.com/incursa/spec-trace@v0:templates`

## Source Of Truth

Canonical authored sources still live at the repository root. This publish submodule is synchronized from those root files by [`../scripts/Sync-PublishModule.ps1`](../scripts/Sync-PublishModule.ps1), which also prunes files that are outside the curated publish set.
