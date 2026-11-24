#!/usr/bin/env python3
"""Independent standard-library checks for a frozen SpecAttest edition manifest."""

from __future__ import annotations

import argparse
import hashlib
import json
import pathlib
import re
import sys

SEMVER = re.compile(r"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-(draft|rc)\.([1-9][0-9]*))?$")
REQ_ID = re.compile(r"^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-[0-9]{4,}$")


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def canonical(value: object) -> bytes:
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"), sort_keys=True).encode("utf-8")


def fail(message: str) -> None:
    raise ValueError(message)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("manifest", type=pathlib.Path)
    parser.add_argument("--root", type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    root = args.root.resolve()
    manifest = json.loads(args.manifest.read_text(encoding="utf-8"))

    if manifest.get("format") != "specattest.standard-edition.v1":
        fail("unsupported manifest format")
    match = SEMVER.fullmatch(manifest.get("edition", ""))
    if not match:
        fail("edition is not an allowed Semantic Versioning label")
    prerelease = match.group(4)
    expected_maturity = "draft" if prerelease == "draft" else "candidate" if prerelease == "rc" else "stable"
    if manifest.get("maturity") != expected_maturity:
        fail("edition prerelease and maturity disagree")

    schema_path = root / "schemas" / "standard-edition.schema.json"
    if sha256(schema_path.read_bytes()) != manifest.get("schemaSha256"):
        fail("schemaSha256 does not match schemas/standard-edition.schema.json")

    revisions = manifest.get("requirementRevisions", [])
    declared = {(item["id"], item["revision"]): item["sha256"] for item in revisions}
    if len(declared) != len(revisions):
        fail("requirement revision coordinates are duplicated")

    discovered: dict[tuple[str, int], str] = {}
    payload_parts: list[bytes] = []
    source_path = pathlib.Path(manifest["source"]["path"])
    for specification_id in manifest["specificationOrder"]:
        path = root / source_path / f"{specification_id}.json"
        raw = path.read_bytes()
        payload_parts.extend([path.relative_to(root).as_posix().encode("utf-8"), b"\0", raw, b"\0"])
        artifact = json.loads(raw)
        if artifact.get("artifact_id") != specification_id:
            fail(f"artifact identity mismatch for {path}")
        for requirement in artifact.get("requirements", []):
            requirement_id = requirement.get("id", "")
            revision = requirement.get("revision")
            if not REQ_ID.fullmatch(requirement_id) or not isinstance(revision, int) or revision < 1:
                fail(f"invalid requirement revision in {path}: {requirement_id}")
            coordinate = (requirement_id, revision)
            if coordinate in discovered:
                fail(f"duplicate requirement coordinate: {coordinate}")
            discovered[coordinate] = sha256(canonical(requirement))

    if discovered != declared:
        missing = sorted(set(discovered) - set(declared))
        unexpected = sorted(set(declared) - set(discovered))
        mismatched = sorted(key for key in set(discovered) & set(declared) if discovered[key] != declared[key])
        fail(f"requirement revision manifest mismatch; missing={missing}, unexpected={unexpected}, digest={mismatched}")
    if sha256(b"".join(payload_parts)) != manifest.get("payloadSha256"):
        fail("payloadSha256 does not match the ordered canonical suite")
    validators = manifest.get("validatedBy", [])
    if len(validators) < 2:
        fail("at least two validator records are required")
    expected_roles = {"independent", "candidate"} if manifest.get("bootstrap") else {"accepted", "candidate"}
    actual_roles = {item.get("role") for item in validators}
    if not expected_roles.issubset(actual_roles):
        fail(f"validator roles must include {sorted(expected_roles)}")
    for item in validators:
        if item.get("standardEdition") != manifest.get("edition"):
            fail("validator standardEdition does not match the manifest edition")
        if item.get("conformanceCorpusSha256") != manifest.get("conformanceCorpusSha256"):
            fail("validator conformance corpus digest does not match the manifest")
    if not manifest.get("approvalReference") or not manifest.get("signature"):
        fail("approval and signature references are required")

    print(json.dumps({
        "result": "passed",
        "edition": manifest["edition"],
        "specifications": len(manifest["specificationOrder"]),
        "requirements": len(discovered),
        "manifestSha256": sha256(args.manifest.read_bytes()),
    }, sort_keys=True))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (KeyError, OSError, TypeError, ValueError, json.JSONDecodeError) as error:
        print(f"edition bootstrap validation failed: {error}", file=sys.stderr)
        raise SystemExit(1)
