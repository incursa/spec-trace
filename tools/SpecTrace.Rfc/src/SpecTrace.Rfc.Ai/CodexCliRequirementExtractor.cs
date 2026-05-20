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

        if (options.MaxBatchRetries < 0)
        {
            throw new InvalidOperationException("--max-batch-retries cannot be negative.");
        }

        if (options.BatchTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("--batch-timeout-seconds must be greater than zero.");
        }

        var ledger = await Jsonl.ReadAsync<SourceUnit>(options.LedgerPath, cancellationToken);
        var promptTemplate = await File.ReadAllTextAsync(options.PromptPath, cancellationToken);
        var allResults = new List<CandidateDecision>();
        var batchNumber = 0;

        foreach (var batch in ledger.Chunk(options.BatchSize))
        {
            batchNumber++;
            var orderedResults = await ExtractBatchWithRetriesAsync(options, promptTemplate, batchNumber, batch, cancellationToken);
            allResults.AddRange(orderedResults);
        }

        await Jsonl.WriteAsync(options.OutputPath, allResults, cancellationToken);
        return allResults.Count;
    }

    private static async Task<List<CandidateDecision>> ExtractBatchWithRetriesAsync(
        CodexExtractionOptions options,
        string promptTemplate,
        int batchNumber,
        IReadOnlyList<SourceUnit> batch,
        CancellationToken cancellationToken)
    {
        var maxAttempts = options.MaxBatchRetries + 1;
        Exception? lastFailure = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var prompt = BuildPrompt(promptTemplate, batchNumber, batch, lastFailure?.Message);
            string rawResponse;
            try
            {
                rawResponse = await InvokeCodexAsync(options, prompt, cancellationToken);
                await WriteRawArtifactsAsync(options, batchNumber, attempt, prompt, rawResponse, cancellationToken);

                var batchResponse = DeserializeBatch(rawResponse, batchNumber);
                return OrderAndValidateBatch(batch, batchResponse.Results, batchNumber);
            }
            catch (Exception exception) when (attempt < maxAttempts && exception is not OperationCanceledException)
            {
                lastFailure = exception;
                await WriteRawFailureAsync(options, batchNumber, attempt, prompt, exception, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Codex batch {batchNumber} failed after {maxAttempts} attempt(s): {lastFailure?.Message}",
            lastFailure);
    }

    private static string BuildPrompt(string promptTemplate, int batchNumber, IReadOnlyList<SourceUnit> batch, string? previousFailure = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine(promptTemplate.TrimEnd());
        builder.AppendLine();
        builder.AppendLine($"Batch: {batchNumber}");
        builder.AppendLine();
        builder.AppendLine("Return one result for each source_unit_id in this input, in the same order.");
        if (!string.IsNullOrWhiteSpace(previousFailure))
        {
            builder.AppendLine();
            builder.AppendLine("The previous attempt for this same batch failed validation.");
            builder.AppendLine("Correct the response without dropping, renaming, reordering, or inventing source_unit_id values.");
            builder.AppendLine($"Validation failure: {previousFailure}");
        }

        builder.AppendLine();
        builder.AppendLine("Input source units:");
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(batch, RfcJson.Options));
        builder.AppendLine("```");
        return builder.ToString();
    }

    private static async Task WriteRawArtifactsAsync(
        CodexExtractionOptions options,
        int batchNumber,
        int attempt,
        string prompt,
        string rawResponse,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.RawOutputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(options.RawOutputDirectory);
        var suffix = attempt == 1 ? string.Empty : $".attempt-{attempt:00}";
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.prompt.md"), prompt, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.response.json"), rawResponse, cancellationToken);
    }

    private static async Task WriteRawFailureAsync(
        CodexExtractionOptions options,
        int batchNumber,
        int attempt,
        string prompt,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.RawOutputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(options.RawOutputDirectory);
        var suffix = attempt == 1 ? string.Empty : $".attempt-{attempt:00}";
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.prompt.md"), prompt, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.failure.txt"), exception.ToString(), cancellationToken);
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

    internal static List<CandidateDecision> OrderAndValidateBatch(IReadOnlyList<SourceUnit> batch, IReadOnlyList<CandidateDecision> results, int batchNumber)
    {
        if (results.Count == batch.Count)
        {
            var orderedByPosition = new List<CandidateDecision>(batch.Count);
            for (var index = 0; index < batch.Count; index++)
            {
                var decision = NormalizeDecisionForExpectedUnit(results[index], batch[index]);
                CandidateRules.ValidateDecision(decision);
                orderedByPosition.Add(decision);
            }

            return orderedByPosition;
        }

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

    private static CandidateDecision NormalizeDecisionForExpectedUnit(CandidateDecision decision, SourceUnit expectedUnit)
    {
        if (string.Equals(decision.SourceUnitId, expectedUnit.SourceUnitId, StringComparison.Ordinal))
        {
            return decision;
        }

        return new CandidateDecision
        {
            SourceUnitId = expectedUnit.SourceUnitId,
            Decision = decision.Decision,
            Requirements = decision.Requirements
                .Select(requirement => NormalizeRequirementForExpectedUnit(requirement, decision.SourceUnitId, expectedUnit.SourceUnitId))
                .ToList(),
            ReviewFlags = decision.ReviewFlags
                .Append($"source_unit_id_repaired_from:{decision.SourceUnitId}")
                .ToList(),
        };
    }

    private static CandidateRequirement NormalizeRequirementForExpectedUnit(
        CandidateRequirement requirement,
        string actualSourceUnitId,
        string expectedSourceUnitId)
    {
        return new CandidateRequirement
        {
            ProposedIdHint = requirement.ProposedIdHint,
            Title = requirement.Title,
            Statement = requirement.Statement,
            Coverage = requirement.Coverage,
            UpstreamRefs = requirement.UpstreamRefs
                .Select(reference => reference.Replace(actualSourceUnitId, expectedSourceUnitId, StringComparison.Ordinal))
                .ToList(),
            Notes = requirement.Notes,
        };
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
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(options.BatchTimeoutSeconds));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                throw new TimeoutException($"Codex CLI timed out after {options.BatchTimeoutSeconds} second(s).");
            }

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

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}
