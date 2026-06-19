# RFC 8999 Coverage Review

## Bottom Line

The tuned 35-item extraction is the better base.

- It is materially less noisy than the 80-item verbose baseline.
- It captures the core RFC 8999 packet, connection ID, version, and version-negotiation surface.
- It is still not the final canonical set because it compresses some RFC text too aggressively and drops a few intro and notation facts that are still useful requirements.
- Appendix A remains quarantine-only. Those items are explicitly "incorrect assumptions" in the RFC.

## Tuned 35

### Keep

- `REQ-QUIC-RFC8999-S1-0001`
- `REQ-QUIC-RFC8999-S4-0001`
- `REQ-QUIC-RFC8999-S4-0002`
- `REQ-QUIC-RFC8999-S5-0003`
- `REQ-QUIC-RFC8999-S5-0004`
- `REQ-QUIC-RFC8999-S5P1-0001` through `REQ-QUIC-RFC8999-S5P1-0008`
- `REQ-QUIC-RFC8999-S5P2-0002` through `REQ-QUIC-RFC8999-S5P2-0006`
- `REQ-QUIC-RFC8999-S5P3-0001` through `REQ-QUIC-RFC8999-S5P3-0003`
- `REQ-QUIC-RFC8999-S5P4-0001`
- `REQ-QUIC-RFC8999-S5P4-0002`
- `REQ-QUIC-RFC8999-S6-0001`
- `REQ-QUIC-RFC8999-S6-0002`
- `REQ-QUIC-RFC8999-S6-0004`
- `REQ-QUIC-RFC8999-S6-0005`
- `REQ-QUIC-RFC8999-S6-0007` through `REQ-QUIC-RFC8999-S6-0009`
- `REQ-QUIC-RFC8999-S7-0001`
- `REQ-QUIC-RFC8999-S7-0002`

### Merge

- `REQ-QUIC-RFC8999-S5P2-0001`
  - This is a useful short-header summary, but it overlaps the more specific short-header requirements and is not atomic.
- `REQ-QUIC-RFC8999-S6-0003`
  - This compresses the Version Negotiation high-bit, unused-bit, and ignore-on-receipt semantics into one statement.
- `REQ-QUIC-RFC8999-S6-0006`
  - This compresses Supported Version field placement and list semantics into one statement.

## Baseline 80

### Promote Into The Tuned Set

These baseline items are not junk. They are real RFC 8999 content that the tuned set either omitted or compressed too far.

- Section 1
  - `REQ-QUIC-RFC8999-S1-0001`
  - `REQ-QUIC-RFC8999-S1-0002`
  - `REQ-QUIC-RFC8999-S1-0003`
  - `REQ-QUIC-RFC8999-S1-0005`
- Section 2
  - `REQ-QUIC-RFC8999-S2-0001`
  - `REQ-QUIC-RFC8999-S2-0002`
  - `REQ-QUIC-RFC8999-S2-0003`
- Section 4
  - `REQ-QUIC-RFC8999-S4-0001` through `REQ-QUIC-RFC8999-S4-0007`
- Section 5
  - `REQ-QUIC-RFC8999-S5-0001`
  - `REQ-QUIC-RFC8999-S5-0002`
- Section 5.1
  - `REQ-QUIC-RFC8999-S5P1-0001`
  - `REQ-QUIC-RFC8999-S5P1-0002`
  - `REQ-QUIC-RFC8999-S5P1-0003`
- Section 5.3
  - `REQ-QUIC-RFC8999-S5P3-0002`
  - `REQ-QUIC-RFC8999-S5P3-0003`
  - `REQ-QUIC-RFC8999-S5P3-0004`
- Section 5.4
  - `REQ-QUIC-RFC8999-S5P4-0002`
  - `REQ-QUIC-RFC8999-S5P4-0004`
  - `REQ-QUIC-RFC8999-S5P4-0005`
  - `REQ-QUIC-RFC8999-S5P4-0006`
