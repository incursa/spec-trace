import assert from "node:assert/strict";
import test from "node:test";
import { callJsonRpc } from "./_helpers.mjs";

test("search_spec_trace ranks an exact artifact id match first", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_spec_trace",
    arguments: {
      query: "SPEC-STD",
      kind: "spec",
      include_examples: true,
      max_results: 5,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://specs/SPEC-STD");
});

test("search_spec_trace can filter to canonical requirements", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_spec_trace",
    arguments: {
      query: "stable identifier",
      kind: "requirement",
      include_examples: true,
      max_results: 5,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://requirements/REQ-STD-0014");
});

test("search_spec_trace discovers document conversion guidance", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_spec_trace",
    arguments: {
      query: "RFC normative words section numbers",
      kind: "guide",
      include_examples: true,
      max_results: 5,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://guides/rfc-to-requirements");
});

test("search_spec_trace can exclude examples and raw files from results", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_spec_trace",
    arguments: {
      query: "payments",
      kind: "any",
      include_examples: false,
      include_files: false,
      max_results: 20,
    },
  });

  assert.equal(
    response.result.structuredContent.results.some((item) => item.uri.startsWith("spec-trace://examples/")),
    false,
  );
  assert.equal(
    response.result.structuredContent.results.some((item) => item.uri.startsWith("spec-trace://files/")),
    false,
  );
});

test("search_spec_trace returns prioritized results for an empty query", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_spec_trace",
    arguments: {
      query: "",
      kind: "any",
      include_examples: true,
      max_results: 3,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://overview");
});

test("get_requirement returns canonical requirement content", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "get_requirement",
    arguments: {
      requirement_id: "REQ-STD-0001",
    },
  });

  assert.equal(response.result.structuredContent.requirement_id, "REQ-STD-0001");
  assert.equal(response.result.structuredContent.artifact_id, "SPEC-STD");
  assert.match(response.result.structuredContent.statement, /MUST group one or more related requirements/i);
});

test("get_artifact returns a spec artifact and its nested requirements", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "get_artifact",
    arguments: {
      artifact_id: "SPEC-STD",
      include_requirements: true,
    },
  });

  assert.equal(response.result.structuredContent.artifact_id, "SPEC-STD");
  assert.equal(response.result.structuredContent.uri, "spec-trace://specs/SPEC-STD");
  assert.ok(response.result.structuredContent.requirements.some((item) => item.requirement_id === "REQ-STD-0001"));
});

test("get_guidance returns RFC-specific document processing guidance", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "get_guidance",
    arguments: {
      topic: "rfc-to-requirements",
    },
  });

  assert.equal(response.result.structuredContent.uri, "spec-trace://guides/rfc-to-requirements");
  assert.match(response.result.structuredContent.body, /Split each section into paragraphs, then sentences/i);
  assert.match(response.result.structuredContent.body, /REQ-RFC9000-S10P2-0001/i);
});
