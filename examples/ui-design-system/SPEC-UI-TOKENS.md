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
  - [SPEC-UI-BUTTON](./SPEC-UI-BUTTON.md)
  - [SPEC-UI-INPUT](./SPEC-UI-INPUT.md)
---

# [`SPEC-UI-TOKENS`](./SPEC-UI-TOKENS.md) - UI Token Set

## Purpose

Define the canonical token identifiers used by the UI design system example.

## Scope

This specification covers a small set of shared tokens that the button and input components consume. It does not prescribe token storage, export format, or raw CSS values.

## Context

The token names stay abstract so the example can focus on traceable contracts rather than implementation details.

## [`REQ-UI-TOKENS-0001`](./SPEC-UI-TOKENS.md) Define the primary action token
The design system MUST define `color-action-primary` as the canonical token for primary action surfaces.

Trace:
- Related:
  - [REQ-UI-BUTTON-0002](./SPEC-UI-BUTTON.md)
  - [REQ-UI-FOUNDATION-0001](./SPEC-UI-FOUNDATION.md)

## [`REQ-UI-TOKENS-0002`](./SPEC-UI-TOKENS.md) Define the secondary action token
The design system MUST define `color-action-secondary` as the canonical token for secondary action surfaces.

Trace:
- Related:
  - [REQ-UI-BUTTON-0003](./SPEC-UI-BUTTON.md)

## [`REQ-UI-TOKENS-0003`](./SPEC-UI-TOKENS.md) Define the standard spacing token
The design system MUST define `space-control-x-medium` as the canonical horizontal spacing token for controls.

Trace:
- Related:
  - [REQ-UI-BUTTON-0001](./SPEC-UI-BUTTON.md)
  - [REQ-UI-INPUT-0001](./SPEC-UI-INPUT.md)

## [`REQ-UI-TOKENS-0004`](./SPEC-UI-TOKENS.md) Define the compact control-height token
The design system MUST define `control-height-small` as the canonical compact control height token.

Trace:
- Related:
  - [REQ-UI-BUTTON-0004](./SPEC-UI-BUTTON.md)
  - [REQ-UI-INPUT-0003](./SPEC-UI-INPUT.md)

## [`REQ-UI-TOKENS-0005`](./SPEC-UI-TOKENS.md) Define the standard control-height token
The design system MUST define `control-height-medium` as the canonical standard control height token.

Trace:
- Related:
  - [REQ-UI-BUTTON-0004](./SPEC-UI-BUTTON.md)
  - [REQ-UI-INPUT-0003](./SPEC-UI-INPUT.md)

## [`REQ-UI-TOKENS-0006`](./SPEC-UI-TOKENS.md) Define the spacious control-height token
The design system MUST define `control-height-large` as the canonical spacious control height token.

Trace:
- Related:
  - [REQ-UI-BUTTON-0004](./SPEC-UI-BUTTON.md)
  - [REQ-UI-INPUT-0003](./SPEC-UI-INPUT.md)
