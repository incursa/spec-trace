using System.Text.Json;
using Json.Schema;

namespace SpecTrace.Tool;

internal sealed class JsonSchemaValidator
{
    private static readonly object SyncRoot = new();
    private static JsonSchemaValidator? _cached;

    private readonly JsonSchema _artifactSchema;
    private readonly JsonSchema _evidenceSchema;
    private readonly JsonSchema _retiredLedgerSchema;

    private JsonSchemaValidator(JsonSchema artifactSchema, JsonSchema evidenceSchema, JsonSchema retiredLedgerSchema)
    {
        _artifactSchema = artifactSchema;
        _evidenceSchema = evidenceSchema;
        _retiredLedgerSchema = retiredLedgerSchema;
    }

    public static JsonSchemaValidator Load(string rootPath)
    {
        lock (SyncRoot)
        {
            _cached ??= new JsonSchemaValidator(
                LoadSchema(Path.Combine(rootPath, "model", "model.schema.json")),
                LoadSchema(Path.Combine(rootPath, "schemas", "evidence-snapshot.schema.json")),
                LoadSchema(Path.Combine(rootPath, "schemas", "retired-requirement-ledger.schema.json")));

            return _cached;
        }
    }

    public ArtifactModel LoadArtifact(string rootPath, string jsonPath)
    {
        return LoadAndDeserialize<ArtifactModel>(_artifactSchema, rootPath, jsonPath, "artifact");
    }

    public RetiredRequirementLedger LoadRetiredLedger(string rootPath, string jsonPath)
    {
        return LoadAndDeserialize<RetiredRequirementLedger>(_retiredLedgerSchema, rootPath, jsonPath, "retired requirement ledger");
    }

    public EvidenceSnapshotModel LoadEvidenceSnapshot(string rootPath, string jsonPath)
    {
        return LoadAndDeserialize<EvidenceSnapshotModel>(_evidenceSchema, rootPath, jsonPath, "evidence snapshot");
    }

    private static T LoadAndDeserialize<T>(JsonSchema schema, string rootPath, string jsonPath, string documentKind)
    {
        var fullPath = Path.GetFullPath(jsonPath);
        var json = File.ReadAllText(fullPath);

        using var document = JsonDocument.Parse(json);
        Validate(schema, document.RootElement, rootPath, fullPath, documentKind);

        return JsonSerializer.Deserialize<T>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException($"Validated {documentKind} '{NormalizeRepoPath(rootPath, fullPath)}' could not be deserialized.");
    }

    private static void Validate(JsonSchema schema, JsonElement instance, string rootPath, string fullPath, string documentKind)
    {
        var evaluation = schema.Evaluate(instance, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
        });

        if (evaluation.IsValid)
        {
            return;
        }

        var errors = FlattenErrors(evaluation)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(message => message, StringComparer.Ordinal)
            .ToList();

        var relativePath = NormalizeRepoPath(rootPath, fullPath);
        var message = errors.Count == 0
            ? $"JSON Schema validation failed for {documentKind} '{relativePath}'."
            : $"JSON Schema validation failed for {documentKind} '{relativePath}':{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}";

        throw new InvalidOperationException(message);
    }

    private static IEnumerable<string> FlattenErrors(EvaluationResults evaluation)
    {
        if (evaluation.Errors is not null)
        {
            foreach (var pair in evaluation.Errors.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var instanceLocation = evaluation.InstanceLocation.ToString();
                if (string.IsNullOrWhiteSpace(instanceLocation))
                {
                    yield return $"{pair.Key}: {pair.Value}";
                    continue;
                }

                yield return $"{instanceLocation}: {pair.Key}: {pair.Value}";
            }
        }

        if (evaluation.Details is null)
        {
            yield break;
        }

        foreach (var detail in evaluation.Details)
        {
            foreach (var message in FlattenErrors(detail))
            {
                yield return message;
            }
        }
    }

    private static JsonSchema LoadSchema(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Schema file '{path}' was not found.", path);
        }

        return JsonSchema.FromText(File.ReadAllText(path));
    }

    private static string NormalizeRepoPath(string rootPath, string path)
    {
        return CanonicalJsonLoader.NormalizeRepoPath(rootPath, path);
    }
}
