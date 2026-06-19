Review RFC-derived SpecTrace candidate requirements against the source-unit ledger.

For each candidate, decide one action:

- `accept`
- `accept_with_edit`
- `split`
- `merge`
- `skip`
- `gap`
- `quarantine`

Accepted decisions must carry the final requirement payload that should be assembled into canonical SpecTrace JSON. Gap and quarantine decisions should include reviewer notes that explain the unresolved issue.

Use this rough rubric:

- `accept`: the candidate is an independently testable invariant and the wording is already tight enough.
- `accept_with_edit`: the candidate is testable, but the wording needs tightening or a small trace correction.
- `split`: the candidate still combines multiple independently testable obligations.
- `merge`: the candidate is too narrow or too fragmentary and should be combined with adjacent requirement material before canonical assembly.
- `skip`: the candidate is explanatory, historical, motivational, or otherwise not materially normative.
- `gap`: the source unit probably contains behavior, but the available context is not enough to normalize it safely.
- `quarantine`: the candidate looks suspicious or under-specified, and should not be canonicalized until a reviewer resolves the issue.
