---
{
  "uri": "spec-trace://guides/search",
  "slug": "search",
  "title": "Search",
  "summary": "Search is the only dynamic MCP tool in the SpecTrace docs server.",
  "kind": "guide",
  "group": "guides",
  "aliases": ["full-text-search", "find-docs"],
  "relatedUris": [
    "spec-trace://overview",
    "spec-trace://specs/public-surface"
  ],
  "tags": ["search", "ranking", "filters"],
  "priority": 92,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Search

Search is built from the static content index.

It scans:

- titles
- summaries
- aliases
- tags
- the full markdown body
- source paths
- URIs

That means the server can stay deterministic while still helping the model avoid blind guessing.
