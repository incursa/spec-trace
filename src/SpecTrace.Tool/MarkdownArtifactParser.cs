using System.Text.RegularExpressions;

namespace SpecTrace.Tool;

internal static class MarkdownArtifactParser
{
    private static readonly Regex FrontMatterRegex = new(
        @"^(?<full>---\r?\n(?<yaml>.*?)\r?\n---\r?\n?)(?<body>[\s\S]*)$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex H2SectionRegex = new(
        @"(?ms)^##\s+(?<heading>[^\r\n]+)\r?\n(?<content>.*?)(?=^##\s+[^\r\n]+\r?\n|\z)",
        RegexOptions.Compiled);

    private static readonly Regex RequirementHeadingRegex = new(
        @"(?<id>REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,})\s*(?<title>.*)$",
        RegexOptions.Compiled);

    public static ArtifactModel ParseFile(string markdownPath)
    {
        var content = File.ReadAllText(markdownPath);
        var match = FrontMatterRegex.Match(content);
        if (!match.Success)
        {
            throw new InvalidOperationException($"'{markdownPath}' does not contain front matter.");
        }

        var frontMatter = ParseFrontMatter(match.Groups["yaml"].Value);
        var body = match.Groups["body"].Value.Trim();
        var artifactType = GetRequiredString(frontMatter, "artifact_type");

        return artifactType switch
        {
            "specification" => ParseSpecification(frontMatter, body),
            "architecture" => ParseArchitecture(frontMatter, body),
            "work_item" => ParseWorkItem(frontMatter, body),
            "verification" => ParseVerification(frontMatter, body),
            _ => throw new InvalidOperationException($"'{markdownPath}' uses unsupported artifact_type '{artifactType}'."),
        };
    }

    internal static string NormalizeIdentifierText(string value)
    {
        var linkMatch = Regex.Match(value, @"\[(?:`)?(?<id>(?:REQ|SPEC|ARC|WI|VER)-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*(?:-\d{4,})?)(?:`)?\]\([^)]+\)");
        if (linkMatch.Success)
        {
            return linkMatch.Groups["id"].Value;
        }

        var inlineMatch = Regex.Match(value, @"`(?<id>(?:REQ|SPEC|ARC|WI|VER)-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*(?:-\d{4,})?)`");
        if (inlineMatch.Success)
        {
            return inlineMatch.Groups["id"].Value;
        }

        var bareMatch = Regex.Match(value, @"\b(?<id>(?:REQ|SPEC|ARC|WI|VER)-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*(?:-\d{4,})?)\b");
        return bareMatch.Success ? bareMatch.Groups["id"].Value : value.Trim();
    }

    private static ArtifactModel ParseSpecification(Dictionary<string, object> frontMatter, string body)
    {
        var sections = GetSections(body);
        var supplementalSections = new List<SupplementalSection>();
        var requirements = new List<RequirementModel>();
        List<string>? openQuestions = null;

        string? purpose = null;
        string? scope = null;
        string? context = null;

        foreach (var section in sections)
        {
            if (TryParseRequirementHeading(section.Heading, out var requirementId, out var requirementTitle))
            {
                requirements.Add(ParseRequirement(section.Content, requirementId!, requirementTitle!));
                continue;
            }

            switch (section.Heading)
            {
                case "Purpose":
                    purpose = NormalizeText(section.Content);
                    break;
                case "Scope":
                    scope = NormalizeText(section.Content);
                    break;
                case "Context":
                    context = NormalizeText(section.Content);
                    break;
                case "Open Questions":
                    openQuestions = ParseBulletedList(section.Content);
                    break;
                default:
                    supplementalSections.Add(new SupplementalSection
                    {
                        Heading = section.Heading,
                        Content = section.Content.Trim(),
                    });
                    break;
            }
        }

        return new ArtifactModel
        {
            ArtifactId = GetRequiredString(frontMatter, "artifact_id"),
            ArtifactType = "specification",
            Title = GetRequiredString(frontMatter, "title"),
            Domain = GetRequiredString(frontMatter, "domain"),
            Capability = GetRequiredString(frontMatter, "capability"),
            Status = GetRequiredString(frontMatter, "status"),
            Owner = GetRequiredString(frontMatter, "owner"),
            Tags = GetOptionalStringList(frontMatter, "tags"),
            RelatedArtifacts = GetOptionalIdentifierList(frontMatter, "related_artifacts"),
            Purpose = purpose,
            Scope = scope,
            Context = context,
            OpenQuestions = openQuestions,
            SupplementalSections = supplementalSections.Count == 0 ? null : supplementalSections,
            Requirements = requirements,
        };
    }

