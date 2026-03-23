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

# SPEC-BEM-CONCEPTS - BEM Concepts and Semantics

## Purpose

Define the core BEM terms that the naming, CSS, architecture, and verification rules rely on.

## Scope

This specification covers blocks, elements, modifiers, mixes, and the reuse boundaries that make BEM independent. It does not define class-name syntax or selector syntax.

## Context

The naming rules only make sense if the underlying BEM terms are defined first. This file gives those terms concrete meaning before the contract moves on to syntax, CSS behavior, and downstream trace.

In this example, shared meaning is captured as requirements so it can be referenced and verified by ID. That keeps the example traceable without adding a separate glossary artifact.

## REQ-BEM-CONCEPTS-0001 Define a block as an independent reusable unit
A block MUST be a functionally independent reusable unit.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0002
  - REQ-BEM-NAMING-0002
  - REQ-BEM-CSS-0007
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/

Notes:
- A block is the namespace root for its `block`, `block__element`, and `block_modifier` class families.

## REQ-BEM-CONCEPTS-0002 Define an element as part of a block
An element MUST be a composite part of a block that cannot be used separately from it.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0001
  - REQ-BEM-NAMING-0003
  - REQ-BEM-NAMING-0004
  - REQ-BEM-CSS-0010
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/html/

Notes:
- An element name stays local to its block and does not become a standalone namespace.

## REQ-BEM-CONCEPTS-0003 Define a modifier as a variation of a block or element
A modifier MUST define the appearance, state, or behavior of a block or element.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0005
  - REQ-BEM-NAMING-0006
  - REQ-BEM-NAMING-0007
  - REQ-BEM-NAMING-0008
  - REQ-BEM-CSS-0005
  - REQ-BEM-CSS-0010
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/block-modification/

Notes:
- A modifier adds variation to the base entity instead of replacing the base entity.

## REQ-BEM-CONCEPTS-0004 Keep block names semantic
A block name MUST describe purpose rather than appearance or state.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0001
  - REQ-BEM-NAMING-0008
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/css/

Notes:
- The block name is the semantic label that anchors the block namespace.

## REQ-BEM-CONCEPTS-0005 Keep element names semantic
An element name MUST describe purpose rather than appearance or state.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0003
  - REQ-BEM-NAMING-0004
- Source Refs:
  - https://en.bem.info/methodology/quick-start/

Notes:
- Element names stay descriptive of the element's role inside the block.

## REQ-BEM-CONCEPTS-0006 Allow mixes to combine BEM entities
A mix MAY combine multiple BEM entities on the same DOM node.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CSS-0007
  - REQ-BEM-CSS-0009
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/html/

Notes:
- A mix combines existing entities; it does not create a new semantic entity.

## REQ-BEM-CONCEPTS-0007 Keep blocks independent from external geometry
A block SHOULD NOT set its own external geometry or positioning.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CSS-0008
  - REQ-BEM-CSS-0009
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/css/
  - https://en.bem.info/methodology/html/

Notes:
- External layout belongs to the parent context or to a mix, not to the block itself.
