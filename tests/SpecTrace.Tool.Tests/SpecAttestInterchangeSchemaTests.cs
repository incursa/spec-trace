using SpecTrace.Tool;

namespace SpecTrace.Tool.Tests;

public sealed class SpecAttestInterchangeSchemaTests
{
  [Fact]
  public void AcceptsRevisionDistinctFromProviderVersion()
  {
    var path = WriteTempJson(Envelope());
    try
    {
      var envelope = JsonSchemaValidator.Load(RepositoryRoot).LoadSpecAttestInterchange(RepositoryRoot, path);
      Assert.Equal(7, envelope.GetProperty("requirements")[0].GetProperty("revision").GetInt32());
      Assert.Equal(1, envelope.GetProperty("requirements")[0].GetProperty("providerRecordVersion").GetInt32());
    }
    finally { File.Delete(path); }
  }

  [Fact]
  public void RejectsMutableSignatureReference()
  {
    var path = WriteTempJson(Envelope().Replace("refs/tags/standard-v0.1.0-draft.1", "refs/heads/main", StringComparison.Ordinal));
    try
    {
      Assert.Throws<InvalidOperationException>(() => JsonSchemaValidator.Load(RepositoryRoot).LoadSpecAttestInterchange(RepositoryRoot, path));
    }
    finally { File.Delete(path); }
  }

  [Fact]
  public void RejectsZeroNormativeRevision()
  {
    var path = WriteTempJson(Envelope().Replace("\"revision\": 7", "\"revision\": 0", StringComparison.Ordinal));
    try
    {
      Assert.Throws<InvalidOperationException>(() => JsonSchemaValidator.Load(RepositoryRoot).LoadSpecAttestInterchange(RepositoryRoot, path));
    }
    finally { File.Delete(path); }
  }

  private static string Envelope() => """
{
  "format": "specattest.interchange.v1",
  "operation": "stage",
  "standardPin": {
    "contract": "specattest.standard-pin.v1",
    "repository": "https://github.com/incursa/spec-trace",
    "edition": "0.1.0-draft.1",
    "commit": "569569f7ffb2b68a45e4b5c3c276951404973470",
    "schemaPath": "model/model.schema.json",
    "schemaSha256": "27fbfef7d11743adaa5f95b14e16b9f9e71fd9de522e3a9f7e22e62c12e88ddb",
    "editionManifestSha256": "6b54ca666d90456ae7edb277114945fcc4163e6084a3f44e913fc19e40691fda",
    "editionManifestSchemaPath": "schemas/standard-edition.schema.json",
    "editionManifestSchemaSha256": "456de53b5359691b21540bd2803529119930c0db507d760060a224f42e0a7364",
    "payloadSha256": "288593ff470fc5661a370c22377ec7c7503ae24a66f50e014e1f42b0426f3db4",
    "conformanceCorpusSha256": "8d6a2d02d5293ca531aee9eaa9aff771dceb88d34f6d8dd45c396710e0881d33",
    "signatureReference": "refs/tags/standard-v0.1.0-draft.1",
    "signatureSha256": "c0c4e93600a803a3c3f48780e307666cfd76f7122a155c19e416f21774f6d056",
    "canonicalization": "utf8-json-sorted-object-keys-v1",
    "canonicalProfiles": ["core", "traceable", "auditable"],
    "proofProfile": "auditable"
  },
  "authorityMode": "manager",
  "transition": "manager-apply",
  "idempotencyKey": "qualification-001",
  "expectedProjectVersion": 4,
  "requirements": [
    {
      "id": "REQ-DEMO-0001",
      "revision": 7,
      "providerRecordVersion": 1,
      "statement": "A demo MUST preserve its revision.",
      "trace": { "supersedes": ["REQ-DEMO-0000"] }
    }
  ]
}
""";

  private static string RepositoryRoot =>
      Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

  private static string WriteTempJson(string content)
  {
    var path = Path.Combine(Path.GetTempPath(), "specattest-interchange-tests", $"{Guid.NewGuid():N}.json");
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content);
    return path;
  }
}
