# Versioning conformance corpus

These fixtures exercise the observable revision, edition, maturity, and bootstrap rules in `SPEC-VER`.

- `valid-bootstrap-draft.json` is structurally valid bootstrap metadata.
- `invalid-draft-as-stable.json` pairs a draft prerelease with stable maturity and must be rejected.
- `invalid-zero-revision.json` uses revision zero and must be rejected.
- `invalid-zero-prerelease-ordinal.json` uses `draft.0` and must be rejected.
- `invalid-successor-without-review.json` marks a successor without the required prior-edition behavioral review and must be rejected.

The corpus digest included in an edition manifest is SHA-256 over the byte-for-byte corpus bundle selected by the publisher. Validators record the same digest in their retained result evidence.

JSON Schema validates each revision entry. The independent bootstrap validator additionally rejects duplicate `(requirement ID, revision)` coordinates and compares the complete declared coordinate set and per-revision canonical digest with the frozen canonical suite.
