# SpecAttest Standard 0.1.0 stable-promotion preparation

## Status

Stable qualification is **in progress**. This preparation record does not
publish, approve, or represent `0.1.0` as a stable edition.

The candidate soak began when `0.1.0-rc.1` was published on
2026-11-24 at 21:10:39 UTC. The default promotion gate is seven complete days,
so the earliest stable decision is 2026-12-01 at 21:10:39 UTC
(2:10:39 PM Mountain time).

## Intended edition metadata

- Edition: `0.1.0`
- Maturity: Stable
- Previous edition: `0.1.0-rc.1`
- Compatibility: Non-normative
- Governance: Incursa owner approved, after qualification
- Bootstrap: no

Stable means supported under Incursa's documented governance. It does not
claim ratification by an external standards body.

## Promotion invariant

The stable edition is intended to retain the exact normative contract frozen
by `0.1.0-rc.1`: 12 ordered specifications, 235 exact requirement revisions,
the same model and edition-manifest schemas, the same ordered payload, the same
20-case conformance corpus, and the same canonicalization behavior.

If qualification requires any change to accepted content, identifiers, trace
meaning, canonicalization, validation behavior, or required behavior, `0.1.0`
must not be published from RC1. The correction must be frozen as
`0.1.0-rc.2` (or a later candidate) and qualified again.

## Qualification entry baseline

The 2026-11-25 baseline passed:

- the signed `standard-v0.1.0-rc.1` tag verified locally for
  `samuel@mcaravey.pro`, ED25519 fingerprint
  `SHA256:kOwAPw/gfeAOI0sTToyTAtkg/KI1kQRkCdH/SZvONKk`;
- every GitHub release asset reproduced `SHA256SUMS`;
- Workbench independently accepted the exact edition with 12 specifications,
  235 requirements, 20 conformance cases, and validation-subject SHA-256
  `7ee6d7a166c8d8d83cebb5da19bd9aa20fd35d23c5640e5946ac205b6ca04af2`;
- Workbench independently passed all 20 corpus cases and its 298-test suite;
- repository validation passed for 34 artifacts and 309 requirements;
- Manager regenerated the five-vector REST/MCP matrix against the exact RC1
  pin; all ten transport observations passed; and
- the production LXC returned ready, read-write Manager `0.0.0-ci.168` with
  the complete `0.1.0-rc.1` pin.

The RC1-specific Manager evidence SHA-256 is
`d8552eb9043600cb371e3d915a6247e85dec0c08a36642ef69d9f01ce1d5f7a3`;
its validation-subject SHA-256 is
`d39abdea846f06af91c45e593e51ab3f22de3cac10502a5aec50dd7c3f14a819`.

## Remaining promotion gates

- Complete the seven-day soak without an unresolved normative defect,
  cross-tool incompatibility, data-integrity incident, or rollback.
- Review production incidents and RC1 issue reports for the entire window.
- Rerun Standard, Workbench, Manager, release-asset, and live-pin checks at the
  decision point.
- Record the human behavioral review and confirm zero normative differences
  from RC1.
- Obtain explicit Incursa owner approval for stable publication.
- Freeze the exact stable source commit and build the immutable `0.1.0`
  manifest with accepted and candidate validator results.
- Create and verify signed tag `standard-v0.1.0`, publish a non-prerelease
  GitHub release, and independently redownload and verify every asset.
- Update Manager and Workbench to the complete stable pin only after the
  stable edition exists, then requalify the deployed tuple.

The companion checklist is
[`specattest-standard-0.1.0-checklist.md`](./specattest-standard-0.1.0-checklist.md).
