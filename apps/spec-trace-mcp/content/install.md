---
{
  "uri": "spec-trace://install",
  "slug": "install",
  "title": "Install",
  "summary": "How to install the server and start editing the markdown files.",
  "kind": "guide",
  "group": "core",
  "aliases": ["setup", "getting-started"],
  "relatedUris": [
    "spec-trace://overview",
    "spec-trace://update"
  ],
  "tags": ["install", "setup", "npm"],
  "priority": 115,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Install

Use the repo as a template and treat the markdown files as the editable surface.

```bash
npm ci --prefix apps/spec-trace-mcp
npm run build:mcp
```

Edit `apps/spec-trace-mcp/content/**/*.md`, rebuild, and deploy the Worker.

If you clone the server for another project, the first things to change are:

- the package name in `apps/spec-trace-mcp/package.json`
- the namespace in the markdown front matter
- the content files themselves
