using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpecTrace.Tool;

internal static class CatalogSnapshotWriter
{
    public static string WriteCue(CatalogSnapshot snapshot)
    {
        var root = new JsonObject
        {
            ["entries"] = BuildEntriesNode(snapshot.Entries),
            ["references"] = BuildReferencesNode(snapshot.References),
        };

        var builder = new StringBuilder();
        builder.AppendLine("package catalog");
        builder.AppendLine();
        builder.AppendLine(@"import model ""github.com/incursa/spec-trace/model@v0""");
        builder.AppendLine();
        builder.Append("catalog: model.#Catalog & ");
        WriteCueNode(builder, root, 0);
        builder.AppendLine();
        return builder.ToString();
    }

    private static JsonObject BuildEntriesNode(IReadOnlyDictionary<string, CatalogItem> entries)
    {
        var result = new JsonObject();
        foreach (var pair in entries.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            result[pair.Key] = JsonSerializer.SerializeToNode(pair.Value, JsonOptions.Default);
        }

        return result;
    }

    private static JsonArray BuildReferencesNode(IEnumerable<CatalogReference> references)
    {
        var result = new JsonArray();
        foreach (var reference in references
                     .OrderBy(reference => reference.SourceId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(reference => reference.Field, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(reference => reference.TargetId, StringComparer.OrdinalIgnoreCase))
        {
            result.Add(JsonSerializer.SerializeToNode(reference, JsonOptions.Default));
        }

        return result;
    }

    private static void WriteCueNode(StringBuilder builder, JsonNode? node, int indentLevel)
    {
        switch (node)
        {
            case null:
                builder.Append("null");
                break;
            case JsonObject obj:
                WriteCueObject(builder, obj, indentLevel);
                break;
            case JsonArray array:
                WriteCueArray(builder, array, indentLevel);
                break;
            case JsonValue value:
                WriteCueValue(builder, value);
                break;
            default:
                throw new InvalidOperationException($"Unsupported JSON node type '{node.GetType().Name}'.");
        }
    }

    private static void WriteCueObject(StringBuilder builder, JsonObject obj, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var childIndent = new string(' ', (indentLevel + 1) * 4);
        builder.AppendLine("{");

        var properties = obj.ToList();
        for (var index = 0; index < properties.Count; index++)
        {
            var property = properties[index];
            builder.Append(childIndent);
            builder.Append(JsonSerializer.Serialize(property.Key, JsonOptions.Default));
            builder.Append(": ");
            WriteCueNode(builder, property.Value, indentLevel + 1);
            if (index < properties.Count - 1)
            {
                builder.AppendLine();
            }
        }

        builder.AppendLine();
        builder.Append(indent);
        builder.Append("}");
    }

    private static void WriteCueArray(StringBuilder builder, JsonArray array, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var childIndent = new string(' ', (indentLevel + 1) * 4);
        builder.AppendLine("[");

        for (var index = 0; index < array.Count; index++)
        {
            builder.Append(childIndent);
            WriteCueNode(builder, array[index], indentLevel + 1);
            builder.Append(",");
            if (index < array.Count - 1)
            {
                builder.AppendLine();
            }
        }

        builder.AppendLine();
        builder.Append(indent);
        builder.Append("]");
    }

    private static void WriteCueValue(StringBuilder builder, JsonValue value)
    {
        var json = value.ToJsonString(JsonOptions.Default);
        builder.Append(json);
    }
}
