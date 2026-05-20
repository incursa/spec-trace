Normalize one RFC-derived candidate requirement before canonical assembly.

Rules:
1. Keep exactly one uppercase normative keyword in the statement.
2. Preserve the source behavior without broadening support claims.
3. Keep actor, condition, field names, numeric limits, protocol state, and error behavior concrete.
4. Keep provenance in `upstream_refs`.
5. Keep coverage expectations as expectation metadata only; do not treat them as evidence.
6. Return only the normalized candidate requirement JSON.
