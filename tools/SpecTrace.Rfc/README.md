# SpecTrace RFC Workbench

`SpecTrace.Rfc` converts RFC source text into reviewable SpecTrace requirement candidates, then assembles approved candidates into canonical SpecTrace JSON.

The pipeline keeps deterministic steps outside the AI boundary:

1. `ingest` captures RFC source text and metadata.
2. `segment` creates a source-unit ledger with stable IDs and hashes.
3. `extract` asks local Codex to review source units and writes JSONL decisions.
4. `coverage-audit` runs a section-batched review pass that promotes missing testable invariants and writes review decisions.
5. `normalize` runs the merge/trim pass that consolidates over-fragmented review decisions into assembly-ready review decisions.
6. `review-pack` renders a Markdown packet for human review.
7. `assemble` turns accepted candidates or normalized review decisions into a draft `SPEC-...json`.
8. `validate` runs the repository SpecTrace validation gate.

Run commands through the local wrapper:

```powershell
./tools/SpecTrace.Rfc/spec-rfc.ps1 ingest --rfc 9114 --out ./.work/rfc9114/source.json
./tools/SpecTrace.Rfc/spec-rfc.ps1 segment --source ./.work/rfc9114/source.json --out ./.work/rfc9114/source-ledger.jsonl
./tools/SpecTrace.Rfc/spec-rfc.ps1 extract --ledger ./.work/rfc9114/source-ledger.jsonl --out ./.work/rfc9114/candidates.jsonl
./tools/SpecTrace.Rfc/spec-rfc.ps1 coverage-audit --ledger ./.work/rfc9114/source-ledger.jsonl --candidates ./.work/rfc9114/candidates.jsonl --out ./.work/rfc9114/review-decisions.jsonl --report-out ./.work/rfc9114/coverage-audit.md
./tools/SpecTrace.Rfc/spec-rfc.ps1 normalize --ledger ./.work/rfc9114/source-ledger.jsonl --review ./.work/rfc9114/review-decisions.jsonl --out ./.work/rfc9114/review-decisions.normalized.jsonl
./tools/SpecTrace.Rfc/spec-rfc.ps1 review-pack --ledger ./.work/rfc9114/source-ledger.jsonl --candidates ./.work/rfc9114/candidates.jsonl --out ./.work/rfc9114/review.md
./tools/SpecTrace.Rfc/spec-rfc.ps1 assemble --ledger ./.work/rfc9114/source-ledger.jsonl --review ./.work/rfc9114/review-decisions.normalized.jsonl --spec-id SPEC-HTTP3-RFC9114 --domain http3 --capability http3-rfc9114 --out ./specs/requirements/http3/SPEC-HTTP3-RFC9114.json
./tools/SpecTrace.Rfc/spec-rfc.ps1 validate --root . --input-path ./specs/requirements/http3/SPEC-HTTP3-RFC9114.json --profile core
```

`extract` invokes local Codex CLI by default:

```text
codex exec -m gpt-5.4-mini -c model_reasoning_effort="high" ...
```

The Codex step is constrained by `schemas/candidate-requirements.schema.json`; generated candidate decisions remain reviewable JSONL, not canonical SpecTrace artifacts.

By default, `extract` uses `--extraction-scope candidate-units` and `--deterministic-extraction off`, so reviewable source units that look like implementation-relevant behavior are sent to Codex while obvious boilerplate stays out of the candidate stream. Use `--extraction-scope functional` for the broader exploratory mode, `--extraction-scope normative` to restrict extraction to uppercase-keyword units and structured blocks, or `--extraction-scope all` to send every source unit to Codex for exhaustive sweeps. Use `--deterministic-extraction figures` to opt into deterministic packet-figure extraction. The default uses `--batch-size 25`, `--reasoning-effort high`, and `--retry-reasoning-effort xhigh`. A failed or timed-out batch is split into smaller batches before retrying; `xhigh` is reserved for batches that can no longer be split. Successful batch decisions are written to `batches/batch-NNNN.candidates.json`, and `candidates.jsonl` is deterministically merged in ledger order after each successful batch. Use `--resume` for long runs; completed decisions are reused from both `candidates.jsonl` and per-batch artifacts. Use `--ai-mode off` to produce a fast audit baseline where units that would be sent to Codex are marked `needs_human_review`.

`coverage-audit` is the coverage-completeness pass. It reviews the extraction output section-by-section, can write a Markdown audit packet with `--report-out`, and emits `review-decisions.jsonl` for final assembly. Use `--ai-mode off` for a deterministic pass-through baseline when you want to inspect the shape of the review stage without invoking Codex.

`normalize` is the merge/trim pass. It reviews the audited decisions section-by-section, collapses adjacent or overlapping requirement material into tighter canonical review decisions, and emits a normalized `review-decisions.jsonl` for final assembly. Use `--ai-mode off` for a deterministic pass-through baseline when you want to inspect the shape of the normalization stage without invoking Codex.

By default, assembled requirement IDs include the RFC section key, matching the existing QUIC RFC artifacts: `SPEC-HTTP3-RFC9114` + section `4.1` -> `REQ-HTTP3-RFC9114-S4P1-0001`. Section numbering is assigned from canonical source order within each section family, so it stays stable between runs instead of depending on AI output order. RFC section and source-unit provenance is also preserved in `trace.upstream_refs`. Use `--id-style namespace` when targeting a validator that requires requirement IDs to reuse only the containing specification namespace.

For the QUIC RFC corpus under `C:\src\incursa\quic-dotnet\specs\rfcs`, the repo-level batch driver is [`scripts/Convert-QuicRfcs.ps1`](../../scripts/Convert-QuicRfcs.ps1). It writes per-RFC work artifacts under `C:\src\incursa\spec-trace\.work-rfc-batch\rfc####\` and can optionally publish canonical copies to a separate staging root. By default, that staging root is `C:\src\incursa\quic-dotnet\specs\requirements\quic\SPEC-QUIC-RFC####.json`, but you can pass `-PublishCanonicalRoot <folder>` to redirect the canonical-shaped output into `'<folder>\specs\requirements\quic'` instead of the live QUIC repo tree.
