<#
.SYNOPSIS
Bundles discovered specification documents into one Markdown file.

.DESCRIPTION
Scans a target folder recursively for Markdown files with SpecTrace-style
front matter, keeps only real specification artifacts with concrete
`SPEC-...` identifiers, and writes a single Markdown bundle containing a
summary table plus the grouped requirement content for each specification.

.PARAMETER InputPath
Root folder to scan. Point this at a whole repository or a narrower folder.

.PARAMETER OutputPath
Destination Markdown file.

.PARAMETER IncludeTableOfContents
When set, include a per-specification table of contents with requirement links.

.EXAMPLE
.\scripts\Export-SpecTraceBundle.ps1 `
  -InputPath .\specs\requirements\spec-trace `
  -OutputPath .\specs\generated\spec-bundle.md `
  -IncludeTableOfContents

.EXAMPLE
.\scripts\Export-SpecTraceBundle.ps1 `
  -InputPath C:\src\my-repo `
  -OutputPath C:\temp\requirements-bundle.md
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$InputPath,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [switch]$IncludeTableOfContents
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Remove-YamlQuotes {
    param(
        [AllowNull()]
        [string]$Value
    )

    if ($null -eq $Value) {
        return $null
    }

    $trimmed = $Value.Trim()

    if ($trimmed.Length -ge 2) {
        $first = $trimmed[0]
        $last = $trimmed[$trimmed.Length - 1]

        if (($first -eq '"' -and $last -eq '"') -or ($first -eq "'" -and $last -eq "'")) {
            return $trimmed.Substring(1, $trimmed.Length - 2)
        }
    }

    return $trimmed
}

function Get-FrontMatter {
    param(
        [string]$Content
    )

    $match = [regex]::Match(
        $Content,
        '^(?<full>---\r?\n(?<yaml>.*?)\r?\n---\r?\n?)(?<body>[\s\S]*)$',
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    if (-not $match.Success) {
        return $null
    }

    [pscustomobject]@{
        Raw  = $match.Groups['yaml'].Value
        Body = $match.Groups['body'].Value
    }
}

function ConvertFrom-SimpleFrontMatter {
    param(
        [string]$Yaml
    )

    $result = [ordered]@{}
    $currentListKey = $null

    foreach ($line in ($Yaml -split "\r?\n")) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match '^(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*(?<value>.*)$') {
            $key = $Matches['key']
            $value = $Matches['value']

            if ([string]::IsNullOrWhiteSpace($value)) {
                $result[$key] = @()
                $currentListKey = $key
            }
            else {
                $result[$key] = Remove-YamlQuotes $value
                $currentListKey = $null
            }

            continue
        }

        if ($line -match '^\s*-\s*(?<item>.+?)\s*$' -and $null -ne $currentListKey) {
            $currentValue = $result[$currentListKey]

            if ($currentValue -isnot [System.Collections.IList]) {
                $currentValue = @($currentValue)
            }

            $result[$currentListKey] = @($currentValue) + @(Remove-YamlQuotes $Matches['item'])
        }
    }

    return $result
}

function Get-MarkdownSections {
    param(
        [string]$Body
    )

    $normalizedBody = $Body.Trim()
    $normalizedBody = [regex]::Replace($normalizedBody, '^#\s+[^\r\n]+\r?\n*', '', 1)

    $matches = [regex]::Matches(
        $normalizedBody,
        '(?ms)^##\s+(?<heading>[^\r\n]+)\r?\n(?<content>.*?)(?=^##\s+[^\r\n]+\r?\n|\z)'
    )

    $sections = @()
    $intro = ''

    if ($matches.Count -gt 0) {
        $intro = $normalizedBody.Substring(0, $matches[0].Index).Trim()
    }
    else {
        $intro = $normalizedBody
    }

    foreach ($match in $matches) {
        $sections += [pscustomobject]@{
            Heading = $match.Groups['heading'].Value.Trim()
            Content = $match.Groups['content'].Value.Trim()
        }
    }

    [pscustomobject]@{
        Intro    = $intro
        Sections = $sections
    }
}

function ConvertTo-MarkdownAnchor {
    param(
        [string]$Text
    )

    $anchor = $Text.ToLowerInvariant()
    $anchor = [regex]::Replace($anchor, '[^\w\s-]', '')
    $anchor = [regex]::Replace($anchor, '\s+', '-')
    $anchor = [regex]::Replace($anchor, '-+', '-')
    return $anchor.Trim('-')
}

function Test-SpecificationArtifactId {
    param(
        [AllowNull()]
        [string]$ArtifactId
    )

    if ([string]::IsNullOrWhiteSpace($ArtifactId)) {
        return $false
    }

    return $ArtifactId -match '^SPEC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$'
}

function Get-SpecificationDocument {
    param(
        [System.IO.FileInfo]$File,
        [string]$RootPath
    )

    $content = Get-Content -Raw -LiteralPath $File.FullName
    $frontMatter = Get-FrontMatter -Content $content

    if ($null -eq $frontMatter) {
        return $null
    }

    $metadata = ConvertFrom-SimpleFrontMatter -Yaml $frontMatter.Raw

    if (-not $metadata.Contains('artifact_type')) {
        return $null
    }

    if ($metadata['artifact_type'] -ne 'specification') {
        return $null
    }

    $sectionInfo = Get-MarkdownSections -Body $frontMatter.Body
    $requirements = @()
    $otherSections = @()

    foreach ($section in $sectionInfo.Sections) {
        if ($section.Heading -match '^(?<id>REQ-[A-Z0-9-]+)\s*(?<title>.*)$') {
            $requirements += [pscustomobject]@{
                Id      = $Matches['id']
                Title   = $Matches['title'].Trim()
                Heading = $section.Heading
                Content = $section.Content
            }
        }
        else {
            $otherSections += $section
        }
    }

    $artifactId = if ($metadata.Contains('artifact_id')) { $metadata['artifact_id'] } else { $File.BaseName }
    $title = if ($metadata.Contains('title')) { $metadata['title'] } else { $artifactId }

    if (-not (Test-SpecificationArtifactId -ArtifactId $artifactId)) {
        return $null
    }

    [pscustomobject]@{
        ArtifactId        = $artifactId
        Title             = $title
        Domain            = if ($metadata.Contains('domain')) { $metadata['domain'] } else { '' }
        Capability        = if ($metadata.Contains('capability')) { $metadata['capability'] } else { '' }
        Status            = if ($metadata.Contains('status')) { $metadata['status'] } else { '' }
        Owner             = if ($metadata.Contains('owner')) { $metadata['owner'] } else { '' }
        Tags              = if ($metadata.Contains('tags')) { @($metadata['tags']) } else { @() }
        RelativePath      = [System.IO.Path]::GetRelativePath($RootPath, $File.FullName)
        Intro             = $sectionInfo.Intro
        Requirements      = $requirements
        AdditionalSections = $otherSections
    }
}

$resolvedInput = (Resolve-Path -LiteralPath $InputPath).Path
$outputDirectory = Split-Path -Parent $OutputPath

if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

$markdownFiles = Get-ChildItem -LiteralPath $resolvedInput -Recurse -File -Filter *.md |
    Sort-Object FullName

$documents = @()

foreach ($file in $markdownFiles) {
    $document = Get-SpecificationDocument -File $file -RootPath $resolvedInput

    if ($null -ne $document) {
        $documents += $document
    }
}

if ($documents.Count -eq 0) {
    throw "No specification documents were found under '$resolvedInput'."
}

$documents = @($documents | Sort-Object RelativePath, ArtifactId)
$totalRequirements = (@($documents | ForEach-Object { @($_.Requirements).Count }) | Measure-Object -Sum).Sum

$builder = [System.Text.StringBuilder]::new()

[void]$builder.AppendLine('# Specification Bundle')
[void]$builder.AppendLine()
[void]$builder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss K')")
[void]$builder.AppendLine("Source Root: ``$resolvedInput``")
[void]$builder.AppendLine("Specifications: $($documents.Count)")
[void]$builder.AppendLine("Requirements: $totalRequirements")
[void]$builder.AppendLine()
[void]$builder.AppendLine('## Specification Summary')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| Specification | Domain | Capability | Status | Requirements | Source |')
[void]$builder.AppendLine('| --- | --- | --- | --- | ---: | --- |')

foreach ($document in $documents) {
    $specLabel = "$($document.ArtifactId) - $($document.Title)"
    $specAnchor = ConvertTo-MarkdownAnchor -Text $specLabel
    $sourcePath = $document.RelativePath.Replace('\', '/')
    $requirementCount = @($document.Requirements).Count
    [void]$builder.AppendLine("| [$specLabel](#$specAnchor) | $($document.Domain) | $($document.Capability) | $($document.Status) | $requirementCount | ``$sourcePath`` |")
}

if ($IncludeTableOfContents) {
    [void]$builder.AppendLine()
    [void]$builder.AppendLine('## Table Of Contents')
    [void]$builder.AppendLine()

    foreach ($document in $documents) {
        $specLabel = "$($document.ArtifactId) - $($document.Title)"
        $specAnchor = ConvertTo-MarkdownAnchor -Text $specLabel
        [void]$builder.AppendLine("- [$specLabel](#$specAnchor)")

        foreach ($requirement in @($document.Requirements)) {
            $reqLabel = if ([string]::IsNullOrWhiteSpace($requirement.Title)) { $requirement.Id } else { "$($requirement.Id) $($requirement.Title)" }
            $reqAnchor = ConvertTo-MarkdownAnchor -Text $reqLabel
            [void]$builder.AppendLine("  - [$reqLabel](#$reqAnchor)")
        }
    }
}

foreach ($document in $documents) {
    $specLabel = "$($document.ArtifactId) - $($document.Title)"
    $sourcePath = $document.RelativePath.Replace('\', '/')
    $tags = @($document.Tags) | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }
    $requirements = @($document.Requirements)
    $additionalSections = @($document.AdditionalSections)

    [void]$builder.AppendLine()
    [void]$builder.AppendLine("## $specLabel")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("- Source: ``$sourcePath``")
    [void]$builder.AppendLine("- Domain: $($document.Domain)")
    [void]$builder.AppendLine("- Capability: $($document.Capability)")
    [void]$builder.AppendLine("- Status: $($document.Status)")
    [void]$builder.AppendLine("- Owner: $($document.Owner)")

    if ($tags.Count -gt 0) {
        [void]$builder.AppendLine("- Tags: $($tags -join ', ')")
    }

    [void]$builder.AppendLine("- Requirement Count: $($requirements.Count)")

    if (-not [string]::IsNullOrWhiteSpace($document.Intro)) {
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('### Specification Context')
        [void]$builder.AppendLine()
        [void]$builder.AppendLine($document.Intro.TrimEnd())
    }

    if ($requirements.Count -gt 0) {
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('### Requirements')
        [void]$builder.AppendLine()

        foreach ($requirement in $requirements) {
            [void]$builder.AppendLine("#### $($requirement.Heading)")
            [void]$builder.AppendLine()
            [void]$builder.AppendLine($requirement.Content.TrimEnd())
            [void]$builder.AppendLine()
        }
    }

    if ($additionalSections.Count -gt 0) {
        [void]$builder.AppendLine('### Additional Sections')
        [void]$builder.AppendLine()

        foreach ($section in $additionalSections) {
            [void]$builder.AppendLine("#### $($section.Heading)")
            [void]$builder.AppendLine()
            [void]$builder.AppendLine($section.Content.TrimEnd())
            [void]$builder.AppendLine()
        }
    }
}

$outputContent = $builder.ToString().TrimEnd() + [Environment]::NewLine
Set-Content -LiteralPath $OutputPath -Value $outputContent -Encoding UTF8

Write-Host "Bundled $($documents.Count) specification files and $totalRequirements requirements into '$OutputPath'."
