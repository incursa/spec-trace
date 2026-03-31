---
artifact_id: SPEC-UI-FOUNDATION
artifact_type: specification
title: UI Foundation Interaction Rules
domain: ui-design-system
capability: ui-foundation-interaction-rules
status: approved
owner: design-system
tags:
  - ui
  - design-system
  - foundation
  - focus
  - disabled-state
related_artifacts:
  - SPEC-UI-BUTTON
  - SPEC-UI-INPUT
---

# [`SPEC-UI-FOUNDATION`](./SPEC-UI-FOUNDATION.md) - UI Foundation Interaction Rules

## Purpose

Define the shared interaction rules that button and input components reuse.

## Scope

This specification covers focus-visible behavior, disabled-state expectations, and the shared interaction rules that apply across the example components. It does not define component-specific variants or token catalogs.

## Context

The foundation layer keeps the component files short by holding the rules that should behave the same everywhere.

## [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0001-define-focus-visible-behavior) Define focus-visible behavior
Interactive controls MUST expose a visible focus indicator when keyboard focus lands on them.

Trace:
- Related:
  - [`REQ-UI-TOKENS-0001`](./SPEC-UI-TOKENS.md#req-ui-tokens-0001-define-the-primary-action-token)
  - [`REQ-UI-BUTTON-0006`](./SPEC-UI-BUTTON.md#req-ui-button-0006-define-the-focus-visible-button-state)
  - [`REQ-UI-INPUT-0006`](./SPEC-UI-INPUT.md#req-ui-input-0006-define-the-focus-visible-input-state)

## [`REQ-UI-FOUNDATION-0002`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0002-define-disabled-state-expectations) Define disabled-state expectations
Disabled controls MUST remain non-interactive.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0005`](./SPEC-UI-BUTTON.md#req-ui-button-0005-define-the-disabled-button-state)
  - [`REQ-UI-INPUT-0005`](./SPEC-UI-INPUT.md#req-ui-input-0005-define-the-disabled-input-state)

## [`REQ-UI-FOUNDATION-0003`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0003-define-shared-interaction-behavior) Define shared interaction behavior
Interactive controls MUST keep pointer and keyboard behavior aligned with their role.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0001`](./SPEC-UI-BUTTON.md#req-ui-button-0001-expose-the-button-block-surface)
  - [`REQ-UI-INPUT-0001`](./SPEC-UI-INPUT.md#req-ui-input-0001-expose-the-input-block-surface)
