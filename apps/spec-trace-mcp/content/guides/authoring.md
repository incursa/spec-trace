---
{
  "uri": "spec-trace://guides/authoring",
  "slug": "authoring",
  "title": "Authoring guide",
  "summary": "How to write new MCP content files.",
  "kind": "guide",
  "group": "guides",
  "aliases": ["authoring", "content-model"],
  "relatedUris": [
    "spec-trace://overview",
    "spec-trace://guides/search"
  ],
  "tags": ["authoring", "front-matter", "markdown"],
  "priority": 95,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Authoring guide

Use one markdown file per topic.

Front matter should carry the metadata the MCP server needs:

- `uri`
- `slug`
- `title`
- `summary`
- `kind`
- `group`
- `aliases`
- `relatedUris`
- `tags`
- `priority`
- `includeInSearch`
- `searchKind`

The body should hold the actual documentation.

The build validates duplicate URIs, duplicate slugs within a group, unsupported kinds, and broken related-resource references before it writes `apps/spec-trace-mcp/dist/mcp/*.json`.
