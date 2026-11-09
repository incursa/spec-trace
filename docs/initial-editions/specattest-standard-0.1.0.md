# SpecAttest Standard 0.1.0 publication preparation

## Edition metadata

- Title: SpecAttest Standard
- Edition: `0.1.0`
- Maturity: Initial Public Draft
- Versioning policy: Semantic Versioning 2.0.0
- Publisher: Incursa
- Canonical source: `incursa/spec-trace`
- Preserved source base revision: `37a1f73237147427c689c28a75b8fe024a2d0f47`
- Distribution state: prepared, private, and not publicly activated

The edition contains 11 existing specification artifacts and 200 existing requirements. Artifact identifiers, requirement identifiers, document order, repository paths, namespaces, package identifiers, schema identifiers, and MCP tool names are preserved.

## Intentional rename changes

The human-facing standard name, canonical-suite landing page, owner metadata, and standard tag were changed from SpecTrace naming to SpecAttest naming. The `spec-trace` domain value is preserved because repository validation binds it to the unchanged technical directory. Three normative statements received name-only clarification:

- `REQ-STD-0003` now names the SpecAttest Standard while explicitly retaining `specs/requirements/spec-trace/` as the existing technical directory.
- `REQ-LAY-0008` now names the SpecAttest Standard while explicitly retaining the same technical directory.
- `REQ-EVD-0015` now refers to SpecAttest reporting.

No stable artifact or requirement identifier changed. These changes do not rename a repository, directory, namespace, package, schema, or MCP tool.

## Deferred refinement backlog

The initial edition does not attempt a comprehensive rewrite. Future work remains separate:

- additional normative clarification and terminology consistency;
- conformance rules and examples;
- full edition comparisons and change summaries;
- normative versus editorial change classification;
- suggested semantic-version increments;
- publication preview and approval workflow improvements; and
- historical edition browsing.

These items do not block the initial draft unless later review identifies a correctness, integrity, security, or disclosure defect.
