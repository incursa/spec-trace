# Generated Current-Status Rollup

This file is derived output, not canonical requirement text.

Source graph:

- [`SPEC-MATH-DIV`](../SPEC-MATH-DIV.md)
- [`ARC-MATH-DIV-0001`](../sample-architecture.md)
- [`WI-MATH-DIV-0001`](../sample-work-item.md)
- [`VER-MATH-DIV-0001`](../sample-verification.md)
- [`division-evidence.evidence.json`](./division-evidence.evidence.json)

Current rollup:

| Requirement | Derived status | Evidence |
| --- | --- | --- |
| `REQ-MATH-DIV-0001` | verified | `unit_test: passed`; `code_ref: observed` |
| `REQ-MATH-DIV-0002` | verified | `unit_test: passed`; `code_ref: observed` |
| `REQ-MATH-DIV-0003` | verified | `unit_test: passed`; `code_ref: observed` |

Canonical coverage dimensions:

| Requirement | Upstream | Design | Work | Verification |
| --- | --- | --- | --- | --- |
| `REQ-MATH-DIV-0001` | yes | yes | yes | yes |
| `REQ-MATH-DIV-0002` | yes | yes | yes | yes |
| `REQ-MATH-DIV-0003` | yes | yes | yes | yes |

Evidence-by-kind coverage:

| Requirement | unit_test | code_ref |
| --- | --- | --- |
| `REQ-MATH-DIV-0001` | passed | observed |
| `REQ-MATH-DIV-0002` | passed | observed |
| `REQ-MATH-DIV-0003` | passed | observed |

Interpretation:

- The table answers a current-status question over the canonical graph.
- The coverage tables separate canonical authored dimensions from generated evidence-by-kind dimensions.
- The table does not add new requirements, and it does not replace the requirement clauses, trace links, or verification artifact.
- A repository may generate similar rollups for passing, failing, stale, or mixed evidence states according to local policy.
