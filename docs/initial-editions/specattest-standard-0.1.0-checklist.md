# SpecAttest Standard 0.1.0 stable-promotion checklist

## Candidate soak

- [x] Publish immutable `0.1.0-rc.1` with signed tag and complete release assets.
- [x] Start the candidate soak at 2026-11-24T21:10:39Z.
- [ ] Complete seven full days of candidate operation through
  2026-12-01T21:10:39Z.
- [ ] Review the soak window for unresolved normative defects,
  interoperability regressions, data-integrity incidents, and rollbacks.

## Contract invariants

- [ ] Reproduce the same 12 ordered specifications and 235 exact requirement
  revisions as RC1.
- [ ] Prove the stable ordered payload, model schema, manifest schema,
  canonicalization, and 20-case corpus are byte-identical to RC1.
- [ ] Confirm that no accepted content, stable identifier, trace meaning,
  canonicalization, validation behavior, or required behavior changed.
- [ ] Publish another release candidate instead if any normative or behavioral
  correction is required.

## Cross-tool qualification

- [x] Establish the 2026-11-25 entry baseline with the independent Workbench
  edition verifier and all 20 corpus cases.
- [x] Pass Workbench formatting, Release build, and all 298 tests.
- [x] Regenerate RC1-specific Manager REST/MCP evidence for all five protocol
  vectors and ten transport observations.
- [x] Confirm the production LXC is ready, read-write, and advertises the
  complete RC1 pin.
- [ ] Repeat these checks at the stable decision point.
- [ ] Retain final report hashes and the human behavioral-difference review.

## Freeze, approval, and publication

- [ ] Record explicit Incursa owner approval for stable publication.
- [ ] Freeze a clean source commit and build the `0.1.0` immutable manifest.
- [ ] Run the RC1 accepted validator and the `0.1.0` candidate validator over
  the exact stable bundle and shared corpus.
- [ ] Create and locally verify signed tag `standard-v0.1.0`.
- [ ] Publish a non-prerelease GitHub release with the bundle, manifest,
  schemas, checksums, validator results, behavioral review, and qualification
  evidence.
- [ ] Independently redownload the release and reproduce the complete stable
  edition pin.
- [ ] Update and qualify Manager and Workbench against the stable pin.

Unchecked items are release blockers. A checked checklist is necessary but
does not replace the owner's explicit stable-publication approval.
