---
artifact_id: SPEC-UI-INPUT
artifact_type: specification
title: UI Input Component Contract
domain: ui-design-system
capability: input-component
status: approved
owner: design-system
tags:
  - ui
  - design-system
  - input
  - component
related_artifacts:
  - SPEC-UI-CONVENTIONS
  - SPEC-UI-TOKENS
  - SPEC-UI-FOUNDATION
---

# [`SPEC-UI-INPUT`](./SPEC-UI-INPUT.md) - UI Input Component Contract

## Purpose

Define the shared contract for text input surfaces in the UI design system.

## Scope

This specification covers the `ui-input` block, its label and field elements, its size modifiers, and its invalid, disabled, and focus-visible states. It does not define validation policy or form submission behavior.

## Context

The input contract reuses the same conventions, tokens, and foundation rules as the button contract so the example can show a layered design system instead of a one-off component file.

## [`REQ-UI-INPUT-0001`](./SPEC-UI-INPUT.md#req-ui-input-0001-expose-the-input-block-surface) Expose the input block surface
The input component MUST expose the `ui-input` block surface, satisfy [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md), and use `space-control-x-medium` for default control spacing.

Trace:
- Related:
  - [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0001-use-a-ui-block-namespace)
  - [`REQ-UI-TOKENS-0003`](./SPEC-UI-TOKENS.md#req-ui-tokens-0003-define-the-standard-spacing-token)

## [`REQ-UI-INPUT-0002`](./SPEC-UI-INPUT.md#req-ui-input-0002-define-input-element-classes) Define input element classes
The `ui-input__label` and `ui-input__field` element classes MUST satisfy [`REQ-UI-CONVENTIONS-0002`](./SPEC-UI-CONVENTIONS.md).

Trace:
- Related:
  - [`REQ-UI-CONVENTIONS-0002`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0002-use-bem-element-classes)

## [`REQ-UI-INPUT-0003`](./SPEC-UI-INPUT.md#req-ui-input-0003-define-input-size-modifiers) Define input size modifiers
The `ui-input--sm`, `ui-input--md`, and `ui-input--lg` modifiers MUST map to `control-height-small`, `control-height-medium`, and `control-height-large`.

Trace:
- Related:
  - [`REQ-UI-TOKENS-0004`](./SPEC-UI-TOKENS.md#req-ui-tokens-0004-define-the-compact-control-height-token)
  - [`REQ-UI-TOKENS-0005`](./SPEC-UI-TOKENS.md#req-ui-tokens-0005-define-the-standard-control-height-token)
  - [`REQ-UI-TOKENS-0006`](./SPEC-UI-TOKENS.md#req-ui-tokens-0006-define-the-spacious-control-height-token)
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)

## [`REQ-UI-INPUT-0004`](./SPEC-UI-INPUT.md#req-ui-input-0004-define-the-invalid-input-state) Define the invalid input state
The `ui-input--invalid` modifier MUST signal validation failure and satisfy [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md).

Trace:
- Related:
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)

## [`REQ-UI-INPUT-0005`](./SPEC-UI-INPUT.md#req-ui-input-0005-define-the-disabled-input-state) Define the disabled input state
The `ui-input--disabled` modifier MUST satisfy [`REQ-UI-FOUNDATION-0002`](./SPEC-UI-FOUNDATION.md).

Trace:
- Related:
  - [`REQ-UI-FOUNDATION-0002`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0002-define-disabled-state-expectations)
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)

## [`REQ-UI-INPUT-0006`](./SPEC-UI-INPUT.md#req-ui-input-0006-define-the-focus-visible-input-state) Define the focus-visible input state
The `ui-input--focus-visible` modifier MUST satisfy [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md).

Trace:
- Related:
  - [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0001-define-focus-visible-behavior)
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)
