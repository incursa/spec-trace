---
artifact_id: VER-BEM-CSS-0001
artifact_type: verification
title: BEM Concepts, Naming, and CSS Review
domain: bem-css
status: passed
owner: frontend-platform
related_artifacts:
  - SPEC-BEM-CONCEPTS
  - SPEC-BEM-NAMING
  - SPEC-BEM-CSS
  - ARC-BEM-CSS-0001
verifies:
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

# [`VER-BEM-CSS-0001`](./sample-verification.md) - BEM Concepts, Naming, and CSS Review

## Scope

Verify the BEM example's shared terms, naming grammar, selector rules, and composition rules.

## Requirements Verified

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

## Verification Method

Document inspection against the official BEM documentation, the three BEM specification files, and the reciprocal trace links in the example set.

## Preconditions

- The three BEM specifications exist in the example folder.
- The official BEM documentation is available for comparison.
- The architecture artifact exists and lists the same downstream scope.

## Procedure or Approach

1. Check that each requirement is atomic and carries exactly one approved uppercase normative keyword.
2. Confirm the concepts spec introduces the shared vocabulary before naming or CSS rules depend on it.
3. Confirm the naming spec constrains block, element, and modifier class forms and excludes separator collisions.
4. Confirm the CSS spec limits selectors, nested selectors, and wrapper-heavy layout behavior.
5. Confirm each requirement carries `Satisfied By` and `Verified By` links to the shared architecture and verification artifacts.
6. Confirm the front matter `satisfies` and `verifies` lists match the requirement scope.

## Expected Result

The example reads as a coherent, reviewable BEM specification set with complete downstream trace and no ambiguous shared terms.

## Evidence

- [`examples/bem-css/SPEC-BEM-CONCEPTS.md`](./SPEC-BEM-CONCEPTS.md)
- [`examples/bem-css/SPEC-BEM-NAMING.md`](./SPEC-BEM-NAMING.md)
- [`examples/bem-css/SPEC-BEM-CSS.md`](./SPEC-BEM-CSS.md)
- [`examples/bem-css/sample-architecture.md`](./sample-architecture.md)
- official BEM documentation:
- https://en.bem.info/methodology/quick-start/
- https://en.bem.info/methodology/naming-convention/
- https://en.bem.info/methodology/css/
- https://en.bem.info/methodology/html/
- https://en.bem.info/methodology/faq/?lang=en

## Status

This `passed` status applies to every requirement listed in `verifies`.

passed

## Related Artifacts

- [SPEC-BEM-CONCEPTS](./SPEC-BEM-CONCEPTS.md)
- [SPEC-BEM-NAMING](./SPEC-BEM-NAMING.md)
- [SPEC-BEM-CSS](./SPEC-BEM-CSS.md)
- [ARC-BEM-CSS-0001](./sample-architecture.md)
