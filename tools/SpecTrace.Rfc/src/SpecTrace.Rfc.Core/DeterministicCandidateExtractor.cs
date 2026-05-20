using System.Text.RegularExpressions;

namespace SpecTrace.Rfc.Core;

public static class DeterministicCandidateExtractor
{
    private static readonly Regex StructureStart = new(@"^(?<name>[A-Za-z][A-Za-z0-9 /_-]*?)\s*\{\s*$", RegexOptions.Compiled);
    private static readonly Regex FieldLine = new(
        @"^(?<name>[A-Za-z][A-Za-z0-9 _/-]*?)\s*\((?<length>[^)]+)\)(?:\s*=\s*(?<value>[^,]+))?(?:\s*\.\.\.)?$",
        RegexOptions.Compiled);
    private static readonly Regex NormativeKeyword = new(@"\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b", RegexOptions.Compiled);

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
            var requirement = BuildRequirement(structureName, fieldName, length, fixedValue, previousFieldName);
            if (requirement is not null)
            {
                requirements.Add(requirement);
            }

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
        var hasNormativeKeyword = NormativeKeyword.IsMatch(sourceUnit.Text);
        if (IsNotationOnlySection(sourceUnit) && !hasNormativeKeyword)
        {
            return false;
        }

        if (string.Equals(sourceUnit.BlockKind, "figure", StringComparison.Ordinal) ||
            string.Equals(sourceUnit.BlockKind, "table", StringComparison.Ordinal))
        {
            return true;
        }

        return hasNormativeKeyword;
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

    private static CandidateRequirement? BuildRequirement(
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
                return Requirement(
                    title: "Header Form Bit",
                    statement: $"The first bit of a {structureDisplayName} MUST be set to {fixedValue}.");
            }

            return Requirement(
                title: $"{fieldName} fixed value",
                statement: $"The {fieldName} field in a {structureDisplayName} MUST be set to {fixedValue}.");
        }

        if (fieldName.Contains("Version-Specific Data", StringComparison.OrdinalIgnoreCase))
        {
            return Requirement(
                title: "Version-Specific Remainder",
                statement: $"The remainder of a {structureDisplayName} MUST contain version-specific content.");
        }

        if (fieldName.Contains("Version-Specific", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(length, "7", StringComparison.Ordinal))
            {
                return Requirement(
                    title: fieldName,
                    statement: $"The other seven bits in the first byte of a {structureDisplayName} MUST be version-specific.");
            }

            return Requirement(
                title: fieldName,
                statement: $"The {fieldName} field in a {structureDisplayName} MUST be version-specific.");
        }

        if (TryDescribeRange(length, out var rangeDescription))
        {
            if (fieldName.EndsWith("Connection ID", StringComparison.OrdinalIgnoreCase))
            {
                return Requirement(
                    title: $"{fieldName} Size",
                    statement: $"The {fieldName} field MUST follow its length byte and be {rangeDescription}.");
            }

            return Requirement(
                title: $"{fieldName} Size",
                statement: $"The {fieldName} field in a {structureDisplayName} MUST be {rangeDescription}.");
        }

        if (int.TryParse(length, out var bitLength))
        {
            if (string.Equals(fieldName, "Version", StringComparison.OrdinalIgnoreCase) &&
                bitLength == 32)
            {
                return Requirement(
                    title: "Version Field",
                    statement: $"The four bytes after the first byte in a {structureDisplayName} MUST contain a 32-bit Version field.");
            }

            if (fieldName.EndsWith(" Length", StringComparison.OrdinalIgnoreCase) &&
                bitLength == 8 &&
                !string.IsNullOrWhiteSpace(previousFieldName))
            {
                var encodedFieldName = fieldName[..^" Length".Length];
                return Requirement(
                    title: $"{fieldName} Encoding",
                    statement: $"The byte after the {previousFieldName} field MUST encode the {encodedFieldName} length as an 8-bit unsigned integer.");
            }

            return Requirement(
                title: fieldName,
                statement: $"A {structureDisplayName} MUST contain {ArticleFor($"{bitLength}-bit")} {bitLength}-bit {fieldName} field.");
        }

        return Requirement(
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

    private static bool IsNotationOnlySection(SourceUnit sourceUnit)
    {
        return string.Equals(sourceUnit.SectionTitle, "Notational Conventions", StringComparison.OrdinalIgnoreCase);
    }
}
