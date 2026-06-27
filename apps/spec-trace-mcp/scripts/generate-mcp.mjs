import { mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const appRoot = path.resolve(scriptDir, "..");
const repoRoot = path.resolve(appRoot, "..", "..");
const outDir = path.join(appRoot, "dist", "mcp");
const packageJson = JSON.parse(await readFile(path.join(appRoot, "package.json"), "utf8"));
const mcpConfig = JSON.parse(await readFile(path.join(appRoot, "mcp.config.json"), "utf8"));

const namespace = mcpConfig.namespace ?? "spec-trace";
const packageName = mcpConfig.packageName ?? packageJson.name ?? "@incursa/spec-trace-mcp";
const packageVersion = packageJson.version ?? "0.0.0";
const serverName = mcpConfig.serverName ?? "spec-trace-docs";
const displayName = mcpConfig.displayName ?? "SpecTrace MCP";
const serverSummary =
  mcpConfig.summary ?? "A deterministic Cloudflare Worker MCP server generated from the SpecTrace reference repository.";

if (packageJson.name && packageJson.name !== packageName) {
  throw new Error(`package.json name "${packageJson.name}" must match mcp.config.json packageName "${packageName}"`);
}

const groupOrder = ["core", "guides", "specs", "requirements", "schema", "templates", "examples", "ai", "files"];
const allowedKinds = new Set(["guide", "spec", "requirement", "schema", "template", "example", "ai", "file"]);

function normalizeText(value) {
  return String(value ?? "")
    .replace(/\r\n/g, "\n")
    .replace(/\u00a0/g, " ")
    .replace(/[ \t]+\n/g, "\n")
    .replace(/\n{3,}/g, "\n\n")
    .replace(/[ \t]{2,}/g, " ")
    .trim();
}

function normalizeForSearch(value) {
  return normalizeText(value).toLowerCase();
}

function slugify(value) {
  return normalizeForSearch(value)
    .replace(/['"]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

function toPosix(filePath) {
  return filePath.split(path.sep).join("/");
}

function repoRelative(filePath) {
  return toPosix(path.relative(repoRoot, filePath));
}

async function readRepoText(relativePath) {
  return readFile(path.join(repoRoot, relativePath), "utf8");
}

async function readAppText(relativePath) {
  return readFile(path.join(appRoot, relativePath), "utf8");
}

function markdownToText(markdown) {
  const lines = String(markdown ?? "").replace(/\r\n/g, "\n").split("\n");
  const output = [];
  let inCode = false;

  for (const line of lines) {
    const trimmed = line.trimEnd();
    if (/^\s*```/.test(trimmed)) {
      inCode = !inCode;
      output.push(trimmed);
      continue;
    }

    if (!inCode) {
      if (/^#{1,6}\s+/.test(trimmed)) {
        output.push(trimmed.replace(/^#{1,6}\s+/, ""));
        continue;
      }

      if (/^\s*[-*]\s+/.test(trimmed)) {
        output.push(trimmed.replace(/^\s*[-*]\s+/, "- "));
        continue;
      }
    }

    output.push(trimmed);
  }

  return normalizeText(
    output
      .join("\n")
      .replace(/\[(.*?)\]\((.*?)\)/g, "$1")
      .replace(/`([^`]+)`/g, "$1"),
  );
}

function splitSection(markdown, sectionHeading) {
  const lines = String(markdown ?? "").replace(/\r\n/g, "\n").split("\n");
  const escaped = sectionHeading.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const headingPattern = new RegExp(`^#{1,6}\\s+${escaped}\\s*$`, "i");
  let start = -1;

  for (let index = 0; index < lines.length; index += 1) {
    if (headingPattern.test(lines[index].trim())) {
      start = index + 1;
      break;
    }
  }

  if (start < 0) {
    return "";
  }

  const body = [];
  for (let index = start; index < lines.length; index += 1) {
    const line = lines[index];
    if (/^#{1,2}\s+/.test(line) && body.length > 0) {
      break;
    }
    body.push(line);
  }

  return normalizeText(body.join("\n"));
}

async function walkFiles(dir, predicate, files = []) {
  const entries = await readdir(dir, { withFileTypes: true });
  for (const entry of entries) {
    const absolute = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      await walkFiles(absolute, predicate, files);
    } else if (entry.isFile() && predicate(absolute)) {
      files.push(absolute);
    }
  }
  return files;
}

function buildSearchText(resource) {
  return normalizeForSearch(
    [
      resource.uri,
      resource.title,
      resource.kind,
      resource.searchKind,
      resource.group,
      resource.summary,
      resource.artifactId,
      resource.requirementId,
      resource.artifactType,
      resource.domain,
      resource.capability,
      ...(resource.aliases ?? []),
      ...(resource.tags ?? []),
      ...(resource.relatedUris ?? []),
      ...(resource.sourcePaths ?? []),
      resource.body,
    ]
      .filter(Boolean)
      .join("\n"),
  );
}

function createResourceRecord({
  uri,
  title,
  kind,
  searchKind = kind,
  summary,
  body,
  sourcePaths,
  group,
  priority = 50,
  mimeType = "text/markdown; charset=utf-8",
  aliases = [],
  relatedUris = [],
  tags = [],
  includeInSearch = true,
  artifactId,
  requirementId,
  artifactType,
  domain,
  capability,
}) {
  if (!uri.startsWith(`${namespace}://`)) {
    throw new Error(`Resource URI "${uri}" must use ${namespace}://`);
  }

  if (!allowedKinds.has(kind)) {
    throw new Error(`Unsupported MCP resource kind "${kind}" for ${uri}`);
  }

  const record = {
    uri,
    title: normalizeText(title),
    kind,
    searchKind,
    summary: normalizeText(summary),
    body: normalizeText(body),
    sourcePaths: sourcePaths.map(toPosix),
    mimeType,
    aliases,
    relatedUris,
    group,
    priority,
    includeInSearch,
    tags,
  };

  if (artifactId) record.artifactId = artifactId;
  if (requirementId) record.requirementId = requirementId;
  if (artifactType) record.artifactType = artifactType;
  if (domain) record.domain = domain;
  if (capability) record.capability = capability;

  record.searchText = buildSearchText(record);
  return record;
}

function jsonBody(value) {
  return JSON.stringify(value, null, 2);
}

function artifactBody(artifact) {
  const requirementLines = (artifact.requirements ?? []).map((requirement) => `- ${requirement.id}: ${requirement.title}`).join("\n");
  return normalizeText(
    [
      `${artifact.artifact_id}: ${artifact.title}`,
      "",
      artifact.purpose,
      artifact.scope,
      artifact.context,
      requirementLines ? `Requirements:\n${requirementLines}` : "",
      "",
      "Canonical JSON:",
      "```json",
      jsonBody(artifact),
      "```",
    ]
      .filter(Boolean)
      .join("\n\n"),
  );
}

function requirementBody(requirement, artifact) {
  return normalizeText(
    [
      `${requirement.id}: ${requirement.title}`,
      "",
      requirement.statement,
      requirement.coverage ? `Coverage expectation:\n${jsonBody(requirement.coverage)}` : "",
      requirement.trace ? `Trace:\n${jsonBody(requirement.trace)}` : "",
      requirement.notes?.length ? `Notes:\n${requirement.notes.map((note) => `- ${note}`).join("\n")}` : "",
      "",
      `Parent artifact: ${artifact.artifact_id} - ${artifact.title}`,
      "",
      "Canonical JSON:",
      "```json",
      jsonBody(requirement),
      "```",
    ]
      .filter(Boolean)
      .join("\n\n"),
  );
}

function templateSummary(fileName) {
  return (
    {
      "spec-template.json": "Starter JSON shape for a specification artifact with requirements.",
      "architecture-template.json": "Starter JSON shape for architecture artifacts that satisfy requirements.",
      "work-item-template.json": "Starter JSON shape for delivery work linked to requirements and verification.",
      "verification-template.json": "Starter JSON shape for recording verification proof summaries.",
    }[fileName] ?? `Template file ${fileName}.`
  );
}

function makeTemplate(uriTemplate, group, title, resources) {
  return {
    uriTemplate,
    group,
    title,
    description: `${title} resources`,
    mimeType: "text/markdown; charset=utf-8",
    list: resources
      .filter((resource) => resource.group === group)
      .map((resource) => ({
        uri: resource.uri,
        name: resource.title,
        title: resource.title,
        description: resource.summary,
        mimeType: resource.mimeType,
      })),
  };
}

async function buildMcpCatalog() {
  const readme = await readRepoText("README.md");
  const llms = await readRepoText("LLMS.txt");
  const agents = await readRepoText("AGENTS.md");
  const authoring = await readRepoText("authoring.md");
  const overview = await readRepoText("overview.md");
  const layout = await readRepoText("layout.md");
  const appReadme = await readAppText("README.md");
  const appPackage = JSON.parse(await readAppText("package.json"));
  const wrangler = await readAppText("wrangler.toml");

  const resources = [];

  resources.push(
    createResourceRecord({
      uri: `${namespace}://overview`,
      title: "Overview",
      kind: "guide",
      searchKind: "guide",
      summary: "Practical front door for the SpecTrace standard and repository authority model.",
      body: [splitSection(readme, "Why Use It"), splitSection(readme, "The Basic Shape"), markdownToText(overview)].join("\n\n"),
      sourcePaths: ["README.md", "overview.md"],
      group: "core",
      aliases: ["overview", "what is spectrace", "spec trace overview"],
      priority: 120,
      relatedUris: [`${namespace}://specs/SPEC-STD`, `${namespace}://schema/model`],
      tags: ["intro", "authority", "standard"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://start-a-spec`,
      title: "Start a spec",
      kind: "guide",
      searchKind: "guide",
      summary: "The shortest path to authoring a new SpecTrace specification artifact.",
      body: [splitSection(readme, "Start A Spec"), splitSection(readme, "What You Can Add Later")].join("\n\n"),
      sourcePaths: ["README.md", "spec-template.json", "architecture-template.json", "work-item-template.json", "verification-template.json"],
      group: "guides",
      aliases: ["start", "new spec", "author a spec", "templates"],
      priority: 118,
      relatedUris: [`${namespace}://templates/spec-template`, `${namespace}://guides/authoring`],
      tags: ["authoring", "starter", "templates"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://commands`,
      title: "Common commands",
      kind: "guide",
      searchKind: "guide",
      summary: "Local validation, catalog, topic-view, evidence, attestation, and MCP commands.",
      body: [splitSection(readme, "Common Commands"), splitSection(appReadme, "Local development")].join("\n\n"),
      sourcePaths: ["README.md", "apps/spec-trace-mcp/README.md"],
      group: "guides",
      aliases: ["commands", "validation commands", "test commands"],
      priority: 112,
      relatedUris: [`${namespace}://guides/authoring`, `${namespace}://mcp-server`],
      tags: ["commands", "validation", "mcp"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://profiles`,
      title: "Conformance profiles",
      kind: "guide",
      searchKind: "guide",
      summary: "Core, traceable, and auditable repository conformance profiles.",
      body: splitSection(readme, "Conformance Profiles"),
      sourcePaths: ["README.md", "specs/requirements/spec-trace/SPEC-PRF.json"],
      group: "guides",
      aliases: ["profiles", "core profile", "traceable", "auditable"],
      priority: 111,
      relatedUris: [`${namespace}://specs/SPEC-PRF`],
      tags: ["profiles", "validation"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://repository-layout`,
      title: "Repository layout",
      kind: "guide",
      searchKind: "guide",
      summary: "Where canonical artifacts, schemas, examples, tools, generated outputs, and MCP surfaces live.",
      body: [splitSection(readme, "Repository Map"), markdownToText(layout)].join("\n\n"),
      sourcePaths: ["README.md", "layout.md"],
      group: "guides",
      aliases: ["layout", "repository map", "folders"],
      priority: 108,
      relatedUris: [`${namespace}://specs/SPEC-LAY`],
      tags: ["layout", "repo"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://publish-surface`,
      title: "Publish surface",
      kind: "guide",
      searchKind: "guide",
      summary: "Release and reusable publish mirror expectations for SpecTrace.",
      body: splitSection(readme, "Release And Versioning"),
      sourcePaths: ["README.md", "publish/README.md"],
      group: "guides",
      aliases: ["release", "publish", "versioning"],
      priority: 106,
      relatedUris: [`${namespace}://files/publish/README.md`],
      tags: ["release", "publish"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://mcp-server`,
      title: "MCP server",
      kind: "guide",
      searchKind: "guide",
      summary: "Cloudflare Worker MCP server contract, routes, build commands, and deployment handoff.",
      body: [
        markdownToText(appReadme),
        "",
        "Worker package:",
        "```json",
        jsonBody(appPackage),
        "```",
        "",
        "Wrangler configuration:",
        "```toml",
        wrangler,
        "```",
      ].join("\n"),
      sourcePaths: ["apps/spec-trace-mcp/README.md", "apps/spec-trace-mcp/package.json", "apps/spec-trace-mcp/wrangler.toml"],
      group: "guides",
      aliases: ["mcp", "mcp server", "worker", "cloudflare"],
      priority: 104,
      tags: ["mcp", "cloudflare", "worker"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://guides/authoring`,
      title: "Authoring workflow",
      kind: "guide",
      searchKind: "guide",
      summary: "Task-oriented SpecTrace authoring guidance.",
      body: markdownToText(authoring),
      sourcePaths: ["authoring.md"],
      group: "guides",
      aliases: ["authoring", "workflow", "write requirements"],
      priority: 105,
      relatedUris: [`${namespace}://start-a-spec`, `${namespace}://schema/model`],
      tags: ["authoring"],
    }),
  );

  const guidanceResources = [
    {
      slug: "document-to-requirements",
      title: "Document to requirements workflow",
      summary: "How an agent should turn an arbitrary source document, URL, excerpt, or requirements note into SpecTrace requirements.",
      aliases: ["document import", "requirements extraction", "source document"],
      tags: ["authoring", "extraction", "requirements"],
      body: `
Use this workflow when the input is a full document, a URL, a copied excerpt, or a requirements document that was not written in SpecTrace form.

1. Preserve provenance first.
   - Record the document URL or source label before drafting requirements.
   - Record section, heading, paragraph, table, figure, or line anchors when available.
   - If the input is only a snippet, mark the source scope as a snippet and do not imply full-document coverage.

2. Segment before writing requirements.
   - Split the source into sections and paragraphs.
   - Split paragraphs into candidate sentences or clauses.
   - Keep list items, table rows, and definition entries as separate candidates when they contain independent obligations.

3. Classify each candidate.
   - Normative obligation: contains explicit requirement language such as MUST, MUST NOT, SHALL, SHALL NOT, SHOULD, SHOULD NOT, or MAY.
   - Definition: assigns terminology or semantics but may not be independently testable.
   - Context: explains rationale, examples, background, or non-normative guidance.
   - Ambiguous: appears requirement-like but lacks a clear actor, behavior, condition, or outcome.

4. Draft requirements as small as possible.
   - Each SpecTrace requirement statement should express one testable obligation.
   - Keep exactly one approved uppercase keyword in each requirement statement.
   - Split compound source sentences when they contain multiple independent obligations.
   - Preserve conditions from the source, but do not combine unrelated conditions into one requirement.
   - Prefer stable source-backed wording over invented product language.

5. Map requirement strength.
   - MUST, MUST NOT, SHALL, and SHALL NOT normally become required obligations.
   - SHOULD and SHOULD NOT normally become recommended obligations; keep the uppercase keyword in the statement and explain any optionality in notes or coverage.
   - MAY normally becomes a permitted capability; keep the statement testable by naming what is permitted and under which condition.
   - Lowercase words such as must, should, or may are plain English unless the source defines them as normative.

6. Create identifiers from the source structure.
   - Use a stable document key in the artifact and requirement IDs.
   - Include section numbers when they exist.
   - Convert decimal section numbers to ID-safe groups that start with a letter: section 6.2.1 becomes S6P2P1.
   - Example requirement id: REQ-RFC9000-S6P2P1-0001.
   - Example spec id for a section-scoped artifact: SPEC-RFC9000-S6P2P1.
   - If no section exists, use a stable topical group such as REQ-SOURCE-AUTH-0001 or REQ-POLICY-ACCESS-0001.

7. Keep generated artifacts honest.
   - Do not claim complete coverage unless every relevant source section was processed.
   - Put uncertain interpretations into notes or open questions.
   - Put source citations in trace.upstream_refs or notes according to the repository convention.
   - Validate the resulting JSON against model/model.schema.json and the repository validator.
`,
    },
    {
      slug: "rfc-to-requirements",
      title: "RFC to requirements workflow",
      summary: "RFC-specific guidance for turning protocol text into small SpecTrace requirements.",
      aliases: ["RFC import", "protocol requirements", "normative RFC language"],
      tags: ["authoring", "rfc", "protocol", "requirements"],
      body: `
Use this workflow when the source is an RFC, an Internet-Draft, or protocol text written in an RFC style.

1. Identify the RFC authority and scope.
   - Capture the RFC number, title, URL, publication status, and the sections being processed.
   - Check whether the document defines RFC 2119 or RFC 8174 keyword usage.
   - If keyword usage is not defined, treat uppercase requirement words cautiously and record the ambiguity.

2. Segment by RFC structure.
   - Use section numbers and headings as the primary grouping boundaries.
   - Split each section into paragraphs, then sentences.
   - Split semicolon-heavy sentences, bullet lists, and algorithm steps when they contain separate obligations.
   - Preserve references to figures, tables, registries, packet fields, and ABNF rules when they constrain behavior.

3. Detect normative clauses.
   - Treat uppercase MUST, MUST NOT, SHALL, SHALL NOT, SHOULD, SHOULD NOT, and MAY as candidate requirement keywords when the RFC defines them as normative.
   - Keep one keyword per SpecTrace requirement statement.
   - If one sentence contains multiple uppercase keywords, split it into multiple requirements.
   - If a sentence contains one keyword but multiple independent objects, split when separate tests would be needed.

4. Decide requirement shape.
   - Actor: name the implementation role, endpoint, sender, receiver, client, server, intermediary, encoder, decoder, or other subject.
   - Condition: preserve the RFC condition such as "when receiving X", "before sending Y", or "if Z is present".
   - Behavior: state the required action, prohibition, recommendation, or permitted behavior.
   - Outcome: include observable result, validation rule, state transition, emitted frame, error, or field value when present.

5. Encode section-aware identifiers.
   - Use the RFC number as a stable document key: RFC9000, RFC9114, RFC8446, and so on.
   - Convert section numbers to ID-safe groups: 10 becomes S10; 10.2 becomes S10P2; Appendix A.1 becomes APPA1.
   - Requirement id examples: REQ-RFC9000-S10P2-0001, REQ-RFC9114-S4P1-0002, REQ-RFC8446-APPA1-0001.
   - Artifact id examples: SPEC-RFC9000-S10P2 or SPEC-RFC9000-TRANSPORT.
   - Keep numbering stable once reviewed; do not renumber existing accepted requirements casually.

6. Record provenance.
   - Put the RFC URL and section reference in trace.upstream_refs or notes.
   - Include exact section numbers in notes even when the requirement ID already carries them.
   - For snippets, record that the input was a snippet and include the quoted section label if known.

7. Handle non-normative RFC text.
   - Definitions can become notes, glossary support, or requirements only when the definition imposes testable behavior.
   - Examples are usually notes unless the RFC explicitly makes them normative.
   - Security considerations can produce requirements when they contain normative implementation behavior.
   - IANA considerations can produce requirements when they constrain registries, codepoints, or extensibility behavior.
`,
    },
    {
      slug: "requirement-slicing",
      title: "Requirement slicing rules",
      summary: "Rules for keeping generated SpecTrace requirements atomic, stable, and testable.",
      aliases: ["atomic requirements", "small requirements", "requirement splitting"],
      tags: ["authoring", "slicing", "requirements"],
      body: `
Use these slicing rules after candidate obligations have been identified.

- One requirement should have one approved uppercase keyword.
- One requirement should be testable with one focused evidence target.
- Split AND clauses when each side can pass or fail independently.
- Split OR clauses when alternatives need separate tests or different implementation behavior.
- Keep a condition with the behavior it constrains.
- Keep exceptions in the same requirement only when they are necessary to understand the obligation.
- Put rationale, examples, citations, and uncertainty into notes rather than the statement.
- Do not merge required, recommended, and permitted behavior into one statement.
- Do not turn a whole paragraph into one requirement just because it is one paragraph.
- Prefer several small source-backed requirements over one broad paraphrase.
`,
    },
  ];

  for (const guidance of guidanceResources) {
    resources.push(
      createResourceRecord({
        uri: `${namespace}://guides/${guidance.slug}`,
        title: guidance.title,
        kind: "guide",
        searchKind: "guide",
        summary: guidance.summary,
        body: guidance.body,
        sourcePaths: ["apps/spec-trace-mcp/scripts/generate-mcp.mjs"],
        group: "guides",
        aliases: guidance.aliases,
        priority: guidance.slug === "document-to-requirements" ? 116 : guidance.slug === "rfc-to-requirements" ? 115 : 114,
        relatedUris: [`${namespace}://start-a-spec`, `${namespace}://guides/authoring`, `${namespace}://templates/spec-template`],
        tags: guidance.tags,
      }),
    );
  }

  resources.push(
    createResourceRecord({
      uri: `${namespace}://ai/llms-txt`,
      title: "LLMS.txt",
      kind: "ai",
      searchKind: "ai",
      summary: "AI-readable bootstrap for the SpecTrace reference repository.",
      body: markdownToText(llms),
      sourcePaths: ["LLMS.txt"],
      group: "ai",
      aliases: ["llms", "bootstrap", "ai guidance"],
      priority: 96,
      tags: ["ai"],
    }),
  );

  resources.push(
    createResourceRecord({
      uri: `${namespace}://ai/agent-instructions`,
      title: "Agent instructions",
      kind: "ai",
      searchKind: "ai",
      summary: "Repository-specific agent instructions and authority order.",
      body: markdownToText(agents),
      sourcePaths: ["AGENTS.md"],
      group: "ai",
      aliases: ["agents", "agent instructions", "authority order"],
      priority: 95,
      tags: ["ai", "authority"],
    }),
  );

  const specDir = path.join(repoRoot, "specs", "requirements", "spec-trace");
  const specFiles = (await walkFiles(specDir, (file) => file.endsWith(".json"))).sort((left, right) => left.localeCompare(right));
  const artifacts = [];

  for (const filePath of specFiles) {
    const relativePath = repoRelative(filePath);
    const artifact = JSON.parse(await readFile(filePath, "utf8"));
    artifacts.push({ artifact, relativePath });
    const artifactUri = `${namespace}://specs/${artifact.artifact_id}`;

    resources.push(
      createResourceRecord({
        uri: artifactUri,
        title: `${artifact.artifact_id}: ${artifact.title}`,
        kind: "spec",
        searchKind: "spec",
        summary: artifact.purpose ?? `${artifact.artifact_id} specification artifact.`,
        body: artifactBody(artifact),
        sourcePaths: [relativePath],
        group: "specs",
        aliases: [artifact.artifact_id, artifact.title, artifact.capability].filter(Boolean),
        priority: artifact.artifact_id === "SPEC-STD" ? 110 : 100,
        artifactId: artifact.artifact_id,
        artifactType: artifact.artifact_type,
        domain: artifact.domain,
        capability: artifact.capability,
        relatedUris: (artifact.related_artifacts ?? []).map((id) => `${namespace}://artifacts/${id}`),
        tags: artifact.tags ?? [],
      }),
    );

    for (const requirement of artifact.requirements ?? []) {
      resources.push(
        createResourceRecord({
          uri: `${namespace}://requirements/${requirement.id}`,
          title: `${requirement.id}: ${requirement.title}`,
          kind: "requirement",
          searchKind: "requirement",
          summary: requirement.statement,
          body: requirementBody(requirement, artifact),
          sourcePaths: [relativePath],
          group: "requirements",
          aliases: [requirement.id, requirement.title, artifact.artifact_id],
          priority: 80,
          artifactId: artifact.artifact_id,
          requirementId: requirement.id,
          artifactType: "requirement",
          domain: artifact.domain,
          capability: artifact.capability,
          relatedUris: [artifactUri],
          tags: artifact.tags ?? [],
        }),
      );
    }
  }

  const schemaFiles = [
    ["model/model.schema.json", "model", "Authoritative JSON Schema", "Authoritative JSON Schema for canonical SpecTrace artifacts and derived data shapes."],
    ["artifact-id-policy.json", "artifact-id-policy", "Artifact ID policy", "Compatibility identifier export for artifact and requirement ID policy."],
  ];

  for (const [relativePath, slug, title, summary] of schemaFiles) {
    const text = await readRepoText(relativePath);
    resources.push(
      createResourceRecord({
        uri: `${namespace}://schema/${slug}`,
        title,
        kind: "schema",
        searchKind: "schema",
        summary,
        body: text,
        sourcePaths: [relativePath],
        group: "schema",
        mimeType: "application/json",
        aliases: [slug, path.basename(relativePath)],
        priority: slug === "model" ? 108 : 86,
        tags: ["schema", "validation"],
      }),
    );
  }

  const templateFiles = ["spec-template.json", "architecture-template.json", "work-item-template.json", "verification-template.json"];
  for (const relativePath of templateFiles) {
    const text = await readRepoText(relativePath);
    const slug = relativePath.replace(/\.json$/, "");
    resources.push(
      createResourceRecord({
        uri: `${namespace}://templates/${slug}`,
        title: relativePath,
        kind: "template",
        searchKind: "template",
        summary: templateSummary(relativePath),
        body: text,
        sourcePaths: [relativePath],
        group: "templates",
        mimeType: "application/json",
        aliases: [slug, relativePath, relativePath.replace("-template.json", "")],
        priority: relativePath === "spec-template.json" ? 102 : 92,
        tags: ["template", "authoring"],
      }),
    );
  }

  const examplesReadme = await readRepoText("examples/README.md");
  resources.push(
    createResourceRecord({
      uri: `${namespace}://examples/index`,
      title: "Examples index",
      kind: "example",
      searchKind: "example",
      summary: "Worked SpecTrace examples for small and fuller adoption slices.",
      body: markdownToText(examplesReadme),
      sourcePaths: ["examples/README.md"],
      group: "examples",
      aliases: ["examples", "worked examples"],
      priority: 88,
      tags: ["examples"],
    }),
  );

  const exampleFiles = (await walkFiles(path.join(repoRoot, "examples"), (file) => file.endsWith(".json"))).sort((left, right) =>
    left.localeCompare(right),
  );
  for (const filePath of exampleFiles) {
    const relativePath = repoRelative(filePath);
    const artifact = JSON.parse(await readFile(filePath, "utf8"));
    const id = artifact.artifact_id ?? path.basename(filePath, ".json");
    resources.push(
      createResourceRecord({
        uri: `${namespace}://examples/${id}`,
        title: `${id}: ${artifact.title ?? path.basename(filePath)}`,
        kind: "example",
        searchKind: "example",
        summary: artifact.purpose ?? `Worked example artifact from ${relativePath}.`,
        body: artifactBody(artifact),
        sourcePaths: [relativePath],
        group: "examples",
        aliases: [id, artifact.title, relativePath].filter(Boolean),
        priority: 70,
        artifactId: id,
        artifactType: artifact.artifact_type,
        domain: artifact.domain,
        capability: artifact.capability,
        tags: artifact.tags ?? ["example"],
      }),
    );
  }

  const rawFiles = [
    "README.md",
    "LLMS.txt",
    "AGENTS.md",
    "authoring.md",
    "overview.md",
    "layout.md",
    "artifact-model-explainer.md",
    "profiles-and-attestation-explainer.md",
    "docs/maintainer-readiness.md",
    "publish/README.md",
    "catalog/retired-requirements.json",
    "scripts/Test-SpecTraceRepository.ps1",
    "scripts/Build-SpecTraceCatalog.ps1",
    "scripts/Resolve-SpecTraceTopicView.ps1",
    "scripts/Validate-SpecTraceEvidence.ps1",
    "scripts/Render-SpecTraceAttestation.ps1",
    "scripts/Sync-PublishModule.ps1",
  ];

  for (const relativePath of rawFiles) {
    const text = await readRepoText(relativePath);
    const isJson = relativePath.endsWith(".json");
    resources.push(
      createResourceRecord({
        uri: `${namespace}://files/${relativePath}`,
        title: relativePath,
        kind: "file",
        searchKind: "file",
        summary: `Raw curated SpecTrace repository file: ${relativePath}.`,
        body: text,
        sourcePaths: [relativePath],
        group: "files",
        mimeType: isJson ? "application/json" : "text/plain; charset=utf-8",
        aliases: [path.basename(relativePath), slugify(relativePath)],
        priority: 20,
        includeInSearch: false,
        tags: ["file"],
      }),
    );
  }

  const seenUris = new Map();
  for (const resource of resources) {
    if (seenUris.has(resource.uri)) {
      throw new Error(`Duplicate MCP URI "${resource.uri}" from ${resource.sourcePaths.join(", ")} and ${seenUris.get(resource.uri)}`);
    }
    seenUris.set(resource.uri, resource.sourcePaths.join(", "));
  }

  resources.sort((left, right) => {
    const groupDelta = groupOrder.indexOf(left.group) - groupOrder.indexOf(right.group);
    if (groupDelta !== 0) return groupDelta;
    if (left.priority !== right.priority) return right.priority - left.priority;
    return left.title.localeCompare(right.title);
  });

  const resourceTemplates = [
    makeTemplate(`${namespace}://specs/{artifact_id}`, "specs", "Spec artifact", resources),
    makeTemplate(`${namespace}://requirements/{requirement_id}`, "requirements", "Requirement", resources),
    makeTemplate(`${namespace}://schema/{name}`, "schema", "Schema", resources),
    makeTemplate(`${namespace}://templates/{name}`, "templates", "Template", resources),
    makeTemplate(`${namespace}://examples/{artifact_id}`, "examples", "Example", resources),
    makeTemplate(`${namespace}://files/{path}`, "files", "Curated file", resources),
  ];

  const searchIndex = resources
    .filter((resource) => resource.includeInSearch !== false)
    .map(({ body, ...resource }) => ({
      ...resource,
      excerpt: body.slice(0, 260),
    }));

  const groupedResources = {};
  for (const resource of resources) {
    const bucket = groupedResources[resource.group] ?? [];
    bucket.push(resource);
    groupedResources[resource.group] = bucket;
  }

  return {
    displayName,
    serverName,
    summary: serverSummary,
    namespace,
    packageName,
    packageVersion,
    sourceRepository: {
      name: "spec-trace",
      root: repoRelative(repoRoot),
      canonicalSpecPath: "specs/requirements/spec-trace",
      schemaPath: "model/model.schema.json",
    },
    authorityOrder: [
      "specs/requirements/spec-trace/",
      "model/model.schema.json and root JSON templates",
      "examples/ and generated outputs",
      "root summaries and AI convenience files",
    ],
    resourceTemplates,
    searchFields: ["title", "summary", "aliases", "tags", "body", "sourcePaths", "relatedUris", "uri", "artifactId", "requirementId"],
    resources,
    searchTool: {
      name: "search_spec_trace",
      description: "Search the generated SpecTrace standard, requirement, schema, template, example, and guide index.",
    },
    groupedResources,
    artifactCount: artifacts.length,
    requirementCount: resources.filter((resource) => resource.group === "requirements").length,
  };
}

async function writeJson(relativePath, value) {
  const filePath = path.join(outDir, relativePath);
  await mkdir(path.dirname(filePath), { recursive: true });
  await writeFile(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

const manifest = await buildMcpCatalog();

await rm(outDir, { recursive: true, force: true });
await mkdir(outDir, { recursive: true });
await writeJson("manifest.json", manifest);
await writeJson("resources.json", manifest);
await writeJson("search-index.json", manifest.resources.filter((resource) => resource.includeInSearch !== false).map(({ body, ...resource }) => ({
  ...resource,
  excerpt: body.slice(0, 260),
})));

for (const [group, entries] of Object.entries(manifest.groupedResources)) {
  await writeJson(`${group}.json`, entries);
}

for (const resource of manifest.resources) {
  if (resource.group === "files" || resource.group === "requirements") continue;
  const leaf = resource.uri.slice(`${namespace}://`.length).replace(/[/:]/g, "__");
  await writeJson(path.join(resource.group, `${leaf}.json`), resource);
}

console.log(
  `Generated ${manifest.resources.length} MCP resources (${manifest.artifactCount} specs, ${manifest.requirementCount} requirements) in ${path.relative(
    appRoot,
    outDir,
  )}`,
);
