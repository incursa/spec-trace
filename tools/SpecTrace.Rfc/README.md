# SpecTrace RFC Workbench

`SpecTrace.Rfc` converts RFC source text into reviewable SpecTrace requirement candidates, then assembles approved candidates into canonical SpecTrace JSON.

The pipeline keeps deterministic steps outside the AI boundary:

1. `ingest` captures RFC source text and metadata.
2. `segment` creates a source-unit ledger with stable IDs and hashes.
3. `extract` deterministically handles parseable RFC figures, asks local Codex for remaining likely requirement-bearing units, and writes JSONL decisions.
4. `review-pack` renders a Markdown packet for human review.
5. `assemble` turns accepted candidates or review decisions into a draft `SPEC-...json`.
6. `validate` runs the repository SpecTrace validation gate.

Run commands through the local wrapper:

```powershell
./tools/SpecTrace.Rfc/spec-rfc.ps1 ingest --rfc 9114 --out ./.work/rfc9114/source.json
./tools/SpecTrace.Rfc/spec-rfc.ps1 segment --source ./.work/rfc9114/source.json --out ./.work/rfc9114/source-ledger.jsonl
./tools/SpecTrace.Rfc/spec-rfc.ps1 extract --ledger ./.work/rfc9114/source-ledger.jsonl --out ./.work/rfc9114/candidates.jsonl
./tools/SpecTrace.Rfc/spec-rfc.ps1 review-pack --ledger ./.work/rfc9114/source-ledger.jsonl --candidates ./.work/rfc9114/candidates.jsonl --out ./.work/rfc9114/review.md
./tools/SpecTrace.Rfc/spec-rfc.ps1 assemble --ledger ./.work/rfc9114/source-ledger.jsonl --candidates ./.work/rfc9114/candidates.jsonl --spec-id SPEC-HTTP3-RFC9114 --domain http3 --capability http3-rfc9114 --out ./specs/requirements/http3/SPEC-HTTP3-RFC9114.json
./tools/SpecTrace.Rfc/spec-rfc.ps1 validate --root . --input-path ./specs/requirements/http3/SPEC-HTTP3-RFC9114.json --profile core
```

`extract` invokes local Codex CLI by default:

```text
codex exec -m gpt-5.4-mini -c model_reasoning_effort="xhigh" ...
```

The Codex step is constrained by `schemas/candidate-requirements.schema.json`; generated candidate decisions remain reviewable JSONL, not canonical SpecTrace artifacts.

By default, `extract` uses `--extraction-scope candidate-units`, so explanatory prose without normative keywords is recorded as a deterministic skip and is not sent to Codex. It also uses `--batch-size 1` to keep source-unit provenance isolated; larger batches are faster but are validated strictly and can be rejected if Codex shifts requirements onto the wrong source unit. Use `--extraction-scope all` when you want Codex to review every source unit. Use `--ai-mode off` to produce a fast deterministic baseline: parseable figures are emitted, likely requirement-bearing prose is marked `needs_human_review`, and non-candidate prose is skipped.

By default, assembled requirement IDs include the RFC section key, matching the existing QUIC RFC artifacts: `SPEC-HTTP3-RFC9114` + section `4.1` -> `REQ-HTTP3-RFC9114-S4P1-0001`. RFC section and source-unit provenance is also preserved in `trace.upstream_refs`. Use `--id-style namespace` when targeting a validator that requires requirement IDs to reuse only the containing specification namespace.