    private static ArtifactModel ParseArchitecture(Dictionary<string, object> frontMatter, string body)
    {
        var sections = GetSections(body);
        string? purpose = null;
        string? designSummary = null;
        string? dataAndState = null;
        List<string>? keyComponents = null;
        List<string>? edgeCases = null;
        List<string>? alternatives = null;
        List<string>? risks = null;
        List<string>? openQuestions = null;
        var supplementalSections = new List<SupplementalSection>();

        foreach (var section in sections)
        {
            switch (section.Heading)
            {
                case "Purpose":
                    purpose = NormalizeText(section.Content);
                    break;
                case "Requirements Satisfied":
                    break;
                case "Design Summary":
                    designSummary = NormalizeText(section.Content);
                    break;
                case "Key Components":
                    keyComponents = ParseBulletedList(section.Content);
                    break;
                case "Data and State Considerations":
                    dataAndState = NormalizeText(section.Content);
                    break;
                case "Edge Cases and Constraints":
                    edgeCases = ParseBulletedList(section.Content);
                    break;
                case "Alternatives Considered":
                    alternatives = ParseBulletedList(section.Content);
                    break;
                case "Risks":
                    risks = ParseBulletedList(section.Content);
                    break;
                case "Open Questions":
                    openQuestions = ParseBulletedList(section.Content);
                    break;
                default:
                    supplementalSections.Add(new SupplementalSection
                    {
                        Heading = section.Heading,
                        Content = section.Content.Trim(),
                    });
                    break;
            }
        }

        return new ArtifactModel
        {
            ArtifactId = GetRequiredString(frontMatter, "artifact_id"),
            ArtifactType = "architecture",
            Title = GetRequiredString(frontMatter, "title"),
            Domain = GetRequiredString(frontMatter, "domain"),
            Status = GetRequiredString(frontMatter, "status"),
            Owner = GetRequiredString(frontMatter, "owner"),
            Satisfies = GetOptionalIdentifierList(frontMatter, "satisfies"),
            RelatedArtifacts = GetOptionalIdentifierList(frontMatter, "related_artifacts"),
            Purpose = purpose,
            DesignSummary = designSummary,
            KeyComponents = keyComponents,
            DataAndStateConsiderations = dataAndState,
            EdgeCasesAndConstraints = edgeCases,
            AlternativesConsidered = alternatives,
            Risks = risks,
            OpenQuestions = openQuestions,
            SupplementalSections = supplementalSections.Count == 0 ? null : supplementalSections,
        };
    }

