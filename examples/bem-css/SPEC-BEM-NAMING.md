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
  - ARC-BEM-CSS-0001
  - VER-BEM-CSS-0001
  - SPEC-BEM-CONCEPTS
  - SPEC-BEM-CSS
---

# SPEC-BEM-NAMING - BEM Naming Grammar

## Purpose

Define the class-name grammar used by the BEM methodology.

## Scope

This specification covers block, element, and modifier name forms plus the negative rules that keep those forms unambiguous. It does not define CSS selectors or layout behavior.

## Context

The naming grammar is the part most teams implement first, so it needs concrete syntax rules instead of a vague style preference. The grammar also has to stay aligned with the shared terms in `SPEC-BEM-CONCEPTS` so the namespace rules remain meaningful.

## REQ-BEM-NAMING-0001 Match block names to the block-name pattern
A block name MUST match `^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$`.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0004
  - REQ-BEM-NAMING-0009
- Source Refs:
  - https://en.bem.info/methodology/naming-convention/
  - https://en.bem.info/methodology/quick-start/

Notes:
- The pattern keeps block names lowercase, hyphen-separated, and free of separator collisions.

## REQ-BEM-NAMING-0002 Use block names as namespaces
A block name MUST define the namespace for its elements and modifiers.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0001
  - REQ-BEM-CONCEPTS-0002
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

Notes:
- The namespace is the `block-name` prefix that anchors `block-name__element-name` and `block-name_modifier-name` forms.

## REQ-BEM-NAMING-0003 Use double underscores for element names
An element name MUST use the `block-name__element-name` form.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0002
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## REQ-BEM-NAMING-0004 Forbid element-of-element names
An element name MUST NOT use nested element separators such as `block__elem1__elem2`.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0003
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## REQ-BEM-NAMING-0005 Use single underscores for modifiers
A modifier name MUST use the `block-name_modifier-name` or `block-name__element-name_modifier-name` form.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0003
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## REQ-BEM-NAMING-0006 Require a keyed modifier value suffix
A keyed block or element modifier name MUST add a `_modifier-value` suffix.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0005
- Source Refs:
  - https://en.bem.info/methodology/quick-start/
  - https://en.bem.info/methodology/naming-convention/

## REQ-BEM-NAMING-0007 Omit the modifier value for Boolean modifiers
A Boolean block or element modifier MUST omit the modifier value.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0003
- Source Refs:
  - https://en.bem.info/methodology/quick-start/

Notes:
- The presence of the class is the signal for a Boolean modifier.

## REQ-BEM-NAMING-0008 Forbid standalone modifiers
A block or element modifier MUST NOT be used without its modified block or element.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-CONCEPTS-0003
- Source Refs:
  - https://en.bem.info/methodology/quick-start/

## REQ-BEM-NAMING-0009 Exclude separator characters from block names
A block name MUST NOT contain `__` or `_` separators.

Trace:
- Satisfied By:
  - ARC-BEM-CSS-0001
- Verified By:
  - VER-BEM-CSS-0001
- Related:
  - REQ-BEM-NAMING-0001
  - REQ-BEM-NAMING-0002
- Source Refs:
  - https://en.bem.info/methodology/naming-convention/
  - https://en.bem.info/methodology/quick-start/

Notes:
- Excluding separator characters keeps the block namespace unambiguous.
