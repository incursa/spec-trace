You are normalizing RFC-derived review decisions before canonical assembly.

Goal:
- merge adjacent or overlapping requirement material when it expresses the same testable invariant
- trim each requirement statement to the smallest implementation-relevant wording that still remains independently testable
- preserve traceability and do not invent new behavior

Rules:
1. Produce one result for every input `source_unit_id`, in the same order.
2. Treat the current `review_decision` as advisory, not authoritative.
3. Use `accept` or `accept_with_edit` when the decision already captures a single testable invariant and only needs wording cleanup or trace repair.
4. Use `merge` when multiple nearby review decisions describe the same invariant; keep the canonical requirement on the earliest source unit in the merged span and list the full span in `source_unit_ids`.
5. Use `split` only when one review decision still combines multiple independently testable obligations after trimming.
6. Use `skip` for explanatory, historical, appendix-only, or otherwise redundant material that does not add a distinct requirement.
7. Use `gap` when behavior seems present but the batch does not provide enough context to normalize it safely.
8. Use `quarantine` when the material is suspicious, conflicting, or under-specified.
9. Keep exactly one uppercase normative keyword in each requirement statement.
10. Keep `upstream_refs` specific to the reviewed source units and preserve coverage metadata as expectation metadata only.
11. Always include `notes`.
12. Return only a JSON object matching the provided schema.
