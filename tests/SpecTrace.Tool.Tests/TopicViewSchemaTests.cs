using System.Text.Json;
using SpecTrace.Tool;

namespace SpecTrace.Tool.Tests;

public sealed class TopicViewSchemaTests
{
    [Fact]
    public void ValidateTopicViewDefinitionAcceptsValidSelectionObject()
    {
        var validator = JsonSchemaValidator.Load(RepositoryRoot);
        var path = WriteTempJson("""
{
  "name": "tls-topic",
  "description": "Select requirements related to TLS behavior in QUIC.",
  "match": {
    "all": [
      {
        "literal": {
          "fields": [
            "requirement.statement",
            "requirement.trace.upstream_refs"
          ],
          "value": "TLS",
          "case": "insensitive"
        }
      },
      {
        "not": {
          "requirement_ids": {
            "values": [
              "REQ-SAMPLE-9999"
            ]
          }
        }
      }
    ]
  },
  "include_requirements": [
    "REQ-SAMPLE-0001"
  ],
  "exclude_requirements": [
    "REQ-SAMPLE-0002"
  ]
}
""");

        try
        {
            var topicView = validator.LoadTopicViewDefinition(RepositoryRoot, path);
            Assert.Equal("tls-topic", topicView.GetProperty("name").GetString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ValidateTopicViewDefinitionRejectsUnknownOperator()
    {
        var validator = JsonSchemaValidator.Load(RepositoryRoot);
        var path = WriteTempJson("""
{
  "name": "broken-topic",
  "match": {
    "fuzzy": {}
  }
}
""");

        try
        {
            Assert.Throws<InvalidOperationException>(() => validator.LoadTopicViewDefinition(RepositoryRoot, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ValidateTopicViewDefinitionRejectsUnknownSelector()
    {
        var validator = JsonSchemaValidator.Load(RepositoryRoot);
        var path = WriteTempJson("""
{
  "name": "broken-topic",
  "match": {
    "literal": {
      "fields": [
        "requirement.statement",
        "custom.selector"
      ],
      "value": "TLS",
      "case": "insensitive"
    }
  }
}
""");

        try
        {
            Assert.Throws<InvalidOperationException>(() => validator.LoadTopicViewDefinition(RepositoryRoot, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string WriteTempJson(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "spec-trace-topic-view-tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }
}
