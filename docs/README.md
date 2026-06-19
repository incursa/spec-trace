---
title: "Documentation"
---

# Documentation

This directory contains secondary operational documentation for the `spec-trace` repository.

The canonical standard remains the JSON-authored SPEC suite under [`../specs/requirements/spec-trace/`](../specs/requirements/spec-trace/). Root guidance such as [`../README.md`](../README.md), [`../authoring.md`](../authoring.md), and [`../overview.md`](../overview.md) is the preferred starting point for readers.

`docs.site.json` and `.github/workflows/sync-docs.yml` define the mirror
target and pull-request flow for the central docs site. Do not edit the
generated `incursa-docs` copy directly.

## Operational Docs

- [`contributor-agreement-automation.md`](./contributor-agreement-automation.md) - Incursa Contributor Agreement workflow setup, signing flow, required secret, and branch-rule status check.
- [`maintainer-readiness.md`](./maintainer-readiness.md) - validation floor, release expectations, downstream adoption path, and known gaps.

## Source And Validation

- [`../README.md`](../README.md) - repository boundary, release surface, and mirrored-docs note
- [`../specs/requirements/spec-trace/_index.md`](../specs/requirements/spec-trace/_index.md) - canonical navigation into the standard
- [`../model/model.schema.json`](../model/model.schema.json) - authoritative JSON Schema
- [`../scripts/Test-SpecTraceRepository.ps1`](../scripts/Test-SpecTraceRepository.ps1) - local repository validation entrypoint
