using System.Text.Json;
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
      "coverage": {
        "positive": "required",
        "negative": "optional",
        "edge": "required",
        "fuzz": "not_applicable"
      },
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
    public async Task ValidateRejectsInvalidCoverageStatusViaJsonSchema()
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
      "title": "Invalid coverage status",
      "statement": "The sample MUST validate.",
      "coverage": {
        "positive": "required",
        "negative": "sometimes",
        "edge": "required",
        "fuzz": "optional"
      }
    }
  ]
}
""");

        var exitCode = await CommandDispatcher.RunAsync(["validate", "--root", _rootPath, "--profile", "core"]);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ValidateRejectsUnexpectedCoverageKeyViaJsonSchema()
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
      "title": "Unexpected coverage key",
      "statement": "The sample MUST validate.",
      "coverage": {
        "positive": "required",
        "negative": "optional",
        "edge": "required",
        "fuzz": "optional",
        "benchmark": "required"
      }
    }
  ]
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
    public async Task ResolveTopicViewWritesMachineReadableSelectionResult()
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
  "purpose": "Resolve a topic view.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Carry one keyword",
      "statement": "The sample MUST mention TLS behavior.",
      "trace": {
        "related": [
          "REQ-SAMPLE-0002"
        ]
      }
    },
    {
      "id": "REQ-SAMPLE-0002",
      "title": "Carry another keyword",
      "statement": "The sample MUST mention QUIC behavior."
    }
  ]
}
"""));

        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        Console.SetOut(stdout);
        Console.SetError(stderr);

        try
        {
            var exitCode = await CommandDispatcher.RunAsync([
                "resolve-topic-view",
                "--root", _rootPath,
                "--topic-view-json", """
{
  "name": "sample-topic",
  "match": {
    "literal": {
      "fields": [
        "requirement.statement"
      ],
      "value": "TLS",
      "case": "insensitive"
    }
  }
}
"""
            ]);

            Assert.Equal(0, exitCode);

            var json = stdout.ToString().Trim();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.Equal("1", root.GetProperty("version").GetString());
            Assert.Equal("inline", root.GetProperty("input").GetProperty("kind").GetString());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("selected_count").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("matched_count").GetInt32());
            Assert.Equal(0, root.GetProperty("summary").GetProperty("warning_count").GetInt32());
            Assert.Empty(root.GetProperty("findings").EnumerateArray());

            var validator = JsonSchemaValidator.Load(RepositoryRoot);
            var resultPath = WriteTempJson(json);
            try
            {
                var resultDocument = validator.LoadTopicViewResult(RepositoryRoot, resultPath);
                Assert.Equal("1", resultDocument.GetProperty("version").GetString());
            }
            finally
            {
                File.Delete(resultPath);
            }

            var selected = root.GetProperty("selected_requirements").EnumerateArray().Single();
            Assert.Equal("REQ-SAMPLE-0001", selected.GetProperty("requirement_id").GetString());
            Assert.Equal("SPEC-SAMPLE", selected.GetProperty("artifact_id").GetString());
            Assert.True(selected.GetProperty("selection").GetProperty("selected").GetBoolean());
            Assert.True(selected.GetProperty("selection").GetProperty("matched").GetBoolean());
            Assert.False(selected.GetProperty("selection").GetProperty("explicit_include").GetBoolean());
            Assert.False(selected.GetProperty("selection").GetProperty("explicit_exclude").GetBoolean());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }

    [Fact]
    public async Task ResolveTopicViewReportsUnknownExplicitRequirementIdsAsWarnings()
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
  "purpose": "Resolve a topic view.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Carry one keyword",
      "statement": "The sample MUST mention TLS behavior."
    }
  ]
}
"""));

        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        Console.SetOut(stdout);
        Console.SetError(stderr);

        try
        {
            var exitCode = await CommandDispatcher.RunAsync([
                "resolve-topic-view",
                "--root", _rootPath,
                "--topic-view-json", """
{
  "name": "sample-topic",
  "include_requirements": [
    "REQ-SAMPLE-9999"
  ]
}
"""
            ]);

            Assert.Equal(0, exitCode);

            var json = stdout.ToString().Trim();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.Equal(0, root.GetProperty("summary").GetProperty("selected_count").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("warning_count").GetInt32());

            var finding = root.GetProperty("findings").EnumerateArray().Single();
            Assert.Equal("warning", finding.GetProperty("severity").GetString());
            Assert.Equal("unknown-explicit-include", finding.GetProperty("code").GetString());
            Assert.Equal("REQ-SAMPLE-9999", finding.GetProperty("requirement_id").GetString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }

    [Fact]
    public async Task ResolveTopicViewReportsExplicitMembershipConflictsAsWarnings()
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
  "purpose": "Resolve a topic view.",
  "requirements": [
    {
      "id": "REQ-SAMPLE-0001",
      "title": "Carry one keyword",
      "statement": "The sample MUST mention TLS behavior."
    }
  ]
}
"""));

        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        Console.SetOut(stdout);
        Console.SetError(stderr);

        try
        {
            var exitCode = await CommandDispatcher.RunAsync([
                "resolve-topic-view",
                "--root", _rootPath,
                "--topic-view-json", """
{
  "name": "sample-topic",
  "include_requirements": [
    "REQ-SAMPLE-0001"
  ],
  "exclude_requirements": [
    "REQ-SAMPLE-0001"
  ]
}
"""
            ]);

            Assert.Equal(0, exitCode);

            var json = stdout.ToString().Trim();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.Equal(0, root.GetProperty("summary").GetProperty("selected_count").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("warning_count").GetInt32());
            Assert.Equal(1, root.GetProperty("summary").GetProperty("conflict_count").GetInt32());

            var finding = root.GetProperty("findings").EnumerateArray().Single();
            Assert.Equal("explicit-membership-conflict", finding.GetProperty("code").GetString());
            Assert.Equal("REQ-SAMPLE-0001", finding.GetProperty("requirement_id").GetString());

            var excluded = root.GetProperty("explicitly_excluded_requirements").EnumerateArray().Single();
            Assert.Equal("REQ-SAMPLE-0001", excluded.GetProperty("requirement_id").GetString());
            Assert.False(excluded.GetProperty("selection").GetProperty("selected").GetBoolean());
            Assert.True(excluded.GetProperty("selection").GetProperty("explicit_include").GetBoolean());
            Assert.True(excluded.GetProperty("selection").GetProperty("explicit_exclude").GetBoolean());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
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

    private static string WriteTempJson(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "spec-trace-topic-view-tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
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
