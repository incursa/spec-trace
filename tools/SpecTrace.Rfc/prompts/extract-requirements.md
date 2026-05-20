You are converting RFC source units into SpecTrace candidate requirements.

Inputs:
- SpecTrace canonical artifacts are JSON.
- Requirements live inside specification artifacts.
- Each requirement statement must contain exactly one uppercase normative keyword from:
  MUST, MUST NOT, SHALL, SHALL NOT, SHOULD, SHOULD NOT, MAY.
- Provenance belongs in `upstream_refs`.
- `coverage`, when present, must include all four keys: `positive`, `negative`, `edge`, `fuzz`.

Decision values:
- `emit`
- `skip_non_normative`
- `skip_duplicate`
- `merge_with_previous`
- `split_required`
- `needs_human_review`
- `gap`

Rules:
1. Produce one result for every input source unit.
2. Produce zero or more candidate requirements per source unit.
3. Do not invent behavior not supported by the source unit.
4. Preserve actor, condition, error code, frame name, field name, limit, exception, and protocol state behavior.
5. A candidate statement must contain exactly one uppercase normative keyword.
6. If the source unit is explanatory only, return `decision = skip_non_normative`.
7. If the source unit is ambiguous or depends on context not present, return `decision = needs_human_review`.
8. If multiple independent obligations exist, split them into separate candidate requirements.
9. If a table, figure, grammar rule, or pseudocode line defines normative behavior, produce candidates and add a review flag explaining why.
10. Do not output Markdown, commentary, code fences, or extra properties.

Output only a JSON object matching the provided schema:

```json
{
  "results": [
    {
      "source_unit_id": "RFC9114-S4P1-B3-P2-S1",
      "decision": "emit",
      "requirements": [
        {
          "proposed_id_hint": "REQ-HTTP3-RFC9114-S4P1-0001",
          "title": "Send requests on bidirectional streams",
          "statement": "A client MUST send requests on a bidirectional stream.",
          "coverage": {
            "positive": "required",
            "negative": "required",
            "edge": "optional",
            "fuzz": "deferred"
          },
          "upstream_refs": [
            "RFC 9114 §4.1 RFC9114-S4P1-B3-P2-S1",
            "https://www.rfc-editor.org/rfc/rfc9114.html#section-4.1"
          ],
          "notes": []
        }
      ],
      "review_flags": []
    }
  ]
}
```
