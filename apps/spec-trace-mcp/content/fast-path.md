---
{
  "uri": "spec-trace://fast-path",
  "slug": "fast-path",
  "title": "Fast path",
  "summary": "The shortest path from docs to a usable MCP server.",
  "kind": "guide",
  "group": "core",
  "aliases": ["decision-tree", "short-path"],
  "relatedUris": [
    "spec-trace://guides/authoring",
    "spec-trace://guides/search"
  ],
  "tags": ["quickstart", "workflow"],
  "priority": 110,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Fast path

If you already have SpecTrace documentation, the simplest path is:

1. Put the content in markdown files.
2. Add front matter for title, summary, and URI.
3. Run the build.
4. Deploy the Worker.

The server only needs to know how to list, read, and search those files.
