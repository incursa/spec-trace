---
artifact_id: ARC-<DOMAIN>[-<GROUPING>...]-###
artifact_type: architecture
title: <Architecture or Design Title>
domain: <domain>
status: draft
owner: <team-or-role>
satisfies:
  - REQ-<DOMAIN>[-<GROUPING>...]-###
related_artifacts:
  - SPEC-<DOMAIN>[-<GROUPING>...]-###
  - ADR-###
---

# ARC-<DOMAIN>[-<GROUPING>...]-### - <Architecture or Design Title>

Optional grouping segments may appear between the domain code and the terminal number.

## Purpose

Describe how the named requirements will be satisfied by design. Keep this focused on the design intent, not implementation trivia.

## Requirements Satisfied

- REQ-<DOMAIN>[-<GROUPING>...]-###

## Design Summary

Summarize the design approach and the main mechanism used to satisfy the requirement.

## Key Components

- <component or concept>
- <component or concept>

## Data and State Considerations

Describe the data that must be stored, derived, or compared, and note any state transitions that matter to the design.

## Edge Cases and Constraints

Call out the behavior for duplicates, failures, retries, boundary conditions, and any explicit constraints.

## Alternatives Considered

List the main alternatives and why the chosen approach is preferred.

## Risks

List the main technical or operational risks and any mitigation or follow-up work.

## Open Questions

- <question>
