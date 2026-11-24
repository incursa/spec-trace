#!/usr/bin/env python3
"""Build a reproducible SpecAttest Standard edition manifest from a preparation record."""

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
    parser.add_argument(
        "--preparation",
        type=pathlib.Path,
        default=pathlib.Path("docs/initial-editions/specattest-standard-0.1.0-draft.2.json"),
    )
    parser.add_argument("--approval-reference", required=True)
    parser.add_argument("--tag-reference", required=True)
    parser.add_argument("--tag-object-sha256", required=True)
    parser.add_argument("--accepted-report", "--schema-report", dest="accepted_report", type=pathlib.Path)
    parser.add_argument("--candidate-report", "--independent-report", dest="candidate_report", type=pathlib.Path)
    parser.add_argument("--root", type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    root = args.root.resolve()

    preparation_path = args.preparation if args.preparation.is_absolute() else root / args.preparation
    preparation = json.loads(preparation_path.read_text(encoding="utf-8"))
    edition = preparation["edition"]
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
        "edition": edition,
        "maturity": preparation["editionMaturity"],
        "governance": preparation["governance"],
        "source": {
            "repository": "https://github.com/incursa/spec-trace",
            "commit": source_commit,
            "path": source_path.as_posix() + "/",
        },
        "payloadSha256": sha256(b"".join(payload_parts)),
        "modelSchemaSha256": sha256((root / "model/model.schema.json").read_bytes()),
        "schemaSha256": sha256((root / "schemas/standard-edition.schema.json").read_bytes()),
        "canonicalization": "utf8-json-sorted-object-keys-v1",
        "specificationOrder": order,
        "requirementRevisions": revisions,
        "conformanceCorpusSha256": corpus_sha256,
        "validatedBy": [
            {
                "validator": preparation["validators"][0]["validator"],
                "version": preparation["validators"][0]["version"],
                "role": preparation["validators"][0]["role"],
                "standardEdition": edition,
                "result": "passed",
                "resultSha256": file_digest(args.accepted_report),
                "conformanceCorpusSha256": corpus_sha256,
            },
            {
                "validator": preparation["validators"][1]["validator"],
                "version": preparation["validators"][1]["version"],
                "role": preparation["validators"][1]["role"],
                "standardEdition": edition,
                "result": "passed",
                "resultSha256": file_digest(args.candidate_report),
                "conformanceCorpusSha256": corpus_sha256,
            },
        ],
        "approvalReference": args.approval_reference,
        "knownGaps": preparation.get("knownGaps", []),
        "signature": {
            "kind": "git-tag",
            "reference": args.tag_reference,
            "sha256": args.tag_object_sha256,
        },
        "bootstrap": preparation["bootstrap"],
    }
    if not preparation["bootstrap"]:
        manifest["previousEdition"] = preparation["previousEdition"]
        manifest["behavioralReview"] = preparation["behavioralReview"]
        manifest["compatibility"] = preparation["compatibility"]
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
