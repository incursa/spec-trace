import { mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import matter from "gray-matter";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const rootDir = path.resolve(scriptDir, "..");
const contentDir = path.join(rootDir, "content");
const outDir = path.join(rootDir, "dist", "mcp");
const packageJson = JSON.parse(await readFile(path.join(rootDir, "package.json"), "utf8"));
const mcpConfig = JSON.parse(await readFile(path.join(rootDir, "mcp.config.json"), "utf8"));

const namespace = mcpConfig.namespace ?? "spec-trace";
const packageName = mcpConfig.packageName ?? packageJson.name ?? "@incursa/spec-trace-mcp";
const packageVersion = packageJson.version ?? "0.0.0";
const serverName = mcpConfig.serverName ?? "spec-trace-docs";
const displayName = mcpConfig.displayName ?? "SpecTrace Documentation";
const serverSummary =
  mcpConfig.summary ?? "A deterministic Cloudflare Worker MCP server built from SpecTrace markdown documentation.";

if (packageJson.name && packageJson.name !== packageName) {
  throw new Error(`package.json name "${packageJson.name}" must match mcp.config.json packageName "${packageName}"`);
}

const groupOrder = ["core", "guides", "components", "patterns", "specs", "examples", "ai"];
const allowedGroups = new Set(groupOrder);
const allowedKinds = new Set(["guide", "component", "pattern", "spec", "example"]);
const allowedSearchKinds = new Set([...allowedKinds, "any"]);

function normalizeText(value) {
  return String(value ?? "")
    .replace(/\r\n/g, "\n")
    .replace(/\u00a0/g, " ")
    .replace(/[ \t]+\n/g, "\n")
    .replace(/\n{3,}/g, "\n\n")
    .replace(/[ \t]{2,}/g, " ")
    .trim();
}

function normalizeKey(value) {
  return normalizeText(value)
    .toLowerCase()
    .replace(/['"]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

function toPosix(filePath) {
  return filePath.split(path.sep).join("/");
}

function titleFromSlug(slug) {
  return slug
    .split(/[-_/]+/g)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

function inferGroup(relativePath) {
  const [firstPart] = relativePath.split("/");
  if (["guides", "components", "patterns", "specs", "examples", "ai"].includes(firstPart)) {
    return firstPart;
  }
  return "core";
}

function inferKind(group) {
  return (
    {
      core: "guide",
      guides: "guide",
      components: "component",
      patterns: "pattern",
      specs: "spec",
      examples: "example",
      ai: "guide",
    }[group] ?? "guide"
  );
}

function defaultPriority(group) {
  return (
    {
      core: 120,
      guides: 90,
      components: 80,
      patterns: 70,
      specs: 60,
      examples: 50,
      ai: 40,
      files: 10,
    }[group] ?? 50
  );
}

function ensureArray(value) {
  if (Array.isArray(value)) {
    return value.map((item) => String(item)).filter((item) => item.trim().length > 0);
  }
  if (value == null || value === "") {
    return [];
  }
  return [String(value)];
}

function ensureStringField(value, fieldName, relativePath, required = false) {
  if (value == null || value === "") {
    if (required) {
      throw new Error(`Missing required front matter field "${fieldName}" in ${relativePath}`);
    }
    return "";
  }

  if (typeof value !== "string" && typeof value !== "number") {
    throw new Error(`Front matter field "${fieldName}" in ${relativePath} must be a string or number`);
  }

  return String(value);
}

function ensureBooleanField(value, fieldName, relativePath) {
  if (value == null) {
    return undefined;
  }
  if (typeof value !== "boolean") {
    throw new Error(`Front matter field "${fieldName}" in ${relativePath} must be a boolean`);
  }
  return value;
}

function ensureNumberField(value, fieldName, relativePath) {
  if (value == null || value === "") {
    return undefined;
  }
  const numeric = Number(value);
  if (!Number.isFinite(numeric)) {
    throw new Error(`Front matter field "${fieldName}" in ${relativePath} must be numeric`);
  }
  return numeric;
}

function ensureStringArrayField(value, fieldName, relativePath) {
  if (value == null || value === "") {
    return [];
  }
  if (!Array.isArray(value)) {
    throw new Error(`Front matter field "${fieldName}" in ${relativePath} must be an array of strings`);
  }
  return value.map((item, index) => {
    if (typeof item !== "string") {
      throw new Error(`Front matter field "${fieldName}" in ${relativePath} must contain only strings (item ${index + 1})`);
    }
    const normalized = item.trim();
    if (!normalized) {
      throw new Error(`Front matter field "${fieldName}" in ${relativePath} contains an empty value`);
    }
    return normalized;
  });
}

function deriveUri(frontMatter, group, slug) {
  if (frontMatter.uri) {
    return String(frontMatter.uri);
  }
  return group === "core" ? `${namespace}://${slug}` : `${namespace}://${group}/${slug}`;
}

function deriveSummary(frontMatter, body) {
  if (frontMatter.summary) {
    return String(frontMatter.summary);
  }

  const paragraphs = normalizeText(body)
    .split(/\n\s*\n/g)
    .map((part) => part.trim())
    .filter(Boolean);
  const candidate = paragraphs.find((part) => !part.startsWith("#")) ?? paragraphs[0] ?? "";
  return candidate.replace(/^#+\s*/, "").slice(0, 180);
}

function buildSearchText(resource) {
  return normalizeText(
    [
      resource.title,
      resource.summary,
      resource.kind,
      resource.searchKind,
      resource.group,
      resource.aliases.join(" "),
      resource.tags.join(" "),
      resource.relatedUris.join(" "),
      resource.sourcePaths.join(" "),
      resource.uri,
      resource.body,
    ]
      .filter(Boolean)
      .join("\n"),
  ).toLowerCase();
}

async function walkMarkdownFiles(dir, files = []) {
  const entries = await readdir(dir, { withFileTypes: true });
  for (const entry of entries) {
    const absolute = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      await walkMarkdownFiles(absolute, files);
    } else if (entry.isFile() && entry.name.endsWith(".md")) {
      files.push(absolute);
    }
  }
  return files;
}

async function writeJson(relativePath, value) {
  const filePath = path.join(outDir, relativePath);
  await mkdir(path.dirname(filePath), { recursive: true });
  await writeFile(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

const markdownFiles = (await walkMarkdownFiles(contentDir)).sort((left, right) => left.localeCompare(right));
const resources = [];
const seenUris = new Map();
const seenSlugKeys = new Map();

for (const filePath of markdownFiles) {
  const raw = await readFile(filePath, "utf8");
  const parsed = matter(raw);
  const frontMatter = parsed.data ?? {};
  const relativePath = toPosix(path.relative(contentDir, filePath));
  if (typeof frontMatter !== "object" || Array.isArray(frontMatter)) {
    throw new Error(`Front matter in ${relativePath} must be an object`);
  }

  const slugSource = ensureStringField(frontMatter.slug ?? path.basename(filePath, ".md"), "slug", relativePath, true);
  const slug = normalizeKey(slugSource);
  if (!slug) {
    throw new Error(`Front matter in ${relativePath} produced an empty slug`);
  }

  const group = ensureStringField(frontMatter.group ?? inferGroup(relativePath), "group", relativePath, true);
  if (!allowedGroups.has(group)) {
    throw new Error(`Front matter in ${relativePath} uses unsupported group "${group}"`);
  }

  const kind = ensureStringField(frontMatter.kind ?? inferKind(group), "kind", relativePath, true);
  if (!allowedKinds.has(kind)) {
    throw new Error(`Front matter in ${relativePath} uses unsupported kind "${kind}"`);
  }

  const searchKind = ensureStringField(frontMatter.searchKind ?? kind, "searchKind", relativePath);
  if (!allowedSearchKinds.has(searchKind)) {
    throw new Error(`Front matter in ${relativePath} uses unsupported searchKind "${searchKind}"`);
  }

  const uri = deriveUri(frontMatter, group, slug);
  if (!uri.startsWith(`${namespace}://`)) {
    throw new Error(`Front matter in ${relativePath} must use the ${namespace}:// namespace`);
  }

  if (seenUris.has(uri)) {
    throw new Error(`Duplicate MCP URI "${uri}" generated by ${relativePath} and ${seenUris.get(uri)}`);
  }
  seenUris.set(uri, relativePath);

  const slugKey = `${group}::${slug}`;
  if (seenSlugKeys.has(slugKey)) {
    throw new Error(`Duplicate slug "${slug}" in group "${group}" for ${relativePath} and ${seenSlugKeys.get(slugKey)}`);
  }
  seenSlugKeys.set(slugKey, relativePath);

  const title = ensureStringField(frontMatter.title ?? titleFromSlug(slug), "title", relativePath, true);
  const body = normalizeText(parsed.content);
  const summary = ensureStringField(frontMatter.summary ?? deriveSummary(frontMatter, body), "summary", relativePath);
  const aliases = ensureStringArrayField(frontMatter.aliases, "aliases", relativePath);
  const relatedUris = ensureStringArrayField(frontMatter.relatedUris, "relatedUris", relativePath);
  const tags = ensureStringArrayField(frontMatter.tags, "tags", relativePath);
  const priority = ensureNumberField(frontMatter.priority, "priority", relativePath) ?? defaultPriority(group);
  const includeInSearch = ensureBooleanField(frontMatter.includeInSearch, "includeInSearch", relativePath);

  resources.push({
    uri,
    title,
    kind,
    searchKind,
    summary,
    body,
    sourcePaths: [relativePath],
    mimeType: "text/markdown; charset=utf-8",
    aliases,
    relatedUris,
    group,
    priority,
    includeInSearch: includeInSearch ?? true,
    tags,
    searchText: buildSearchText({
      title,
      summary,
      kind,
      searchKind,
      group,
      aliases,
      tags,
      relatedUris,
      sourcePaths: [relativePath],
      uri,
      body,
    }),
  });
}

const resourceUris = new Set(resources.map((resource) => resource.uri));
for (const resource of resources) {
  for (const relatedUri of resource.relatedUris) {
    if (relatedUri.startsWith(`${namespace}://`) && !resourceUris.has(relatedUri)) {
      throw new Error(`Resource ${resource.uri} references missing related URI ${relatedUri}`);
    }
  }
}

resources.sort((left, right) => {
  if (left.priority !== right.priority) {
    return right.priority - left.priority;
  }
  return left.title.localeCompare(right.title);
});

const searchIndex = resources
  .filter((resource) => resource.includeInSearch !== false)
  .map(({ body, ...resource }) => ({
    ...resource,
    excerpt: body.slice(0, 240),
  }));

const groupedResources = {};
for (const resource of resources) {
  const bucket = groupedResources[resource.group] ?? [];
  bucket.push(resource);
  groupedResources[resource.group] = bucket;
}

const manifest = {
  displayName,
  serverName,
  summary: serverSummary,
  namespace,
  packageName,
  packageVersion,
  resourceTemplates: [`${namespace}://file/{path}`],
  frontMatterContract: {
    required: ["title", "kind", "group"],
    optional: ["uri", "slug", "summary", "aliases", "relatedUris", "tags", "priority", "includeInSearch", "searchKind"],
  },
  searchFields: ["title", "summary", "aliases", "tags", "body", "sourcePaths", "relatedUris", "uri"],
  resources,
  searchTool: {
    name: "search_docs",
    description: "Search the static markdown docs index.",
  },
  groupedResources,
};

await rm(outDir, { recursive: true, force: true });
await mkdir(outDir, { recursive: true });
await writeJson("manifest.json", manifest);
await writeJson("resources.json", manifest);
await writeJson("search-index.json", searchIndex);

for (const [group, entries] of Object.entries(groupedResources)) {
  await writeJson(`${group}.json`, entries);
}

console.log(`Generated MCP manifests in ${path.relative(rootDir, outDir)}`);
