using System.Text.RegularExpressions;

namespace SpecTrace.Rfc.Core;

public static class DeterministicCandidateExtractor
{
    private static readonly Regex StructureStart = new(@"^(?<name>[A-Za-z][A-Za-z0-9 /_-]*?)\s*\{\s*$", RegexOptions.Compiled);
    private static readonly Regex FieldLine = new(
        @"^(?<name>[A-Za-z][A-Za-z0-9 _/-]*?)\s*\((?<length>[^)]+)\)(?:\s*=\s*(?<value>[^,]+))?(?:\s*\.\.\.)?$",
        RegexOptions.Compiled);
    private static readonly Regex NormativeKeyword = new(@"\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b", RegexOptions.Compiled);
    private static readonly Regex BehavioralCue = new(
        @"\b(?:packet|header|frame|field|length|size|byte|bit|connection id|datagram|stream|state|transition|algorithm|encoding|layout|negotiation|error|limit|timer|reserved|ignore|packet number|flow control|congestion|path|token|handshake|transport parameter|retry|acknowledg(?:e|ement|ed|ment)?|retransmission)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CaptionOnly = new(@"^(?:Figure|Table)\s+\d+\s*(?::.*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static CandidateDecision? TryExtract(SourceUnit sourceUnit)
    {
        if (!string.Equals(sourceUnit.BlockKind, "figure", StringComparison.Ordinal))
        {
            return null;
        }

        var lines = sourceUnit.Text
            .Split('\n')
            .Select(line => line.Trim().TrimEnd(','))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        if (lines.Count < 3)
        {
            return null;
        }

        var structureMatch = StructureStart.Match(lines[0]);
        if (!structureMatch.Success)
        {
            return null;
        }

        var structureName = structureMatch.Groups["name"].Value.Trim();
        if (structureName.Contains("Example", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var requirements = new List<CandidateRequirement>();
        string? previousFieldName = null;
        foreach (var line in lines.Skip(1))
        {
            if (line is "}" or "};")
            {
                continue;
            }

            var fieldMatch = FieldLine.Match(line);
            if (!fieldMatch.Success)
            {
                return null;
            }

            var fieldName = fieldMatch.Groups["name"].Value.Trim();
            var length = fieldMatch.Groups["length"].Value.Trim();
            var fixedValue = fieldMatch.Groups["value"].Success
                ? fieldMatch.Groups["value"].Value.Trim()
                : null;
            requirements.AddRange(BuildRequirements(structureName, fieldName, length, fixedValue, previousFieldName));

            previousFieldName = fieldName;
        }

        return requirements.Count == 0
            ? null
            : new CandidateDecision
            {
                SourceUnitId = sourceUnit.SourceUnitId,
                Decision = "emit",
                Requirements = requirements,
                ReviewFlags = ["deterministic_figure_extraction"],
            };
    }

    public static bool ShouldSendToAi(SourceUnit sourceUnit)
    {
        if (IsDocumentBoilerplate(sourceUnit))
        {
            return false;
        }

        var hasNormativeKeyword = NormativeKeyword.IsMatch(sourceUnit.Text);
        if (string.Equals(sourceUnit.BlockKind, "figure", StringComparison.Ordinal) ||
            string.Equals(sourceUnit.BlockKind, "table", StringComparison.Ordinal))
        {
            return true;
        }

        return hasNormativeKeyword || IsReviewableSourceUnit(sourceUnit);
    }

    public static bool ShouldSendToCandidateUnits(SourceUnit sourceUnit)
    {
        if (IsDocumentBoilerplate(sourceUnit))
        {
            return false;
        }

        if (string.Equals(sourceUnit.BlockKind, "figure", StringComparison.Ordinal) ||
            string.Equals(sourceUnit.BlockKind, "table", StringComparison.Ordinal))
        {
            return true;
        }

        if (NormativeKeyword.IsMatch(sourceUnit.Text))
        {
            return true;
        }

        return IsReviewableSourceUnit(sourceUnit) && HasBehavioralCue(sourceUnit);
    }

    public static bool HasNormativeKeywordOrStructuredBlock(SourceUnit sourceUnit)
    {
        if (IsDocumentBoilerplate(sourceUnit))
        {
            return false;
        }

        if (string.Equals(sourceUnit.BlockKind, "figure", StringComparison.Ordinal) ||
            string.Equals(sourceUnit.BlockKind, "table", StringComparison.Ordinal))
        {
            return true;
        }

        return NormativeKeyword.IsMatch(sourceUnit.Text);
    }

    public static CandidateDecision Skip(SourceUnit sourceUnit)
    {
        return new CandidateDecision
        {
            SourceUnitId = sourceUnit.SourceUnitId,
            Decision = "skip_non_normative",
            Requirements = [],
            ReviewFlags = ["deterministic_scope_skip"],
        };
    }

    private static IEnumerable<CandidateRequirement> BuildRequirements(
        string structureName,
        string fieldName,
        string length,
        string? fixedValue,
        string? previousFieldName)
    {
        var structureDisplayName = BuildStructureDisplayName(structureName);
        if (!string.IsNullOrWhiteSpace(fixedValue))
        {
            if (string.Equals(fieldName, "Header Form", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(length, "1", StringComparison.Ordinal) &&
                (fixedValue is "0" or "1"))
            {
                yield return Requirement(
                    title: "Header Form Bit",
                    statement: $"The first bit of a {structureDisplayName} MUST be set to {fixedValue}.");
                yield break;
            }

            yield return Requirement(
                title: $"{fieldName} fixed value",
                statement: $"The {fieldName} field in a {structureDisplayName} MUST be set to {fixedValue}.");
            yield break;
        }

        if (fieldName.Contains("Version-Specific Data", StringComparison.OrdinalIgnoreCase))
        {
            yield return Requirement(
                title: "Version-Specific Remainder",
                statement: $"The remainder of a {structureDisplayName} MUST contain version-specific content.");
            yield break;
        }

        if (fieldName.Contains("Version-Specific", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(length, "7", StringComparison.Ordinal))
            {
                yield return Requirement(
                    title: fieldName,
                    statement: $"The other seven bits in the first byte of a {structureDisplayName} MUST be version-specific.");
                yield break;
            }

            yield return Requirement(
                title: fieldName,
                statement: $"The {fieldName} field in a {structureDisplayName} MUST be version-specific.");
            yield break;
        }

        if (TryDescribeRange(length, out var rangeDescription))
        {
            if (fieldName.EndsWith("Connection ID", StringComparison.OrdinalIgnoreCase))
            {
                yield return Requirement(
                    title: $"{fieldName} Position",
                    statement: $"The {fieldName} field MUST follow its length byte.");
                yield return Requirement(
                    title: $"{fieldName} Size",
                    statement: $"The {fieldName} field MUST be {rangeDescription}.");
                yield break;
            }

            yield return Requirement(
                title: $"{fieldName} Size",
                statement: $"The {fieldName} field in a {structureDisplayName} MUST be {rangeDescription}.");
            yield break;
        }

        if (int.TryParse(length, out var bitLength))
        {
            if (string.Equals(fieldName, "Version", StringComparison.OrdinalIgnoreCase) &&
                bitLength == 32)
            {
                yield return Requirement(
                    title: "Version Field",
                    statement: $"The four bytes after the first byte in a {structureDisplayName} MUST contain a 32-bit Version field.");
                yield break;
            }

            if (fieldName.EndsWith(" Length", StringComparison.OrdinalIgnoreCase) &&
                bitLength == 8 &&
                !string.IsNullOrWhiteSpace(previousFieldName))
            {
                var encodedFieldName = fieldName[..^" Length".Length];
                yield return Requirement(
                    title: $"{fieldName} Encoding",
                    statement: $"The byte after the {previousFieldName} field MUST encode the {encodedFieldName} length as an 8-bit unsigned integer.");
                yield break;
            }

            yield return Requirement(
                title: fieldName,
                statement: $"A {structureDisplayName} MUST contain {ArticleFor($"{bitLength}-bit")} {bitLength}-bit {fieldName} field.");
            yield break;
        }

        yield return Requirement(
            title: fieldName,
            statement: $"The {fieldName} field in a {structureDisplayName} MUST be present.");
    }

    private static CandidateRequirement Requirement(string title, string statement)
    {
        return new CandidateRequirement
        {
            Title = title,
            Statement = statement,
            Coverage = new RequirementCoverage
            {
                Positive = "required",
                Negative = "required",
                Edge = "required",
                Fuzz = "deferred",
            },
        };
    }

    private static bool TryDescribeRange(string length, out string description)
    {
        description = string.Empty;
        var parts = length.Split("..", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!TryParseOptionalBound(parts[0], defaultValue: 0, out var lower))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(parts[1]))
        {
            description = $"at least {DescribeBitCount(lower)} long";
            return true;
        }

        if (!int.TryParse(parts[1], out var upper))
        {
            return false;
        }

        description = lower == 0 && upper % 8 == 0
            ? $"between 0 and {DescribeBitCount(upper)} long"
            : $"between {DescribeBitCount(lower)} and {DescribeBitCount(upper)} long";
        return true;
    }

    private static bool TryParseOptionalBound(string value, int defaultValue, out int result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = defaultValue;
            return true;
        }

        return int.TryParse(value, out result);
    }

    private static string DescribeBitCount(int bits)
    {
        return bits % 8 == 0
            ? $"{bits / 8} bytes"
            : $"{bits} bits";
    }

    private static string BuildStructureDisplayName(string structureName)
    {
        return structureName switch
        {
            "Long Header Packet" => "QUIC long header packet",
            "Short Header Packet" => "QUIC short header packet",
            "Version Negotiation Packet" => "QUIC version negotiation packet",
            _ => structureName,
        };
    }

    private static string ArticleFor(string phrase)
    {
        if (phrase.StartsWith('8'))
        {
            return "an";
        }

        return phrase.Length > 0 && "AEFHILMNORSXaefhilmnorsx".Contains(phrase[0], StringComparison.Ordinal)
            ? "an"
            : "a";
    }

    private static bool IsDocumentBoilerplate(SourceUnit sourceUnit)
    {
        var text = sourceUnit.Text.Trim();
        if (string.Equals(sourceUnit.Section, "0", StringComparison.Ordinal) &&
            IsFrontMatterBoilerplateText(text))
        {
            return true;
        }

        if (IsBoilerplateSectionTitle(sourceUnit.SectionTitle))
        {
            return true;
        }

        return CaptionOnly.IsMatch(text);
    }

    private static bool IsReviewableSourceUnit(SourceUnit sourceUnit)
    {
        var text = sourceUnit.Text.Trim();
        if (text.Length < 12)
        {
            return false;
        }

        return sourceUnit.BlockKind is "paragraph" or "list_item" or "figure" or "table";
    }

    private static bool HasBehavioralCue(SourceUnit sourceUnit)
    {
        return BehavioralCue.IsMatch(sourceUnit.Text);
    }

    private static bool IsBoilerplateSectionTitle(string sectionTitle)
    {
        return sectionTitle.Contains("References", StringComparison.OrdinalIgnoreCase) ||
               sectionTitle.Equals("Acknowledgments", StringComparison.OrdinalIgnoreCase) ||
               sectionTitle.Equals("Acknowledgements", StringComparison.OrdinalIgnoreCase) ||
               sectionTitle.Equals("Contributors", StringComparison.OrdinalIgnoreCase) ||
               sectionTitle.Equals("Authors' Addresses", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFrontMatterBoilerplateText(string text)
    {
        if (text.Length < 12)
        {
            return true;
        }

        return text.Equals("Abstract", StringComparison.OrdinalIgnoreCase) ||
               text.Equals("Status of This Memo", StringComparison.OrdinalIgnoreCase) ||
               text.Equals("Copyright Notice", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Internet Engineering Task Force", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Request for Comments:", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("ISSN:", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Copyright (c)", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("All rights reserved", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("This document is a product of the Internet Engineering Task Force", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("It represents the consensus of the IETF community", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("It has received public review", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Further information on Internet Standards", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Information about the current status of this document", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("This document is subject to BCP 78", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Please review these documents carefully", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("Code Components extracted from this document", StringComparison.OrdinalIgnoreCase);
    }
}
