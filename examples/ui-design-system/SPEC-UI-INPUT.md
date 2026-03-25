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
  - [SPEC-UI-CONVENTIONS](./SPEC-UI-CONVENTIONS.md)
  - [SPEC-UI-TOKENS](./SPEC-UI-TOKENS.md)
  - [SPEC-UI-FOUNDATION](./SPEC-UI-FOUNDATION.md)
---

# [`SPEC-UI-INPUT`](./SPEC-UI-INPUT.md) - UI Input Component Contract

## Purpose

Define the shared contract for text input surfaces in the UI design system.

## Scope

This specification covers the `ui-input` block, its label and field elements, its size modifiers, and its invalid, disabled, and focus-visible states. It does not define validation policy or form submission behavior.

## Context

The input contract reuses the same conventions, tokens, and foundation rules as the button contract so the example can show a layered design system instead of a one-off component file.

## [`REQ-UI-INPUT-0001`](./SPEC-UI-INPUT.md) Expose the input block surface
The input component MUST expose the `ui-input` block surface, satisfy [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md), and use `space-control-x-medium` for default control spacing.

Trace:
- Related:
  - [REQ-UI-CONVENTIONS-0001](./SPEC-UI-CONVENTIONS.md)
  - [REQ-UI-TOKENS-0003](./SPEC-UI-TOKENS.md)

## [`REQ-UI-INPUT-0002`](./SPEC-UI-INPUT.md) Define input element classes
The `ui-input__label` and `ui-input__field` element classes MUST satisfy [`REQ-UI-CONVENTIONS-0002`](./SPEC-UI-CONVENTIONS.md).

Trace:
- Related:
  - [REQ-UI-CONVENTIONS-0002](./SPEC-UI-CONVENTIONS.md)

## [`REQ-UI-INPUT-0003`](./SPEC-UI-INPUT.md) Define input size modifiers
The `ui-input--sm`, `ui-input--md`, and `ui-input--lg` modifiers MUST map to `control-height-small`, `control-height-medium`, and `control-height-large`.

Trace:
- Related:
  - [REQ-UI-TOKENS-0004](./SPEC-UI-TOKENS.md)
  - [REQ-UI-TOKENS-0005](./SPEC-UI-TOKENS.md)
  - [REQ-UI-TOKENS-0006](./SPEC-UI-TOKENS.md)
  - [REQ-UI-CONVENTIONS-0003](./SPEC-UI-CONVENTIONS.md)

## [`REQ-UI-INPUT-0004`](./SPEC-UI-INPUT.md) Define the invalid input state
The `ui-input--invalid` modifier MUST signal validation failure and satisfy [`REQ-UI-CONVENTIONS-0003`](./SPEC-UI-CONVENTIONS.md).

Trace:
- Related:
  - [REQ-UI-CONVENTIONS-0003](./SPEC-UI-CONVENTIONS.md)

## [`REQ-UI-INPUT-0005`](./SPEC-UI-INPUT.md) Define the disabled input state
The `ui-input--disabled` modifier MUST satisfy [`REQ-UI-FOUNDATION-0002`](./SPEC-UI-FOUNDATION.md).

Trace:
- Related:
  - [REQ-UI-FOUNDATION-0002](./SPEC-UI-FOUNDATION.md)
  - [REQ-UI-CONVENTIONS-0003](./SPEC-UI-CONVENTIONS.md)

## [`REQ-UI-INPUT-0006`](./SPEC-UI-INPUT.md) Define the focus-visible input state
The `ui-input--focus-visible` modifier MUST satisfy [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md).

Trace:
- Related:
  - [REQ-UI-FOUNDATION-0001](./SPEC-UI-FOUNDATION.md)
  - [REQ-UI-CONVENTIONS-0003](./SPEC-UI-CONVENTIONS.md)
