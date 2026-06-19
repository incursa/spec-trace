# RFC 8999 extraction overfragmentation review

## Summary

The current RFC extraction pipeline is capable of producing a much larger requirement set than the older QUIC repo artifact, but the increase in count is not by itself evidence of higher quality.

For RFC 8999, the problem is not that the extractor invents behavior wholesale. The problem is that the current tooling is tuned to turn a very fine-grained source-unit ledger into equally fine-grained requirements, even when many of those units are descriptive prose that should be classified as either testable behavior, explanatory context, or background noise.

## What I checked

- `tools/SpecTrace.Rfc/README.md`
- `tools/SpecTrace.Rfc/prompts/extract-requirements.md`
- `tools/SpecTrace.Rfc/prompts/normalize-requirement.md`
- `tools/SpecTrace.Rfc/src/SpecTrace.Rfc.Core/RfcSegmenter.cs`
- `tools/SpecTrace.Rfc/src/SpecTrace.Rfc.Core/DeterministicCandidateExtractor.cs`
- `tools/SpecTrace.Rfc/src/SpecTrace.Rfc.Ai/CodexCliRequirementExtractor.cs`
- `tools/SpecTrace.Rfc/src/SpecTrace.Rfc.Core/CandidateRules.cs`
- `tools/SpecTrace.Rfc/src/SpecTrace.Rfc.Core/ReviewPackRenderer.cs`
- `tools/SpecTrace.Rfc/src/SpecTrace.Rfc.Core/SpecAssembler.cs`

I also ran the RFC 8999 ingest + segment path locally and got:

- 151 source units total
- 127 paragraph units
- 17 list-item units
- 7 figure units
- 12 source units containing explicit uppercase RFC keywords

That ratio is the real warning sign. It means the pipeline is offering a lot of prose to the model as potential requirement material, and the current prompt is permissive enough that many of those units can become requirements.

## Where the inflation comes from

### 1. The segmenter is sentence-level for prose

`RfcSegmenter` splits paragraph and list blocks into sentence-sized source units before Codex ever sees them.

That means a single RFC paragraph becomes multiple candidate inputs, not one.

This is good for traceability, but it also makes over-fragmentation much easier because the model is no longer deciding at the paragraph or section level.

### 2. The extraction prompt encourages maximal splitting

The current `extract-requirements.md` prompt explicitly says:

- produce one result for every source unit
- prefer the smallest independently testable requirement units
- split actor, condition, field presence, field order, field size, field value, frame type, state transition, error code, limit, exception, and algorithm step into separate requirements
- treat descriptive protocol behavior as requirement material even when the RFC does not use uppercase keywords

That combination strongly biases the model toward atomizing nearly every sentence into one or more requirements.

The important nuance is that this is not inherently wrong. One requirement can span multiple sentences, and one sentence can legitimately yield multiple requirements when it contains multiple independently testable obligations.

The problem is that the prompt does not draw a hard enough line between:

- descriptive text that encodes a testable invariant
- descriptive text that is only explanatory
- descriptive text that is too ambiguous to normalize safely

### 3. The default scope sends everything to Codex

`tools/SpecTrace.Rfc/README.md` says the default `extract` mode uses `--extraction-scope all`.

So the model is not only seeing normative text. It is seeing:

- background prose
- definitions
- structural descriptions
- section text that is not directly normative

The prompt then asks it to normalize much of that prose into requirements anyway.

### 4. The validator enforces one normative keyword per statement

`CandidateRules.ValidateRequirement` requires exactly one uppercase normative keyword in every candidate statement.

That is a useful guardrail for statement shape, but it is not the root problem. It only becomes harmful when the extraction prompt uses it as a proxy for "split everything."

### 5. Structured figures can explode into many requirements

`DeterministicCandidateExtractor` already splits RFC packet figures into separate field-based requirements.

For RFC 8999 long-header figures, that is reasonable for the header layout.

But combined with sentence-level segmentation and the prompt rules, the overall pipeline can multiply requirements quickly.

## What this means for RFC 8999

The larger requirement count is not automatically junk.

It is plausible for the extractor to produce many requirements from RFC 8999 because the RFC contains a lot of descriptive prose that the current prompt treats as requirement material.

