using SpecTrace.Tool;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SpecTrace.Tool.Tests;

public sealed class CommandDispatcherTests : IDisposable
{
    private readonly string _rootPath;

    public CommandDispatcherTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "spec-trace-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
        CopyDirectory(Path.Combine(RepositoryRoot, "model"), Path.Combine(_rootPath, "model"));
        CopyDirectory(Path.Combine(RepositoryRoot, "schemas"), Path.Combine(_rootPath, "schemas"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "specs", "requirements", "sample"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "catalog"));
    }

    [Fact]
    public async Task ValidateAcceptsValidRepository()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", SpecificationJson("""
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Sample Specification",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "purpose": "Validate a simple requirement.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Carry one keyword",
      "statement": "The sample MUST carry one keyword.",
      "trace": {
        "satisfied_by": [
          "ARC-SAMPLE-0001"
        ]
      }
    }
  ]
}
"""));

        WriteJson("examples/sample/ARC-SAMPLE-0001.json", """
{
  "artifact_id": "ARC-SAMPLE-0001",
  "artifact_type": "architecture",
  "title": "Sample Architecture",
  "domain": "sample",
  "status": "approved",
  "owner": "sample-team",
  "satisfies": [
    "REQ-SAMPLE-0001"
  ],
  "purpose": "Satisfy the sample requirement.",
  "design_summary": "A small architecture record."
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsMissingPurpose()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", """
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Spec",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Missing purpose",
      "statement": "The sample MUST validate."
    }
  ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsBrokenReferences()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", SpecificationJson("""
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Spec",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "purpose": "Purpose.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Broken reference",
      "statement": "The sample MUST validate.",
      "trace": {
        "verified_by": [
          "VER-SAMPLE-0001"
        ]
      }
    }
  ]
}
"""));

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "traceable"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsUnexpectedPropertiesViaJsonSchema()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", """
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Spec",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "purpose": "Purpose.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Unexpected property",
      "statement": "The sample MUST validate."
    }
  ],
  "unexpected_field": "not allowed"
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task BuildCatalogWritesJsonOutput()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", SpecificationJson("""
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Sample Specification",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "purpose": "Validate catalog export.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Carry one keyword",
      "statement": "The sample MUST carry one keyword.",
      "trace": {
        "implemented_by": [
          "WI-SAMPLE-0001"
        ]
      }
    }
  ]
}
"""));

        WriteJson("examples/sample/WI-SAMPLE-0001.json", """
{
  "artifact_id": "WI-SAMPLE-0001",
  "artifact_type": "work_item",
  "title": "Work item",
  "domain": "sample",
  "status": "planned",
  "owner": "sample-team",
  "addresses": [
    "REQ-SAMPLE-0001"
  ],
  "design_links": [
    "ARC-SAMPLE-0001"
  ],
  "verification_links": [
    "VER-SAMPLE-0001"
  ],
  "summary": "Do work.",
  "planned_changes": "Change something.",
  "verification_plan": "Verify something."
}
""");

        WriteJson("examples/sample/ARC-SAMPLE-0001.json", """
{
  "artifact_id": "ARC-SAMPLE-0001",
  "artifact_type": "architecture",
  "title": "Architecture",
  "domain": "sample",
  "status": "draft",
  "owner": "sample-team",
  "satisfies": [
    "REQ-SAMPLE-0001"
  ],
  "purpose": "Design.",
  "design_summary": "Summary."
}
""");

        WriteJson("examples/sample/VER-SAMPLE-0001.json", """
{
  "artifact_id": "VER-SAMPLE-0001",
  "artifact_type": "verification",
  "title": "Verification",
  "domain": "sample",
  "status": "planned",
  "owner": "sample-team",
  "verifies": [
    "REQ-SAMPLE-0001"
  ],
  "scope": "Scope.",
  "verification_method": "Method.",
  "procedure": [
    "Run step."
  ],
  "expected_result": "Result."
}
""");

        var jsonOutputPath = Path.Combine(_rootPath, "specs", "generated", "catalog.json");
        var exitCode = await CommandDispatcher.RunAsync([
            "build-catalog",
            "--root", _rootPath,
            "--json-out", jsonOutputPath,
        ]);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(jsonOutputPath));

        var json = File.ReadAllText(jsonOutputPath);
        Assert.Contains("SPEC-SAMPLE", json, StringComparison.Ordinal);
        Assert.Contains("REQ-SAMPLE-0001", json, StringComparison.Ordinal);
        Assert.DoesNotContain("markdown_path", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateEvidenceAcceptsValidEvidenceSnapshot()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", SpecificationJson("""
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Spec",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "purpose": "Purpose.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Collect evidence",
      "statement": "The sample MUST collect evidence."
    }
  ]
}
"""));

        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample", "generated"));
        WriteJson("examples/sample/generated/sample.evidence.json", """
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
    public async Task GenerateAttestationWritesHtmlAndJsonOutputs()
    {
        WriteJson("specs/requirements/sample/SPEC-SAMPLE.json", SpecificationJson("""
{
  "artifact_id": "SPEC-SAMPLE",
  "artifact_type": "specification",
  "title": "Spec",
  "domain": "sample",
  "capability": "sample-capability",
  "status": "draft",
  "owner": "sample-team",
  "purpose": "Purpose.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Collect evidence",
      "statement": "The sample MUST collect evidence.",
      "trace": {
        "satisfied_by": [
          "ARC-SAMPLE-0001"
        ],
        "implemented_by": [
          "WI-SAMPLE-0001"
        ],
        "verified_by": [
          "VER-SAMPLE-0001"
        ]
      }
    }
  ]
}
"""));

        WriteJson("examples/sample/ARC-SAMPLE-0001.json", """
{
  "artifact_id": "ARC-SAMPLE-0001",
  "artifact_type": "architecture",
  "title": "Architecture",
  "domain": "sample",
  "status": "approved",
  "owner": "sample-team",
  "satisfies": [
    "REQ-SAMPLE-0001"
  ],
  "purpose": "Design.",
  "design_summary": "Summary."
}
""");

        WriteJson("examples/sample/WI-SAMPLE-0001.json", """
{
  "artifact_id": "WI-SAMPLE-0001",
  "artifact_type": "work_item",
  "title": "Work item",
  "domain": "sample",
  "status": "complete",
  "owner": "sample-team",
  "addresses": [
    "REQ-SAMPLE-0001"
  ],
  "design_links": [
    "ARC-SAMPLE-0001"
  ],
  "verification_links": [
    "VER-SAMPLE-0001"
  ],
  "summary": "Do work.",
  "planned_changes": "Change something.",
  "verification_plan": "Verify something."
}
""");

        WriteJson("examples/sample/VER-SAMPLE-0001.json", """
{
  "artifact_id": "VER-SAMPLE-0001",
  "artifact_type": "verification",
  "title": "Verification",
  "domain": "sample",
  "status": "passed",
  "owner": "sample-team",
  "verifies": [
    "REQ-SAMPLE-0001"
  ],
  "scope": "Scope.",
  "verification_method": "Method.",
  "procedure": [
    "Run step."
  ],
  "expected_result": "Result."
}
""");

        Directory.CreateDirectory(Path.Combine(_rootPath, "examples", "sample", "generated"));
        WriteJson("examples/sample/generated/sample.evidence.json", """
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

    private static string SpecificationJson(string json) => json;

    private void WriteJson(string relativePath, string content)
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
