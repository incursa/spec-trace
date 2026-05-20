# SpecTrace MCP Docs

Deterministic Cloudflare Worker documentation server for the `spec-trace` repository.

The source of truth is static markdown under [`content/`](./content/). [`mcp.config.json`](./mcp.config.json) centralizes the display name, server name, URI namespace, and package identifier. [`wrangler.toml`](./wrangler.toml) sets the public path prefix through `MCP_PATH_PREFIX`, which defaults to `/spec-trace`. A build step reads front matter from those files, compiles a manifest and search index into [`dist/mcp/`](./dist/mcp/), and bundles a small Worker that serves:

- `GET /mcp` for the human-readable docs index
- `POST /mcp` for MCP JSON-RPC traffic
- `GET /mcp/resource/<uri>` for browsable resource pages
- `spec-trace://file/{path}` as the markdown source-path template

The only dynamic tool in v1 is `search_docs`.

## Local Development

Run the commands from the repository root:

```bash
npm ci --prefix apps/spec-trace-mcp
npm run build:mcp
npm test
npm run dev:mcp
```

`npm run dev:mcp` starts the Worker locally through Wrangler. Open the URL it prints and use:

- `GET /mcp` to browse the generated docs index
- `GET /mcp/resource/<uri>` to inspect a specific resource page
- `POST /mcp` from an MCP client

When you want to mirror the deployed path prefix, use the configured public path such as `/spec-trace/mcp`.

## Markdown Authoring

Each documentation file is a markdown file with JSON front matter.

Example:

```md
---
{
  "uri": "spec-trace://overview",
  "slug": "overview",
  "title": "Overview",
  "summary": "What the server does and how to extend it.",
  "kind": "guide",
  "group": "core",
  "aliases": ["home", "intro"],
  "relatedUris": ["spec-trace://install", "spec-trace://fast-path"],
  "tags": ["overview", "getting-started"],
  "priority": 120,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Overview

The body contains the actual documentation.
```

Supported front matter fields:

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

Build-time validation fails on duplicate URIs, duplicate slugs within a group, unsupported kinds, and broken related-resource references.

## Build Pipeline

The build stays deterministic:

1. Read markdown files from [`content/`](./content/)
2. Parse front matter and validate the file graph
3. Compile `dist/mcp/manifest.json`, `dist/mcp/resources.json`, and `dist/mcp/search-index.json`
4. Bundle the Worker into `dist/mcp/worker.mjs`

Generated files under `dist/mcp/` are build output only.

## Tests

The test suite rebuilds the Worker and checks:

- docs index rendering
- `initialize`
- resource listing
- resource templates
- resource reads
- `search_docs`

## Cloudflare Deployment

```bash
npm run deploy:mcp
```

The deployment workflow expects:

- `CLOUDFLARE_API_TOKEN`
- `CLOUDFLARE_ACCOUNT_ID`
- `MCP_PATH_PREFIX` in [`wrangler.toml`](./wrangler.toml) should match the public load-balancer path

The Worker endpoint is:

```text
https://<your-worker-host>/mcp
```

If you are serving the Worker behind the configured prefix, the public endpoint becomes `https://<your-worker-host>/spec-trace/mcp`.

## Adapting This Project

To reuse this for another project:

1. Update the package name in [`package.json`](./package.json).
2. Replace the markdown files in [`content/`](./content/) with your own docs.
3. Change the branding fields in [`mcp.config.json`](./mcp.config.json).
4. Adjust `MCP_PATH_PREFIX` in [`wrangler.toml`](./wrangler.toml) if the public load-balancer path changes.
5. Adjust front matter `uri` values if you want a different URI namespace.
6. Re-run `npm run build:mcp`.
7. Deploy the Worker.

Keep the runtime simple:

- no runtime crawling
- no LLM calls
- no database unless you truly need one
- no dynamic content generation beyond search