But the current configuration is too eager to convert descriptive prose into canonical requirements, so the output is likely over-fine-grained for a compact spec artifact.

In other words:

- the newer output is probably more complete in the sense that it covers more source text
- the newer output is not automatically higher quality as a canonical spec
- the newer output is likely noisier than necessary for a maintainable requirement set

The boundary I would actually want is:

- keep text that describes externally observable, implementation-relevant, independently testable behavior
- skip text that is purely explanatory, historical, or motivational
- review text that might encode behavior but is ambiguous without surrounding context

That allows buffer layout, packet layout, field ordering, field encoding, algorithm steps, and state transitions to become requirements when they are real testable constraints, without forcing every prose sentence into canonical form.

A practical rule of thumb:

- Keep: `Header Form (1) = 1`, field widths, byte order, connection-id length encoding, packet format, state transitions, error behavior, negotiation rules.
- Review: prose that sounds like behavior but needs surrounding context to know whether it is an invariant, a recommendation, or just explanation.
- Skip: motivation, rationale, historical notes, comparisons to other versions, and prose that only explains why a property matters.

## Why the older 8-requirement artifact still matters

The older QUIC repo RFC 8999 spec is a curated header-invariant slice, not a whole-document sentence inventory.

That smaller artifact is useful because it captures the stable packet-shape obligations without turning every descriptive sentence into its own requirement.

So the right comparison is not:

- 8 requirements = incomplete
- 100+ requirements = complete

The real comparison is:

- 8 requirements = compact, curated, implementation-facing slice
- 100+ requirements = potentially more exhaustive, but probably over-fragmented unless every item is justified by traceable source text

## Likely fixes

### Short term

- Change the default extraction scope away from `all` for canonical generation runs.
- Prefer `candidate-units` as the default path for reviewable extraction.
- Keep `functional` as the broader exploratory path and `all` as an explicit opt-in for exhaustive sweeps.

### Prompt changes

- Replace the blanket "many small requirements are better" guidance with a narrower rule: split only when the source unit really contains independently testable obligations.
- Stop treating all descriptive behavior as automatic requirement material.
- Require the model to keep descriptive behavior only when it exposes a testable invariant or other implementation-relevant constraint.
- Require the model to skip or quarantine prose that is explanatory, motivational, or otherwise not materially normative.
- Keep the one-keyword constraint, but do not let it become a proxy for "split everything."

### Segmentation changes

- Consider preserving paragraph-level units for prose-heavy RFC sections instead of always splitting into sentences.
- Keep sentence-level units only where they are clearly useful, such as tightly normative prose or structured blocks.

### Review changes

- Add a quality gate that flags suspicious source-unit-to-requirement ratios.
- Add a review report that highlights sections where most requirements come from descriptive prose rather than explicit normative statements.
- Make over-splitting visible so humans can decide whether to merge or quarantine the result.
- Give reviewers an explicit merge/keep/skip rubric for descriptive-but-testable text versus explanatory text.

## Practical recommendation

For now, I would treat the current RFC 8999 generation path as a useful experimental extractor, not as a mature canonicalization pipeline.

If the goal is to produce a maintainable spec suite that can scale to future RFCs, the first thing to fix is the prompt/scope policy, not the final assembler.

The highest-value next change is to reduce the default eagerness to normalize descriptive text into requirements.

## After Tuning

I applied the prompt and gate changes, then reran RFC 8999 with a preserved verbose baseline alongside the tuned run.

- Baseline verbose run: 80 requirements
- Tuned run: 35 requirements
- Existing QUIC repo artifact: 8 requirements

The tuned result is materially less noisy than the verbose baseline, but it is still broader than the older QUIC artifact because it intentionally captures more RFC 8999 sections than the old curated header-layout slice.

Section-level comparison:

- Baseline `S6`: 24 requirements
- Tuned `S6`: 9 requirements
- Baseline `SA`: 8 requirements
- Tuned `SA`: 0 requirements

That is the main improvement signal. The new prompt and default scope kept the extractor from turning as much explanatory prose into canonical requirements, while still preserving the packet-layout and version-negotiation material that is actually useful.
