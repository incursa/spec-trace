# SpecTrace MCP Server

Deterministic Cloudflare Worker MCP server for the `spec-trace` reference repository.

The generated MCP surface is built from the real repository authority chain, not a copied docs folder:

1. canonical SPEC artifacts under `specs/requirements/spec-trace/`
2. `model/model.schema.json`, root JSON templates, and compatibility schema files
3. worked examples, root guidance, AI guidance, and curated tool/readiness files

`mcp.config.json` centralizes the display name, server name, URI namespace, and package identifier. `wrangler.toml` sets the public path prefix through `MCP_PATH_PREFIX`, which defaults to `/spec-trace`.

The Worker serves:

- `GET /mcp` for the human-readable resource index
- `POST /mcp` for MCP JSON-RPC traffic
- `GET /mcp/resource/<uri>` for browsable resource pages
- resource templates such as `spec-trace://specs/{artifact_id}`, `spec-trace://requirements/{requirement_id}`, and `spec-trace://files/{path}`

The dynamic MCP tools are:

- `search_spec_trace`: search canonical specs, requirements, schema, templates, examples, AI guidance, and curated docs
- `get_requirement`: return a canonical requirement by `REQ-...` id
- `get_artifact`: return a canonical artifact by `SPEC-...`, `ARC-...`, `WI-...`, or `VER-...` id when present in the generated catalog
- `get_guidance`: return full authoring guidance by topic, including `document-to-requirements`, `rfc-to-requirements`, and `requirement-slicing`

## Local Development

Run from the repository root:

```bash
npm ci --prefix apps/spec-trace-mcp
npm run build:mcp
npm test
npm run dev:mcp
```

`npm run dev:mcp` starts the Worker locally through Wrangler. Open the URL it prints and use:

- `GET /mcp` to browse the generated index
- `GET /mcp/resource/<uri>` to inspect a resource page
- `POST /mcp` from an MCP client

When you want to mirror the deployed path prefix, use `/spec-trace/mcp`.

## Build Pipeline

The build stays deterministic:

1. read curated repository files from the `spec-trace` repo root
2. compile `dist/mcp/manifest.json`, `dist/mcp/resources.json`, and `dist/mcp/search-index.json`
3. emit grouped JSON indexes for specs, requirements, schemas, templates, examples, AI guidance, and files
4. bundle the Worker into `dist/mcp/worker.mjs`

Generated files under `dist/mcp/` are build output only.

## Tests

The package test rebuilds the Worker and checks:

- index rendering
- prefixed routing through `MCP_PATH_PREFIX`
- `initialize`
- resource listing
- resource templates
- resource reads
- `search_spec_trace`
- `get_requirement`
- `get_artifact`
- `get_guidance`

## Cloudflare Deployment

```bash
npm run deploy:mcp
```

The repo-root MCP workflow expects:

- `CLOUDFLARE_API_TOKEN`
- `CLOUDFLARE_ACCOUNT_ID`

`MCP_PATH_PREFIX` in `wrangler.toml` should match the public load-balancer or Astro-site path. Without a route binding, the direct Worker endpoint is:

```text
https://<your-worker-host>/mcp
```

If you serve the Worker behind the configured prefix, the public endpoint is:

```text
https://<your-worker-host>/spec-trace/mcp
```

The `wrangler.toml` file intentionally does not declare a custom domain yet. Add Cloudflare routes during deployment setup once the public host/path decision is final.
