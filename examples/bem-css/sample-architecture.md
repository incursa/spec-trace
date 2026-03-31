---
artifact_id: ARC-BEM-CSS-0001
artifact_type: architecture
title: BEM Traceability and Composition Design
domain: bem-css
status: approved
owner: frontend-platform
related_artifacts:
  - SPEC-BEM-CONCEPTS
  - SPEC-BEM-NAMING
  - SPEC-BEM-CSS
  - VER-BEM-CSS-0001
satisfies:
  - REQ-BEM-CONCEPTS-0001
  - REQ-BEM-CONCEPTS-0002
  - REQ-BEM-CONCEPTS-0003
  - REQ-BEM-CONCEPTS-0004
  - REQ-BEM-CONCEPTS-0005
  - REQ-BEM-CONCEPTS-0006
  - REQ-BEM-CONCEPTS-0007
  - REQ-BEM-NAMING-0001
  - REQ-BEM-NAMING-0002
  - REQ-BEM-NAMING-0003
  - REQ-BEM-NAMING-0004
  - REQ-BEM-NAMING-0005
  - REQ-BEM-NAMING-0006
  - REQ-BEM-NAMING-0007
  - REQ-BEM-NAMING-0008
  - REQ-BEM-NAMING-0009
  - REQ-BEM-CSS-0001
  - REQ-BEM-CSS-0002
  - REQ-BEM-CSS-0003
  - REQ-BEM-CSS-0004
  - REQ-BEM-CSS-0005
  - REQ-BEM-CSS-0006
  - REQ-BEM-CSS-0007
  - REQ-BEM-CSS-0008
  - REQ-BEM-CSS-0009
  - REQ-BEM-CSS-0010
---

# [`ARC-BEM-CSS-0001`](./sample-architecture.md) - BEM Traceability and Composition Design

## Purpose

Explain how a BEM implementation satisfies the shared concept, naming, and CSS requirements.

## Requirements Satisfied

- [`REQ-BEM-CONCEPTS-0001`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0001-define-a-block-as-an-independent-reusable-unit)
- [`REQ-BEM-CONCEPTS-0002`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0002-define-an-element-as-part-of-a-block)
- [`REQ-BEM-CONCEPTS-0003`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0003-define-a-modifier-as-a-variation-of-a-block-or-element)
- [`REQ-BEM-CONCEPTS-0004`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0004-keep-block-names-semantic)
- [`REQ-BEM-CONCEPTS-0005`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0005-keep-element-names-semantic)
- [`REQ-BEM-CONCEPTS-0006`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0006-allow-mixes-to-combine-bem-entities)
- [`REQ-BEM-CONCEPTS-0007`](./SPEC-BEM-CONCEPTS.md#req-bem-concepts-0007-keep-blocks-independent-from-external-geometry)
- [`REQ-BEM-NAMING-0001`](./SPEC-BEM-NAMING.md#req-bem-naming-0001-match-block-names-to-the-block-name-pattern)
- [`REQ-BEM-NAMING-0002`](./SPEC-BEM-NAMING.md#req-bem-naming-0002-use-block-names-as-namespaces)
- [`REQ-BEM-NAMING-0003`](./SPEC-BEM-NAMING.md#req-bem-naming-0003-use-double-underscores-for-element-names)
- [`REQ-BEM-NAMING-0004`](./SPEC-BEM-NAMING.md#req-bem-naming-0004-forbid-element-of-element-names)
- [`REQ-BEM-NAMING-0005`](./SPEC-BEM-NAMING.md#req-bem-naming-0005-use-single-underscores-for-modifiers)
- [`REQ-BEM-NAMING-0006`](./SPEC-BEM-NAMING.md#req-bem-naming-0006-require-a-keyed-modifier-value-suffix)
- [`REQ-BEM-NAMING-0007`](./SPEC-BEM-NAMING.md#req-bem-naming-0007-omit-the-modifier-value-for-boolean-modifiers)
- [`REQ-BEM-NAMING-0008`](./SPEC-BEM-NAMING.md#req-bem-naming-0008-forbid-standalone-modifiers)
- [`REQ-BEM-NAMING-0009`](./SPEC-BEM-NAMING.md#req-bem-naming-0009-exclude-separator-characters-from-block-names)
- [`REQ-BEM-CSS-0001`](./SPEC-BEM-CSS.md#req-bem-css-0001-use-class-selectors-for-bem-entities)
- [`REQ-BEM-CSS-0002`](./SPEC-BEM-CSS.md#req-bem-css-0002-avoid-tag-and-id-selectors)
- [`REQ-BEM-CSS-0003`](./SPEC-BEM-CSS.md#req-bem-css-0003-avoid-tag-and-class-selectors)
- [`REQ-BEM-CSS-0004`](./SPEC-BEM-CSS.md#req-bem-css-0004-minimize-nested-selectors)
- [`REQ-BEM-CSS-0005`](./SPEC-BEM-CSS.md#req-bem-css-0005-limit-nested-selectors-to-block-state-or-theme-set-changes)
- [`REQ-BEM-CSS-0006`](./SPEC-BEM-CSS.md#req-bem-css-0006-avoid-combined-selectors)
- [`REQ-BEM-CSS-0007`](./SPEC-BEM-CSS.md#req-bem-css-0007-allow-mixes-to-combine-entities-on-one-node)
- [`REQ-BEM-CSS-0008`](./SPEC-BEM-CSS.md#req-bem-css-0008-use-mixes-for-external-geometry-and-positioning)
- [`REQ-BEM-CSS-0009`](./SPEC-BEM-CSS.md#req-bem-css-0009-avoid-wrapper-heavy-positioning)
- [`REQ-BEM-CSS-0010`](./SPEC-BEM-CSS.md#req-bem-css-0010-use-selector-names-that-accurately-describe-the-entity)

## Design Summary

The design uses three layers:

- shared concepts define the block, element, modifier, and mix vocabulary
- naming grammar turns those concepts into stable class tokens
- CSS rules remain class-based and shallow, with modifier classes representing state and mixes representing cross-block composition

Because the concepts are defined first, later rules can refer to them without restating their meaning. A block class is the namespace root; element classes extend that namespace; modifiers add variation without changing the base entity; and mixes combine multiple entities on a single DOM node. External geometry stays outside the block and is applied by the parent context or a mix.

## Key Components

- block namespace
- element names
- modifier names
- mix composition
- selector hygiene
- traceable documentation links

## Data and State Considerations

State is expressed through modifier classes rather than tag names, IDs, or wrapper structure. A single node can carry multiple class tokens when a mix is required. Block names remain stable and semantic so the class family stays reusable. Element names remain local to one block and do not become nested namespaces.

## Edge Cases and Constraints

- Boolean and keyed modifiers use distinct class forms.
- Nested selectors may express block state or theme changes, but not general element-of-element traversal.
- Wrapper blocks are avoided when a mix can express the composition directly.
- Class names are semantic; appearance-only names are treated as a smell.

## Alternatives Considered

- A glossary-only document was rejected because later requirements need stable IDs and downstream trace.
- Tag or ID selectors were rejected because they weaken BEM composition and specificity control.
- Wrapper-heavy layouts were rejected because they make composition harder to reuse.

## Risks

- Teams may drift toward visual or layout-driven names if review does not enforce the naming grammar.
- Overuse of nested selectors can hide BEM structure and inflate specificity.
- If downstream trace links are omitted, the example loses its value as a traceability reference.

## Open Questions

- None.
