# Complete edition-pin verification

This document gives the reference algorithm for `REQ-SCH-0035`. The canonical requirements remain authoritative.

A verifier fails closed unless every step succeeds:

1. Parse the edition manifest with `schemas/standard-edition.schema.json` and the interchange pin with the `standardPin` definition in `schemas/specattest-interchange.schema.json`.
2. Require every pin coordinate. A partial tuple is never equivalent to a complete tuple, even when the supplied values match.
3. Require `signatureReference` to name an immutable signed tag under `refs/tags/`. Reject branch names, remote-tracking names, symbolic names, and other mutable references.
4. Resolve and cryptographically verify the signed tag according to the consumer's trust policy. Peel it to a commit and require that commit to equal both `pin.commit` and `manifest.source.commit`.
5. Hash the signed tag object bytes and compare them with `pin.signatureSha256`.
6. Hash the exact edition-manifest bytes and compare them with `pin.editionManifestSha256`.
7. Fetch the declared model schema and edition-manifest schema from the exact source commit, hash their bytes, and compare them with the corresponding pin coordinates.
8. Rebuild the ordered canonical payload using the manifest's `specificationOrder`, canonical source path, and byte framing; compare it with both `manifest.payloadSha256` and `pin.payloadSha256`.
9. Rebuild the declared conformance-corpus archive from the exact source commit and compare it with both the manifest and pin corpus hashes.
10. Require exact equality for repository, edition, schema paths, canonicalization, canonical profiles, and proof profile. Do not infer or fill a missing value from local defaults.
11. Run the declared validators and conformance corpus. Preserve their result hashes as verification evidence.

Any missing coordinate, digest mismatch, substituted object, mutable reference, signature failure, or validator disagreement rejects the pin. A branch head may help locate development content but never identifies a published edition.
