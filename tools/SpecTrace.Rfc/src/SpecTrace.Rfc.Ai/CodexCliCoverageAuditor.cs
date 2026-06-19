using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpecTrace.Rfc.Core;

namespace SpecTrace.Rfc.Ai;

public sealed class CodexCliCoverageAuditor
{
    private sealed class BatchCounter(int value)
    {
        public int Value { get; private set; } = value;

        public int Next()
        {
            Value++;
            return Value;
        }
    }

    private sealed class AuditInputRecord
    {
        [JsonPropertyName("source_unit_id")]
        public required string SourceUnitId { get; init; }

        [JsonPropertyName("source_unit")]
        public required SourceUnit SourceUnit { get; init; }

        [JsonPropertyName("candidate_decision")]
        public CandidateDecision? CandidateDecision { get; init; }
    }

    public async Task<int> AuditAsync(CodexAuditOptions options, CancellationToken cancellationToken = default)
    {
        if (options.BatchSize <= 0)
        {
            throw new InvalidOperationException("--batch-size must be greater than zero.");
        }

        if (options.MinBatchSize <= 0)
        {
            throw new InvalidOperationException("--adaptive-min-batch-size must be greater than zero.");
        }

        if (options.MaxBatchRetries < 0)
        {
            throw new InvalidOperationException("--max-batch-retries cannot be negative.");
        }

        if (options.BatchTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("--batch-timeout-seconds must be greater than zero.");
        }

        ValidateAiMode(options.AiMode);

        var ledger = await Jsonl.ReadAsync<SourceUnit>(options.LedgerPath, cancellationToken);
        var candidates = await Jsonl.ReadAsync<CandidateDecision>(options.CandidatePath, cancellationToken);
        EnsureUniqueSourceUnitIds(ledger);
        EnsureUniqueCandidateSourceUnitIds(candidates);

        var candidateLookup = candidates.ToDictionary(candidate => candidate.SourceUnitId, StringComparer.Ordinal);
        var promptTemplate = await File.ReadAllTextAsync(options.PromptPath, cancellationToken);
        var decisionsBySourceUnitId = new Dictionary<string, ReviewDecision>(StringComparer.Ordinal);
        var batchOutputDirectory = GetBatchOutputDirectory(options);
        var batchCounter = new BatchCounter(FindLastBatchNumber(options.RawOutputDirectory, batchOutputDirectory));

        if (!string.IsNullOrWhiteSpace(options.ReportPath))
        {
            await CoverageAuditRenderer.RenderAsync(options.ReportPath!, ledger, candidateLookup, cancellationToken);
        }

        if (options.Resume)
        {
            await LoadResumeDecisionsAsync(options.OutputPath, batchOutputDirectory, ledger, decisionsBySourceUnitId, cancellationToken);
            Console.WriteLine($"Loaded {decisionsBySourceUnitId.Count} existing review decision(s) from {Path.GetFullPath(options.OutputPath)}");
            await PersistCompletedDecisionsAsync(options.OutputPath, ledger, decisionsBySourceUnitId, cancellationToken);
        }

        var batches = BuildSectionBatches(ledger, options.BatchSize);
        foreach (var batch in batches)
        {
            var pending = batch
                .Where(sourceUnit => !decisionsBySourceUnitId.ContainsKey(sourceUnit.SourceUnitId))
                .ToList();

            if (pending.Count == 0)
            {
                continue;
            }

            if (string.Equals(options.AiMode, "off", StringComparison.OrdinalIgnoreCase))
            {
                var reviewDecisions = pending
                    .Select(unit => BuildDeterministicDecision(unit, candidateLookup.TryGetValue(unit.SourceUnitId, out var candidate) ? candidate : null))
                    .ToList();

                await CompleteBatchAsync(
                    options,
                    batchOutputDirectory,
                    batchCounter.Next(),
                    options.ReasoningEffort,
                    pending,
                    reviewDecisions,
                    ledger,
                    decisionsBySourceUnitId,
                    cancellationToken);
                continue;
            }

            await ExtractAdaptiveBatchAsync(
                options,
                promptTemplate,
                batchOutputDirectory,
                batchCounter,
                pending,
                ledger,
                candidateLookup,
                decisionsBySourceUnitId,
                cancellationToken);
        }

        var allResults = ledger
            .Select(sourceUnit => decisionsBySourceUnitId.TryGetValue(sourceUnit.SourceUnitId, out var decision)
                ? decision
                : BuildDeterministicDecision(sourceUnit, candidateLookup.TryGetValue(sourceUnit.SourceUnitId, out var candidate) ? candidate : null))
            .ToList();

        await Jsonl.WriteAsync(options.OutputPath, allResults, cancellationToken);
        return allResults.Count;
    }

