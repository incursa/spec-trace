using System.Text.Json;

namespace SpecTrace.Tool;

internal static class CanonicalJsonLoader
{
    public static async Task<List<(string SourcePath, ArtifactModel Artifact)>> LoadArtifactsAsync(string rootPath, string? inputPath, CancellationToken cancellationToken = default)
    {
        var schemaValidator = JsonSchemaValidator.Load(rootPath);
        var artifactFiles = EnumerateArtifactFiles(rootPath, inputPath)
            .Select(Path.GetFullPath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var artifacts = new List<(string SourcePath, ArtifactModel Artifact)>(artifactFiles.Count);
        foreach (var artifactFile in artifactFiles)
        {
            artifacts.Add((artifactFile, await LoadArtifactAsync(schemaValidator, rootPath, artifactFile, cancellationToken)));
        }

        return artifacts;
    }

    public static Task<ArtifactModel> LoadArtifactAsync(string rootPath, string path, CancellationToken cancellationToken = default)
    {
        return LoadArtifactAsync(JsonSchemaValidator.Load(rootPath), rootPath, path, cancellationToken);
    }

    private static Task<ArtifactModel> LoadArtifactAsync(JsonSchemaValidator schemaValidator, string rootPath, string path, CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(schemaValidator.LoadArtifact(rootPath, path));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FileNotFoundException)
        {
            throw new InvalidOperationException($"Invalid JSON artifact '{NormalizeRepoPath(rootPath, path)}': {exception.Message}", exception);
        }
    }

    public static async Task<RetiredRequirementLedger?> LoadRetiredLedgerAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        var ledgerPath = Path.Combine(rootPath, "catalog", "retired-requirements.json");
        if (!File.Exists(ledgerPath))
        {
            return null;
        }

        try
        {
            return await Task.FromResult(JsonSchemaValidator.Load(rootPath).LoadRetiredLedger(rootPath, ledgerPath));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FileNotFoundException)
        {
            throw new InvalidOperationException($"Invalid retired requirement ledger '{NormalizeRepoPath(rootPath, ledgerPath)}': {exception.Message}", exception);
        }
    }

    public static IEnumerable<string> EnumerateArtifactFiles(string rootPath, string? inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return EnumerateArtifactFilesInDirectory(Path.Combine(rootPath, "specs"), rootPath)
                .Concat(EnumerateArtifactFilesInDirectory(Path.Combine(rootPath, "examples"), rootPath));
        }

        if (File.Exists(inputPath))
        {
            return ShouldIncludeArtifactFile(rootPath, inputPath) ? [inputPath] : [];
        }

        if (!Directory.Exists(inputPath))
        {
            return [];
        }

        return EnumerateArtifactFilesInDirectory(inputPath, rootPath);
    }

    private static IEnumerable<string> EnumerateArtifactFilesInDirectory(string directory, string rootPath)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories)
            .Where(path => ShouldIncludeArtifactFile(rootPath, path));
    }

    private static bool ShouldIncludeArtifactFile(string rootPath, string path)
    {
        if (path.Contains($"{Path.DirectorySeparatorChar}generated{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = NormalizeRepoPath(rootPath, path);
        if (!(relativePath.StartsWith("specs/", StringComparison.OrdinalIgnoreCase) ||
              relativePath.StartsWith("examples/", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (Path.GetFileName(relativePath) is "_index.json" or "README.json")
        {
            return false;
        }

        return relativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
               !relativePath.EndsWith(".schema.json", StringComparison.OrdinalIgnoreCase) &&
               !relativePath.EndsWith(".evidence.json", StringComparison.OrdinalIgnoreCase) &&
               !relativePath.EndsWith("attestation.json", StringComparison.OrdinalIgnoreCase) &&
               !relativePath.EndsWith("spec-trace-catalog.json", StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeRepoPath(string rootPath, string path)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath.Replace('\\', '/');
        }

        return Path.GetRelativePath(rootPath, fullPath).Replace('\\', '/');
    }
}
