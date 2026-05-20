using System.Text.Json;

namespace SpecTrace.Rfc.Core;

public static class Jsonl
{
    public static async Task<List<T>> ReadAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        var records = new List<T>();
        var lineNumber = 0;

        await foreach (var line in File.ReadLinesAsync(path, cancellationToken))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var record = JsonSerializer.Deserialize<T>(line, RfcJson.Options)
                    ?? throw new JsonException("The JSON line deserialized to null.");
                records.Add(record);
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException($"Invalid JSONL record in '{path}' at line {lineNumber}: {exception.Message}", exception);
            }
        }

        return records;
    }

    public static async Task WriteAsync<T>(string path, IEnumerable<T> records, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream);

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, RfcJson.JsonlOptions);
            await writer.WriteLineAsync(json.AsMemory(), cancellationToken);
        }
    }
}
