# SpecAttest Standard 0.1.0-draft.1 bootstrap checklist

This checklist is completed against the exact frozen bundle. A checked box without a retained result artifact and SHA-256 value is not publication evidence.

## Canonical content

- [ ] Record the final source commit and verify the worktree is clean.
- [ ] Confirm the ordered canonical suite contains the 12 artifacts in the preparation record.
- [ ] Confirm all 228 requirement IDs are unique and every active requirement has revision 1 or greater.
- [ ] Confirm changed obligations use new IDs and lineage rather than silently reusing prior IDs.
- [ ] Review normative statements for exactly one approved uppercase keyword.
- [ ] Record known gaps and confirm they do not hide correctness, integrity, security, or disclosure defects.

## Independent validation

- [ ] Validate canonical artifacts with `model/model.schema.json` and retain the result hash.
- [ ] Validate the edition manifest with `schemas/standard-edition.schema.json` and retain the result hash.
- [ ] Run `scripts/Validate-EditionBootstrap.py` independently and retain the result hash.
- [ ] Run the shared versioning conformance corpus against every declared validator.
- [ ] Record validator names, versions, result hashes, and the conformance-corpus hash.

## Freeze and approval

- [ ] Freeze the exact artifact bundle and record its payload SHA-256.
- [ ] Record the authoritative schema SHA-256 and canonicalization identifier.
- [ ] Record the governance approval reference and completed behavioral review.
- [ ] Create and verify a signed Git tag or signed release manifest.
- [ ] Verify the published edition manifest against `schemas/standard-edition.schema.json`.
- [ ] Independently download the publication and reproduce every declared digest.
