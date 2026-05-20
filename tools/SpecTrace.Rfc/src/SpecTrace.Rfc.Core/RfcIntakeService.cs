using System.Text.Json;
using System.Text.RegularExpressions;

namespace SpecTrace.Rfc.Core;

public sealed class RfcIngestOptions
{
    public string? RfcNumber { get; init; }

    public string? Source { get; init; }

    public string? SourceId { get; init; }

    public string? Title { get; init; }
}

public sealed class RfcIntakeService
{
    private static readonly Regex NonIdentifierCharacters = new("[^A-Za-z0-9]+", RegexOptions.Compiled);
    private readonly HttpClient _httpClient;

    public RfcIntakeService()
        : this(new HttpClient())
    {
    }

    public RfcIntakeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RfcSourceDocument> IngestAsync(RfcIngestOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.RfcNumber) && string.IsNullOrWhiteSpace(options.Source))
        {
            throw new InvalidOperationException("Provide --rfc or --source.");
        }

        var source = BuildSource(options);
        var rawContent = await ReadSourceTextAsync(source, cancellationToken);
        var isHtml = RfcHtmlTextExtractor.LooksLikeHtml(source, rawContent);
        var content = isHtml ? RfcHtmlTextExtractor.ToPlainText(rawContent) : rawContent;
        var rfcNumber = NormalizeRfcNumber(options.RfcNumber) ??
            (isHtml ? RfcHtmlTextExtractor.TryGetRfcNumber(rawContent) : null) ??
            TryInferRfcNumber(source, rawContent);
        var sourceId = NormalizeSourceId(options.SourceId, rfcNumber, source);
        var title = string.IsNullOrWhiteSpace(options.Title)
            ? (isHtml ? RfcHtmlTextExtractor.TryGetTitle(rawContent) ?? InferTitle(content, sourceId) : InferTitle(content, sourceId))
            : options.Title.Trim();

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        metadata["source_format"] = isHtml ? "html" : "text";
        metadata["raw_text_hash"] = Hashing.Sha256Text(rawContent);
        if (!string.IsNullOrWhiteSpace(rfcNumber))
        {
            metadata["rfc_number"] = rfcNumber;
        }

        var sourceUrl = IsHttpSource(source)
            ? source
            : isHtml ? RfcHtmlTextExtractor.TryGetSavedFromUrl(rawContent) : null;
        sourceUrl ??= string.IsNullOrWhiteSpace(rfcNumber) ? null : $"https://www.rfc-editor.org/rfc/rfc{rfcNumber}.txt";

        return new RfcSourceDocument
        {
            SourceId = sourceId,
            RfcNumber = rfcNumber,
            Title = title,
            SourceUrl = sourceUrl,
            CanonicalUrl = string.IsNullOrWhiteSpace(rfcNumber) ? null : $"https://www.rfc-editor.org/rfc/rfc{rfcNumber}.html",
            RetrievedAt = DateTimeOffset.UtcNow.ToString("O"),
            TextHash = Hashing.Sha256Text(content),
            Content = content,
            Metadata = metadata,
        };
    }

    public static async Task WriteAsync(string path, RfcSourceDocument document, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(document, RfcJson.Options), cancellationToken);
    }

    public static async Task<RfcSourceDocument> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<RfcSourceDocument>(json, RfcJson.Options)
            ?? throw new InvalidOperationException($"RFC source document '{path}' deserialized to null.");
    }

    private static string BuildSource(RfcIngestOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Source))
        {
            return options.Source.Trim();
        }

        var rfcNumber = NormalizeRfcNumber(options.RfcNumber)
            ?? throw new InvalidOperationException("RFC number was empty.");
        return $"https://www.rfc-editor.org/rfc/rfc{rfcNumber}.txt";
    }

    private async Task<string> ReadSourceTextAsync(string source, CancellationToken cancellationToken)
    {
        if (IsHttpSource(source))
        {
            return await _httpClient.GetStringAsync(source, cancellationToken);
        }

        var path = Path.GetFullPath(source);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"RFC source file '{path}' was not found.", path);
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    private static bool IsHttpSource(string source)
    {
        return Uri.TryCreate(source, UriKind.Absolute, out var uri) &&
               (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeRfcNumber(string? rfcNumber)
    {
        if (string.IsNullOrWhiteSpace(rfcNumber))
        {
            return null;
        }

        var trimmed = rfcNumber.Trim();
        return trimmed.StartsWith("RFC", StringComparison.OrdinalIgnoreCase)
            ? trimmed[3..].Trim()
            : trimmed;
    }

    private static string NormalizeSourceId(string? requestedSourceId, string? rfcNumber, string source)
    {
        if (!string.IsNullOrWhiteSpace(requestedSourceId))
        {
            return requestedSourceId.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(rfcNumber))
        {
            return $"RFC{rfcNumber}";
        }

        var leaf = IsHttpSource(source)
            ? Path.GetFileName(new Uri(source).AbsolutePath)
            : Path.GetFileName(source);
        var withoutExtension = Path.GetFileNameWithoutExtension(leaf);
        var normalized = NonIdentifierCharacters.Replace(withoutExtension, string.Empty).ToUpperInvariant();

        return string.IsNullOrWhiteSpace(normalized) ? "RFC-SOURCE" : normalized;
    }

    private static string? TryInferRfcNumber(string source, string content)
    {
        var sourceMatch = Regex.Match(source, @"rfc[\s_-]*(?<value>\d+)", RegexOptions.IgnoreCase);
        if (sourceMatch.Success)
        {
            return sourceMatch.Groups["value"].Value;
        }

        var contentMatch = Regex.Match(content, @"Request\s+for\s+Comments:\s*(?<value>\d+)", RegexOptions.IgnoreCase);
        return contentMatch.Success ? contentMatch.Groups["value"].Value : null;
    }

    private static string InferTitle(string content, string sourceId)
    {
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 ||
                trimmed.StartsWith("Request for Comments:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("RFC ", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("[Page ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return trimmed.Length > 120 ? trimmed[..120] : trimmed;
        }

        return sourceId;
    }
}
