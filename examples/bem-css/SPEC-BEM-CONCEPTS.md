---
artifact_id: SPEC-BEM-CONCEPTS
artifact_type: specification
title: BEM Concepts and Semantics
domain: bem-css
capability: bem-concepts
status: approved
owner: frontend-platform
tags:
  - bem
  - css
  - methodology
  - concepts
  - semantics
related_artifacts:
  - ARC-BEM-CSS-0001
  - VER-BEM-CSS-0001
  - SPEC-BEM-NAMING
  - SPEC-BEM-CSS
---

# [`SPEC-BEM-CONCEPTS`](./SPEC-BEM-CONCEPTS.md) - BEM Concepts and Semantics

## Purpose

Define the core BEM terms that the naming, CSS, architecture, and verification rules rely on.

## Scope

This specification covers blocks, elements, modifiers, mixes, and the reuse boundaries that make BEM independent. It does not define class-name syntax or selector syntax.

## Context

The naming rules only make sense if the underlying BEM terms are defined first. This file gives those terms concrete meaning before the contract moves on to syntax, CSS behavior, and downstream trace.

In this example, shared meaning is captured as requirements so it can be referenced and verified by ID. That keeps the example traceable without adding a separate glossary artifact.

## [`REQ-BEM-CONCEPTS-0001`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0001-define-a-block-as-an-independent-reusable-unit) Define a block as an independent reusable unit
A block MUST be a functionally independent reusable unit.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/
- Related:
  - [`REQ-BEM-CONCEPTS-0002`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0002-define-an-element-as-part-of-a-block)
  - [`REQ-BEM-NAMING-0002`](./SPEC-BEM-NAMING.md#req-bem-naming-0002-use-block-names-as-namespaces)
  - [`REQ-BEM-CSS-0007`](./SPEC-BEM-CSS.md#req-bem-css-0007-allow-mixes-to-combine-entities-on-one-node)

Notes:
- A block is the namespace root for its `block`, `block__element`, and `block_modifier` class families.

## [`REQ-BEM-CONCEPTS-0002`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0002-define-an-element-as-part-of-a-block) Define an element as part of a block
An element MUST be a composite part of a block that cannot be used separately from it.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/
- Related:
  - [`REQ-BEM-CONCEPTS-0001`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0001-define-a-block-as-an-independent-reusable-unit)
  - [`REQ-BEM-NAMING-0003`](./SPEC-BEM-NAMING.md#req-bem-naming-0003-use-double-underscores-for-element-names)
  - [`REQ-BEM-NAMING-0004`](./SPEC-BEM-NAMING.md#req-bem-naming-0004-forbid-element-of-element-names)
  - [`REQ-BEM-CSS-0010`](./SPEC-BEM-CSS.md#req-bem-css-0010-use-selector-names-that-accurately-describe-the-entity)

Notes:
- An element name stays local to its block and does not become a standalone namespace.

## [`REQ-BEM-CONCEPTS-0003`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0003-define-a-modifier-as-a-variation-of-a-block-or-element) Define a modifier as a variation of a block or element
A modifier MUST define the appearance, state, or behavior of a block or element.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/block-modification/
- Related:
  - [`REQ-BEM-NAMING-0005`](./SPEC-BEM-NAMING.md#req-bem-naming-0005-use-single-underscores-for-modifiers)
  - [`REQ-BEM-NAMING-0006`](./SPEC-BEM-NAMING.md#req-bem-naming-0006-require-a-keyed-modifier-value-suffix)
  - [`REQ-BEM-NAMING-0007`](./SPEC-BEM-NAMING.md#req-bem-naming-0007-omit-the-modifier-value-for-boolean-modifiers)
  - [`REQ-BEM-NAMING-0008`](./SPEC-BEM-NAMING.md#req-bem-naming-0008-forbid-standalone-modifiers)
  - [`REQ-BEM-CSS-0005`](./SPEC-BEM-CSS.md#req-bem-css-0005-limit-nested-selectors-to-block-state-or-theme-set-changes)
  - [`REQ-BEM-CSS-0010`](./SPEC-BEM-CSS.md#req-bem-css-0010-use-selector-names-that-accurately-describe-the-entity)

Notes:
- A modifier adds variation to the base entity instead of replacing the base entity.

## [`REQ-BEM-CONCEPTS-0004`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0004-keep-block-names-semantic) Keep block names semantic
A block name MUST describe purpose rather than appearance or state.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-NAMING-0001`](./SPEC-BEM-NAMING.md#req-bem-naming-0001-match-block-names-to-the-block-name-pattern)
  - [`REQ-BEM-NAMING-0008`](./SPEC-BEM-NAMING.md#req-bem-naming-0008-forbid-standalone-modifiers)

Notes:
- The block name is the semantic label that anchors the block namespace.

## [`REQ-BEM-CONCEPTS-0005`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0005-keep-element-names-semantic) Keep element names semantic
An element name MUST describe purpose rather than appearance or state.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
- Related:
  - [`REQ-BEM-NAMING-0003`](./SPEC-BEM-NAMING.md#req-bem-naming-0003-use-double-underscores-for-element-names)
  - [`REQ-BEM-NAMING-0004`](./SPEC-BEM-NAMING.md#req-bem-naming-0004-forbid-element-of-element-names)

Notes:
- Element names stay descriptive of the element's role inside the block.

## [`REQ-BEM-CONCEPTS-0006`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0006-allow-mixes-to-combine-bem-entities) Allow mixes to combine BEM entities
A mix MAY combine multiple BEM entities on the same DOM node.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/html/
- Related:
  - [`REQ-BEM-CSS-0007`](./SPEC-BEM-CSS.md#req-bem-css-0007-allow-mixes-to-combine-entities-on-one-node)
  - [`REQ-BEM-CSS-0009`](./SPEC-BEM-CSS.md#req-bem-css-0009-avoid-wrapper-heavy-positioning)

Notes:
- A mix combines existing entities; it does not create a new semantic entity.

## [`REQ-BEM-CONCEPTS-0007`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0007-keep-blocks-independent-from-external-geometry) Keep blocks independent from external geometry
A block SHOULD NOT set its own external geometry or positioning.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/html/
- Related:
  - [`REQ-BEM-CSS-0008`](./SPEC-BEM-CSS.md#req-bem-css-0008-use-mixes-for-external-geometry-and-positioning)
  - [`REQ-BEM-CSS-0009`](./SPEC-BEM-CSS.md#req-bem-css-0009-avoid-wrapper-heavy-positioning)

Notes:
- External layout belongs to the parent context or to a mix, not to the block itself.
