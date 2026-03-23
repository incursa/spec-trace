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

# SPEC-UI-BUTTON - UI Button Component Contract

## Purpose

Define the shared contract for button surfaces in the UI design system.

## Scope

This specification covers the `ui-button` block, its primary and secondary variants, its size modifiers, and its shared disabled and focus-visible states. It does not define iconography, menu behavior, or application-specific actions.

## Context

The button contract stays small so the example can show how a component spec consumes shared conventions, tokens, and foundation rules without restating those shared rules in full.

## REQ-UI-BUTTON-0001 Expose the button block surface
The button component MUST expose the `ui-button` block surface, satisfy `REQ-UI-CONVENTIONS-0001`, and use `space-control-x-medium` for default inline spacing.

Trace:
- Related:
  - REQ-UI-CONVENTIONS-0001
  - REQ-UI-TOKENS-0003

## REQ-UI-BUTTON-0002 Define the primary button variant
The `ui-button--primary` modifier MUST use `color-action-primary` and satisfy `REQ-UI-CONVENTIONS-0003`.

Trace:
- Related:
  - REQ-UI-CONVENTIONS-0003
  - REQ-UI-TOKENS-0001

## REQ-UI-BUTTON-0003 Define the secondary button variant
The `ui-button--secondary` modifier MUST use `color-action-secondary` and satisfy `REQ-UI-CONVENTIONS-0003`.

Trace:
- Related:
  - REQ-UI-CONVENTIONS-0003
  - REQ-UI-TOKENS-0002

## REQ-UI-BUTTON-0004 Define button size modifiers
The `ui-button--sm`, `ui-button--md`, and `ui-button--lg` modifiers MUST map to `control-height-small`, `control-height-medium`, and `control-height-large`.

Trace:
- Related:
  - REQ-UI-TOKENS-0004
  - REQ-UI-TOKENS-0005
  - REQ-UI-TOKENS-0006
  - REQ-UI-CONVENTIONS-0003

## REQ-UI-BUTTON-0005 Define the disabled button state
The `ui-button--disabled` modifier MUST satisfy `REQ-UI-FOUNDATION-0002`.

Trace:
- Related:
  - REQ-UI-FOUNDATION-0002
  - REQ-UI-CONVENTIONS-0003

## REQ-UI-BUTTON-0006 Define the focus-visible button state
The `ui-button--focus-visible` modifier MUST satisfy `REQ-UI-FOUNDATION-0001`.

Trace:
- Related:
  - REQ-UI-FOUNDATION-0001
  - REQ-UI-CONVENTIONS-0003
