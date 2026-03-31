---
artifact_id: SPEC-UI-TOKENS
artifact_type: specification
title: UI Token Set
domain: ui-design-system
capability: ui-token-set
status: approved
owner: design-system
tags:
  - ui
  - design-system
  - tokens
  - color
  - spacing
related_artifacts:
  - SPEC-UI-BUTTON
  - SPEC-UI-INPUT
---

# [`SPEC-UI-TOKENS`](./SPEC-UI-TOKENS.md) - UI Token Set

## Purpose

Define the canonical token identifiers used by the UI design system example.

## Scope

This specification covers a small set of shared tokens that the button and input components consume. It does not prescribe token storage, export format, or raw CSS values.

## Context

The token names stay abstract so the example can focus on traceable contracts rather than implementation details.

## [`REQ-UI-TOKENS-0001`](./SPEC-UI-TOKENS.md#req-ui-tokens-0001-define-the-primary-action-token) Define the primary action token
The design system MUST define `color-action-primary` as the canonical token for primary action surfaces.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0002`](./SPEC-UI-BUTTON.md#req-ui-button-0002-define-the-primary-button-variant)
  - [`REQ-UI-FOUNDATION-0001`](./SPEC-UI-FOUNDATION.md#req-ui-foundation-0001-define-focus-visible-behavior)

## [`REQ-UI-TOKENS-0002`](./SPEC-UI-TOKENS.md#req-ui-tokens-0002-define-the-secondary-action-token) Define the secondary action token
The design system MUST define `color-action-secondary` as the canonical token for secondary action surfaces.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0003`](./SPEC-UI-BUTTON.md#req-ui-button-0003-define-the-secondary-button-variant)

## [`REQ-UI-TOKENS-0003`](./SPEC-UI-TOKENS.md#req-ui-tokens-0003-define-the-standard-spacing-token) Define the standard spacing token
The design system MUST define `space-control-x-medium` as the canonical horizontal spacing token for controls.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0001`](./SPEC-UI-BUTTON.md#req-ui-button-0001-expose-the-button-block-surface)
  - [`REQ-UI-INPUT-0001`](./SPEC-UI-INPUT.md#req-ui-input-0001-expose-the-input-block-surface)

## [`REQ-UI-TOKENS-0004`](./SPEC-UI-TOKENS.md#req-ui-tokens-0004-define-the-compact-control-height-token) Define the compact control-height token
The design system MUST define `control-height-small` as the canonical compact control height token.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0004`](./SPEC-UI-BUTTON.md#req-ui-button-0004-define-button-size-modifiers)
  - [`REQ-UI-INPUT-0003`](./SPEC-UI-INPUT.md#req-ui-input-0003-define-input-size-modifiers)

## [`REQ-UI-TOKENS-0005`](./SPEC-UI-TOKENS.md#req-ui-tokens-0005-define-the-standard-control-height-token) Define the standard control-height token
The design system MUST define `control-height-medium` as the canonical standard control height token.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0004`](./SPEC-UI-BUTTON.md#req-ui-button-0004-define-button-size-modifiers)
  - [`REQ-UI-INPUT-0003`](./SPEC-UI-INPUT.md#req-ui-input-0003-define-input-size-modifiers)

## [`REQ-UI-TOKENS-0006`](./SPEC-UI-TOKENS.md#req-ui-tokens-0006-define-the-spacious-control-height-token) Define the spacious control-height token
The design system MUST define `control-height-large` as the canonical spacious control height token.

Trace:
- Related:
  - [`REQ-UI-BUTTON-0004`](./SPEC-UI-BUTTON.md#req-ui-button-0004-define-button-size-modifiers)
  - [`REQ-UI-INPUT-0003`](./SPEC-UI-INPUT.md#req-ui-input-0003-define-input-size-modifiers)
