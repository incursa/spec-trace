# Generated Coverage-Dimensions Rollup

This file is derived output, not canonical requirement text.

Source graph:

- [`SPEC-MATH-DIV`](../SPEC-MATH-DIV.md)
- [`ARC-MATH-DIV-0001`](../sample-architecture.md)
- [`WI-MATH-DIV-0001`](../sample-work-item.md)
- [`VER-MATH-DIV-0001`](../sample-verification.md)
- [`division-evidence.evidence.json`](./division-evidence.evidence.json)

Canonical coverage dimensions:

| Requirement | Upstream Refs | Satisfied By | Implemented By | Verified By |
| --- | --- | --- | --- | --- |
| `REQ-MATH-DIV-0001` | yes | yes | yes | yes |
| `REQ-MATH-DIV-0002` | no | yes | yes | yes |
| `REQ-MATH-DIV-0003` | no | yes | yes | yes |

Evidence-by-kind dimensions:

| Requirement | unit_test | code_ref |
| --- | --- | --- |
| `REQ-MATH-DIV-0001` | passed | observed |
| `REQ-MATH-DIV-0002` | passed | observed |
| `REQ-MATH-DIV-0003` | passed | observed |

Interpretation:

- The table shows coverage by dimension rather than collapsing everything into one gate result.
- Canonical coverage and evidence coverage stay separate so the report does not
  overload the authored trace model.
- Partial coverage in one dimension does not create a new canonical profile by itself.
- A repository may add local dimensions such as freshness or release readiness without changing the core artifact families.
