---
artifact_id: SPEC-UI-CONVENTIONS
artifact_type: specification
title: UI Naming Conventions
domain: ui-design-system
capability: ui-naming-conventions
status: approved
owner: design-system
tags:
  - ui
  - design-system
  - bem
  - conventions
related_artifacts:
  - SPEC-UI-BUTTON
  - SPEC-UI-INPUT
---

# SPEC-UI-CONVENTIONS - UI Naming Conventions

## Purpose

Define the shared BEM-style naming rules used by the UI design system.

## Scope

This specification covers the block, element, and modifier forms used by the button and input components in this example. It does not define token values or component behavior.

## Context

The component specs reuse the same naming contract so the example can show shared requirements without repeating the class-name rules in each component file.

## REQ-UI-CONVENTIONS-0001 Use a UI block namespace
Shared UI block classes MUST use a `ui-` namespace prefix.

Trace:
- Related:
  - REQ-UI-BUTTON-0001
  - REQ-UI-INPUT-0001

## REQ-UI-CONVENTIONS-0002 Use BEM element classes
UI element classes MUST use the `block__element` form.

Trace:
- Related:
  - REQ-UI-INPUT-0002

## REQ-UI-CONVENTIONS-0003 Use BEM modifier classes
UI modifier classes MUST use the `block--modifier` or `block__element--modifier` form.

Trace:
- Related:
  - REQ-UI-BUTTON-0002
  - REQ-UI-BUTTON-0003
  - REQ-UI-BUTTON-0004
  - REQ-UI-BUTTON-0005
  - REQ-UI-BUTTON-0006
  - REQ-UI-INPUT-0003
  - REQ-UI-INPUT-0004
  - REQ-UI-INPUT-0005
  - REQ-UI-INPUT-0006

Notes:
- Examples include `ui-button`, `ui-button--primary`, `ui-button--disabled`, `ui-input__field`, and `ui-input--invalid`.
