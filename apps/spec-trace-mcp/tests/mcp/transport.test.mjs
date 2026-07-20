import assert from "node:assert/strict";
import test from "node:test";
import { callJsonRpc, callWorker, loadWorker } from "./_helpers.mjs";

test("GET /mcp renders the SpecTrace MCP index", async () => {
  const response = await callWorker("/mcp");
  assert.equal(response.status, 200);

  const html = await response.text();
  assert.match(html, /SpecTrace standard, MCP delivery/i);
  assert.match(html, /class="brand-lockup" aria-label="SpecTrace"/i);
  assert.match(html, /search_spec_trace/i);
  assert.match(html, /get_requirement/i);
  assert.match(html, /get_guidance/i);
  assert.match(html, /spec-trace:\/\/files\/\{path\}/i);
  assert.match(html, /SpecTrace Documentation/i);
  assert.match(html, /GET \/spec-trace\/mcp/i);
  assert.match(html, /href="\/spec-trace\/mcp\/resource\//i);
  assert.match(html, /SPEC-STD: Core Standard Model/i);
});

test("worker module exposes both named and default fetch handlers", async () => {
  const worker = await loadWorker();
  assert.equal(typeof worker.fetch, "function");
  assert.equal(typeof worker.default.fetch, "function");
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

test("resources/list exposes canonical resources and resource templates", async () => {
  const resources = await callJsonRpc("resources/list", {});
  const templates = await callJsonRpc("resources/templates/list", {});

  const uris = resources.result.resources.map((resource) => resource.uri);
  assert.ok(uris.includes("spec-trace://overview"));
  assert.ok(uris.includes("spec-trace://specs/SPEC-STD"));
  assert.ok(uris.includes("spec-trace://requirements/REQ-STD-0001"));
  assert.ok(uris.includes("spec-trace://schema/model"));

  const templateUris = templates.result.resourceTemplates.map((template) => template.uriTemplate);
  assert.ok(templateUris.includes("spec-trace://specs/{artifact_id}"));
  assert.ok(templateUris.includes("spec-trace://requirements/{requirement_id}"));
  assert.ok(templateUris.includes("spec-trace://files/{path}"));
});

test("resources/read returns canonical specs, requirements, and file-template content", async () => {
  const spec = await callJsonRpc("resources/read", {
    uri: "spec-trace://specs/SPEC-STD",
  });

  assert.equal(spec.result.contents.length, 1);
  assert.match(spec.result.contents[0].text, /Core Standard Model and Publication Rules/i);

  const requirement = await callJsonRpc("resources/read", {
    uri: "spec-trace://requirements/REQ-STD-0001",
  });

  assert.equal(requirement.result.contents.length, 1);
  assert.match(requirement.result.contents[0].text, /A specification MUST group/i);

  const fileTemplate = await callJsonRpc("resources/read", {
    uri: "spec-trace://files/README.md",
  });

  assert.equal(fileTemplate.result.contents.length, 1);
  assert.match(fileTemplate.result.contents[0].text, /SpecTrace is a small, JSON-first standard/i);
});

test("tools/list exposes SpecTrace search and lookup tools", async () => {
  const tools = await callJsonRpc("tools/list", {});
  assert.deepEqual(
    tools.result.tools.map((tool) => tool.name),
    ["search_spec_trace", "get_requirement", "get_artifact", "get_guidance"],
  );
});

test("resource page renders browsable HTML for canonical resources", async () => {
  const response = await callWorker(`/mcp/resource/${encodeURIComponent("spec-trace://specs/SPEC-STD")}`);
  assert.equal(response.status, 200);

  const html = await response.text();
  assert.match(html, /Core Standard Model and Publication Rules/i);
  assert.match(html, /Source/i);
  assert.match(html, /href="\/spec-trace\/mcp"/i);
});
