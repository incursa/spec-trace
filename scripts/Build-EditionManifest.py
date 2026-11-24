#!/usr/bin/env python3
"""Build the reproducible SpecAttest Standard 0.1.0-draft.1 edition manifest."""

from __future__ import annotations

import argparse
import hashlib
import json
import pathlib
import subprocess


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def canonical(value: object) -> bytes:
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"), sort_keys=True).encode("utf-8")


def file_digest(path: pathlib.Path | None) -> str:
    return "0" * 64 if path is None else sha256(path.read_bytes())


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True, type=pathlib.Path)
    parser.add_argument("--approval-reference", required=True)
    parser.add_argument("--tag-reference", required=True)
    parser.add_argument("--tag-object-sha256", required=True)
    parser.add_argument("--schema-report", type=pathlib.Path)
    parser.add_argument("--independent-report", type=pathlib.Path)
    parser.add_argument("--root", type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    root = args.root.resolve()

    preparation = json.loads((root / "docs/initial-editions/specattest-standard-0.1.0-draft.1.json").read_text(encoding="utf-8"))
    order = preparation["specificationOrder"]
    source_path = pathlib.Path("specs/requirements/spec-trace")
    revisions: list[dict[str, object]] = []
    payload_parts: list[bytes] = []
    for specification_id in order:
        path = root / source_path / f"{specification_id}.json"
        raw = path.read_bytes()
        payload_parts.extend([path.relative_to(root).as_posix().encode("utf-8"), b"\0", raw, b"\0"])
        artifact = json.loads(raw)
        revisions.extend({
            "id": requirement["id"],
            "revision": requirement["revision"],
            "sha256": sha256(canonical(requirement)),
        } for requirement in artifact["requirements"])

    source_commit = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=root, text=True).strip()
    corpus = subprocess.check_output(["git", "archive", "--format=tar", "HEAD", "conformance/versioning"], cwd=root)
    corpus_sha256 = sha256(corpus)
    manifest = {
        "$schema": "https://raw.githubusercontent.com/incursa/spec-trace/" + source_commit + "/schemas/standard-edition.schema.json",
        "format": "specattest.standard-edition.v1",
        "standardTitle": "SpecAttest Standard",
        "edition": "0.1.0-draft.1",
        "maturity": "draft",
        "governance": "incursa-owner-approved",
        "source": {
            "repository": "https://github.com/incursa/spec-trace",
            "commit": source_commit,
            "path": source_path.as_posix() + "/",
        },
        "payloadSha256": sha256(b"".join(payload_parts)),
        "schemaSha256": sha256((root / "schemas/standard-edition.schema.json").read_bytes()),
        "canonicalization": "utf8-json-sorted-object-keys-v1",
        "specificationOrder": order,
        "requirementRevisions": revisions,
        "conformanceCorpusSha256": corpus_sha256,
        "validatedBy": [
            {
                "validator": "spec-trace-tool-json-schema",
                "version": "1.0.0.0",
                "role": "candidate",
                "standardEdition": "0.1.0-draft.1",
                "result": "passed",
                "resultSha256": file_digest(args.schema_report),
                "conformanceCorpusSha256": corpus_sha256,
            },
            {
                "validator": "specattest-independent-bootstrap-validator",
                "version": "1",
                "role": "independent",
                "standardEdition": "0.1.0-draft.1",
                "result": "passed",
                "resultSha256": file_digest(args.independent_report),
                "conformanceCorpusSha256": corpus_sha256,
            },
        ],
        "approvalReference": args.approval_reference,
        "knownGaps": [
            "Draft maturity carries no compatibility promise.",
            "As the bootstrap edition, this release cannot be checked by a previously accepted SpecAttest validator.",
        ],
        "signature": {
            "kind": "git-tag",
            "reference": args.tag_reference,
            "sha256": args.tag_object_sha256,
        },
        "bootstrap": True,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
