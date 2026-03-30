# BEM CSS Example

This example turns the BEM methodology into a compact, real specification set with downstream design and verification artifacts.

It is based on the official BEM documentation and keeps the shared meaning in a dedicated concepts spec so later requirements can reference it directly.

It demonstrates:

- shared BEM concepts for blocks, elements, modifiers, and mixes
- class-name rules with explicit negative constraints
- class-based selectors and selector hygiene
- a downstream architecture artifact that explains the composition model
- a downstream verification artifact that shows the requirement set can be reviewed as a coherent whole

How to read it:

- [`SPEC-BEM-CONCEPTS`](./SPEC-BEM-CONCEPTS.md) defines the shared terms and the reusable semantic contract.
- [`SPEC-BEM-NAMING`](./SPEC-BEM-NAMING.md) defines the class-name grammar and negative rules.
- [`SPEC-BEM-CSS`](./SPEC-BEM-CSS.md) defines selector, nesting, and mix behavior.
- [`sample-architecture.md`](./sample-architecture.md) shows how those requirements are satisfied in a BEM implementation.
- [`sample-verification.md`](./sample-verification.md) records the review used to verify the requirement set.
- Each requirement is atomic and can be referenced directly by ID.
- The `Trace` blocks use `Satisfied By` and `Verified By` for downstream links, plus `Upstream Refs` and `Related` where useful.

Definitions stay in the specification family. In this repo, definition-like statements are still written as requirements when they need stable IDs and downstream trace.
