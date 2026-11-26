# QUIC RFC requirement migration playbook

Use this playbook when reconciling generated or staged RFC requirements with an existing repository that already has stable requirement IDs and downstream trace.

## Goal

Preserve semantic identity and traceability while adopting a more complete RFC-derived requirement set. The migration is semantic, not a filename or identifier rewrite.

## Principles

1. Preserve the live requirement ID when the obligation is materially unchanged.
2. Assign a new requirement ID when the obligation changes.
3. Use new IDs for splits and merges, with lineage back to the source IDs.
4. Treat exact statement text as the primary mapping signal.
5. Treat title matches and proposed-ID hints as secondary signals.
6. Treat source provenance as supporting evidence, not a standalone join key.
7. Never reuse a retired requirement ID for a different obligation.
8. Rewrite dependent trace surfaces in the same change as an identity change.

The lineage model supports `derived_from`, `supersedes`, and retired-ID ledgers. See [SPEC-LIN](./specs/requirements/spec-trace/SPEC-LIN.json) and [SPEC-SCH](./specs/requirements/spec-trace/SPEC-SCH.json).

## Crosswalk classifications

Classify each live requirement against the staged set as one of:

- `preserve`: the obligation is unchanged; keep the live ID.
- `rename-with-lineage`: the staged clause replaces the live clause; record lineage and retire the old ID.
- `split-merge`: obligations split or combine; create appropriate new IDs and record every relationship.
- `new-only`: no credible live counterpart exists; create a new obligation.

The crosswalk records live and staged IDs, classification, statement and title confidence, split or merge notes, lineage direction, and dependent trace surfaces.

## Migration order

1. Build and review a machine-readable crosswalk before editing live requirements.
2. Pilot one bounded RFC or section with strong exact-statement overlap.
3. Process mixed-overlap RFCs only after the pilot validates the lineage rules.
4. Treat RFCs without credible overlap as explicit rebaselines.
5. Update requirement-home paths, test ownership markers, architecture, work-item, verification, and cross-RFC references with each identity change.
6. Validate after every slice.

## Execution rules

- Do not publish staged outputs as a direct replacement for live requirements.
- Do not use proposed-ID hints or source provenance as primary keys.
- Do not rewrite IDs before the crosswalk is approved.
- Do not let the retired-ID ledger lag behind an identity change.
- Do not use a migration summary as proof; canonical artifacts and repository validation remain authoritative.

## Validation gates

Each slice passes, in order:

1. structural JSON validation;
2. repository-wide SpecTrace validation;
3. downstream repository trace validation;
4. requirement-home and test-reference validation; and
5. the stricter traceable or auditable profile when that profile is claimed.

## Deliverables

For each migrated RFC or bounded slice, produce:

- the reviewed crosswalk;
- retired-ID ledger updates;
- updated canonical requirement JSON;
- updated downstream trace and ownership references; and
- a verification artifact that records reproducible validation evidence.

Point-in-time branch names, commits, test counts, and migration progress belong in the work item or pull request, not this playbook.
