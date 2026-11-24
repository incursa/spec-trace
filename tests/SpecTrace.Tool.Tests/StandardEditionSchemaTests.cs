using System.Text.Json;
using SpecTrace.Tool;

namespace SpecTrace.Tool.Tests;

public sealed class StandardEditionSchemaTests
{
    [Fact]
    public void AcceptsBootstrapDraftWithIndependentValidation()
    {
        var path = WriteTempJson(Manifest("0.1.0-draft.1", "draft", true));
        try
        {
            var manifest = JsonSchemaValidator.Load(RepositoryRoot).LoadStandardEdition(RepositoryRoot, path);
            Assert.Equal("0.1.0-draft.1", manifest.GetProperty("edition").GetString());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RejectsMaturityThatDisagreesWithPrereleaseLabel()
    {
        var path = WriteTempJson(Manifest("0.1.0-rc.1", "draft", true));
        try
        {
            Assert.Throws<InvalidOperationException>(() => JsonSchemaValidator.Load(RepositoryRoot).LoadStandardEdition(RepositoryRoot, path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RejectsZeroPrereleaseOrdinal()
    {
        var path = WriteTempJson(Manifest("0.1.0-draft.0", "draft", true));
        try
        {
            Assert.Throws<InvalidOperationException>(() => JsonSchemaValidator.Load(RepositoryRoot).LoadStandardEdition(RepositoryRoot, path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RejectsSuccessorWithoutPriorEditionReview()
    {
        var path = WriteTempJson(Manifest("0.2.0-rc.1", "candidate", false));
        try
        {
            Assert.Throws<InvalidOperationException>(() => JsonSchemaValidator.Load(RepositoryRoot).LoadStandardEdition(RepositoryRoot, path));
        }
        finally { File.Delete(path); }
    }

    private static string Manifest(string edition, string maturity, bool bootstrap) => $$"""
{
  "format": "specattest.standard-edition.v1",
  "standardTitle": "SpecAttest Standard",
  "edition": "{{edition}}",
  "maturity": "{{maturity}}",
  "governance": "incursa-owner-approved",
  "source": {
    "repository": "https://github.com/incursa/spec-trace",
    "commit": "0000000000000000000000000000000000000000",
    "path": "specs/requirements/spec-trace/"
  },
  "payloadSha256": "0000000000000000000000000000000000000000000000000000000000000000",
  "schemaSha256": "0000000000000000000000000000000000000000000000000000000000000000",
  "canonicalization": "utf8-json-sorted-object-keys-v1",
  "specificationOrder": ["SPEC-STD"],
  "requirementRevisions": [
    {
      "id": "REQ-STD-0001",
      "revision": 1,
      "sha256": "0000000000000000000000000000000000000000000000000000000000000000"
    }
  ],
  "conformanceCorpusSha256": "0000000000000000000000000000000000000000000000000000000000000000",
  "validatedBy": [
    {
      "validator": "json-schema-validator",
      "version": "1",
      "role": "independent",
      "standardEdition": "{{edition}}",
      "result": "passed",
      "resultSha256": "0000000000000000000000000000000000000000000000000000000000000000",
      "conformanceCorpusSha256": "0000000000000000000000000000000000000000000000000000000000000000"
    },
    {
      "validator": "independent-validator",
      "version": "1",
      "role": "candidate",
      "standardEdition": "{{edition}}",
      "result": "passed",
      "resultSha256": "0000000000000000000000000000000000000000000000000000000000000000",
      "conformanceCorpusSha256": "0000000000000000000000000000000000000000000000000000000000000000"
    }
  ],
  "approvalReference": "review/1",
  "signature": {
    "kind": "release-manifest",
    "reference": "release/1",
    "sha256": "0000000000000000000000000000000000000000000000000000000000000000"
  },
  "bootstrap": {{bootstrap.ToString().ToLowerInvariant()}}
}
""";

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string WriteTempJson(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "specattest-edition-tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }
}
