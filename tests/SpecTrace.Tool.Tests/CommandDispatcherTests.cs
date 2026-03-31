using SpecTrace.Tool;

namespace SpecTrace.Tool.Tests;

public sealed class CommandDispatcherTests : IDisposable
{
    private readonly string _rootPath;

    public CommandDispatcherTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "spec-trace-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
        CopyDirectory(Path.Combine(RepositoryRoot, "cue.mod"), Path.Combine(_rootPath, "cue.mod"));
        CopyDirectory(Path.Combine(RepositoryRoot, "model"), Path.Combine(_rootPath, "model"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "specs", "requirements", "sample"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample"));
    }

    [Fact]
    public async Task ValidateAcceptsValidRepository()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Sample Specification"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Validate a simple requirement."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Carry one keyword"
            statement: "The sample MUST carry one keyword."
            trace: {
                satisfied_by: [
                    "ARC-SAMPLE-0001",
                ]
            }
        },
    ]
}
""");

        WriteCue("examples/sample/ARC-SAMPLE-0001.cue", """
package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Architecture & {
    artifact_id:   "ARC-SAMPLE-0001"
    artifact_type: "architecture"
    title:         "Sample Architecture"
    domain:        "sample"
    status:        "approved"
    owner:         "sample-team"
    satisfies: [
        "REQ-SAMPLE-0001",
    ]
    purpose:        "Satisfy the sample requirement."
    design_summary: "A small architecture record."
    key_components: [
        "component",
    ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsDuplicateArtifactIds()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec A"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "One keyword"
            statement: "The sample MUST validate."
        },
    ]
}
""");

        WriteCue("examples/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec B"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0002"
            title:     "One keyword"
            statement: "The sample MUST validate."
        },
    ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "traceable"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsBrokenReferences()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Broken reference"
            statement: "The sample MUST validate."
            trace: {
                verified_by: [
                    "VER-SAMPLE-0001",
                ]
            }
        },
    ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "traceable"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsWrongTargetKind()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Wrong target kind"
            statement: "The sample MUST validate."
            trace: {
                satisfied_by: [
                    "WI-SAMPLE-0001",
                ]
            }
        },
    ]
}
""");

        WriteCue("examples/sample/WI-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#WorkItem & {
    artifact_id:   "WI-SAMPLE-0001"
    artifact_type: "work_item"
    title:         "Work item"
    domain:        "sample"
    status:        "planned"
    owner:         "sample-team"
    summary:       "Do work."
    addresses: [
        "REQ-SAMPLE-0001",
    ]
    planned_changes:   "Change something."
    verification_plan: "Verify something."
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "traceable"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsInvalidNormativeKeywordCount()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Too many keywords"
            statement: "The sample MUST and SHOULD validate."
        },
    ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsMissingRequiredField()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Missing purpose"
            statement: "The sample MUST validate."
        },
    ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsInvalidEnumValue()
    {
        WriteCue("examples/sample/VER-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Verification & {
    artifact_id:   "VER-SAMPLE-0001"
    artifact_type: "verification"
    title:         "Verification"
    domain:        "sample"
    status:        "ready"
    owner:         "sample-team"
    verifies: [
        "REQ-SAMPLE-0001",
    ]
    scope:               "Verify something."
    verification_method: "Run a check."
    procedure: [
        "Run the check.",
    ]
    expected_result: "The check passes."
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task MigrateMarkdownAndGenerateMarkdownRoundTrips()
    {
        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample"));
        File.WriteAllText(Path.Combine(_rootPath, "examples", "sample", "SPEC-SAMPLE.md"), """
---
artifact_id: SPEC-SAMPLE
artifact_type: specification
title: Sample Specification
domain: sample
capability: sample-capability
status: draft
owner: sample-team
---

# SPEC-SAMPLE - Sample Specification

## Purpose

State the purpose.

## [`REQ-SAMPLE-0001`](./SPEC-SAMPLE.md) Carry one keyword
The sample MUST validate.
""");

        var migrateExitCode = await CommandDispatcher.RunAsync(["migrate-markdown", "--root", _rootPath]);
        Assert.Equal(0, migrateExitCode);
        Assert.True(File.Exists(Path.Combine(_rootPath, "examples", "sample", "SPEC-SAMPLE.cue")));

        var generateExitCode = await CommandDispatcher.RunAsync(["generate-markdown", "--root", _rootPath]);
        Assert.Equal(0, generateExitCode);

        var generatedMarkdown = File.ReadAllText(Path.Combine(_rootPath, "examples", "sample", "SPEC-SAMPLE.md"));
        Assert.Contains("artifact_id: SPEC-SAMPLE", generatedMarkdown, StringComparison.Ordinal);
        Assert.Contains("REQ-SAMPLE-0001", generatedMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateMarkdownCheckRejectsOutOfDateMarkdown()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Sample Specification"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Validate generated Markdown drift detection."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Carry one keyword"
            statement: "The sample MUST carry one keyword."
        },
    ]
}
""");

        File.WriteAllText(Path.Combine(_rootPath, "specs", "requirements", "sample", "SPEC-SAMPLE.md"), "# stale");

        var exitCode = await CommandDispatcher.RunAsync(["generate-markdown", "--root", _rootPath, "--check"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task BuildCatalogWritesJsonAndCueOutputs()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts

import model "github.com/incursa/spec-trace/model@v0"

artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Sample Specification"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Validate catalog export."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Carry one keyword"
            statement: "The sample MUST carry one keyword."
            trace: {
                implemented_by: [
                    "WI-SAMPLE-0001",
                ]
            }
        },
    ]
}
""");

        WriteCue("examples/sample/WI-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#WorkItem & {
    artifact_id:   "WI-SAMPLE-0001"
    artifact_type: "work_item"
    title:         "Work item"
    domain:        "sample"
    status:        "planned"
    owner:         "sample-team"
    addresses: [
        "REQ-SAMPLE-0001",
    ]
    design_links: [
        "ARC-SAMPLE-0001",
    ]
    verification_links: [
        "VER-SAMPLE-0001",
    ]
    summary:           "Do work."
    planned_changes:   "Change something."
    verification_plan: "Verify something."
}
""");

        WriteCue("examples/sample/ARC-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Architecture & {
    artifact_id:   "ARC-SAMPLE-0001"
    artifact_type: "architecture"
    title:         "Architecture"
    domain:        "sample"
    status:        "draft"
    owner:         "sample-team"
    satisfies: [
        "REQ-SAMPLE-0001",
    ]
    purpose:        "Design."
    design_summary: "Summary."
}
""");

        WriteCue("examples/sample/VER-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Verification & {
    artifact_id:   "VER-SAMPLE-0001"
    artifact_type: "verification"
    title:         "Verification"
    domain:        "sample"
    status:        "planned"
    owner:         "sample-team"
    verifies: [
        "REQ-SAMPLE-0001",
    ]
    scope:               "Scope."
    verification_method: "Method."
    procedure: [
        "Run step.",
    ]
    expected_result: "Result."
}
""");

        var jsonOutputPath = Path.Combine(_rootPath, "specs", "generated", "catalog.json");
        var cueOutputPath = Path.Combine(_rootPath, "specs", "generated", "catalog.cue");

        var exitCode = await CommandDispatcher.RunAsync([
            "build-catalog",
            "--root", _rootPath,
            "--json-out", jsonOutputPath,
            "--cue-out", cueOutputPath,
        ]);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(jsonOutputPath));
        Assert.True(File.Exists(cueOutputPath));

        var json = File.ReadAllText(jsonOutputPath);
        Assert.Contains("SPEC-SAMPLE", json, StringComparison.Ordinal);
        Assert.Contains("REQ-SAMPLE-0001", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateEvidenceAcceptsValidEvidenceSnapshot()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Collect evidence"
            statement: "The sample MUST collect evidence."
        },
    ]
}
""");

        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample", "generated"));
        File.WriteAllText(Path.Combine(_rootPath, "examples", "sample", "generated", "sample.evidence.json"), """
{
  "snapshot_id": "sample-evidence-001",
  "generated_at": "2026-03-30T18:00:00Z",
  "producer": {
    "name": "spec-trace-tests",
    "version": "1.0.0"
  },
  "requirements": [
    {
      "requirement_id": "REQ-SAMPLE-0001",
      "observations": [
        {
          "kind": "unit_test",
          "status": "passed",
          "refs": [
            "tests/sample/unit"
          ]
        }
      ]
    }
  ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync([
            "validate-evidence",
            "--root", _rootPath,
            "--evidence-path", Path.Combine(_rootPath, "examples", "sample", "generated", "sample.evidence.json"),
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ValidateEvidenceRejectsUnknownRequirementReference()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Collect evidence"
            statement: "The sample MUST collect evidence."
        },
    ]
}
""");

        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample", "generated"));
        File.WriteAllText(Path.Combine(_rootPath, "examples", "sample", "generated", "invalid.evidence.json"), """
{
  "snapshot_id": "sample-evidence-002",
  "generated_at": "2026-03-30T18:00:00Z",
  "producer": {
    "name": "spec-trace-tests",
    "version": "1.0.0"
  },
  "requirements": [
    {
      "requirement_id": "REQ-SAMPLE-9999",
      "observations": [
        {
          "kind": "unit_test",
          "status": "passed"
        }
      ]
    }
  ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync([
            "validate-evidence",
            "--root", _rootPath,
            "--evidence-path", Path.Combine(_rootPath, "examples", "sample", "generated", "invalid.evidence.json"),
        ]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task GenerateAttestationWritesHtmlAndJsonOutputs()
    {
        WriteCue("specs/requirements/sample/SPEC-SAMPLE.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Specification & {
    artifact_id:   "SPEC-SAMPLE"
    artifact_type: "specification"
    title:         "Spec"
    domain:        "sample"
    capability:    "sample-capability"
    status:        "draft"
    owner:         "sample-team"
    purpose:       "Purpose."
    requirements: [
        {
            id:        "REQ-SAMPLE-0001"
            title:     "Collect evidence"
            statement: "The sample MUST collect evidence."
            trace: {
                satisfied_by: [
                    "ARC-SAMPLE-0001",
                ]
                implemented_by: [
                    "WI-SAMPLE-0001",
                ]
                verified_by: [
                    "VER-SAMPLE-0001",
                ]
            }
        },
    ]
}
""");

        WriteCue("examples/sample/ARC-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Architecture & {
    artifact_id:   "ARC-SAMPLE-0001"
    artifact_type: "architecture"
    title:         "Architecture"
    domain:        "sample"
    status:        "approved"
    owner:         "sample-team"
    satisfies: [
        "REQ-SAMPLE-0001",
    ]
    purpose:        "Design."
    design_summary: "Summary."
}
""");

        WriteCue("examples/sample/WI-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#WorkItem & {
    artifact_id:   "WI-SAMPLE-0001"
    artifact_type: "work_item"
    title:         "Work item"
    domain:        "sample"
    status:        "complete"
    owner:         "sample-team"
    addresses: [
        "REQ-SAMPLE-0001",
    ]
    design_links: [
        "ARC-SAMPLE-0001",
    ]
    verification_links: [
        "VER-SAMPLE-0001",
    ]
    summary:           "Do work."
    planned_changes:   "Change something."
    verification_plan: "Verify something."
}
""");

        WriteCue("examples/sample/VER-SAMPLE-0001.cue", """
package artifacts
import model "github.com/incursa/spec-trace/model@v0"
artifact: model.#Verification & {
    artifact_id:   "VER-SAMPLE-0001"
    artifact_type: "verification"
    title:         "Verification"
    domain:        "sample"
    status:        "passed"
    owner:         "sample-team"
    verifies: [
        "REQ-SAMPLE-0001",
    ]
    scope:               "Scope."
    verification_method: "Method."
    procedure: [
        "Run step."
    ]
    expected_result: "Result."
}
""");

        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample", "generated"));
        File.WriteAllText(Path.Combine(_rootPath, "examples", "sample", "generated", "sample.evidence.json"), """
{
  "snapshot_id": "sample-evidence-003",
  "generated_at": "2026-03-30T18:00:00Z",
  "producer": {
    "name": "spec-trace-tests",
    "version": "1.0.0"
  },
  "requirements": [
    {
      "requirement_id": "REQ-SAMPLE-0001",
      "observations": [
        {
          "kind": "unit_test",
          "status": "passed",
          "refs": [
            "tests/sample/unit"
          ]
        },
        {
          "kind": "code_ref",
          "status": "observed",
          "refs": [
            "src/sample/implementation"
          ]
        }
      ]
    }
  ]
}
""");

        var outputDirectory = Path.Combine(_rootPath, "artifacts", "spec-trace", "attestation");
        var exitCode = await CommandDispatcher.RunAsync([
            "generate-attestation",
            "--root", _rootPath,
            "--profile", "core",
            "--emit", "both",
            "--out-dir", outputDirectory,
            "--evidence-path", Path.Combine(_rootPath, "examples", "sample", "generated", "sample.evidence.json"),
        ]);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(Path.Combine(outputDirectory, "index.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "summary.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "details.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "attestation.json")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "specs", "requirements", "sample", "SPEC-SAMPLE", "index.html")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private void WriteCue(string relativePath, string content)
    {
        var path = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)));
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            CopyDirectory(directory, Path.Combine(destinationDirectory, Path.GetFileName(directory)));
        }
    }
}
