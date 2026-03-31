# JSON Schema Model

This directory contains the authoritative schema layer for authored `spec-trace` artifacts.

- [`model.schema.json`](./model.schema.json) defines the shared artifact, requirement, trace, evidence, retired-ledger, and catalog shapes used by canonical JSON documents and validation tooling.
- Canonical authored artifacts are JSON documents.
- Repository tooling uses this schema first and then applies repository-level cross-file validation.
