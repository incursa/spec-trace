# SpecAttest Standard 0.1.0-rc.1 publication preparation

## Edition metadata

- Edition: `0.1.0-rc.1`
- Maturity: Candidate
- Previous edition: `0.1.0-draft.2`
- Compatibility: Non-normative
- Governance: Incursa owner approved
- Bootstrap: no
- Behavioral review: [`VER-WB-0012-specattest-draft-qualification`](https://github.com/incursa/workbench/blob/167b247bd9bfbb2ee10455b2332fcd7d303a8d23/specs/verification/WB/VER-WB-0012-specattest-draft-qualification.md)

This candidate freezes the same 12 canonical specification artifacts and 235
requirement revisions as `0.1.0-draft.2`. Promotion changes publication
maturity and retained qualification evidence; it does not change normative
interpretation, accepted content, identifiers, trace meaning, canonicalization,
or required behavior.

## Qualification basis

- Workbench independently validates the complete draft.2 edition pin and all 20
  frozen conformance cases.
- Manager retains authenticated results for all five safe-mutation protocol
  vectors through both real REST and MCP bindings: 10 of 10 transport
  observations passed.
- The Manager evidence whole-file SHA-256 is
  `32874adfc64d33cf1bd45a8554c48b5da02b0482168e41009fe60de53213f529`.
- The Manager evidence validation-subject SHA-256 is
  `97186df819d0d63f2094b775ea80c14adc1a73e26e782b68dde1e6259ee5ccee`.
- Manager browser, Debian LXC package, and release-provenance gates passed.
- Workbench Windows and macOS build/test, documentation, verification,
  pre-commit, and CodeQL gates passed.

The immutable manifest is built only after this source commit is frozen. The
release assets retain results from the validator accepted for
`0.1.0-draft.2`, the `0.1.0-rc.1` candidate validator, the shared conformance
corpus, the behavioral review, the signed tag, and an independent redownload
verification.
