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

# SPEC-BEM-CSS - BEM CSS Selector and Layout Contract

## Purpose

Define a compact, testable contract for the CSS rules used by the BEM methodology.

## Scope

This specification covers class-based selectors, nested selector limits, mixes, and the layout rules that keep BEM blocks reusable. It does not define class-name syntax or build tooling.

## Context

The official BEM documentation presents BEM as a reusable component methodology with class-based selector rules and mix-based composition. This example translates those rules into requirement form so the repository can show a real, useful specification instead of a synthetic one.

## REQ-BEM-CSS-0001 Use class selectors for BEM entities
BEM entity styling MUST use class selectors.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0001
  - REQ-BEM-NAMING-0003
  - REQ-BEM-NAMING-0005
  - REQ-BEM-NAMING-0006
- Source Refs:
  - https://en.bem.info/methodology/css/

## REQ-BEM-CSS-0002 Avoid tag and ID selectors
BEM entity styling MUST NOT use tag or ID selectors.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Source Refs:
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/quick-start/

## REQ-BEM-CSS-0003 Avoid tag-and-class selectors
BEM entity styling SHOULD NOT combine tags and classes in a selector.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CSS-0001
- Source Refs:
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/faq/?lang=en

## REQ-BEM-CSS-0004 Minimize nested selectors
Nested selectors SHOULD be minimized.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Source Refs:
  - https://en.bem.info/methodology/css/

## REQ-BEM-CSS-0005 Limit nested selectors to block state or theme set changes
Nested selectors MAY change element styles relative to a block state or theme set.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CSS-0004
  - REQ-BEM-CONCEPTS-0003
- Source Refs:
  - https://en.bem.info/methodology/css/

## REQ-BEM-CSS-0006 Avoid combined selectors
Combined selectors SHOULD NOT be used.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CSS-0001
- Source Refs:
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/faq/?lang=en

## REQ-BEM-CSS-0007 Allow mixes to combine entities on one node
A mix MAY combine multiple BEM entities on the same DOM node.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0006
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/
  - https://en.bem.info/methodology/css/

## REQ-BEM-CSS-0008 Use mixes for external geometry and positioning
External geometry and positioning SHOULD be set via the parent block or a mix.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0007
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/
  - https://en.bem.info/methodology/css/

## REQ-BEM-CSS-0009 Avoid wrapper-heavy positioning
Implementations SHOULD use mixes instead of wrapper blocks when positioning one block relative to another.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CSS-0008
  - REQ-BEM-CONCEPTS-0006
- Source Refs:
  - https://en.bem.info/methodology/html/
  - https://en.bem.info/methodology/css/

## REQ-BEM-CSS-0010 Use selector names that accurately describe the entity
The name of a selector SHOULD fully and accurately describe the BEM entity it represents.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0001
  - REQ-BEM-CONCEPTS-0002
  - REQ-BEM-CONCEPTS-0003
- Source Refs:
  - https://en.bem.info/methodology/css/

Notes:
- Example class names from the official docs include `menu__item`, `menu_hidden`, `menu__item_visible`, and `button button_size_m`.
- The example stays with the classic naming scheme and does not cover alternative naming variants.
