---
artifact_id: SPEC-BEM-CSS
artifact_type: specification
title: BEM CSS Selector and Layout Contract
domain: bem-css
capability: bem-css-selector-rules
status: approved
owner: frontend-platform
tags:
  - bem
  - css
  - methodology
  - selectors
  - mixes
related_artifacts:
  - ARC-BEM-CSS-0001
  - VER-BEM-CSS-0001
  - SPEC-BEM-CONCEPTS
  - SPEC-BEM-NAMING
---

# [`SPEC-BEM-CSS`](./SPEC-BEM-CSS.md) - BEM CSS Selector and Layout Contract

## Purpose

Define a compact, testable contract for the CSS rules used by the BEM methodology.

## Scope

This specification covers class-based selectors, nested selector limits, mixes, and the layout rules that keep BEM blocks reusable. It does not define class-name syntax or build tooling.

## Context

The official BEM documentation presents BEM as a reusable component methodology with class-based selector rules and mix-based composition. This example translates those rules into requirement form so the repository can show a real, useful specification instead of a synthetic one.

## [`REQ-BEM-CSS-0001`](./SPEC-BEM-CSS.md#req-bem-css-0001-use-class-selectors-for-bem-entities) Use class selectors for BEM entities
BEM entity styling MUST use class selectors.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-NAMING-0001`](./SPEC-BEM-NAMING.md#req-bem-naming-0001-match-block-names-to-the-block-name-pattern)
  - [`REQ-BEM-NAMING-0003`](./SPEC-BEM-NAMING.md#req-bem-naming-0003-use-double-underscores-for-element-names)
  - [`REQ-BEM-NAMING-0005`](./SPEC-BEM-NAMING.md#req-bem-naming-0005-use-single-underscores-for-modifiers)
  - [`REQ-BEM-NAMING-0006`](./SPEC-BEM-NAMING.md#req-bem-naming-0006-require-a-keyed-modifier-value-suffix)

## [`REQ-BEM-CSS-0002`](./SPEC-BEM-CSS.md#req-bem-css-0002-avoid-tag-and-id-selectors) Avoid tag and ID selectors
BEM entity styling MUST NOT use tag or ID selectors.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/quick-start/

## [`REQ-BEM-CSS-0003`](./SPEC-BEM-CSS.md#req-bem-css-0003-avoid-tag-and-class-selectors) Avoid tag-and-class selectors
BEM entity styling SHOULD NOT combine tags and classes in a selector.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/faq/?lang=en
- Related:
  - [`REQ-BEM-CSS-0001`](./SPEC-BEM-CSS.md#req-bem-css-0001-use-class-selectors-for-bem-entities)

## [`REQ-BEM-CSS-0004`](./SPEC-BEM-CSS.md#req-bem-css-0004-minimize-nested-selectors) Minimize nested selectors
Nested selectors SHOULD be minimized.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/

## [`REQ-BEM-CSS-0005`](./SPEC-BEM-CSS.md#req-bem-css-0005-limit-nested-selectors-to-block-state-or-theme-set-changes) Limit nested selectors to block state or theme set changes
Nested selectors MAY change element styles relative to a block state or theme set.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-CSS-0004`](./SPEC-BEM-CSS.md#req-bem-css-0004-minimize-nested-selectors)
  - [`REQ-BEM-CONCEPTS-0003`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0003-define-a-modifier-as-a-variation-of-a-block-or-element)

## [`REQ-BEM-CSS-0006`](./SPEC-BEM-CSS.md#req-bem-css-0006-avoid-combined-selectors) Avoid combined selectors
Combined selectors SHOULD NOT be used.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/faq/?lang=en
- Related:
  - [`REQ-BEM-CSS-0001`](./SPEC-BEM-CSS.md#req-bem-css-0001-use-class-selectors-for-bem-entities)

## [`REQ-BEM-CSS-0007`](./SPEC-BEM-CSS.md#req-bem-css-0007-allow-mixes-to-combine-entities-on-one-node) Allow mixes to combine entities on one node
A mix MAY combine multiple BEM entities on the same DOM node.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-CONCEPTS-0006`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0006-allow-mixes-to-combine-bem-entities)

## [`REQ-BEM-CSS-0008`](./SPEC-BEM-CSS.md#req-bem-css-0008-use-mixes-for-external-geometry-and-positioning) Use mixes for external geometry and positioning
External geometry and positioning SHOULD be set via the parent block or a mix.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-CONCEPTS-0007`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0007-keep-blocks-independent-from-external-geometry)

## [`REQ-BEM-CSS-0009`](./SPEC-BEM-CSS.md#req-bem-css-0009-avoid-wrapper-heavy-positioning) Avoid wrapper-heavy positioning
Implementations SHOULD use mixes instead of wrapper blocks when positioning one block relative to another.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/html/
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-CSS-0008`](./SPEC-BEM-CSS.md#req-bem-css-0008-use-mixes-for-external-geometry-and-positioning)
  - [`REQ-BEM-CONCEPTS-0006`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0006-allow-mixes-to-combine-bem-entities)

## [`REQ-BEM-CSS-0010`](./SPEC-BEM-CSS.md#req-bem-css-0010-use-selector-names-that-accurately-describe-the-entity) Use selector names that accurately describe the entity
The name of a selector SHOULD fully and accurately describe the BEM entity it represents.

Trace:
- Satisfied By:
  - [ARC-BEM-CSS-0001](./sample-architecture.md)
- Verified By:
  - [VER-BEM-CSS-0001](./sample-verification.md)
- Upstream Refs:
  - https://en.bem.info/methodology/css/
- Related:
  - [`REQ-BEM-CONCEPTS-0001`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0001-define-a-block-as-an-independent-reusable-unit)
  - [`REQ-BEM-CONCEPTS-0002`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0002-define-an-element-as-part-of-a-block)
  - [`REQ-BEM-CONCEPTS-0003`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0003-define-a-modifier-as-a-variation-of-a-block-or-element)

Notes:
- Example class names from the official docs include `menu__item`, `menu_hidden`, `menu__item_visible`, and `button button_size_m`.
- The example stays with the classic naming scheme and does not cover alternative naming variants.
