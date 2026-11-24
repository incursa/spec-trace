# Versioning conformance corpus

These fixtures exercise the observable revision, edition, maturity, bootstrap, complete-pin, interchange, and safe-mutation rules in `SPEC-VER` and `SPEC-SCH`.

- `valid-bootstrap-draft.json` is structurally valid bootstrap metadata.
- `invalid-draft-as-stable.json` pairs a draft prerelease with stable maturity and must be rejected.
- `invalid-zero-revision.json` uses revision zero and must be rejected.
- `invalid-zero-prerelease-ordinal.json` uses `draft.0` and must be rejected.
- `invalid-successor-without-review.json` marks a successor without the required prior-edition behavioral review and must be rejected.
- `pin-*.json` mutate one trusted complete pin and must be rejected for wrong commits, branches, missing coordinates, or altered manifest, schema, payload, corpus, and tag hashes.
- `revision-storage-version-divergence.json` and `canonical-reorder-equivalence.json` preserve stable IDs, exact normative revisions, and lineage independently from provider record versions and JSON object-key order.
- `protocol-*.json` require equivalent REST and MCP outcomes for stale conflicts, atomic failure, deterministic replay, cancel, and resume.

The corpus digest included in an edition manifest is SHA-256 over the byte-for-byte corpus bundle selected by the publisher. Validators record the same digest in their retained result evidence.

Run `scripts/Validate-ConformanceCorpus.py` for the transport-neutral vectors. JSON Schema validates each revision entry and the interchange envelope. The independent edition validator additionally rejects duplicate `(requirement ID, revision)` coordinates and compares the complete declared coordinate set and per-revision canonical digest with the frozen canonical suite.