    private static IReadOnlyList<IReadOnlyList<SourceUnit>> BuildSectionBatches(IReadOnlyList<SourceUnit> ledger, int batchSize)
    {
        var batches = new List<IReadOnlyList<SourceUnit>>();
        var current = new List<SourceUnit>();
        var currentSection = string.Empty;

        foreach (var unit in ledger)
        {
            if (current.Count == 0)
            {
                currentSection = unit.Section;
                current.Add(unit);
                continue;
            }

            if (!string.Equals(unit.Section, currentSection, StringComparison.Ordinal) || current.Count >= batchSize)
            {
                batches.Add(current);
                current = new List<SourceUnit>();
                currentSection = unit.Section;
            }

            current.Add(unit);
        }

        if (current.Count > 0)
        {
            batches.Add(current);
        }

        return batches;
    }

    private static ReviewDecision BuildDeterministicDecision(SourceUnit sourceUnit, CandidateDecision? candidate)
    {
        if (candidate is null)
        {
            return new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "gap",
                Notes = ["no_candidate_decision"],
            };
        }

        var requirements = candidate.Requirements.Count == 0
            ? []
            : candidate.Requirements;

        return candidate.Decision switch
        {
            "emit" when requirements.Count == 0 => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "gap",
                Notes = candidate.ReviewFlags.Count == 0 ? ["candidate_emitted_without_requirements"] : candidate.ReviewFlags.ToList(),
            },
            "emit" when requirements.Count == 1 => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "accept",
                Requirements = requirements,
                Notes = candidate.ReviewFlags.Count == 0 ? [] : candidate.ReviewFlags.ToList(),
            },
            "emit" => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "split",
                Requirements = requirements,
                Notes = candidate.ReviewFlags.Count == 0 ? ["candidate_requires_split"] : candidate.ReviewFlags.ToList(),
            },
            "skip_non_normative" or "skip_duplicate" => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "skip",
                Notes = candidate.ReviewFlags.Count == 0 ? [candidate.Decision] : candidate.ReviewFlags.ToList(),
            },
            "merge_with_previous" => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = requirements.Count == 0 ? "gap" : "merge",
                Requirements = requirements,
                Notes = candidate.ReviewFlags.Count == 0 ? [candidate.Decision] : candidate.ReviewFlags.ToList(),
            },
            "split_required" => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = requirements.Count == 0 ? "gap" : "split",
                Requirements = requirements,
                Notes = candidate.ReviewFlags.Count == 0 ? [candidate.Decision] : candidate.ReviewFlags.ToList(),
            },
            "needs_human_review" or "gap" => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "gap",
                Notes = candidate.ReviewFlags.Count == 0 ? [candidate.Decision] : candidate.ReviewFlags.ToList(),
            },
            _ => new ReviewDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnitIds = [sourceUnit.SourceUnitId],
                Action = "quarantine",
                Notes = candidate.ReviewFlags.Count == 0 ? [candidate.Decision] : candidate.ReviewFlags.ToList(),
            },
        };
    }

    private static async Task LoadResumeDecisionsAsync(
        string outputPath,
        string batchOutputDirectory,
        IReadOnlyList<SourceUnit> ledger,
        Dictionary<string, ReviewDecision> decisionsBySourceUnitId,
        CancellationToken cancellationToken)
    {
        var ledgerSourceUnitIds = ledger
            .Select(sourceUnit => sourceUnit.SourceUnitId)
            .ToHashSet(StringComparer.Ordinal);

        if (File.Exists(outputPath))
        {
            foreach (var decision in await Jsonl.ReadAsync<ReviewDecision>(outputPath, cancellationToken))
            {
                AddResumeDecision(decisionsBySourceUnitId, decision, ledgerSourceUnitIds, outputPath);
            }
        }

        if (!Directory.Exists(batchOutputDirectory))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFiles(batchOutputDirectory, "batch-*.reviews.json").OrderBy(path => path, StringComparer.Ordinal))
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var artifact = JsonSerializer.Deserialize<ReviewBatchArtifact>(json, RfcJson.Options)
                ?? throw new InvalidOperationException($"Batch artifact '{path}' deserialized to null.");

            ValidateBatchArtifact(artifact, path);
            foreach (var decision in artifact.Results)
            {
                AddResumeDecision(decisionsBySourceUnitId, decision, ledgerSourceUnitIds, path);
            }
        }
    }

    private static void ValidateBatchArtifact(ReviewBatchArtifact artifact, string path)
    {
        if (artifact.SourceUnitIds.Count != artifact.Results.Count)
        {
            throw new InvalidOperationException($"Batch artifact '{path}' has {artifact.SourceUnitIds.Count} source_unit_ids but {artifact.Results.Count} result(s).");
        }

        for (var index = 0; index < artifact.Results.Count; index++)
        {
            var decision = artifact.Results[index];
            CandidateRules.ValidateReviewDecision(decision);
            if (!string.Equals(artifact.SourceUnitIds[index], decision.SourceUnitId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Batch artifact '{path}' result {index + 1} does not match its source_unit_ids entry.");
            }
        }
    }

    private static void AddResumeDecision(
        Dictionary<string, ReviewDecision> decisionsBySourceUnitId,
        ReviewDecision decision,
        IReadOnlySet<string> ledgerSourceUnitIds,
        string sourcePath)
    {
        if (!ledgerSourceUnitIds.Contains(decision.SourceUnitId))
        {
            return;
        }

        CandidateRules.ValidateReviewDecision(decision);
        if (!decisionsBySourceUnitId.TryGetValue(decision.SourceUnitId, out var existing))
        {
            decisionsBySourceUnitId[decision.SourceUnitId] = decision;
            return;
        }

        var existingJson = JsonSerializer.Serialize(existing, RfcJson.JsonlOptions);
        var newJson = JsonSerializer.Serialize(decision, RfcJson.JsonlOptions);
        if (!string.Equals(existingJson, newJson, StringComparison.Ordinal))
        {
            return;
        }
    }

    private static async Task PersistCompletedDecisionsAsync(
        string outputPath,
        IReadOnlyList<SourceUnit> ledger,
        IReadOnlyDictionary<string, ReviewDecision> decisionsBySourceUnitId,
        CancellationToken cancellationToken)
    {
        var completed = ledger
            .Select(sourceUnit => decisionsBySourceUnitId.TryGetValue(sourceUnit.SourceUnitId, out var decision) ? decision : null)
            .OfType<ReviewDecision>()
            .ToList();

        await Jsonl.WriteAsync(outputPath, completed, cancellationToken);
    }

    private static async Task ExtractAdaptiveBatchAsync(
        CodexAuditOptions options,
        string promptTemplate,
        string batchOutputDirectory,
        BatchCounter batchCounter,
        IReadOnlyList<SourceUnit> batch,
        IReadOnlyList<SourceUnit> ledger,
        IReadOnlyDictionary<string, CandidateDecision> candidateLookup,
        Dictionary<string, ReviewDecision> decisionsBySourceUnitId,
        CancellationToken cancellationToken)
    {
        var pending = batch
            .Where(sourceUnit => !decisionsBySourceUnitId.ContainsKey(sourceUnit.SourceUnitId))
            .ToList();

        if (pending.Count == 0)
        {
            return;
        }

        var batchNumber = batchCounter.Next();
        try
        {
            var orderedResults = await ExtractBatchOnceAsync(
                options,
                promptTemplate,
                batchNumber,
                pending,
                candidateLookup,
                options.ReasoningEffort,
                previousFailure: null,
                cancellationToken);

            await CompleteBatchAsync(
                options,
                batchOutputDirectory,
                batchNumber,
                options.ReasoningEffort,
                pending,
                orderedResults,
                ledger,
                decisionsBySourceUnitId,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException && pending.Count > Math.Max(1, options.MinBatchSize))
        {
            Console.WriteLine($"Coverage audit batch {batchNumber} failed; splitting {pending.Count} source unit(s). Failure: {exception.Message}");
            foreach (var split in SplitBatchForRetry(pending))
            {
                await ExtractAdaptiveBatchAsync(
                    options,
                    promptTemplate,
                    batchOutputDirectory,
                    batchCounter,
                    split,
                    ledger,
                    candidateLookup,
                    decisionsBySourceUnitId,
                    cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException &&
                                          options.MaxBatchRetries > 0 &&
                                          !string.IsNullOrWhiteSpace(options.RetryReasoningEffort) &&
                                          !string.Equals(options.RetryReasoningEffort, options.ReasoningEffort, StringComparison.OrdinalIgnoreCase))
        {
            var retryBatchNumber = batchCounter.Next();
            var retryResults = await ExtractBatchOnceAsync(
                options,
                promptTemplate,
                retryBatchNumber,
                pending,
                candidateLookup,
                options.RetryReasoningEffort!,
                exception.Message,
                cancellationToken);

            await CompleteBatchAsync(
                options,
                batchOutputDirectory,
                retryBatchNumber,
                options.RetryReasoningEffort!,
                pending,
                retryResults,
                ledger,
                decisionsBySourceUnitId,
                cancellationToken);
        }
    }

    internal static IReadOnlyList<IReadOnlyList<SourceUnit>> SplitBatchForRetry(IReadOnlyList<SourceUnit> batch)
    {
        if (batch.Count <= 1)
        {
            return [batch];
        }

        var midpoint = Math.Max(1, batch.Count / 2);
        return
        [
            batch.Take(midpoint).ToList(),
            batch.Skip(midpoint).ToList(),
        ];
    }

    private static async Task CompleteBatchAsync(
        CodexAuditOptions options,
        string batchOutputDirectory,
        int batchNumber,
        string reasoningEffort,
        IReadOnlyList<SourceUnit> batch,
        IReadOnlyList<ReviewDecision> orderedResults,
        IReadOnlyList<SourceUnit> ledger,
        Dictionary<string, ReviewDecision> decisionsBySourceUnitId,
        CancellationToken cancellationToken)
    {
        foreach (var result in orderedResults)
        {
            decisionsBySourceUnitId[result.SourceUnitId] = result;
        }

        await WriteBatchArtifactAsync(options, batchOutputDirectory, batchNumber, reasoningEffort, batch, orderedResults, cancellationToken);
        await PersistCompletedDecisionsAsync(options.OutputPath, ledger, decisionsBySourceUnitId, cancellationToken);
        Console.WriteLine(
            $"Completed coverage audit batch {batchNumber}; wrote {decisionsBySourceUnitId.Count}/{ledger.Count} review decision(s) to {Path.GetFullPath(options.OutputPath)}");
    }

    private static async Task<List<ReviewDecision>> ExtractBatchOnceAsync(
        CodexAuditOptions options,
        string promptTemplate,
        int batchNumber,
        IReadOnlyList<SourceUnit> batch,
        IReadOnlyDictionary<string, CandidateDecision> candidateLookup,
        string reasoningEffort,
        string? previousFailure,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(promptTemplate, batchNumber, batch, candidateLookup, previousFailure);
        try
        {
            var rawResponse = await InvokeCodexAsync(options, prompt, reasoningEffort, cancellationToken);
            await WriteRawArtifactsAsync(options, batchNumber, reasoningEffort, prompt, rawResponse, cancellationToken);

            var batchResponse = DeserializeBatch(rawResponse, batchNumber);
            return OrderAndValidateBatch(batch, batchResponse.Results, batchNumber);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await WriteRawFailureAsync(options, batchNumber, reasoningEffort, prompt, exception, cancellationToken);
            throw;
        }
    }

    private static string BuildPrompt(
        string promptTemplate,
        int batchNumber,
        IReadOnlyList<SourceUnit> batch,
        IReadOnlyDictionary<string, CandidateDecision> candidateLookup,
        string? previousFailure = null)
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
        builder.AppendLine("Audit input records:");
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(
            batch.Select(sourceUnit => new AuditInputRecord
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                SourceUnit = sourceUnit,
                CandidateDecision = candidateLookup.TryGetValue(sourceUnit.SourceUnitId, out var candidate) ? candidate : null,
            }).ToList(),
            RfcJson.Options));
        builder.AppendLine("```");
        return builder.ToString();
    }

    private static async Task WriteRawArtifactsAsync(
        CodexAuditOptions options,
        int batchNumber,
        string reasoningEffort,
        string prompt,
        string rawResponse,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.RawOutputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(options.RawOutputDirectory);
        var suffix = $".{reasoningEffort}";
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.prompt.md"), prompt, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.response.json"), rawResponse, cancellationToken);
    }

    private static async Task WriteRawFailureAsync(
        CodexAuditOptions options,
        int batchNumber,
        string reasoningEffort,
        string prompt,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.RawOutputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(options.RawOutputDirectory);
        var suffix = $".{reasoningEffort}";
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.prompt.md"), prompt, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(options.RawOutputDirectory, $"batch-{batchNumber:0000}{suffix}.failure.txt"), exception.ToString(), cancellationToken);
    }

    private static async Task WriteBatchArtifactAsync(
        CodexAuditOptions options,
        string batchOutputDirectory,
        int batchNumber,
        string reasoningEffort,
        IReadOnlyList<SourceUnit> batch,
        IReadOnlyList<ReviewDecision> orderedResults,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(batchOutputDirectory);
        var artifact = new ReviewBatchArtifact
        {
            BatchNumber = batchNumber,
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
            Model = options.Model,
            ReasoningEffort = reasoningEffort,
            SourceUnitIds = batch.Select(sourceUnit => sourceUnit.SourceUnitId).ToList(),
            Results = orderedResults.ToList(),
        };

        var path = Path.Combine(batchOutputDirectory, $"batch-{batchNumber:0000}.reviews.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(artifact, RfcJson.Options), cancellationToken);
    }

    private static string GetBatchOutputDirectory(CodexAuditOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.BatchOutputDirectory))
        {
            return Path.GetFullPath(options.BatchOutputDirectory);
        }

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))
            ?? Directory.GetCurrentDirectory();
        return Path.Combine(outputDirectory, "review-batches");
    }

    private static int FindLastBatchNumber(string? rawOutputDirectory, string batchOutputDirectory)
    {
        var max = 0;
        if (!string.IsNullOrWhiteSpace(rawOutputDirectory) && Directory.Exists(rawOutputDirectory))
        {
            max = Math.Max(max, FindLastBatchNumberInDirectory(rawOutputDirectory, "batch-*.*"));
        }

        if (Directory.Exists(batchOutputDirectory))
        {
            max = Math.Max(max, FindLastBatchNumberInDirectory(batchOutputDirectory, "batch-*.reviews.json"));
        }

        return max;
    }

    private static int FindLastBatchNumberInDirectory(string directory, string searchPattern)
    {
        var max = 0;
        foreach (var path in Directory.EnumerateFiles(directory, searchPattern))
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

    private static ReviewBatchResponse DeserializeBatch(string rawResponse, int batchNumber)
    {
        var json = ExtractJsonObject(rawResponse);
        try
        {
            return JsonSerializer.Deserialize<ReviewBatchResponse>(json, RfcJson.Options)
                ?? throw new JsonException("Response deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Coverage audit batch {batchNumber} returned invalid review JSON: {exception.Message}", exception);
        }
    }

    internal static List<ReviewDecision> OrderAndValidateBatch(IReadOnlyList<SourceUnit> batch, IReadOnlyList<ReviewDecision> results, int batchNumber)
    {
        var ordered = new List<ReviewDecision>(batch.Count);

        for (var index = 0; index < batch.Count; index++)
        {
            if (index >= results.Count)
            {
                throw new InvalidOperationException($"Coverage audit batch {batchNumber} did not return a result for source unit '{batch[index].SourceUnitId}'.");
            }

            var unit = batch[index];
            var decision = NormalizeDecisionForExpectedUnit(results[index], unit);
            CandidateRules.ValidateReviewDecision(decision);
            ValidateCodexUpstreamRefs(decision, batchNumber);
            ordered.Add(decision);
        }

        if (results.Count > batch.Count)
        {
            var extra = results
                .Skip(batch.Count)
                .Select(result => result.SourceUnitId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            if (extra.Count > 0)
            {
                throw new InvalidOperationException($"Coverage audit batch {batchNumber} returned unexpected source unit ids: {string.Join(", ", extra)}.");
            }
        }

        return ordered;
    }

    private static void ValidateCodexUpstreamRefs(ReviewDecision decision, int batchNumber)
    {
        var sourceIds = new HashSet<string>(decision.SourceUnitIds.Count == 0
            ? [decision.SourceUnitId]
            : [decision.SourceUnitId, .. decision.SourceUnitIds], StringComparer.Ordinal);

        foreach (var requirement in decision.Requirements)
        {
            if (requirement.UpstreamRefs.Count == 0)
            {
                continue;
            }

            if (!requirement.UpstreamRefs.Any(reference => sourceIds.Any(sourceId => reference.Contains(sourceId, StringComparison.Ordinal))))
            {
                throw new InvalidOperationException(
                    $"Coverage audit batch {batchNumber} returned a requirement for '{decision.SourceUnitId}' whose upstream_refs do not contain a reviewed source unit id.");
            }
        }
    }

    private static ReviewDecision NormalizeDecisionForExpectedUnit(ReviewDecision decision, SourceUnit expectedUnit)
    {
        var normalizedSourceUnitIds = NormalizeSourceUnitIds(decision.SourceUnitIds, decision.SourceUnitId, expectedUnit.SourceUnitId);
        var repairNeeded = !string.Equals(decision.SourceUnitId, expectedUnit.SourceUnitId, StringComparison.Ordinal);
        return new ReviewDecision
        {
            SourceUnitId = expectedUnit.SourceUnitId,
            SourceUnitIds = normalizedSourceUnitIds,
            Action = decision.Action,
            Requirements = decision.Requirements
                .Select(requirement => NormalizeRequirementForExpectedUnit(requirement, decision.SourceUnitId, expectedUnit.SourceUnitId))
                .ToList(),
            Notes = repairNeeded
                ? decision.Notes
                    .Select(note => note.Replace(decision.SourceUnitId, expectedUnit.SourceUnitId, StringComparison.Ordinal))
                    .Append($"source_unit_id_repaired_from:{decision.SourceUnitId}")
                    .ToList()
                : decision.Notes.ToList(),
        };
    }

    private static CandidateRequirement NormalizeRequirementForExpectedUnit(
        CandidateRequirement requirement,
        string actualSourceUnitId,
        string expectedSourceUnitId)
    {
        var upstreamRefs = requirement.UpstreamRefs
            .Select(reference => reference.Replace(actualSourceUnitId, expectedSourceUnitId, StringComparison.Ordinal))
            .ToList();

        return new CandidateRequirement
        {
            ProposedIdHint = requirement.ProposedIdHint,
            Title = requirement.Title,
            Statement = requirement.Statement,
            Coverage = requirement.Coverage,
            UpstreamRefs = upstreamRefs,
            Notes = requirement.Notes,
        };
    }

    private static List<string> NormalizeSourceUnitIds(IEnumerable<string> sourceUnitIds, string actualSourceUnitId, string expectedSourceUnitId)
    {
        var normalized = new List<string>();
        foreach (var sourceUnitId in sourceUnitIds)
        {
            var repaired = string.Equals(sourceUnitId, actualSourceUnitId, StringComparison.Ordinal)
                ? expectedSourceUnitId
                : sourceUnitId;

            if (!string.IsNullOrWhiteSpace(repaired) && !normalized.Contains(repaired, StringComparer.Ordinal))
            {
                normalized.Add(repaired);
            }
        }

        if (normalized.Count == 0)
        {
            normalized.Add(expectedSourceUnitId);
            return normalized;
        }

        var expectedIndex = normalized.FindIndex(sourceUnitId => string.Equals(sourceUnitId, expectedSourceUnitId, StringComparison.Ordinal));
        if (expectedIndex > 0)
        {
            normalized.RemoveAt(expectedIndex);
            normalized.Insert(0, expectedSourceUnitId);
            return normalized;
        }

        if (expectedIndex < 0)
        {
            normalized.Insert(0, expectedSourceUnitId);
        }

        return normalized;
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

    private static async Task<string> InvokeCodexAsync(
        CodexAuditOptions options,
        string prompt,
        string reasoningEffort,
        CancellationToken cancellationToken)
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
            startInfo.ArgumentList.Add($"model_reasoning_effort={JsonSerializer.Serialize(reasoningEffort)}");
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

    private static void ValidateAiMode(string aiMode)
    {
        if (!string.Equals(aiMode, "codex", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(aiMode, "off", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("--ai-mode must be one of: codex, off.");
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

    private static void EnsureUniqueCandidateSourceUnitIds(IReadOnlyList<CandidateDecision> candidates)
    {
        var duplicate = candidates
            .GroupBy(candidate => candidate.SourceUnitId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Candidate set contains duplicate source_unit_id '{duplicate.Key}'. Re-run extract with the current tooling.");
        }
    }
}
