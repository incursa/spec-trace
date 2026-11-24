#!/usr/bin/env python3
"""Standard-library validation for the shared SpecAttest conformance vectors."""

from __future__ import annotations

import argparse
import copy
import hashlib
import json
import pathlib
import re
import sys

SEMVER = re.compile(r"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-(draft|rc)\.([1-9][0-9]*))?$")
SHA256 = re.compile(r"^[a-f0-9]{64}$")
COMMIT = re.compile(r"^[a-f0-9]{40}$")
TAG = re.compile(r"^refs/tags/[A-Za-z0-9._/-]+$")

PIN_FIELDS = {
    "contract", "repository", "edition", "commit", "schemaPath", "schemaSha256",
    "editionManifestSha256", "editionManifestSchemaPath", "editionManifestSchemaSha256",
    "payloadSha256", "conformanceCorpusSha256", "signatureReference", "signatureSha256",
    "canonicalization", "canonicalProfiles", "proofProfile",
}
HASH_FIELDS = {
    "schemaSha256", "editionManifestSha256", "editionManifestSchemaSha256",
    "payloadSha256", "conformanceCorpusSha256", "signatureSha256",
}


def canonical(value: object) -> bytes:
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"), sort_keys=True).encode("utf-8")


def pin_shape_valid(pin: dict[str, object]) -> bool:
    return (
        set(pin) == PIN_FIELDS
        and pin.get("contract") == "specattest.standard-pin.v1"
        and isinstance(pin.get("repository"), str) and str(pin["repository"]).startswith("https://")
        and isinstance(pin.get("edition"), str) and SEMVER.fullmatch(str(pin["edition"])) is not None
        and isinstance(pin.get("commit"), str) and COMMIT.fullmatch(str(pin["commit"])) is not None
        and all(isinstance(pin.get(field), str) and SHA256.fullmatch(str(pin[field])) for field in HASH_FIELDS)
        and isinstance(pin.get("signatureReference"), str) and TAG.fullmatch(str(pin["signatureReference"])) is not None
        and pin.get("canonicalization") == "utf8-json-sorted-object-keys-v1"
        and pin.get("canonicalProfiles") == ["core", "traceable", "auditable"]
        and pin.get("proofProfile") in {"core", "traceable", "auditable"}
        and all(isinstance(pin.get(field), str) and bool(pin[field]) for field in {"schemaPath", "editionManifestSchemaPath"})
    )


def validate_versioning(case: dict[str, object]) -> bool:
    match = SEMVER.fullmatch(str(case.get("edition", "")))
    valid = match is not None
    if match:
        prerelease = match.group(4)
        expected = "draft" if prerelease == "draft" else "candidate" if prerelease == "rc" else "stable"
        valid = case.get("maturity") == expected
    requirement = case.get("requirement", {})
    valid = valid and isinstance(requirement, dict) and isinstance(requirement.get("revision"), int) and requirement["revision"] >= 1
    if not case.get("bootstrap", False):
        valid = valid and bool(case.get("previousEdition")) and bool(case.get("behavioralReview"))
    return valid


def validate_pin_mutation(case: dict[str, object]) -> bool:
    trusted = case.get("trustedPin")
    mutation = case.get("mutation")
    if not isinstance(trusted, dict) or not isinstance(mutation, dict) or not pin_shape_valid(trusted):
        return False
    candidate = copy.deepcopy(trusted)
    field = mutation.get("field")
    if field not in PIN_FIELDS:
        return False
    if mutation.get("operation") == "remove":
        candidate.pop(str(field), None)
    elif mutation.get("operation") == "replace":
        candidate[str(field)] = mutation.get("value")
    else:
        return False
    return pin_shape_valid(candidate) and canonical(candidate) == canonical(trusted)


def validate_interchange(case: dict[str, object]) -> bool:
    source = case.get("inputRequirement")
    output = case.get("outputRequirement")
    provider_version = case.get("providerRecordVersion")
    if not isinstance(source, dict) or not isinstance(output, dict) or not isinstance(provider_version, int) or provider_version < 1:
        return False
    if not isinstance(source.get("revision"), int) or source["revision"] < 1:
        return False
    preserved = all(source.get(field) == output.get(field) for field in ("id", "revision", "trace"))
    divergence_proved = source["revision"] != provider_version
    first = case.get("canonicalA")
    second = case.get("canonicalB")
    canonical_equal = True if first is None and second is None else canonical(first) == canonical(second)
    return preserved and divergence_proved and canonical_equal


def validate_protocol(case: dict[str, object]) -> bool:
    scenario = case.get("scenario")
    rest = case.get("restOutcome")
    mcp = case.get("mcpOutcome")
    if rest != mcp:
        return False
    if scenario in {"stale-commit", "atomic-failure"}:
        return rest == "rejected" and case.get("partialMutation") is False
    if scenario == "deterministic-replay":
        request = case.get("logicalRequest")
        replay = case.get("replayedLogicalRequest")
        return request == replay and case.get("serverDefaultsExcluded") is True and hashlib.sha256(canonical(request)).digest() == hashlib.sha256(canonical(replay)).digest()
    if scenario == "cancel":
        return rest == "canceled" and case.get("partialMutation") is False
    if scenario == "resume":
        return rest == "committed" and case.get("atomicCommit") is True
    return False


def validate_case(case: dict[str, object]) -> bool:
    kind = case.get("kind", "versioning")
    if kind == "versioning":
        return validate_versioning(case)
    if kind == "edition-pin-mutation":
        return validate_pin_mutation(case)
    if kind == "interchange-roundtrip":
        return validate_interchange(case)
    if kind == "safe-mutation-protocol":
        return validate_protocol(case)
    raise ValueError(f"unsupported conformance kind: {kind}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    corpus = args.root.resolve() / "conformance" / "versioning"
    cases = sorted(corpus.rglob("*.json"))
    if not cases:
        raise ValueError("versioning conformance corpus is empty")
    for path in cases:
        case = json.loads(path.read_text(encoding="utf-8"))
        actual = validate_case(case)
        if actual != case.get("valid"):
            raise ValueError(f"conformance case produced the wrong result: {path.relative_to(corpus)}")
    print(json.dumps({"result": "passed", "cases": len(cases)}, sort_keys=True))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, TypeError, ValueError, json.JSONDecodeError) as error:
        print(f"conformance validation failed: {error}", file=sys.stderr)
        raise SystemExit(1)
