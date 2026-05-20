import assert from "node:assert/strict";
import test from "node:test";
import { callJsonRpc } from "./_helpers.mjs";

test("search_docs ranks an exact title match first", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_docs",
    arguments: {
      query: "overview",
      kind: "guide",
      include_examples: true,
      max_results: 5,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://overview");
});

test("search_docs respects kind filtering", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_docs",
    arguments: {
      query: "resource card",
      kind: "component",
      include_examples: true,
      max_results: 5,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://components/button");
});

test("search_docs can exclude examples from the result set", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_docs",
    arguments: {
      query: "reference",
      kind: "any",
      include_examples: false,
      max_results: 10,
    },
  });

  assert.equal(
    response.result.structuredContent.results.some((item) => item.uri.startsWith("spec-trace://examples/")),
    false,
  );
});

test("search_docs returns prioritized results for an empty query", async () => {
  const response = await callJsonRpc("tools/call", {
    name: "search_docs",
    arguments: {
      query: "",
      kind: "any",
      include_examples: true,
      max_results: 3,
    },
  });

  assert.equal(response.result.structuredContent.results[0].uri, "spec-trace://overview");
});
