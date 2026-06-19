# RFC 9002 Coverage Review

## Bottom Line

The current pipeline can convert RFC 9002 end-to-end, but the audited result is still too granular to treat as the final canonical set.

- Existing QUIC repo artifact: 224 requirements
- RFC 9002 audited draft: 359 requirements
- Gap: +135 requirements over the existing baseline

That delta is too large to ignore. The audited draft is likely more complete than the older artifact, but it is still over-fragmented enough that a final merge/trim pass is warranted before we call it canonical.

## Normalized 357

I added a dedicated `normalize` pass and reran the RFC 9002 assembly through it.

- Normalized draft: 357 requirements
- Reduction from the audited draft: -2 requirements
- Gap over the existing baseline: +133 requirements

This confirms the merge/trim stage is wired into the standard flow, but the current prompt is still too conservative to collapse the over-fragmentation down to something close to the existing QUIC artifact. It is a functional stage, not yet a decisive one.

## Stable Numbering

I also tightened requirement numbering so section-scoped IDs are assigned from canonical source order instead of AI arrival order or `proposed_id_hint` values.

- Stable reassembly: 357 requirements
- Output: [`SPEC-QUIC-RFC9002.stable.json`](../../../.work-rfc9002/SPEC-QUIC-RFC9002.stable.json)

The numbering shape now matches the QUIC repo convention more closely: section families are contiguous, sequence numbers are zero-padded, and the IDs are stable for the same RFC source order across reruns. This does not reduce the 357-count coverage set, but it makes iteration-to-iteration comparison meaningful.

## What I Ran

- `ingest --rfc 9002`
- `segment`
- `extract --batch-size 10`
- `coverage-audit --batch-size 25`
- `normalize --batch-size 25`
- `assemble`

The working artifacts are stored in `.work-rfc9002/`:

- [`source.json`](../../../.work-rfc9002/source.json)
- [`source-ledger.jsonl`](../../../.work-rfc9002/source-ledger.jsonl)
- [`candidates.jsonl`](../../../.work-rfc9002/candidates.jsonl)
- [`review-decisions.jsonl`](../../../.work-rfc9002/review-decisions.jsonl)
- [`coverage-audit.md`](../../../.work-rfc9002/coverage-audit.md)
- [`SPEC-QUIC-RFC9002.audited.json`](../../../.work-rfc9002/SPEC-QUIC-RFC9002.audited.json)

## Observations

- The extractor completed cleanly at 617/617 source units.
- The audit also completed cleanly at 617/617 review decisions.
- The main validator splits happened in recovery and congestion-control prose where single source units carried multiple uppercase normative keywords.
- Appendix sections still contribute a lot of surface area, but the audited draft does not look obviously invented.
- The remaining problem is granularity, not gross coverage.

## Recommendation

Add the final merge/trim pass before considering RFC 9002 canonical.

The existing extract -> coverage-audit -> assemble pipeline is good enough to recover the RFC surface, but not yet good enough to normalize it into a maintainable final spec on its own.