    private static ArtifactModel ParseWorkItem(Dictionary<string, object> frontMatter, string body)
    {
        var sections = GetSections(body);
        string? summary = null;
        string? plannedChanges = null;
        string? verificationPlan = null;
        string? completionNotes = null;
        List<string>? outOfScope = null;
        var supplementalSections = new List<SupplementalSection>();

        foreach (var section in sections)
        {
            switch (section.Heading)
            {
                case "Summary":
                    summary = NormalizeText(section.Content);
                    break;
                case "Requirements Addressed":
                case "Design Inputs":
                case "Trace Links":
                    break;
                case "Planned Changes":
                    plannedChanges = NormalizeText(section.Content);
                    break;
                case "Out of Scope":
                    outOfScope = ParseBulletedList(section.Content);
                    break;
                case "Verification Plan":
                    verificationPlan = NormalizeText(section.Content);
                    break;
                case "Completion Notes":
                    completionNotes = NormalizeText(section.Content);
                    break;
                default:
                    supplementalSections.Add(new SupplementalSection
                    {
                        Heading = section.Heading,
                        Content = section.Content.Trim(),
                    });
                    break;
            }
        }

        return new ArtifactModel
        {
            ArtifactId = GetRequiredString(frontMatter, "artifact_id"),
            ArtifactType = "work_item",
            Title = GetRequiredString(frontMatter, "title"),
            Domain = GetRequiredString(frontMatter, "domain"),
            Status = GetRequiredString(frontMatter, "status"),
            Owner = GetRequiredString(frontMatter, "owner"),
            Addresses = GetOptionalIdentifierList(frontMatter, "addresses"),
            DesignLinks = GetOptionalIdentifierList(frontMatter, "design_links"),
            VerificationLinks = GetOptionalIdentifierList(frontMatter, "verification_links"),
            RelatedArtifacts = GetOptionalIdentifierList(frontMatter, "related_artifacts"),
            Summary = summary,
            PlannedChanges = plannedChanges,
            OutOfScope = outOfScope,
            VerificationPlan = verificationPlan,
            CompletionNotes = completionNotes,
            SupplementalSections = supplementalSections.Count == 0 ? null : supplementalSections,
        };
    }

    private static ArtifactModel ParseVerification(Dictionary<string, object> frontMatter, string body)
    {
        var sections = GetSections(body);
        string? scope = null;
        string? verificationMethod = null;
        string? expectedResult = null;
        string? statusSummary = null;
        List<string>? preconditions = null;
        List<string>? procedure = null;
        List<string>? evidence = null;
        var supplementalSections = new List<SupplementalSection>();

        foreach (var section in sections)
        {
            switch (section.Heading)
            {
                case "Scope":
                    scope = NormalizeText(section.Content);
                    break;
                case "Requirements Verified":
                case "Related Artifacts":
                    break;
                case "Verification Method":
                    verificationMethod = NormalizeText(section.Content);
                    break;
                case "Preconditions":
                    preconditions = ParseBulletedList(section.Content);
                    break;
                case "Procedure or Approach":
                    procedure = ParseNumberedList(section.Content);
                    break;
                case "Expected Result":
                    expectedResult = NormalizeText(section.Content);
                    break;
                case "Evidence":
                    evidence = ParseBulletedList(section.Content);
                    break;
                case "Status":
                    statusSummary = NormalizeText(section.Content);
                    break;
                default:
                    supplementalSections.Add(new SupplementalSection
                    {
                        Heading = section.Heading,
                        Content = section.Content.Trim(),
                    });
                    break;
            }
        }

        return new ArtifactModel
        {
            ArtifactId = GetRequiredString(frontMatter, "artifact_id"),
            ArtifactType = "verification",
            Title = GetRequiredString(frontMatter, "title"),
            Domain = GetRequiredString(frontMatter, "domain"),
            Status = GetRequiredString(frontMatter, "status"),
            Owner = GetRequiredString(frontMatter, "owner"),
            Verifies = GetOptionalIdentifierList(frontMatter, "verifies"),
            RelatedArtifacts = GetOptionalIdentifierList(frontMatter, "related_artifacts"),
            Scope = scope,
            VerificationMethod = verificationMethod,
            Preconditions = preconditions,
            Procedure = procedure,
            ExpectedResult = expectedResult,
            Evidence = evidence,
            StatusSummary = statusSummary,
            SupplementalSections = supplementalSections.Count == 0 ? null : supplementalSections,
        };
    }

