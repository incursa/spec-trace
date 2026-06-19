You are auditing RFC source units against the current SpecTrace candidate set.

Goal:
- close coverage gaps
- recover missing testable invariants
- reduce over-fragmentation without dropping source units

Rules:
1. Produce one result for every input source unit, in the same order.
2. Treat current candidate decisions as advisory, not authoritative.
3. Use `accept` or `accept_with_edit` when the source unit clearly states an independently testable invariant, even if the extraction pass skipped it.
4. Use `split` when one source unit contains multiple independently testable obligations.
5. Use `merge` only when multiple adjacent source units clearly belong to one requirement; place the canonical requirement on the earliest unit in the span and reference the full span in `source_unit_ids`.
6. Use `skip` for explanatory, historical, motivational, appendix-only, or background text.
7. Use `gap` when behavior seems present but the context is insufficient to normalize safely.
8. Use `quarantine` when the text is suspicious, conflicting, or under-specified.
9. Always include `notes`. Use a non-empty note for every `skip`, `gap`, `quarantine`, `split`, or `merge` decision; use an empty array for `accept` and `accept_with_edit` when there is nothing else to add.
10. Always include `source_unit_ids`. For `accept`, `accept_with_edit`, `skip`, `gap`, and `quarantine`, use a single-element list containing `source_unit_id`. For `merge`, include the full span in order and start with `source_unit_id`.
11. Never omit a source unit or invent a `source_unit_id`.
12. Keep wording close to the RFC source, but tighten it only as much as needed to make the requirement testable.
13. If the extraction pass missed a clearly testable invariant, promote it here.

Output only a JSON object matching the provided schema.

```json
{
  "results": [
    {
      "source_unit_id": "RFC8999-S5P1-B5-P5-S1",
      "source_unit_ids": ["RFC8999-S5P1-B5-P5-S1"],
      "action": "accept",
      "requirements": [
        {
          "proposed_id_hint": "REQ-QUIC-RFC8999-S5P1-0001",
          "title": "Long header version field",
          "statement": "A long-header packet MUST include a 32-bit Version field in the next four bytes.",
          "coverage": {
            "positive": "required",
            "negative": "required",
            "edge": "optional",
            "fuzz": "deferred"
          },
          "upstream_refs": [
            "RFC 8999 §5.1 RFC8999-S5P1-B5-P5-S1",
            "https://www.rfc-editor.org/rfc/rfc8999.html#section-5.1"
          ],
          "notes": []
        }
      ],
      "notes": []
    }
  ]
}
```
