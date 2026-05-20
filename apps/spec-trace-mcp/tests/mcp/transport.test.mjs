import assert from "node:assert/strict";
import test from "node:test";
import { callJsonRpc, callWorker } from "./_helpers.mjs";

test("GET /mcp renders the markdown-first docs index", async () => {
  const response = await callWorker("/mcp");
  assert.equal(response.status, 200);

  const html = await response.text();
  assert.match(html, /Static markdown, MCP delivery/i);
  assert.match(html, /search_docs/i);
  assert.match(html, /apps\/spec-trace-mcp\/content\//i);
  assert.match(html, /spec-trace:\/\/file\/\{path\}/i);
  assert.match(html, /SpecTrace Documentation/i);
  assert.match(html, /GET \/spec-trace\/mcp/i);
  assert.match(html, /href="\/spec-trace\/mcp\/resource\//i);
});

test("prefixed /spec-trace/mcp routes to the same MCP server and keeps links prefix-aware", async () => {
  const response = await callWorker("/spec-trace/mcp");
  assert.equal(response.status, 200);

  const html = await response.text();
  assert.match(html, /href="\/spec-trace\/mcp\/resource\//i);

  const initialize = await callJsonRpc(
    "initialize",
    {
      protocolVersion: "2024-11-05",
      clientInfo: { name: "test-client", version: "1.0.0" },
      capabilities: {},
    },
    { pathname: "/spec-trace/mcp" },
  );

  assert.equal(initialize.result.serverInfo.name, "spec-trace-docs");
  assert.equal(initialize.result.serverInfo.version, "0.1.0");
});

test("custom MCP_PATH_PREFIX override routes through the configured prefix", async () => {
  const env = { MCP_PATH_PREFIX: "/docs" };
  const response = await callWorker("/docs/mcp", { env });
  assert.equal(response.status, 200);

  const html = await response.text();
  assert.match(html, /href="\/docs\/mcp\/resource\//i);

  const initialize = await callJsonRpc(
    "initialize",
    {
      protocolVersion: "2024-11-05",
      clientInfo: { name: "test-client", version: "1.0.0" },
      capabilities: {},
    },
    { pathname: "/docs/mcp", env },
  );

  assert.equal(initialize.result.serverInfo.name, "spec-trace-docs");
  assert.equal(initialize.result.serverInfo.version, "0.1.0");
});

test("initialize returns stable MCP server metadata", async () => {
  const response = await callJsonRpc("initialize", {
    protocolVersion: "2024-11-05",
    clientInfo: { name: "test-client", version: "1.0.0" },
    capabilities: {},
  });

  assert.equal(response.jsonrpc, "2.0");
  assert.equal(response.id, 1);
  assert.equal(response.result.serverInfo.name, "spec-trace-docs");
  assert.equal(response.result.serverInfo.version, "0.1.0");
});

test("resources/list exposes markdown resources and a file template", async () => {
  const resources = await callJsonRpc("resources/list", {});
  const templates = await callJsonRpc("resources/templates/list", {});

  const uris = resources.result.resources.map((resource) => resource.uri);
  assert.ok(uris.includes("spec-trace://overview"));
  assert.ok(uris.includes("spec-trace://install"));
  assert.ok(uris.includes("spec-trace://guides/search"));
  assert.equal(templates.result.resourceTemplates.length, 1);
  assert.equal(templates.result.resourceTemplates[0].uriTemplate, "spec-trace://file/{path}");
});

test("resources/read returns canonical markdown and file-template content", async () => {
  const canonical = await callJsonRpc("resources/read", {
    uri: "spec-trace://overview",
  });

  assert.equal(canonical.result.contents.length, 1);
  assert.match(canonical.result.contents[0].text, /deterministic MCP server/i);

  const fileTemplate = await callJsonRpc("resources/read", {
    uri: "spec-trace://file/overview.md",
  });

  assert.equal(fileTemplate.result.contents.length, 1);
  assert.match(fileTemplate.result.contents[0].text, /deterministic MCP server/i);
});

test("tools/list exposes only search_docs", async () => {
  const tools = await callJsonRpc("tools/list", {});
  assert.deepEqual(
    tools.result.tools.map((tool) => tool.name),
    ["search_docs"],
  );
});

test("resource page renders browsable HTML for markdown source paths", async () => {
  const response = await callWorker(`/mcp/resource/${encodeURIComponent("spec-trace://file/overview.md")}`);
  assert.equal(response.status, 200);

  const html = await response.text();
  assert.match(html, /Overview/i);
  assert.match(html, /Source/i);
  assert.match(html, /href="\/spec-trace\/mcp"/i);
});