    private static RequirementModel ParseRequirement(string content, string requirementId, string requirementTitle)
    {
        var traceIndex = content.IndexOf("\nTrace:", StringComparison.Ordinal);
        if (traceIndex < 0 && content.StartsWith("Trace:", StringComparison.Ordinal))
        {
            traceIndex = 0;
        }

        var notesIndex = content.IndexOf("\nNotes:", StringComparison.Ordinal);
        if (notesIndex < 0 && content.StartsWith("Notes:", StringComparison.Ordinal))
        {
            notesIndex = 0;
        }

        var splitIndex = new[] { traceIndex, notesIndex }.Where(index => index >= 0).DefaultIfEmpty(content.Length).Min();
        var statement = content[..splitIndex].Trim();

        RequirementTraceModel? trace = null;
        if (traceIndex >= 0)
        {
            var traceStart = traceIndex == 0 ? "Trace:".Length : traceIndex + "\nTrace:".Length;
            var traceEnd = notesIndex > traceIndex ? notesIndex : content.Length;
            trace = ParseTrace(content[traceStart..traceEnd].Trim());
        }

        List<string>? notes = null;
        if (notesIndex >= 0)
        {
            var notesStart = notesIndex == 0 ? "Notes:".Length : notesIndex + "\nNotes:".Length;
            var notesBody = content[notesStart..].Trim();
            notes = ParseBulletedList(notesBody);
            if (notes.Count == 0 && !string.IsNullOrWhiteSpace(notesBody))
            {
                notes = [NormalizeText(notesBody)];
            }
        }

        return new RequirementModel
        {
            Id = requirementId,
            Title = requirementTitle,
            Statement = NormalizeText(statement),
            Trace = trace,
            Notes = notes,
        };
    }

    private static RequirementTraceModel? ParseTrace(string content)
    {
        var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? currentLabel = null;

        foreach (var rawLine in content.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var labelMatch = Regex.Match(line, @"^\s*-\s*(?<label>[A-Za-z][A-Za-z ]*):\s*$");
            if (labelMatch.Success)
            {
                currentLabel = labelMatch.Groups["label"].Value.Trim();
                if (!map.ContainsKey(currentLabel))
                {
                    map[currentLabel] = [];
                }

                continue;
            }

            var itemMatch = Regex.Match(line, @"^\s*-\s+(?<item>.+?)\s*$");
            if (!itemMatch.Success || currentLabel is null)
            {
                continue;
            }

            var item = itemMatch.Groups["item"].Value.Trim();
            map[currentLabel].Add(string.Equals(currentLabel, "Upstream Refs", StringComparison.OrdinalIgnoreCase)
                ? item
                : NormalizeIdentifierText(item));
        }

        if (map.Count == 0)
        {
            return null;
        }

        return new RequirementTraceModel
        {
            SatisfiedBy = GetOptionalMappedList(map, "Satisfied By"),
            ImplementedBy = GetOptionalMappedList(map, "Implemented By"),
            VerifiedBy = GetOptionalMappedList(map, "Verified By"),
            DerivedFrom = GetOptionalMappedList(map, "Derived From"),
            Supersedes = GetOptionalMappedList(map, "Supersedes"),
            UpstreamRefs = GetOptionalMappedList(map, "Upstream Refs"),
            Related = GetOptionalMappedList(map, "Related"),
        };
    }

    private static List<MarkdownSection> GetSections(string body)
    {
        var normalizedBody = new Regex(@"^#\s+[^\r\n]+\r?\n*", RegexOptions.Singleline).Replace(body.Trim(), string.Empty, 1);
        return H2SectionRegex.Matches(normalizedBody)
            .Select(match => new MarkdownSection(match.Groups["heading"].Value.Trim(), match.Groups["content"].Value.Trim()))
            .ToList();
    }

    private static Dictionary<string, object> ParseFrontMatter(string yaml)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        string? currentListKey = null;