- Section 6
  - `REQ-QUIC-RFC8999-S6-0005`
  - `REQ-QUIC-RFC8999-S6-0011`
  - `REQ-QUIC-RFC8999-S6-0012`
  - `REQ-QUIC-RFC8999-S6-0013`
  - `REQ-QUIC-RFC8999-S6-0014`

### Already Represented By The Tuned Set

These baseline items are still useful, but the tuned 35 already captures them through broader or merged requirements.

- `REQ-QUIC-RFC8999-S1-0004`
- `REQ-QUIC-RFC8999-S4-0008`
- `REQ-QUIC-RFC8999-S4-0009`
- `REQ-QUIC-RFC8999-S5-0003`
- `REQ-QUIC-RFC8999-S5-0004`
- `REQ-QUIC-RFC8999-S5P1-0004` through `REQ-QUIC-RFC8999-S5P1-0011`
- `REQ-QUIC-RFC8999-S5P2-0001`
- `REQ-QUIC-RFC8999-S5P2-0003`
- `REQ-QUIC-RFC8999-S5P2-0004`
- `REQ-QUIC-RFC8999-S5P3-0001`
- `REQ-QUIC-RFC8999-S5P3-0005`
- `REQ-QUIC-RFC8999-S5P3-0006`
- `REQ-QUIC-RFC8999-S5P4-0001`
- `REQ-QUIC-RFC8999-S5P4-0003`
- `REQ-QUIC-RFC8999-S6-0001` through `REQ-QUIC-RFC8999-S6-0004`
- `REQ-QUIC-RFC8999-S6-0006` through `REQ-QUIC-RFC8999-S6-0024`
- `REQ-QUIC-RFC8999-S7-0001`

### Quarantine

- `REQ-QUIC-RFC8999-SA-0001` through `REQ-QUIC-RFC8999-SA-0008`

These are the RFC's incorrect assumptions appendix. They are intentionally non-canonical and should not be promoted into the authoritative requirement set.

## Coverage Gaps To Close Next

The tuned set is still missing some useful requirements that are present in the baseline and in the RFC text:

- Section 1 still wants explicit connection-oriented / datagram / shared-state statements.
- Section 2 still wants the version-negotiation and IP-version-independent invariants.
- Section 4 still wants the notation and field-length conventions.
- Section 5 still wants the datagram-exchange and header-type statements.
- Section 5.1 still wants the long-header form / first-bit / version-specific-bit statements.
- Section 5.3 still wants the lower-layer misdelivery and endpoint-delivery semantics.
- Section 5.4 still wants the version-identification and "all versions / nonconforming protocol" semantics.

Section 6 is mostly fine semantically, but `REQ-QUIC-RFC8999-S6-0003` and `REQ-QUIC-RFC8999-S6-0006` should probably be split further if the goal is maximum traceability.

## Review Workflow Recommendation

Use a second-pass review model, but make it a review pass over extracted requirements, not a second extraction pass over the raw RFC.

For large RFCs like RFC 9000:

- chunk by top-level section or subsection
- cap each batch by source units and by candidate requirement count
- have the review pass return only `keep`, `merge`, `promote`, or `quarantine`
- avoid asking the model to re-derive the entire document in one shot

That keeps the review scalable while still catching missing coverage and over-fragmentation.

## Audited 50

I added the explicit `coverage-audit` phase and reran RFC 8999 against the tuned 35-item extraction.

Result:

- Audited run: 50 requirements
- Baseline verbose run: 80 requirements
- Tuned extraction: 35 requirements
- Existing QUIC repo artifact: 8 requirements

Section distribution for the audited run:

- `S1`: 1
- `S4`: 2
- `S5`: 4
- `S5P1`: 14
- `S5P2`: 7
- `S5P3`: 3
- `S5P4`: 2
- `S6`: 15
- `S7`: 2

The audited output is the strongest coverage result so far. It is still more granular than the tuned 35-item set, but it stays free of the appendix `SA` noise and recovers more of the RFC 8999 normative surface, especially around `S5P1` and `S6`.
