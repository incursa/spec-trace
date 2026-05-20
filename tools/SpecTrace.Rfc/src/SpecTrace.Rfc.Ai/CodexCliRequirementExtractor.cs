using System.Diagnostics;
using System.Text;
using System.Text.Json;
using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Ai;

public sealed class CodexCliRequirementExtractor
{
    public async Task<int> ExtractAsync(CodexExtractionOptions options, CancellationToken cancellationToken = default)
    {
        if (options.BatchSize <= 0)
        {
            throw new InvalidOperationException("--batch-size must be greater than zero.");
        }

        var ledger = await Jsonl.ReadAsync<SourceUnit>(options.LedgerPath, cancellationToken);
        var promptTemplate = await File.ReadAllTextAsync(options.PromptPath, cancellationToken);
        var allResults = new List<CandidateDecision>();
        var batchNumber = 0;

        foreach (var batch in ledger.Chunk(options.BatchSize))
        {
            batchNumber++;
            var prompt = BuildPrompt(promptTemplate, batchNumber, batch);
            var rawResponse = await InvokeCodexAsync(options, prompt, cancellationToken);

            if (!string.IsNullOrWhiteSpace(options.RawOutputDirectory))
            {
                Directory.CreateDirectory(options.RawOutputDirectory);
                await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}.prompt.md"), prompt, cancellationToken);
                await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}.response.json"), rawResponse, cancellationToken);
            }

            var batchResponse = DeserializeBatch(rawResponse, batchNumber);
            var orderedResults = OrderAndValidateBatch(batch, batchResponse.Results, batchNumber);
            allResults.AddRange(orderedResults);
        }

        await Jsonl.WriteAsync(options.OutputPath, allResults, cancellationToken);
        return allResults.Count;
    }

    private static string BuildPrompt(string promptTemplate, int batchNumber, IReadOnlyList<SourceUnit> batch)
    {
        var builder = new StringBuilder();
        builder.AppendLine(promptTemplate.TrimEnd());
        builder.AppendLine();
        builder.AppendLine($"Batch: {batchNumber}");
        builder.AppendLine();
        builder.AppendLine("Return one result for each source_unit_id in this input, in the same order.");
        builder.AppendLine();
        builder.AppendLine("Input source units:");
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(batch, RfcJson.Options));
        builder.AppendLine("```");
        return builder.ToString();
    }

    private static CandidateBatchResponse DeserializeBatch(string rawResponse, int batchNumber)
    {
        var json = ExtractJsonObject(rawResponse);
        try
        {
            return JsonSerializer.Deserialize<CandidateBatchResponse>(json, RfcJson.Options)
                ?? throw new JsonException("Response deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Codex batch {batchNumber} returned invalid candidate JSON: {exception.Message}", exception);
        }
    }

    private static List<CandidateDecision> OrderAndValidateBatch(IReadOnlyList<SourceUnit> batch, IReadOnlyList<CandidateDecision> results, int batchNumber)
    {
        var byId = results.ToDictionary(result => result.SourceUnitId, StringComparer.Ordinal);
        var ordered = new List<CandidateDecision>(batch.Count);

        foreach (var unit in batch)
        {
            if (!byId.TryGetValue(unit.SourceUnitId, out var decision))
            {
                throw new InvalidOperationException($"Codex batch {batchNumber} did not return a result for source unit '{unit.SourceUnitId}'.");
            }

            CandidateRules.ValidateDecision(decision);
            ordered.Add(decision);
        }

        if (byId.Count != batch.Count)
        {
            var expected = batch.Select(unit => unit.SourceUnitId).ToHashSet(StringComparer.Ordinal);
            var extra = byId.Keys.Where(id => !expected.Contains(id)).OrderBy(id => id, StringComparer.Ordinal).ToList();
            if (extra.Count > 0)
            {
                throw new InvalidOperationException($"Codex batch {batchNumber} returned unexpected source unit ids: {string.Join(", ", extra)}.");
            }
        }

        return ordered;
    }

    private static string ExtractJsonObject(string rawResponse)
    {
        var trimmed = rawResponse.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd >= 0)
            {
                trimmed = trimmed[(firstLineEnd + 1)..].Trim();
            }

            if (trimmed.EndsWith("```", StringComparison.Ordinal))
            {
                trimmed = trimmed[..^3].Trim();
            }
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end < start)
        {
            throw new InvalidOperationException("Codex response did not contain a JSON object.");
        }

        return trimmed[start..(end + 1)];
    }

    private static async Task<string> InvokeCodexAsync(CodexExtractionOptions options, string prompt, CancellationToken cancellationToken)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"spec-trace-rfc-codex-{Guid.NewGuid():N}.json");
        try
        {
            var resolvedCommand = CodexCommandResolver.Resolve(options.CodexCommand);
            var startInfo = new ProcessStartInfo
            {
                FileName = resolvedCommand.FileName,
                WorkingDirectory = Path.GetFullPath(options.WorkingDirectory),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            foreach (var argument in resolvedCommand.PrefixArguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            startInfo.ArgumentList.Add("exec");
            startInfo.ArgumentList.Add("--ephemeral");
            startInfo.ArgumentList.Add("--skip-git-repo-check");
            startInfo.ArgumentList.Add("--color");
            startInfo.ArgumentList.Add("never");
            startInfo.ArgumentList.Add("-m");
            startInfo.ArgumentList.Add(options.Model);
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add($"model_reasoning_effort={JsonSerializer.Serialize(options.ReasoningEffort)}");
            startInfo.ArgumentList.Add("-s");
            startInfo.ArgumentList.Add("read-only");
            startInfo.ArgumentList.Add("--output-schema");
            startInfo.ArgumentList.Add(Path.GetFullPath(options.SchemaPath));
            startInfo.ArgumentList.Add("-o");
            startInfo.ArgumentList.Add(outputPath);
            startInfo.ArgumentList.Add("-");

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start Codex CLI.");

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.StandardInput.WriteAsync(prompt.AsMemory(), cancellationToken);
            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Codex CLI exited with code {process.ExitCode}.{Environment.NewLine}{stderr}{Environment.NewLine}{stdout}");
            }

            if (File.Exists(outputPath))
            {
                return await File.ReadAllTextAsync(outputPath, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                return stdout;
            }

            throw new InvalidOperationException("Codex CLI completed without writing a response.");
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}
