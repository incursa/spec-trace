using System.Diagnostics;
using System.Text.Json;

namespace SpecTrace.Tool;

internal static class CueCli
{
    public static async Task<ArtifactModel> ExportArtifactAsync(string rootPath, string cuePath, CancellationToken cancellationToken)
    {
        var json = await ExportExpressionAsync(rootPath, cuePath, "artifact", cancellationToken);
        var artifact = JsonSerializer.Deserialize<ArtifactModel>(json, JsonOptions.Default);
        return artifact ?? throw new InvalidOperationException($"cue export returned no artifact payload for '{cuePath}'.");
    }

    public static async Task<RetiredRequirementLedger> ExportRetiredLedgerAsync(string rootPath, string cuePath, CancellationToken cancellationToken)
    {
        var json = await ExportExpressionAsync(rootPath, cuePath, "ledger", cancellationToken);
        var ledger = JsonSerializer.Deserialize<RetiredRequirementLedger>(json, JsonOptions.Default);
        return ledger ?? throw new InvalidOperationException($"cue export returned no retired ledger payload for '{cuePath}'.");
    }

    public static async Task VetJsonFileAsync(
        string rootPath,
        string jsonPath,
        string schemaPath,
        string expression,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveCueExecutable(rootPath),
            WorkingDirectory = rootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add("vet");
        startInfo.ArgumentList.Add(jsonPath);
        startInfo.ArgumentList.Add(schemaPath);
        startInfo.ArgumentList.Add("-d");
        startInfo.ArgumentList.Add(expression);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        _ = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"cue vet failed for '{jsonPath}': {standardError.Trim()}");
        }
    }

    private static async Task<string> ExportExpressionAsync(string rootPath, string cuePath, string expression, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveCueExecutable(rootPath),
            WorkingDirectory = rootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add("export");
        startInfo.ArgumentList.Add(cuePath);
        startInfo.ArgumentList.Add("-e");
        startInfo.ArgumentList.Add(expression);
        startInfo.ArgumentList.Add("--out");
        startInfo.ArgumentList.Add("json");

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"cue export failed for '{cuePath}': {standardError.Trim()}");
        }

        return standardOutput;
    }

    private static string ResolveCueExecutable(string rootPath)
    {
        var envOverride = Environment.GetEnvironmentVariable("SPEC_TRACE_CUE_BIN");
        if (!string.IsNullOrWhiteSpace(envOverride) && File.Exists(envOverride))
        {
            return envOverride;
        }

        var repoLocal = Path.Combine(rootPath, ".tools", "cue", "bin", OperatingSystem.IsWindows() ? "cue.exe" : "cue");
        if (File.Exists(repoLocal))
        {
            return repoLocal;
        }

        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var goBin = Path.Combine(homeDirectory, "go", "bin", OperatingSystem.IsWindows() ? "cue.exe" : "cue");
        if (File.Exists(goBin))
        {
            return goBin;
        }

        return "cue";
    }
}
