using System.Text.RegularExpressions;

namespace SpecTrace.Rfc.Core;

public static class RfcSegmenter
{
    private static readonly Regex SectionHeading = new(
        @"^\s*(?<section>(?:\d+|[A-Z])(?:\.\d+)*)\.?\s+(?<title>[A-Z0-9][^\r\n]+?)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex PageMarker = new(@"\[Page\s+\d+\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TableOfContentsMarker = new(@"^\s*Table of Contents\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ListMarker = new(@"^\s*(?:[-*+]|\d+\.)\s+", RegexOptions.Compiled);
    private static readonly Regex TableMarker = new(@"^\s*(?:\+[-+]+\+|\|.+\||[-=]{3,})\s*$", RegexOptions.Compiled);
    private static readonly Regex FigureMarker = new(@"(?:[{}]|\b0x[0-9a-fA-F]+\b|=\s*[A-Za-z0-9]|\.\.)", RegexOptions.Compiled);
    private static readonly Regex SentenceBoundary = new(@"(?<=[.!?])\s+(?=[""'\(\[]?[A-Z0-9])", RegexOptions.Compiled);
    private static readonly HashSet<string> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        "e.g.",
        "i.e.",
        "etc.",
        "vs.",
        "no.",
        "fig.",
    };

    public static List<SourceUnit> Segment(RfcSourceDocument source)
    {
        var units = new List<SourceUnit>();
        var lines = NormalizeLines(source.Content);
        var blockLines = new List<string>();
        var section = "0";
        var sectionTitle = "Front Matter";
        var blockIndex = 0;
        var paragraphIndex = 0;
        var countersBySection = new Dictionary<string, (int BlockIndex, int ParagraphIndex)>(StringComparer.Ordinal);
        var inTableOfContents = false;

        foreach (var line in lines)
        {
            if (ShouldSkipLine(line))
            {
                continue;
            }

            if (TableOfContentsMarker.IsMatch(line))
            {
                FlushBlock(source, units, blockLines, section, sectionTitle, ref blockIndex, ref paragraphIndex);
                blockLines.Clear();
                StoreSectionCounters(countersBySection, section, blockIndex, paragraphIndex);
                inTableOfContents = true;
                continue;
            }

            if (inTableOfContents)
            {
                if (string.IsNullOrWhiteSpace(line) || char.IsWhiteSpace(line[0]))
                {
                    continue;
                }

                inTableOfContents = false;
            }

            var heading = TryParseSectionHeading(line);
            if (heading is not null)
            {
                FlushBlock(source, units, blockLines, section, sectionTitle, ref blockIndex, ref paragraphIndex);
                blockLines.Clear();
                StoreSectionCounters(countersBySection, section, blockIndex, paragraphIndex);
                section = heading.Value.Section;
                sectionTitle = heading.Value.Title;
                var counters = GetSectionCounters(countersBySection, section);
                blockIndex = counters.BlockIndex;
                paragraphIndex = counters.ParagraphIndex;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                FlushBlock(source, units, blockLines, section, sectionTitle, ref blockIndex, ref paragraphIndex);
                blockLines.Clear();
                continue;
            }

            blockLines.Add(line.TrimEnd());
        }

        FlushBlock(source, units, blockLines, section, sectionTitle, ref blockIndex, ref paragraphIndex);
        return units;
    }

    public static string SectionKey(string section)
    {
        var key = Regex.Replace(section.ToUpperInvariant(), "[^A-Z0-9]+", "P").Trim('P');
        return string.IsNullOrWhiteSpace(key) ? "S0" : $"S{key}";
    }

    private static void StoreSectionCounters(
        Dictionary<string, (int BlockIndex, int ParagraphIndex)> countersBySection,
        string section,
        int blockIndex,
        int paragraphIndex)
    {
        var sectionKey = SectionKey(section);
        if (!countersBySection.TryGetValue(sectionKey, out var existing) ||
            blockIndex > existing.BlockIndex ||
            paragraphIndex > existing.ParagraphIndex)
        {
            countersBySection[sectionKey] = (blockIndex, paragraphIndex);
        }
    }

    private static (int BlockIndex, int ParagraphIndex) GetSectionCounters(
        IReadOnlyDictionary<string, (int BlockIndex, int ParagraphIndex)> countersBySection,
        string section)
    {
        return countersBySection.TryGetValue(SectionKey(section), out var counters)
            ? counters
            : (0, 0);
    }

    private static IEnumerable<string> NormalizeLines(string content)
    {
        return content.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.Replace("\f", string.Empty, StringComparison.Ordinal));
    }

    private static bool ShouldSkipLine(string line)
    {
        return PageMarker.IsMatch(line) || string.Equals(line.Trim(), "\f", StringComparison.Ordinal);
    }

    private static (string Section, string Title)? TryParseSectionHeading(string line)
    {
        if (line.Length - line.TrimStart().Length > 3)
        {
            return null;
        }

        var match = SectionHeading.Match(line);
        if (!match.Success)
        {
            return null;
        }

        var section = match.Groups["section"].Value.Trim().TrimEnd('.');
        var title = match.Groups["title"].Value.Trim();
        if (title.EndsWith(".", StringComparison.Ordinal))
        {
            title = title[..^1].TrimEnd();
        }

        return (section, title);
    }

    private static void FlushBlock(
        RfcSourceDocument source,
        List<SourceUnit> units,
        List<string> blockLines,
        string section,
        string sectionTitle,
        ref int blockIndex,
        ref int paragraphIndex)
    {
        if (blockLines.Count == 0)
        {
            return;
        }

        var blockKind = DetermineBlockKind(blockLines);
        var text = NormalizeBlockText(blockLines, blockKind);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        blockIndex++;
        paragraphIndex++;

        var sourceTexts = blockKind is "paragraph" or "list_item"
            ? SplitSentences(text)
            : [text];

        var sentenceIndex = 0;
        foreach (var sourceText in sourceTexts)
        {
            var unitText = sourceText.Trim();
            if (unitText.Length == 0)
            {
                continue;
            }

            sentenceIndex++;
            var sourceUnitId = $"{source.SourceId}-{SectionKey(section)}-B{blockIndex}-P{paragraphIndex}-S{sentenceIndex}";
            units.Add(new SourceUnit
            {
                SourceUnitId = sourceUnitId,
                SourceId = source.SourceId,
                Section = section,
                SectionTitle = sectionTitle,
                BlockIndex = blockIndex,
                ParagraphIndex = paragraphIndex,
                SentenceIndex = sentenceIndex,
                BlockKind = blockKind,
                Text = unitText,
                SourceUrl = BuildSourceUrl(source, section),
                TextHash = Hashing.Sha256Text(unitText),
            });
        }
    }

    private static string DetermineBlockKind(IReadOnlyList<string> lines)
    {
        if (lines.Any(line => TableMarker.IsMatch(line)))
        {
            return "table";
        }

        var text = string.Join('\n', lines.Select(line => line.TrimEnd()));
        if (lines.All(line => line.StartsWith("   ", StringComparison.Ordinal) || line.StartsWith("\t", StringComparison.Ordinal)) &&
            FigureMarker.IsMatch(text) &&
            !LooksLikeIndentedProse(text))
        {
            return "figure";
        }

        if (lines.Any(line => ListMarker.IsMatch(line)))
        {
            return "list_item";
        }

        return "paragraph";
    }

    private static bool LooksLikeIndentedProse(string text)
    {
        var normalized = Regex.Replace(text, @"\s+", " ").Trim();
        if (normalized.Length == 0)
        {
            return false;
        }

        return Regex.IsMatch(normalized, @"\b(?:the|that|when|which|endpoint|application|transport|connection)\b", RegexOptions.IgnoreCase) &&
               Regex.IsMatch(normalized, @"[.!?](?:\s|$)");
    }

    private static string NormalizeBlockText(IReadOnlyList<string> lines, string blockKind)
    {
        return blockKind is "figure" or "table"
            ? string.Join('\n', lines.Select(line => line.TrimEnd())).Trim()
            : Regex.Replace(string.Join(' ', lines.Select(line => line.Trim())), @"\s+", " ").Trim();
    }

    private static List<string> SplitSentences(string text)
    {
        var results = new List<string>();
        var start = 0;

        foreach (Match match in SentenceBoundary.Matches(text))
        {
            var candidate = text[start..match.Index].Trim();
            if (ShouldSuppressBoundary(candidate))
            {
                continue;
            }

            if (candidate.Length > 0)
            {
                results.Add(candidate);
            }

            start = match.Index + match.Length;
        }

        var tail = text[start..].Trim();
        if (tail.Length > 0)
        {
            results.Add(tail);
        }

        return results.Count == 0 ? [text] : results;
    }

    private static bool ShouldSuppressBoundary(string candidate)
    {
        if (candidate.Length == 0)
        {
            return true;
        }

        var lastToken = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (lastToken is not null && Abbreviations.Contains(lastToken))
        {
            return true;
        }

        return Regex.IsMatch(candidate, @"\b\d+(?:\.\d+)+\.$");
    }

    private static string? BuildSourceUrl(RfcSourceDocument source, string section)
    {
        var baseUrl = source.CanonicalUrl ?? source.SourceUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        if (string.Equals(section, "0", StringComparison.Ordinal))
        {
            return baseUrl;
        }

        var anchor = char.IsLetter(section[0])
            ? $"appendix-{section}"
            : $"section-{section}";
        return $"{baseUrl.Split('#')[0]}#{anchor}";
    }
}
