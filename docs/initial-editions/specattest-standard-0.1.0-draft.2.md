# SpecAttest Standard 0.1.0-draft.2 publication preparation

## Edition metadata

- Edition: `0.1.0-draft.2`
- Maturity: Draft
- Previous edition: `0.1.0-draft.1`
- Compatibility: Breaking during `0.x`
- Governance: Incursa owner approved
- Bootstrap: no
- Behavioral review input: `VER-WB-0012-specattest-draft-qualification`

This successor draft closes ambiguities discovered when Workbench and Manager were qualified against draft 1. It is not a release candidate and makes no compatibility promise.

## Normative changes

- `REQ-VER-0002` revision 2 preserves imported normative revisions and forbids deriving them from provider concurrency versions.
- `REQ-VER-0014` revision 2 distinguishes model-schema and edition-manifest-schema hashes.
- New requirements separate stable identity and normative revision from provider record, project, and operation versions.
- Safe-mutation replay excludes server-generated defaults from its logical request digest.
- Complete edition-pin verification fails closed on partial tuples, mutable references, and substituted objects.
- The authoritative interchange schema and corpus cover revision round trips and safe-mutation agreement across REST and MCP.

The immutable manifest can only be built after the source commit is frozen. Run the repository validation, retain accepted and candidate validator reports, then use `scripts/Build-EditionManifest.py` with the approval reference and signed tag-object digest. Do not add the resulting manifest back to the source commit it identifies.
