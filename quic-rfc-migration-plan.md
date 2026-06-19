# QUIC RFC Migration Plan

## Purpose

Plan a safe migration from the current `quic-dotnet` QUIC requirement set to the newly converted RFC outputs without breaking the existing requirement-home, ARC, WI, VER, and test trace.

This plan assumes the current live QUIC requirement IDs remain the authoritative starting point unless a requirement's meaning actually changes. It does **not** assume the staged `.stable.json` outputs can be published into `quic-dotnet` unchanged.

## Current State

The live QUIC repo currently carries section-keyed requirement IDs and full downstream trace. The staged RFC conversion outputs are deterministic, but they are not ID-aligned with the live set and they currently carry only upstream provenance.

| RFC | Live requirements | Staged requirements | Exact ID overlap | Exact statement overlap | Migration read |
| --- | ---: | ---: | ---: | ---: | --- |
| RFC8999 | 8 | 65 | 0 | 0 | Mostly rebaseline |
| RFC9000 | 1443 | 1641 | 0 | about 380 | Best pilot for overlap mapping |
| RFC9001 | 96 | 428 | 0 | 14 | Mixed preserve and split/merge cases |
| RFC9002 | 224 | 382 | 0 | 45 | Mixed preserve and split/merge cases |

Key facts:

- There is no exact ID overlap between the live QUIC set and the staged RFC outputs.
- Statement overlap exists, but only partially.
- The live QUIC set already includes downstream trace such as `satisfied_by`, `implemented_by`, `verified_by`, and test references.
- The staged RFC outputs are not yet trace-complete for migration into the live repo.
- The trace-lineage model already supports `derived_from`, `supersedes`, and retired-ID ledgers. See [SPEC-LIN](./specs/requirements/spec-trace/SPEC-LIN.json) and [SPEC-SCH](./specs/requirements/spec-trace/SPEC-SCH.json).

## Migration Principles

1. Preserve the live requirement ID when the obligation is materially unchanged.
2. Assign a new requirement ID when the obligation changes.
3. Use new IDs for splits and merges, with lineage recorded back to the source IDs.
4. Treat exact statement text as the primary mapping signal.
5. Treat title matches and staged `proposed_id_hint` values as secondary hints only.
6. Treat source provenance as supporting evidence, not as a standalone join key.
7. Never reuse a retired requirement ID for a different obligation.
8. Keep the downstream trace graph intact unless a specific requirement is actually being replaced.

## Recommended Bucket Model

Classify every live requirement against the staged RFC set into one of four buckets:

- `preserve`
  - The obligation is the same.
  - Keep the live ID.
  - Update the wording only if needed.
- `rename-with-lineage`
  - The staged clause is the canonical replacement for the live clause.
  - Record `derived_from` and/or `supersedes`.
  - Add the old ID to the retired-ID ledger.
- `split-merge`
  - One old requirement maps to multiple staged requirements, or multiple old requirements collapse into one staged requirement.
  - Create new IDs for the resulting requirements.
  - Preserve the relationship with lineage fields.
- `new-only`
  - No credible live counterpart exists.
  - Create the staged requirement as a new obligation.

## Recommended Migration Order

1. Build a crosswalk report before touching live requirement files.
2. Pilot the mapping on RFC9000 first.
   - It has the strongest exact statement overlap.
   - It is the best place to validate preserve vs split/merge decisions.
3. Process RFC9001 and RFC9002 next.
   - These are mixed cases with smaller exact overlap and more manual review.
4. Treat RFC8999 as a separate rebaseline lane.
   - It has no exact statement overlap with the current live set.
   - It is likely to require the most explicit re-anchoring of trace.
5. Rewrite all dependent surfaces in the same slice for any requirement that changes identity.
   - requirement-home filenames
   - `[Requirement(...)]` attributes
   - ARC / WI / VER references
   - `x_test_refs`
   - cross-RFC references inside other tests and specs
6. Validate after each slice, not only at the end.

## What the Crosswalk Needs To Contain

For every live requirement, the crosswalk should record:

- live requirement ID
- staged requirement ID or IDs
- bucket classification
- statement match confidence
- title match confidence
- split/merge notes
- lineage direction
- any dependent trace surfaces that must move with the requirement

The crosswalk should be machine-readable and reviewable as markdown or JSON. It should be the only place where the old and new IDs are compared directly.

## Execution Rules

- Do not publish the staged `.stable.json` outputs into `quic-dotnet` as a direct replacement.
- Do not use `proposed_id_hint` as a primary key.
- Do not rely on source-unit provenance alone to decide identity.
- Do not rewrite IDs before the crosswalk is approved.
- Do not let the retirement ledger lag behind any ID changes.

## Validation Gates

Each migration slice should pass these gates in order:

1. Structural JSON validation.
2. Repository-wide SpecTrace validation.
3. QUIC repo trace validation.
4. Requirement-home and test reference validation.
5. Auditable/strict validation where the repo expects downstream proof.

The migration is not complete until the live repo validates cleanly with the new mapping in place and the retired-ID ledger accounts for everything that changed identity.

## Deliverables Per RFC

For each RFC, produce:

- a crosswalk report
- a retired-ID ledger update, if any IDs change
- updated live requirement JSON
- updated requirement-home paths and test ownership markers
- updated ARC / WI / VER references
- a validation summary

## Open Decisions

Before any large rewrite, confirm:

1. Whether the QUIC repo wants to keep section-keyed IDs as the canonical live form.
2. Whether the staged deterministic IDs are only a migration source or the future canonical naming scheme.
3. Whether the retired-ID ledger will be checked in as a durable file or generated on demand.
4. Whether `proposed_id_hint` should remain a hidden helper or become part of the review report.

## Bottom Line

This is a semantic migration, not a blind rename.

The safest path is:

1. map exact or near-exact statement matches first,
2. preserve live IDs whenever the obligation is unchanged,
3. introduce new IDs only for changed, split, or merged obligations,
4. carry lineage and retired-ID metadata along with the rewrite,
5. validate each RFC slice before moving to the next one.
