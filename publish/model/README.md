# CUE Model

This package is the canonical schema layer for authored `spec-trace` artifacts.

- [`model.cue`](./model.cue) defines the shared artifact, requirement, trace, evidence, retired-ID ledger, and catalog shapes used by canonical `.cue` documents and validation tooling.
- The import path is `github.com/incursa/spec-trace/model@v0`.
- Concrete authored artifacts should unify a top-level `artifact` value with one of the exported definitions, for example `model.#Specification` or `model.#WorkItem`.

JSON, YAML, and Markdown may still be produced as derived outputs or integration views, but the CUE definitions in this package are authoritative.