        foreach (var rawLine in yaml.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var keyMatch = Regex.Match(line, @"^(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*(?<value>.*)$");
            if (keyMatch.Success)
            {
                var key = keyMatch.Groups["key"].Value;
                var value = keyMatch.Groups["value"].Value.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    result[key] = new List<string>();
                    currentListKey = key;
                }
                else
                {
                    result[key] = TrimYamlQuotes(value);
                    currentListKey = null;
                }

                continue;
            }

            var listMatch = Regex.Match(line, @"^\s*-\s*(?<item>.+?)\s*$");
            if (listMatch.Success && currentListKey is not null)
            {
                ((List<string>)result[currentListKey]).Add(TrimYamlQuotes(listMatch.Groups["item"].Value.Trim()));
            }
        }

        return result;
    }

    private static string GetRequiredString(Dictionary<string, object> values, string key)
    {
        if (!values.TryGetValue(key, out var value) || value is not string text || string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException($"Missing required front matter key '{key}'.");
        }

        return text.Trim();
    }

    private static List<string>? GetOptionalStringList(Dictionary<string, object> values, string key)
    {
        if (!values.TryGetValue(key, out var value) || value is not List<string> list || list.Count == 0)
        {
            return null;
        }

        var normalized = list.Select(item => item.Trim()).Where(item => item.Length > 0).ToList();
        return normalized.Count == 0 ? null : normalized;
    }

    private static List<string>? GetOptionalIdentifierList(Dictionary<string, object> values, string key)
    {
        var items = GetOptionalStringList(values, key);
        return items?.Select(NormalizeIdentifierText).ToList();
    }

    private static List<string>? GetOptionalMappedList(Dictionary<string, List<string>> values, string key)
    {
        return values.TryGetValue(key, out var list) && list.Count > 0 ? list : null;
    }

    private static bool TryParseRequirementHeading(string heading, out string? requirementId, out string? requirementTitle)
    {
        var normalized = NormalizeRequirementHeading(heading);
        var match = RequirementHeadingRegex.Match(normalized);
        if (!match.Success)
        {
            requirementId = null;
            requirementTitle = null;
            return false;
        }

        requirementId = match.Groups["id"].Value.Trim();
        requirementTitle = match.Groups["title"].Value.Trim();
        return true;
    }

    private static string NormalizeRequirementHeading(string heading)
    {
        var withoutLinks = Regex.Replace(heading, @"\[(?:`)?(?<id>(?:REQ|SPEC|ARC|WI|VER)-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*(?:-\d{4,})?)(?:`)?\]\([^)]+\)", "${id}");
        return Regex.Replace(withoutLinks, @"`(?<id>(?:REQ|SPEC|ARC|WI|VER)-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*(?:-\d{4,})?)`", "${id}");
    }

    private static List<string> ParseBulletedList(string content)
    {
        return ParseStructuredList(content, @"^\s*-\s+(?<item>.+?)\s*$");
    }

    private static List<string> ParseNumberedList(string content)
    {
        return ParseStructuredList(content, @"^\s*\d+\.\s+(?<item>.+?)\s*$");
    }

    private static List<string> ParseStructuredList(string content, string itemPattern)
    {
        var items = new List<string>();
        var current = new List<string>();

        foreach (var rawLine in content.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var itemMatch = Regex.Match(rawLine, itemPattern);
            if (itemMatch.Success)
            {
                FlushCurrentItem(items, current);
                current.Add(itemMatch.Groups["item"].Value.Trim());
                continue;
            }

            if (current.Count == 0)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(rawLine))
            {
                current.Add(string.Empty);
                continue;
            }

            current.Add(rawLine.Trim());
        }

        FlushCurrentItem(items, current);
        return items;
    }

    private static void FlushCurrentItem(List<string> items, List<string> current)
    {
        if (current.Count == 0)
        {
            return;
        }

        items.Add(string.Join('\n', current).Trim());
        current.Clear();
    }

    private static string NormalizeText(string value) => value.Replace("\r\n", "\n").Trim();

    private static string TrimYamlQuotes(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }

        return value;
    }

    private sealed record MarkdownSection(string Heading, string Content);
}
