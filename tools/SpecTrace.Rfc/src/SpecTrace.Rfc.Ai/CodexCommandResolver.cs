namespace SpecTrace.Rfc.Ai;

internal static class CodexCommandResolver
{
    private static readonly string[] WindowsExecutableExtensions = [".ps1", ".cmd", ".bat", ".exe", string.Empty];

    public static ResolvedCommand Resolve(string command)
    {
        if (Path.IsPathRooted(command) || command.Contains(Path.DirectorySeparatorChar) || command.Contains(Path.AltDirectorySeparatorChar))
        {
            return Wrap(command);
        }

        if (!OperatingSystem.IsWindows() || !string.IsNullOrWhiteSpace(Path.GetExtension(command)))
        {
            return Wrap(command);
        }

        var candidates = EnumerateWindowsCandidates(command).ToList();
        var powerShellShim = candidates.FirstOrDefault(path => path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(powerShellShim))
        {
            return Wrap(powerShellShim);
        }

        var commandShim = candidates.FirstOrDefault(path => path.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(commandShim))
        {
            return Wrap(commandShim);
        }

        var executable = candidates.FirstOrDefault(path => path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(executable))
        {
            return Wrap(executable);
        }

        return Wrap(candidates.FirstOrDefault() ?? command);
    }

    private static IEnumerable<string> EnumerateWindowsCandidates(string command)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var extension in WindowsExecutableExtensions)
            {
                var candidate = Path.Combine(directory.Trim('"'), command + extension);
                if (File.Exists(candidate))
                {
                    yield return candidate;
                }
            }
        }
    }

    private static ResolvedCommand Wrap(string command)
    {
        if (!OperatingSystem.IsWindows())
        {
            return new ResolvedCommand(command, []);
        }

        if (command.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedCommand(FindPowerShell(), ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", command]);
        }

        if (command.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ||
            command.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedCommand(Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe", ["/d", "/c", command]);
        }

        return new ResolvedCommand(command, []);
    }

    private static string FindPowerShell()
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory.Trim('"'), "pwsh.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return "powershell.exe";
    }
}

internal sealed record ResolvedCommand(string FileName, IReadOnlyList<string> PrefixArguments);
