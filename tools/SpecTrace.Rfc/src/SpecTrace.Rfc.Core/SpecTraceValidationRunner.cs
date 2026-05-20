using System.Diagnostics;

namespace SpecTrace.Rfc.Core;

public static class SpecTraceValidationRunner
{
    public static async Task<int> RunAsync(
        string rootPath,
        string? inputPath,
        string profile,
        CancellationToken cancellationToken = default)
    {
        var resolvedRoot = Path.GetFullPath(rootPath);
        var scriptPath = Path.Combine(resolvedRoot, "scripts", "Test-SpecTraceRepository.ps1");
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"SpecTrace validation script '{scriptPath}' was not found.", scriptPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("-RootPath");
        startInfo.ArgumentList.Add(resolvedRoot);
        startInfo.ArgumentList.Add("-Profile");
        startInfo.ArgumentList.Add(profile);

        if (!string.IsNullOrWhiteSpace(inputPath))
        {
            startInfo.ArgumentList.Add("-InputPath");
            startInfo.ArgumentList.Add(inputPath);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start SpecTrace validation process.");

        var stdoutTask = PipeAsync(process.StandardOutput, Console.Out, cancellationToken);
        var stderrTask = PipeAsync(process.StandardError, Console.Error, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(stdoutTask, stderrTask);

        return process.ExitCode;
    }

    private static async Task PipeAsync(TextReader reader, TextWriter writer, CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }
}
