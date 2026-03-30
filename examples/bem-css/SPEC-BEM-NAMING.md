---
artifact_id: SPEC-BEM-NAMING
artifact_type: specification
title: BEM Naming Grammar
domain: bem-css
capability: bem-naming-grammar
status: approved
owner: frontend-platform
tags:
  - bem
  - css
  - methodology
  - naming
  - grammar
related_artifacts:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
  - [VER-BEM-CSS-0001](./sample-verification.md)
  - [SPEC-BEM-CONCEPTS](./SPEC-BEM-CONCEPTS.md)
  - [SPEC-BEM-CSS](./SPEC-BEM-CSS.md)
---

# [`SPEC-BEM-NAMING`](./SPEC-BEM-NAMING.md) - BEM Naming Grammar

## Purpose

Define the class-name grammar used by the BEM methodology.

## Scope

This specification covers block, element, and modifier name forms plus the negative rules that keep those forms unambiguous. It does not define CSS selectors or layout behavior.

## Context

The naming grammar is the part most teams implement first, so it needs concrete syntax rules instead of a vague style preference. The grammar also has to stay aligned with the shared terms in [`SPEC-BEM-CONCEPTS`](./SPEC-BEM-CONCEPTS.md) so the namespace rules remain meaningful.

## [`REQ-BEM-NAMING-0001`](./SPEC-BEM-NAMING.md) Match block names to the block-name pattern
A block name MUST match `^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$`.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-CONCEPTS-0004](./SPEC-BEM-CONCEPTS.md)
  - [REQ-BEM-NAMING-0009](./SPEC-BEM-NAMING.md)
- Upstream Refs:
  - https://en.bem.info/methodology/naming-convention/
  - https://en.bem.info/methodology/quick-start/

Notes:
- The pattern keeps block names lowercase, hyphen-separated, and free of separator collisions.

## [`REQ-BEM-NAMING-0002`](./SPEC-BEM-NAMING.md) Use block names as namespaces
A block name MUST define the namespace for its elements and modifiers.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-CONCEPTS-0001](./SPEC-BEM-CONCEPTS.md)
  - [REQ-BEM-CONCEPTS-0002](./SPEC-BEM-CONCEPTS.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

Notes:
- The namespace is the `block-name` prefix that anchors `block-name__element-name` and `block-name_modifier-name` forms.

## [`REQ-BEM-NAMING-0003`](./SPEC-BEM-NAMING.md) Use double underscores for element names
An element name MUST use the `block-name__element-name` form.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-CONCEPTS-0002](./SPEC-BEM-CONCEPTS.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## [`REQ-BEM-NAMING-0004`](./SPEC-BEM-NAMING.md) Forbid element-of-element names
An element name MUST NOT use nested element separators such as `block__elem1__elem2`.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-NAMING-0003](./SPEC-BEM-NAMING.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## [`REQ-BEM-NAMING-0005`](./SPEC-BEM-NAMING.md) Use single underscores for modifiers
A modifier name MUST use the `block-name_modifier-name` or `block-name__element-name_modifier-name` form.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-CONCEPTS-0003](./SPEC-BEM-CONCEPTS.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## [`REQ-BEM-NAMING-0006`](./SPEC-BEM-NAMING.md) Require a keyed modifier value suffix
A keyed block or element modifier name MUST add a `_modifier-value` suffix.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-NAMING-0005](./SPEC-BEM-NAMING.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## [`REQ-BEM-NAMING-0007`](./SPEC-BEM-NAMING.md) Omit the modifier value for Boolean modifiers
A Boolean block or element modifier MUST omit the modifier value.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-CONCEPTS-0003](./SPEC-BEM-CONCEPTS.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/

Notes:
- The presence of the class is the signal for a Boolean modifier.

## [`REQ-BEM-NAMING-0008`](./SPEC-BEM-NAMING.md) Forbid standalone modifiers
A block or element modifier MUST NOT be used without its modified block or element.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-CONCEPTS-0003](./SPEC-BEM-CONCEPTS.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/

## [`REQ-BEM-NAMING-0009`](./SPEC-BEM-NAMING.md) Exclude separator characters from block names
A block name MUST NOT contain `__` or `_` separators.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Related:
  - [REQ-BEM-NAMING-0001](./SPEC-BEM-NAMING.md)
  - [REQ-BEM-NAMING-0002](./SPEC-BEM-NAMING.md)
- Upstream Refs:
  - https://en.bem.info/methodology/naming-convention/
  - https://en.bem.info/methodology/quick-start/

Notes:
- Excluding separator characters keeps the block namespace unambiguous.
