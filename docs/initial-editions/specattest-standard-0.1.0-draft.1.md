# SpecAttest Standard 0.1.0-draft.1 publication preparation

## Edition metadata

- Title: SpecAttest Standard
- Edition: `0.1.0-draft.1`
- Maturity: Draft
- Versioning policy: Semantic Versioning 2.0.0
- Publisher: Incursa
- Canonical source: `incursa/spec-trace`
- Preserved source base revision: `37a1f73237147427c689c28a75b8fe024a2d0f47`
- Distribution state: prepared, private, and not publicly activated
- Governance: Incursa owner approved
- Bootstrap: yes

The prepared edition contains 12 specification artifacts and 228 requirements. `SPEC-VER` adds the normative revision, edition, maturity, compatibility, and bootstrap contract. Existing artifact identifiers, requirement identifiers, document order, repository paths, namespaces, package identifiers, schema identifiers, and MCP tool names are preserved. Existing requirements begin at revision 1 because this is the first revision-aware edition.

This file is a preparation record, not the immutable edition manifest. Publication remains blocked until the exact source commit and artifact bundle are frozen; independent validator results, corpus and payload hashes, approval, and a signed tag or release manifest are recorded; and the bootstrap checklist is complete.

## Intentional rename changes

The human-facing standard name, canonical-suite landing page, owner metadata, and standard tag were changed from SpecTrace naming to SpecAttest naming. The `spec-trace` domain value is preserved because repository validation binds it to the unchanged technical directory. Three normative statements received name-only clarification:

- `REQ-STD-0003` now names the SpecAttest Standard while explicitly retaining `specs/requirements/spec-trace/` as the existing technical directory.
- `REQ-LAY-0008` now names the SpecAttest Standard while explicitly retaining the same technical directory.
- `REQ-EVD-0015` now refers to SpecAttest reporting.

No stable artifact or requirement identifier changed. These changes do not rename a repository, directory, namespace, package, schema, or MCP tool.

## Versioning refinement completed in this draft

- requirement records now carry positive integer revisions;
- `SPEC-VER` defines immutable editions and their compatibility semantics;
- publication maturity is `draft`, `candidate`, or `stable`;
- `active`, `superseded`, and `withdrawn` remain mutable disposition outside the edition digest;
- `schemas/standard-edition.schema.json` defines the frozen edition manifest; and
- bootstrap and successor cross-validation requirements are normative.

## Remaining publication work

The initial edition does not attempt a comprehensive rewrite. Future work remains separate:

- additional normative clarification and terminology consistency;
- conformance rules and examples;
- publication preview and approval workflow improvements; and
- historical edition browsing.

These items do not block the initial draft unless later review identifies a correctness, integrity, security, or disclosure defect.
