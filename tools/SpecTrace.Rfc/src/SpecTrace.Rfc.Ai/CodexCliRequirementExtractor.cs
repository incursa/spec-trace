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

        ValidateExtractionScope(options.ExtractionScope);
        ValidateAiMode(options.AiMode);
        ValidateDeterministicExtractionMode(options.DeterministicExtractionMode);

        var ledger = await Jsonl.ReadAsync<SourceUnit>(options.LedgerPath, cancellationToken);
        EnsureUniqueSourceUnitIds(ledger);
        var ledgerSourceUnitIds = ledger
            .Select(sourceUnit => sourceUnit.SourceUnitId)
            .ToHashSet(StringComparer.Ordinal);
        var promptTemplate = await File.ReadAllTextAsync(options.PromptPath, cancellationToken);
        var decisionsBySourceUnitId = new Dictionary<string, CandidateDecision>(StringComparer.Ordinal);
        var aiUnits = new List<SourceUnit>();
        var batchNumber = FindLastRawBatchNumber(options.RawOutputDirectory);

        if (options.Resume && File.Exists(options.OutputPath))
        {
            foreach (var decision in await Jsonl.ReadAsync<CandidateDecision>(options.OutputPath, cancellationToken))
            {
                if (!ledgerSourceUnitIds.Contains(decision.SourceUnitId))
                {
                    continue;
                }

                CandidateRules.ValidateDecision(decision);
                decisionsBySourceUnitId[decision.SourceUnitId] = decision;
            }

            Console.WriteLine($"Loaded {decisionsBySourceUnitId.Count} existing candidate decision(s) from {Path.GetFullPath(options.OutputPath)}");
        }

        foreach (var sourceUnit in ledger)
        {
            if (decisionsBySourceUnitId.ContainsKey(sourceUnit.SourceUnitId))
            {
                continue;
            }

            if (ShouldExtractDeterministically(options, sourceUnit))
            {
                var deterministicDecision = DeterministicCandidateExtractor.TryExtract(sourceUnit);
                if (deterministicDecision is not null)
                {
                    CandidateRules.ValidateDecision(deterministicDecision);
                    decisionsBySourceUnitId[sourceUnit.SourceUnitId] = deterministicDecision;
                    continue;
                }
            }

            if (ShouldSendToAi(options, sourceUnit))
            {
                aiUnits.Add(sourceUnit);
                continue;
            }

            decisionsBySourceUnitId[sourceUnit.SourceUnitId] = DeterministicCandidateExtractor.Skip(sourceUnit);
        }

        if (string.Equals(options.AiMode, "off", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var sourceUnit in aiUnits)
            {
                decisionsBySourceUnitId[sourceUnit.SourceUnitId] = NeedsAiReview(sourceUnit);
            }
        }
        else
        {
            foreach (var batch in aiUnits.Chunk(options.BatchSize))
            {
                batchNumber++;
                var orderedResults = await ExtractBatchWithRetriesAsync(options, promptTemplate, batchNumber, batch, cancellationToken);
                foreach (var result in orderedResults)
                {
                    decisionsBySourceUnitId[result.SourceUnitId] = result;
                }

                await PersistCompletedDecisionsAsync(options.OutputPath, ledger, decisionsBySourceUnitId, cancellationToken);
                Console.WriteLine(
                    $"Completed Codex batch {batchNumber}; wrote {decisionsBySourceUnitId.Count}/{ledger.Count} candidate decision(s) to {Path.GetFullPath(options.OutputPath)}");
            }
        }

        var allResults = ledger
            .Select(sourceUnit => decisionsBySourceUnitId.TryGetValue(sourceUnit.SourceUnitId, out var decision)
                ? decision
                : DeterministicCandidateExtractor.Skip(sourceUnit))
            .ToList();

        await Jsonl.WriteAsync(options.OutputPath, allResults, cancellationToken);
        return allResults.Count;
    }

    private static async Task PersistCompletedDecisionsAsync(
        string outputPath,
        IReadOnlyList<SourceUnit> ledger,
        IReadOnlyDictionary<string, CandidateDecision> decisionsBySourceUnitId,
        CancellationToken cancellationToken)
    {
        var completed = ledger
            .Select(sourceUnit => decisionsBySourceUnitId.TryGetValue(sourceUnit.SourceUnitId, out var decision) ? decision : null)
            .OfType<CandidateDecision>()
            .ToList();

        await Jsonl.WriteAsync(outputPath, completed, cancellationToken);
    }

    private static void ValidateExtractionScope(string extractionScope)
    {
        if (!string.Equals(extractionScope, "all", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extractionScope, "functional", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extractionScope, "candidate-units", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extractionScope, "normative", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("--extraction-scope must be one of: all, functional, candidate-units, normative.");
        }
    }

    private static void ValidateAiMode(string aiMode)
    {
        if (!string.Equals(aiMode, "codex", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(aiMode, "off", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("--ai-mode must be one of: codex, off.");
        }
    }

    private static void ValidateDeterministicExtractionMode(string deterministicExtractionMode)
    {
        if (!string.Equals(deterministicExtractionMode, "off", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(deterministicExtractionMode, "figures", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("--deterministic-extraction must be one of: off, figures.");
        }
    }

    private static void EnsureUniqueSourceUnitIds(IReadOnlyList<SourceUnit> ledger)
    {
        var duplicate = ledger
            .GroupBy(sourceUnit => sourceUnit.SourceUnitId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Ledger contains duplicate source_unit_id '{duplicate.Key}'. Re-run segment with the current tooling.");
        }
    }

    private static bool ShouldSendToAi(CodexExtractionOptions options, SourceUnit sourceUnit)
    {
        return options.ExtractionScope switch
        {
            var scope when string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase) => true,
            var scope when string.Equals(scope, "functional", StringComparison.OrdinalIgnoreCase) => DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit),
            var scope when string.Equals(scope, "candidate-units", StringComparison.OrdinalIgnoreCase) => DeterministicCandidateExtractor.ShouldSendToAi(sourceUnit),
            var scope when string.Equals(scope, "normative", StringComparison.OrdinalIgnoreCase) => DeterministicCandidateExtractor.HasNormativeKeywordOrStructuredBlock(sourceUnit),
            _ => false,
        };
    }

    private static bool ShouldExtractDeterministically(CodexExtractionOptions options, SourceUnit sourceUnit)
    {
        return string.Equals(options.DeterministicExtractionMode, "figures", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(sourceUnit.BlockKind, "figure", StringComparison.Ordinal);
    }

    private static CandidateDecision NeedsAiReview(SourceUnit sourceUnit)
    {
        return new CandidateDecision
        {
            SourceUnitId = sourceUnit.SourceUnitId,
            Decision = "needs_human_review",
            Requirements = [],
            ReviewFlags = ["ai_disabled_candidate_unit"],
        };
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

    private static int FindLastRawBatchNumber(string? rawOutputDirectory)
    {
        if (string.IsNullOrWhiteSpace(rawOutputDirectory) || !Directory.Exists(rawOutputDirectory))
        {
            return 0;
        }

        var max = 0;
        foreach (var path in Directory.EnumerateFiles(rawOutputDirectory, "batch-*.*"))
        {
            var fileName = Path.GetFileName(path);
            if (fileName.Length < 10 ||
                !fileName.StartsWith("batch-", StringComparison.Ordinal) ||
                !int.TryParse(fileName.AsSpan(6, 4), out var batchNumber))
            {
                continue;
            }

            max = Math.Max(max, batchNumber);
        }

        return max;
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
                ValidateCodexUpstreamRefs(decision, batchNumber);
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
            ValidateCodexUpstreamRefs(decision, batchNumber);
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

    private static void ValidateCodexUpstreamRefs(CandidateDecision decision, int batchNumber)
    {
        foreach (var requirement in decision.Requirements)
        {
            if (requirement.UpstreamRefs.Count == 0)
            {
                continue;
            }

            if (!requirement.UpstreamRefs.Any(reference => reference.Contains(decision.SourceUnitId, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    $"Codex batch {batchNumber} returned a requirement for '{decision.SourceUnitId}' whose upstream_refs do not contain that source unit id.");
            }
        }
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
