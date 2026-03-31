---
artifact_id: SPEC-UI-BUTTON
artifact_type: specification
title: UI Button Component Contract
domain: ui-design-system
capability: button-component
status: approved
owner: design-system
tags:
  - ui
  - design-system
  - button
  - component
related_artifacts:
  - SPEC-UI-CONVENTIONS
  - SPEC-UI-TOKENS
  - SPEC-UI-FOUNDATION
---

# [`SPEC-UI-BUTTON`](./SPEC-UI-BUTTON.md) - UI Button Component Contract

## Purpose

Define the shared contract for button surfaces in the UI design system.

## Scope

This specification covers the `ui-button` block, its primary and secondary variants, its size modifiers, and its shared disabled and focus-visible states. It does not define iconography, menu behavior, or application-specific actions.

## Context

The button contract stays small so the example can show how a component spec consumes shared conventions, tokens, and foundation rules without restating those shared rules in full.

## [`REQ-UI-BUTTON-0001`](./SPEC-UI-BUTTON.md#req-ui-button-0001-expose-the-button-block-surface) Expose the button block surface
The button component MUST expose the `ui-button` block surface, satisfy [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md), and use `space-control-x-medium` for default inline spacing.

Trace:
- Related:
  - [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0001-use-a-ui-block-namespace)
  - [`REQ-UI-TOKENS-0003`](./SPEC-UI-TOKENS.md#req-ui-tokens-0003-define-the-standard-spacing-token)

## [`REQ-UI-BUTTON-0002`](./SPEC-UI-BUTTON.md#req-ui-button-0002-define-the-primary-button-variant) Define the primary button variant
The `ui-button--primary` modifier MUST use `color-action-primary` and satisfy [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md).

Trace:
- Related:
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)
  - [`REQ-UI-TOKENS-0001`](./SPEC-UI-TOKENS.md#req-ui-tokens-0001-define-the-primary-action-token)

## [`REQ-UI-BUTTON-0003`](./SPEC-UI-BUTTON.md#req-ui-button-0003-define-the-secondary-button-variant) Define the secondary button variant
The `ui-button--secondary` modifier MUST use `color-action-secondary` and satisfy [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md).

Trace:
- Related:
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)
  - [`REQ-UI-TOKENS-0002`](./SPEC-UI-TOKENS.md#req-ui-tokens-0002-define-the-secondary-action-token)

## [`REQ-UI-BUTTON-0004`](./SPEC-UI-BUTTON.md#req-ui-button-0004-define-button-size-modifiers) Define button size modifiers
The `ui-button--sm`, `ui-button--md`, and `ui-button--lg` modifiers MUST map to `control-height-small`, `control-height-medium`, and `control-height-large`.

Trace:
- Related:
  - [`REQ-UI-TOKENS-0004`](./SPEC-UI-TOKENS.md#req-ui-tokens-0004-define-the-compact-control-height-token)
  - [`REQ-UI-TOKENS-0005`](./SPEC-UI-TOKENS.md#req-ui-tokens-0005-define-the-standard-control-height-token)
  - [`REQ-UI-TOKENS-0006`](./SPEC-UI-TOKENS.md#req-ui-tokens-0006-define-the-spacious-control-height-token)
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)

## [`REQ-UI-BUTTON-0005`](./SPEC-UI-BUTTON.md#req-ui-button-0005-define-the-disabled-button-state) Define the disabled button state
The `ui-button--disabled` modifier MUST satisfy [`REQ-UI-FOUNDATION-0002`](./SPEC-UI-FOUNDATION.md).

Trace:
- Related:
  - [`REQ-UI-FOUNDATION-0002`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0002-define-disabled-state-expectations)
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)

## [`REQ-UI-BUTTON-0006`](./SPEC-UI-BUTTON.md#req-ui-button-0006-define-the-focus-visible-button-state) Define the focus-visible button state
The `ui-button--focus-visible` modifier MUST satisfy [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md).

Trace:
- Related:
  - [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0001-define-focus-visible-behavior)
  - [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md#req-ui-conventions-0003-use-bem-modifier-classes)
