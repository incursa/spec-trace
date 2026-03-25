# UI Design System Example

This directory is a compact Spec Trace example for a UI/CSS design system.

It shows:

- shared specifications for naming, tokens, and shared interaction rules
- component specifications that reuse those shared requirements instead of restating them
- BEM block, element, and modifier usage in practice
- token identifiers such as `color-action-primary` and `control-height-medium`

How to read it:

- [`SPEC-UI-CONVENTIONS`](./SPEC-UI-CONVENTIONS.md) defines the class-name contract.
- [`SPEC-UI-TOKENS`](./SPEC-UI-TOKENS.md) defines the reusable token identifiers.
- [`SPEC-UI-FOUNDATION`](./SPEC-UI-FOUNDATION.md) defines the shared focus-visible, disabled, and interaction rules.
- [`SPEC-UI-BUTTON`](./SPEC-UI-BUTTON.md) and [`SPEC-UI-INPUT`](./SPEC-UI-INPUT.md) point back to the shared requirements with inline references like [`REQ-UI-CONVENTIONS-0001`](./SPEC-UI-CONVENTIONS.md).

The example stays within specification files only. It does not add architecture, work-item, or verification artifacts.
