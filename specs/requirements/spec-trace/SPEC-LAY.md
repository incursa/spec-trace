---
artifact_id: SPEC-LAY
artifact_type: specification
title: Repository Layout and Artifact Placement
domain: spec-trace
capability: repository-layout
status: draft
owner: spec-trace-maintainers
tags:
  - spec-trace
  - layout
  - placement
---

# [`SPEC-LAY`](./SPEC-LAY.md) - Repository Layout and Artifact Placement

## Purpose

Define how repositories should place canonical CUE source artifacts and derived outputs so the structure reinforces the traceability model.

## Scope

This specification covers the recommended `/specs` tree, placement of artifact families, canonical CUE sources, generated Markdown outputs, and the distinction between artifact role and file naming.

## Context

Traceability weakens when requirements, design, work, and verification are scattered according to workflow noise instead of stable product structure.

## [`REQ-LAY-0001`](./SPEC-LAY.md#req-lay-0001-keep-live-source-artifacts-under-the-specs-tree) Keep live source artifacts under the /specs tree
A repository using the standard MUST keep live canonical source artifacts as CUE under [`/specs`](../../../specs) with dedicated areas for requirements, architecture, work-items, verification, generated outputs, templates, and schemas.

Notes:
- Decision records are not part of the core layout today; repositories that need them may add an optional local extension.

## [`REQ-LAY-0002`](./SPEC-LAY.md#req-lay-0002-organize-specifications-by-stable-domain-and-concern) Organize specifications by stable domain and concern
Specifications MUST be organized by stable domain first and by capability, behavior area, interface, or technical concern second.

## [`REQ-LAY-0003`](./SPEC-LAY.md#req-lay-0003-place-artifact-families-according-to-their-role) Place artifact families according to their role
Architecture, work-item, and verification artifacts MUST live in their respective family areas with traceability back to requirement identifiers.

## [`REQ-LAY-0004`](./SPEC-LAY.md#req-lay-0004-keep-generated-outputs-separate-from-source-artifacts) Keep generated outputs separate from source artifacts
Generated indexes, matrices, coverage reports, and evidence snapshots MUST live
under [`/specs/generated`](../../../specs/generated) when they are stored in
the repository, while generated Markdown renderings of canonical artifacts may
live beside their source `.cue` files for readability.

## [`REQ-LAY-0005`](./SPEC-LAY.md#req-lay-0005-include-the-full-specification-id-in-specification-file-names) Include the full specification ID in specification file names
Each canonical specification `.cue` file name MUST include the full specification artifact ID.

## [`REQ-LAY-0006`](./SPEC-LAY.md#req-lay-0006-keep-one-specification-per-canonical-artifact-file) Keep one specification per canonical artifact file
Each specification `.cue` file MUST contain exactly one specification artifact and one or more related requirement records beneath it.

## [`REQ-LAY-0007`](./SPEC-LAY.md#req-lay-0007-treat-index-files-as-navigation-only) Treat index files as navigation only
A domain or concern MAY include an [`_index.md`](./_index.md) file for navigation.

## [`REQ-LAY-0008`](./SPEC-LAY.md#req-lay-0008-keep-the-reference-package-packaging-friendly) Keep the reference package packaging-friendly
The public reference package MUST keep its canonical suite under [`specs/requirements/spec-trace/`](./).

## [`REQ-LAY-0009`](./SPEC-LAY.md#req-lay-0009-keep-file-names-stable-and-readable) Keep file names stable and readable
Specification file names SHOULD remain stable and readable without using dates, sprints, or owner names.

## [`REQ-LAY-0010`](./SPEC-LAY.md#req-lay-0010-keep-indexes-non-authoritative) Keep indexes non-authoritative
An [`_index.md`](./_index.md) file MUST NOT replace the underlying artifacts.

## [`REQ-LAY-0011`](./SPEC-LAY.md#req-lay-0011-allow-copy-friendly-root-guidance-in-the-reference-package) Allow copy-friendly root guidance in the reference package
The public reference package MAY also publish root guidance and templates for copy convenience.
