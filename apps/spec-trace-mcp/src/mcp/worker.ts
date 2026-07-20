import { McpServer, ResourceTemplate } from "@modelcontextprotocol/sdk/server/mcp.js";
import { WebStandardStreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/webStandardStreamableHttp.js";
import * as z from "zod/v4";
import mcpConfig from "../../mcp.config.json";
import resourcesManifest from "../../dist/mcp/resources.json";
import searchIndex from "../../dist/mcp/search-index.json";

type ResourceRecord = {
  uri: string;
  title: string;
  kind: string;
  searchKind: string;
  summary: string;
  body: string;
  sourcePaths: string[];
  mimeType: string;
  aliases: string[];
  relatedUris: string[];
  group: string;
  priority: number;
  includeInSearch?: boolean;
  tags?: string[];
  artifactId?: string;
  requirementId?: string;
  artifactType?: string;
  domain?: string;
  capability?: string;
  searchText: string;
};

type SearchIndexEntry = Pick<
  ResourceRecord,
  | "uri"
  | "title"
  | "kind"
  | "searchKind"
  | "summary"
  | "sourcePaths"
  | "aliases"
  | "relatedUris"
  | "group"
  | "priority"
  | "includeInSearch"
  | "tags"
  | "artifactId"
  | "requirementId"
  | "artifactType"
  | "domain"
  | "capability"
  | "searchText"
> & { excerpt: string };

type WorkerEnv = {
  MCP_PATH_PREFIX?: string;
};

const packageName = resourcesManifest.packageName ?? mcpConfig.packageName ?? "@incursa/spec-trace-mcp";
const packageVersion = resourcesManifest.packageVersion ?? "0.0.0";
const serverName = resourcesManifest.serverName ?? mcpConfig.serverName ?? "spec-trace-docs";
const displayName = resourcesManifest.displayName ?? mcpConfig.displayName ?? "SpecTrace Documentation";
const namespace = resourcesManifest.namespace ?? mcpConfig.namespace ?? "spec-trace";
const resources = resourcesManifest.resources as ResourceRecord[];
const resourceMap = new Map(resources.map((resource) => [resource.uri, resource]));
const sourcePathMap = new Map<string, ResourceRecord>();
const artifactMap = new Map(
  resources
    .filter((resource) => resource.artifactId && resource.kind !== "requirement")
    .map((resource) => [resource.artifactId!.toLowerCase(), resource]),
);
const requirementMap = new Map(
  resources.filter((resource) => resource.requirementId).map((resource) => [resource.requirementId!.toLowerCase(), resource]),
);
const guidanceMap = new Map<string, ResourceRecord>();
const searchEntries = searchIndex as SearchIndexEntry[];

for (const resource of resources) {
  for (const sourcePath of resource.sourcePaths) {
    const normalized = sourcePath.replace(/\\/g, "/").replace(/^\.\//, "").replace(/^\//, "");
    sourcePathMap.set(normalized, resource);
    sourcePathMap.set(normalized.replace(/\.(md|json|ps1)$/i, ""), resource);
    sourcePathMap.set(`/${normalized}`, resource);
  }

  if (resource.group === "guides" || resource.group === "core" || resource.group === "ai") {
    const uriLeaf = resource.uri.slice(`${namespace}://`.length).toLowerCase();
    guidanceMap.set(uriLeaf, resource);
    guidanceMap.set(uriLeaf.replace(/^guides\//, ""), resource);
    guidanceMap.set(resource.title.toLowerCase(), resource);
    for (const alias of resource.aliases ?? []) {
      guidanceMap.set(alias.toLowerCase(), resource);
    }
  }
}

const defaultPathPrefix = "/spec-trace";

function normalizePathPrefix(value?: string) {
  const raw = String(value ?? "").trim();
  const candidate = raw.length > 0 ? raw : defaultPathPrefix;
  const prefixed = candidate.startsWith("/") ? candidate : `/${candidate}`;
  const collapsed = prefixed.replace(/\/{2,}/g, "/");
  return collapsed.length > 1 ? collapsed.replace(/\/+$/, "") : "/";
}

function getPathPrefix(env: WorkerEnv | undefined) {
  return normalizePathPrefix(env?.MCP_PATH_PREFIX);
}

function stripPathPrefix(pathname: string, pathPrefix: string) {
  if (!pathPrefix || pathPrefix === "/") {
    return pathname;
  }
  if (pathname === pathPrefix) {
    return "/";
  }
  if (pathname.startsWith(`${pathPrefix}/`)) {
    return pathname.slice(pathPrefix.length);
  }
  return pathname;
}

function publicPath(pathPrefix: string, pathname: string) {
  const normalizedPathname = pathname.startsWith("/") ? pathname : `/${pathname}`;
  if (!pathPrefix || pathPrefix === "/") {
    return normalizedPathname;
  }
  return `${pathPrefix}${normalizedPathname}`;
}

function createRouteContext(request: Request, env: WorkerEnv | undefined) {
  const pathPrefix = getPathPrefix(env);
  const routedUrl = new URL(request.url);
  routedUrl.pathname = stripPathPrefix(routedUrl.pathname, pathPrefix);
  return { pathPrefix, routedUrl };
}

function normalizeText(value: string) {
  return String(value ?? "")
    .replace(/\r\n/g, "\n")
    .replace(/\u00a0/g, " ")
    .replace(/[ \t]+\n/g, "\n")
    .replace(/\n{3,}/g, "\n\n")
    .replace(/[ \t]{2,}/g, " ")
    .trim()
    .toLowerCase();
}

function escapeHtml(value: string) {
  return String(value ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function titleCase(value: string) {
  return value
    .split(/[-_/]+/g)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

function groupLabel(group: string) {
  return (
    {
      core: "Core",
      guides: "Guides",
      specs: "Specs",
      requirements: "Requirements",
      schema: "Schemas",
      templates: "Templates",
      examples: "Examples",
      ai: "AI",
      files: "Files",
    }[group] ?? titleCase(group)
  );
}

function groupOrder(group: string) {
  return (
    {
      core: 0,
      guides: 1,
      specs: 2,
      requirements: 3,
      schema: 4,
      templates: 5,
      examples: 6,
      ai: 7,
      files: 8,
    }[group] ?? 99
  );
}

function code(text: string) {
  return `<code>${escapeHtml(text)}</code>`;
}

function normalizeSourcePath(filePath: string) {
  const value = String(filePath ?? "");
  const stripped = value.startsWith(`${namespace}://files/`) ? value.slice(`${namespace}://files/`.length) : value;
  return stripped
    .replace(/\\/g, "/")
    .replace(/^\.\//, "")
    .replace(/^\//, "")
    .replace(/\/+/g, "/");
}

function extractExcerpt(text: string, tokens: string[]) {
  const source = normalizeText(text);
  if (!source) {
    return "";
  }

  const firstMatch = tokens.map((token) => source.indexOf(token)).find((index) => index >= 0);
  if (firstMatch == null || firstMatch < 0) {
    return source.slice(0, 220);
  }

  const start = Math.max(0, firstMatch - 80);
  return source.slice(start, start + 220);
}

function renderDocsIndexHtml(pathPrefix: string) {
  const grouped = new Map<string, ResourceRecord[]>();
  for (const resource of resources) {
    const bucket = grouped.get(resource.group) ?? [];
    bucket.push(resource);
    grouped.set(resource.group, bucket);
  }

  const sections = [...grouped.entries()]
    .sort((left, right) => groupOrder(left[0]) - groupOrder(right[0]))
    .map(([group, entries]) => {
      const cards = entries
        .slice()
        .sort((left, right) => right.priority - left.priority || left.title.localeCompare(right.title))
        .map(
          (resource) => `
            <a class="card" href="${escapeHtml(publicPath(pathPrefix, `/mcp/resource/${encodeURIComponent(resource.uri)}`))}">
              <strong>${escapeHtml(resource.title)}</strong>
              <span>${escapeHtml(resource.summary)}</span>
              <small>${escapeHtml(resource.uri)}</small>
            </a>`,
        )
        .join("");

      return `
        <section class="section">
          <div class="section-head">
            <h2>${escapeHtml(groupLabel(group))}</h2>
            <p>${entries.length} resources</p>
          </div>
          <div class="grid">${cards}</div>
        </section>`;
    })
    .join("");

  return `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>${escapeHtml(displayName)}</title>
  <link rel="icon" href="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 64 64'%3E%3Crect width='64' height='64' rx='12' fill='%23EEF1FF'/%3E%3Cg fill='none' stroke='%234459C6' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M31 12H17V19M17 45V52H31' stroke-width='8'/%3E%3Cpath d='M17 19L30 32L17 45L4 32Z' stroke-width='7'/%3E%3Cpath d='M29 32H50' stroke-width='8'/%3E%3C/g%3E%3Crect x='49' y='25.5' width='13' height='13' rx='3' fill='%232B397F'/%3E%3Ccircle cx='17' cy='32' r='3.5' fill='%232B397F'/%3E%3C/svg%3E" />
  <style>
    :root {
      color-scheme: dark;
      --bg: #07111f;
      --panel: rgba(17, 27, 49, 0.8);
      --line: rgba(158, 177, 200, 0.16);
      --text: #e8f0fb;
      --muted: #9eb1c8;
      --accent: #bfc7ee;
      --shadow: 0 28px 60px rgba(2, 6, 23, 0.45);
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      min-height: 100vh;
      font-family: Inter, "Segoe UI", system-ui, -apple-system, sans-serif;
      background:
        radial-gradient(circle at top left, rgba(125, 211, 252, 0.16), transparent 32%),
        radial-gradient(circle at right 12%, rgba(167, 139, 250, 0.12), transparent 24%),
        linear-gradient(180deg, var(--bg), #0c1830);
      color: var(--text);
    }
    a { color: inherit; text-decoration: none; }
    main {
      width: min(1160px, calc(100vw - 32px));
      margin: 0 auto;
      padding: 40px 0 64px;
    }
    .hero, .section {
      border: 1px solid var(--line);
      background: var(--panel);
      backdrop-filter: blur(18px);
      border-radius: 24px;
      box-shadow: var(--shadow);
    }
    .hero {
      display: grid;
      grid-template-columns: 1.2fr 0.8fr;
      gap: 18px;
      padding: 24px;
    }
    .panel {
      padding: 18px;
      border-radius: 18px;
      border: 1px solid rgba(255, 255, 255, 0.06);
      background: rgba(255, 255, 255, 0.04);
    }
    .brand-lockup {
      display: inline-flex;
      align-items: center;
      gap: 14px;
      margin: 0 0 22px;
      color: #fff;
    }
    .brand-lockup svg { width: 58px; height: 58px; flex: 0 0 auto; }
    .brand-lockup strong { font-size: 1.6rem; letter-spacing: -0.02em; }
    h1 {
      margin: 0;
      font-size: clamp(2.4rem, 5vw, 4.4rem);
      line-height: .94;
      max-width: 10ch;
    }
    p { line-height: 1.6; }
    .lead { color: var(--muted); max-width: 66ch; }
    .meta-grid, .grid, .tools {
      display: grid;
      gap: 14px;
    }
    .meta-grid { grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); }
    .section {
      margin-top: 20px;
      padding: 20px;
    }
    .section-head {
      display: flex;
      justify-content: space-between;
      align-items: end;
      gap: 12px;
      margin-bottom: 14px;
    }
    .section-head h2, .section-head p { margin: 0; }
    .section-head p { color: var(--muted); }
    .grid {
      grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
    }
    .card {
      display: grid;
      gap: 10px;
      min-height: 150px;
      padding: 16px;
      border: 1px solid rgba(255, 255, 255, 0.06);
      border-radius: 18px;
      background: rgba(255, 255, 255, 0.04);
      color: inherit;
    }
    .card strong { color: var(--accent); }
    .card span, .card small, .meta p, .tool p { color: var(--muted); }
    .card small, code, pre { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; }
    code {
      background: rgba(125, 211, 252, 0.12);
      border: 1px solid rgba(125, 211, 252, 0.18);
      border-radius: 999px;
      padding: 0.15rem 0.4rem;
    }
    pre {
      margin: 12px 0 0;
      padding: 14px 16px;
      border-radius: 14px;
      overflow: auto;
      background: rgba(2, 6, 23, 0.55);
      border: 1px solid rgba(255, 255, 255, 0.08);
      white-space: pre-wrap;
    }
    @media (max-width: 860px) {
      .hero { grid-template-columns: 1fr; }
      .section-head { flex-direction: column; align-items: start; }
      main { width: min(1160px, calc(100vw - 22px)); padding-top: 20px; }
    }
  </style>
</head>
<body>
  <main>
    <section class="hero">
      <div>
        <div class="brand-lockup" aria-label="SpecTrace">
          <svg viewBox="0 0 64 64" aria-hidden="true">
            <g fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
              <path d="M31 12H17V21M17 43V52H31" stroke-width="8" />
              <path d="M17 21L28 32L17 43L6 32Z" stroke-width="6" />
              <path d="M27 32H50" stroke-width="8" />
            </g>
            <rect x="49" y="26" width="12" height="12" rx="3" fill="currentColor" />
            <circle cx="17" cy="32" r="2.75" fill="currentColor" />
          </svg>
          <strong>SpecTrace</strong>
        </div>
        <p class="lead" style="text-transform:uppercase;letter-spacing:.16em;color:var(--accent);margin:0 0 12px">${escapeHtml(displayName)}</p>
        <h1>SpecTrace standard, MCP delivery.</h1>
        <p class="lead">${escapeHtml(resourcesManifest.summary ?? mcpConfig.summary ?? "A deterministic Cloudflare Worker MCP server.")}</p>
        <div class="panel" style="margin-top:18px">
          <strong>Endpoints</strong>
          <p>${code(`GET ${publicPath(pathPrefix, "/mcp")}`)}, ${code(`POST ${publicPath(pathPrefix, "/mcp")}`)}, and ${code(`GET ${publicPath(pathPrefix, "/mcp/resource/<uri>")}`)}.</p>
        </div>
      </div>
      <div class="meta-grid">
        <div class="panel"><strong>Package</strong><p>${escapeHtml(packageName)}</p></div>
        <div class="panel"><strong>Version</strong><p>${escapeHtml(packageVersion)}</p></div>
        <div class="panel"><strong>Tools</strong><p>${code("search_spec_trace")}, ${code("get_requirement")}, ${code("get_artifact")}, ${code("get_guidance")}</p></div>
        <div class="panel"><strong>Template</strong><p>${code(`${namespace}://files/{path}`)}</p></div>
      </div>
    </section>

    <section class="section">
      <div class="section-head">
        <h2>How it works</h2>
        <p>Canonical repository files become MCP metadata at build time.</p>
      </div>
      <div class="tools">
        <div class="panel"><strong>Authoring</strong><p>Edit canonical SpecTrace JSON, schema, templates, docs, or examples in the repository root.</p></div>
        <div class="panel"><strong>Build</strong><p>Compile the files into ${code("dist/mcp/*.json")}.</p></div>
        <div class="panel"><strong>Search</strong><p>Use the one dynamic tool to search all content.</p></div>
      </div>
    </section>

    <section class="section">
      <div class="section-head">
        <h2>Resources</h2>
        <p>${resources.length} static docs.</p>
      </div>
      ${sections}
    </section>
  </main>
</body>
</html>`;
}

function renderResourcePage(resource: ResourceRecord, pathPrefix: string) {
  const aliases = resource.aliases.length ? resource.aliases.map((alias) => `<code>${escapeHtml(alias)}</code>`).join(" ") : "<span>None</span>";
  const related = resource.relatedUris.length
    ? `<ul>${resource.relatedUris
        .map((uri) => {
          const relatedResource = resourceMap.get(uri);
          const title = relatedResource?.title ?? uri;
          return `<li><a href="${escapeHtml(publicPath(pathPrefix, `/mcp/resource/${encodeURIComponent(uri)}`))}">${escapeHtml(title)}</a></li>`;
        })
        .join("")}</ul>`
    : "<p>No related resources.</p>";

  return `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>${escapeHtml(resource.title)} - ${escapeHtml(displayName)}</title>
  <style>
    :root {
      color-scheme: dark;
      --bg: #07111f;
      --panel: rgba(17, 27, 49, 0.82);
      --line: rgba(158, 177, 200, 0.18);
      --text: #e8f0fb;
      --muted: #9eb1c8;
      --accent: #7dd3fc;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: Inter, "Segoe UI", system-ui, -apple-system, sans-serif;
      background: linear-gradient(180deg, var(--bg), #0c1830);
      color: var(--text);
    }
    main {
      width: min(980px, calc(100vw - 32px));
      margin: 0 auto;
      padding: 40px 0 64px;
    }
    article, section {
      border: 1px solid var(--line);
      background: var(--panel);
      backdrop-filter: blur(18px);
      border-radius: 24px;
      padding: 24px;
      box-shadow: 0 28px 60px rgba(2, 6, 23, 0.45);
    }
    section { margin-top: 20px; }
    p, li { color: var(--muted); line-height: 1.6; }
    code, pre { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; }
    code {
      background: rgba(125, 211, 252, 0.12);
      border: 1px solid rgba(125, 211, 252, 0.18);
      border-radius: 999px;
      padding: 0.15rem 0.4rem;
    }
    pre {
      margin: 12px 0 0;
      padding: 14px 16px;
      border-radius: 14px;
      overflow: auto;
      background: rgba(2, 6, 23, 0.55);
      border: 1px solid rgba(255, 255, 255, 0.08);
      white-space: pre-wrap;
    }
    a { color: var(--accent); }
    .meta {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 12px;
      margin-top: 16px;
    }
    .meta div {
      padding: 14px 16px;
      border-radius: 16px;
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid rgba(255, 255, 255, 0.06);
    }
    .meta strong {
      display: block;
      margin-bottom: 6px;
      color: var(--accent);
      font-size: 0.76rem;
      letter-spacing: .12em;
      text-transform: uppercase;
    }
  </style>
</head>
<body>
  <main>
    <article>
      <p style="text-transform:uppercase;letter-spacing:.16em;color:var(--accent);margin:0 0 12px">${escapeHtml(resource.group)}</p>
      <h1>${escapeHtml(resource.title)}</h1>
      <p>${escapeHtml(resource.summary)}</p>
      <div class="meta">
        <div><strong>URI</strong><code>${escapeHtml(resource.uri)}</code></div>
        <div><strong>Source</strong><code>${escapeHtml(resource.sourcePaths.join(", "))}</code></div>
        <div><strong>Kind</strong><code>${escapeHtml(resource.kind)}</code></div>
      </div>
    </article>

    <section>
      <h2>Aliases</h2>
      <p>${aliases}</p>
    </section>

    <section>
      <h2>Body</h2>
      <pre>${escapeHtml(resource.body)}</pre>
    </section>

    <section>
      <h2>Related resources</h2>
      ${related}
      <p><a href="${escapeHtml(publicPath(pathPrefix, "/mcp"))}">Back to the index</a></p>
    </section>
  </main>
</body>
</html>`;
}

function lookupResourceFromUri(uri) {
  return resourceMap.get(uri) ?? null;
}

function lookupResourceFromFilePath(filePath) {
  const normalized = normalizeSourcePath(filePath);
  return sourcePathMap.get(normalized) ?? sourcePathMap.get(normalized.replace(/\.md$/, "")) ?? null;
}

function searchSpecTrace(args) {
  const query = normalizeText(args.query ?? "");
  const tokens = query.split(/\s+/g).filter(Boolean);
  const kind = args.kind ?? "any";
  const includeExamples = args.include_examples ?? true;
  const includeFiles = args.include_files ?? false;
  const maxResults = Math.min(Math.max(args.max_results ?? 8, 1), 20);

  const filtered = searchEntries.filter((entry) => {
    if (!includeExamples && entry.group === "examples") {
      return false;
    }
    if (!includeFiles && entry.group === "files") {
      return false;
    }
    if (kind !== "any" && entry.searchKind !== kind) {
      return false;
    }
    return true;
  });

  const scored = filtered
    .map((entry) => {
      const title = normalizeText(entry.title);
      const summary = normalizeText(entry.summary);
      const haystack = normalizeText(entry.searchText);
      let score = entry.priority ?? 0;

      if (!query) {
        score += 5;
      } else {
        if (normalizeText(entry.uri) === query) score += 2000;
        if (title === query) score += 1800;
        if (haystack.includes(query)) score += 500;
        for (const token of tokens) {
          if (title.includes(token)) score += 120;
          else if (summary.includes(token)) score += 60;
          else if (haystack.includes(token)) score += 20;
          else score -= 4;
        }
      }

      return {
        ...entry,
        score,
        excerpt: entry.excerpt || extractExcerpt(entry.searchText, tokens),
      };
    })
    .sort((left, right) => right.score - left.score || right.priority - left.priority || left.title.localeCompare(right.title));

  return {
    query: args.query,
    kind,
    include_examples: includeExamples,
    include_files: includeFiles,
    max_results: maxResults,
    results: scored.slice(0, maxResults).map((entry) => ({
      uri: entry.uri,
      title: entry.title,
      kind: entry.kind,
      searchKind: entry.searchKind,
      summary: entry.summary,
      sourcePaths: entry.sourcePaths,
      score: entry.score,
      excerpt: entry.excerpt,
      relatedUris: entry.relatedUris,
      artifactId: entry.artifactId,
      requirementId: entry.requirementId,
    })),
    starterSuggestions: scored.slice(0, 3).map((entry) => ({
      uri: entry.uri,
      title: entry.title,
      kind: entry.kind,
      searchKind: entry.searchKind,
    })),
  };
}

function getRequirement(args) {
  const requirementId = String(args.requirement_id ?? "").trim().toLowerCase();
  const resource = requirementMap.get(requirementId);
  if (!resource) {
    throw new Error(`Unknown requirement id: ${args.requirement_id}`);
  }

  return {
    requirement_id: resource.requirementId,
    title: resource.title,
    statement: resource.summary,
    artifact_id: resource.artifactId,
    sourcePaths: resource.sourcePaths,
    uri: resource.uri,
    body: resource.body,
    relatedUris: resource.relatedUris,
  };
}

function getArtifact(args) {
  const artifactId = String(args.artifact_id ?? "").trim().toLowerCase();
  const resource = artifactMap.get(artifactId);
  if (!resource) {
    throw new Error(`Unknown artifact id: ${args.artifact_id}`);
  }

  const includeRequirements = args.include_requirements ?? true;
  const requirements = includeRequirements
    ? resources
        .filter((candidate) => candidate.group === "requirements" && candidate.artifactId === resource.artifactId)
        .map((candidate) => ({
          requirement_id: candidate.requirementId,
          title: candidate.title,
          statement: candidate.summary,
          uri: candidate.uri,
        }))
    : [];

  return {
    artifact_id: resource.artifactId,
    title: resource.title,
    artifact_type: resource.artifactType,
    domain: resource.domain,
    capability: resource.capability,
    sourcePaths: resource.sourcePaths,
    uri: resource.uri,
    summary: resource.summary,
    body: resource.body,
    requirements,
    relatedUris: resource.relatedUris,
  };
}

function getGuidance(args) {
  const topic = String(args.topic ?? "").trim().toLowerCase();
  const resource = guidanceMap.get(topic) ?? guidanceMap.get(topic.replace(`${namespace}://`, ""));
  if (!resource) {
    throw new Error(`Unknown guidance topic: ${args.topic}`);
  }

  return {
    topic: args.topic,
    title: resource.title,
    summary: resource.summary,
    uri: resource.uri,
    sourcePaths: resource.sourcePaths,
    body: resource.body,
    relatedUris: resource.relatedUris,
  };
}

function registerResources(server) {
  for (const resource of resources) {
    server.registerResource(
      resource.title,
      resource.uri,
      {
        description: resource.summary,
        mimeType: resource.mimeType,
      },
      async () => ({
        contents: [
          {
            uri: resource.uri,
            mimeType: resource.mimeType,
            text: resource.body,
          },
        ],
      }),
    );
  }

  for (const template of resourcesManifest.resourceTemplates ?? []) {
    server.registerResource(
      `${template.group}-template`,
      new ResourceTemplate(template.uriTemplate, {
        list: async () => ({
          resources: template.list,
        }),
      }),
      {
        description: template.description,
        mimeType: template.mimeType,
      },
      async (uri: URL, variables: Record<string, string>) => {
        const variableValue = Object.values(variables ?? {})[0];
        const resource = lookupResourceFromUri(uri.toString()) ?? lookupResourceFromFilePath(variableValue ?? decodeURIComponent(uri.pathname));
        if (!resource) {
          throw new Error(`Unknown resource: ${uri.toString()}`);
        }
        return {
          contents: [
            {
              uri: resource.uri,
              mimeType: resource.mimeType,
              text: resource.body,
            },
          ],
        };
      },
    );
  }
}

function registerTools(server) {
  server.registerTool(
    "search_spec_trace",
    {
      description: resourcesManifest.searchTool?.description ?? "Search the generated SpecTrace MCP index.",
      inputSchema: {
        query: z.string().describe("Search text"),
        kind: z.enum(["guide", "spec", "requirement", "schema", "template", "example", "ai", "file", "any"]).default("any"),
        include_examples: z.boolean().default(true),
        include_files: z.boolean().default(false),
        max_results: z.number().int().positive().max(20).default(8),
      },
      outputSchema: {
        query: z.string(),
        kind: z.string(),
        include_examples: z.boolean(),
        include_files: z.boolean(),
        max_results: z.number(),
        results: z.array(
          z.object({
            uri: z.string(),
            title: z.string(),
            kind: z.string(),
            searchKind: z.string(),
            summary: z.string(),
            sourcePaths: z.array(z.string()),
            score: z.number(),
            excerpt: z.string(),
            relatedUris: z.array(z.string()),
            artifactId: z.string().optional(),
            requirementId: z.string().optional(),
          }),
        ),
        starterSuggestions: z.array(
          z.object({
            uri: z.string(),
            title: z.string(),
            kind: z.string(),
            searchKind: z.string(),
          }),
        ),
      },
    },
    async (args) => {
      const result = searchSpecTrace(args);
      return {
        content: [{ type: "text", text: JSON.stringify(result, null, 2) }],
        structuredContent: result,
      };
    },
  );

  server.registerTool(
    "get_requirement",
    {
      description: "Return a canonical SpecTrace requirement by requirement id.",
      inputSchema: {
        requirement_id: z.string().describe("Requirement id such as REQ-STD-0001"),
      },
      outputSchema: {
        requirement_id: z.string().optional(),
        title: z.string(),
        statement: z.string(),
        artifact_id: z.string().optional(),
        sourcePaths: z.array(z.string()),
        uri: z.string(),
        body: z.string(),
        relatedUris: z.array(z.string()),
      },
    },
    async (args) => {
      const result = getRequirement(args);
      return {
        content: [{ type: "text", text: result.body }],
        structuredContent: result,
      };
    },
  );

  server.registerTool(
    "get_artifact",
    {
      description: "Return a canonical SpecTrace artifact by artifact id.",
      inputSchema: {
        artifact_id: z.string().describe("Artifact id such as SPEC-STD"),
        include_requirements: z.boolean().default(true),
      },
      outputSchema: {
        artifact_id: z.string().optional(),
        title: z.string(),
        artifact_type: z.string().optional(),
        domain: z.string().optional(),
        capability: z.string().optional(),
        sourcePaths: z.array(z.string()),
        uri: z.string(),
        summary: z.string(),
        body: z.string(),
        requirements: z.array(
          z.object({
            requirement_id: z.string().optional(),
            title: z.string(),
            statement: z.string(),
            uri: z.string(),
          }),
        ),
        relatedUris: z.array(z.string()),
      },
    },
    async (args) => {
      const result = getArtifact(args);
      return {
        content: [{ type: "text", text: result.body }],
        structuredContent: result,
      };
    },
  );

  server.registerTool(
    "get_guidance",
    {
      description: "Return full SpecTrace guidance documentation by topic or alias.",
      inputSchema: {
        topic: z
          .string()
          .describe("Guidance topic such as document-to-requirements, rfc-to-requirements, requirement-slicing, authoring, or overview"),
      },
      outputSchema: {
        topic: z.string(),
        title: z.string(),
        summary: z.string(),
        uri: z.string(),
        sourcePaths: z.array(z.string()),
        body: z.string(),
        relatedUris: z.array(z.string()),
      },
    },
    async (args) => {
      const result = getGuidance(args);
      return {
        content: [{ type: "text", text: result.body }],
        structuredContent: result,
      };
    },
  );
}

function createServer() {
  const server = new McpServer({ name: serverName, version: packageVersion }, { capabilities: { logging: {} } });
  registerResources(server);
  registerTools(server);
  return server;
}

async function createRoutedRequest(request: Request, routedUrl: URL) {
  if (request.url === routedUrl.href) {
    return request;
  }

  const init: RequestInit = {
    method: request.method,
    headers: new Headers(request.headers),
  };

  if (request.method !== "GET" && request.method !== "HEAD") {
    init.body = await request.clone().arrayBuffer();
  }

  return new Request(routedUrl.href, init);
}

async function handleMcpRequest(request: Request, routedUrl: URL) {
  const server = createServer();
  const transport = new WebStandardStreamableHTTPServerTransport({
    sessionIdGenerator: undefined,
    enableJsonResponse: true,
  });

  await server.connect(transport);
  return transport.handleRequest(await createRoutedRequest(request, routedUrl));
}

function findResourceForRequest(url) {
  if (url.pathname === "/mcp" || url.pathname === "/") {
    return null;
  }

  const resourcePrefix = "/mcp/resource/";
  const rawResourcePrefix = "/resource/";
  let encodedUri = "";

  if (url.pathname.startsWith(resourcePrefix)) {
    encodedUri = url.pathname.slice(resourcePrefix.length);
  } else if (url.pathname.startsWith(rawResourcePrefix)) {
    encodedUri = url.pathname.slice(rawResourcePrefix.length);
  } else if (url.searchParams.has("uri")) {
    encodedUri = url.searchParams.get("uri") ?? "";
  }

  if (!encodedUri) {
    return null;
  }

  const decodedUri = decodeURIComponent(encodedUri);
  return lookupResourceFromUri(decodedUri) ?? lookupResourceFromFilePath(decodedUri);
}

export async function fetch(request: Request, env: WorkerEnv = {}) {
  const { pathPrefix, routedUrl } = createRouteContext(request, env);

  if (request.method === "POST" && routedUrl.pathname === "/mcp") {
    return handleMcpRequest(request, routedUrl);
  }

  if (request.method === "GET" && (routedUrl.pathname === "/mcp" || routedUrl.pathname === "/")) {
    return new Response(renderDocsIndexHtml(pathPrefix), {
      headers: { "content-type": "text/html; charset=utf-8" },
    });
  }

  if (request.method === "GET") {
    const resource = findResourceForRequest(routedUrl);
    if (resource) {
      return new Response(renderResourcePage(resource, pathPrefix), {
        headers: { "content-type": "text/html; charset=utf-8" },
      });
    }
  }

  return new Response("Not found", { status: 404 });
}

export default {
  fetch,
};
