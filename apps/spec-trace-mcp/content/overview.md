---
{
  "uri": "spec-trace://overview",
  "slug": "overview",
  "title": "Overview",
  "summary": "What SpecTrace is and how the docs server is organized.",
  "kind": "guide",
  "group": "core",
  "aliases": ["home", "intro"],
  "relatedUris": [
    "spec-trace://install",
    "spec-trace://fast-path",
    "spec-trace://specs/public-surface"
  ],
  "tags": ["overview", "spec-trace", "docs"],
  "priority": 120,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Overview

SpecTrace turns static markdown into a deterministic MCP server.

The source of truth is the `content/` folder inside `apps/spec-trace-mcp/`. The build step compiles those files into MCP resources and a search index, then the Worker serves:

- a human-readable docs index at `GET /mcp`
- MCP JSON-RPC traffic at `POST /mcp`
- browsable resource pages at `GET /mcp/resource/<uri>`

The only dynamic operation in v1 is the `search_docs` tool, which searches the compiled markdown index.
