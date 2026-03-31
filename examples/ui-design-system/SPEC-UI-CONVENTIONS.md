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

# [`SPEC-UI-CONVENTIONS`](./SPEC-UI-CONVENTIONS.md) - UI Naming Conventions

## Purpose

Define the shared BEM-style naming rules used by the UI design system.

## Scope

This specification covers the block, element, and modifier forms used by the button and input components in this example. It does not define token values or component behavior.

## Context

The component specs reuse the same naming contract so the example can show shared requirements without repeating the class-name rules in each component file.

## [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0001-use-a-ui-block-namespace) Use a UI block namespace
Shared UI block classes MUST use a `ui-` namespace prefix.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0001`](./SPEC-UI-BUTTON.md#req-ui-button-0001-expose-the-button-block-surface)
  - [`REQ-UI-INPUT-0001`](./SPEC-UI-INPUT.md#req-ui-input-0001-expose-the-input-block-surface)

## [`REQ-UI-CONVENTIONS-0002`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0002-use-bem-element-classes) Use BEM element classes
UI element classes MUST use the `block__element` form.

Trace:
- Related:
  - [`REQ-UI-INPUT-0002`](./SPEC-UI-INPUT.md#req-ui-input-0002-define-input-element-classes)

## [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes) Use BEM modifier classes
UI modifier classes MUST use the `block--modifier` or `block__element--modifier` form.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0002`](./SPEC-UI-BUTTON.md#req-ui-button-0002-define-the-primary-button-variant)
  - [`REQ-UI-BUTTON-0003`](./SPEC-UI-BUTTON.md#req-ui-button-0003-define-the-secondary-button-variant)
  - [`REQ-UI-BUTTON-0004`](./SPEC-UI-BUTTON.md#req-ui-button-0004-define-button-size-modifiers)
  - [`REQ-UI-BUTTON-0005`](./SPEC-UI-BUTTON.md#req-ui-button-0005-define-the-disabled-button-state)
  - [`REQ-UI-BUTTON-0006`](./SPEC-UI-BUTTON.md#req-ui-button-0006-define-the-focus-visible-button-state)
  - [`REQ-UI-INPUT-0003`](./SPEC-UI-INPUT.md#req-ui-input-0003-define-input-size-modifiers)
  - [`REQ-UI-INPUT-0004`](./SPEC-UI-INPUT.md#req-ui-input-0004-define-the-invalid-input-state)
  - [`REQ-UI-INPUT-0005`](./SPEC-UI-INPUT.md#req-ui-input-0005-define-the-disabled-input-state)
  - [`REQ-UI-INPUT-0006`](./SPEC-UI-INPUT.md#req-ui-input-0006-define-the-focus-visible-input-state)

Notes:
- Examples include `ui-button`, `ui-button--primary`, `ui-button--disabled`, `ui-input__field`, and `ui-input--invalid`.
