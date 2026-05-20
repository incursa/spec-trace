---
{
  "uri": "spec-trace://update",
  "slug": "update",
  "title": "Update",
  "summary": "How to refresh content and verify the generated manifests.",
  "kind": "guide",
  "group": "core",
  "aliases": ["refresh", "upgrade"],
  "relatedUris": [
    "spec-trace://install",
    "spec-trace://specs/verification-index"
  ],
  "tags": ["update", "build", "generated"],
  "priority": 112,
  "includeInSearch": true,
  "searchKind": "guide"
}
---

# Update

When the docs change, update the markdown files and rerun the build.

```bash
npm run build:mcp
npm test
```

The generated files in `apps/spec-trace-mcp/dist/mcp/` should always be treated as build output, not hand-edited content.
