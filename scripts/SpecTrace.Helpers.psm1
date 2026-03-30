# Shared parsing helpers for SpecTrace PowerShell scripts.

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

function Get-NormalizedIdentifierText {
    param(
        [AllowNull()]
        [string]$Value
    )

    if ($null -eq $Value) {
        return $null
    }

    $normalized = Remove-YamlQuotes $Value
    if ($null -eq $normalized) {
        return $null
    }

    $normalized = $normalized.Trim()

    if ($normalized -match '^\[(?<label>.+?)\]\((?<target>[^)]+)\)$') {
        $normalized = $Matches['label'].Trim()
    }

    if ($normalized -match '^`(?<id>(?:SPEC|REQ|ARC|WI|VER)-[A-Z0-9-]+)`$') {
        return $Matches['id']
    }

    return $normalized
}

function Get-RequirementHeadingParts {
    param(
        [AllowNull()]
        [string]$Heading
    )

    if ([string]::IsNullOrWhiteSpace($Heading)) {
        return $null
    }

    $trimmed = $Heading.Trim()

    if ($trimmed -match '^\[(?<label>.+?)\]\((?<target>[^)]+)\)\s+(?<title>.+)$') {
        $label = $Matches['label']
        $title = $Matches['title'].Trim()
        $requirementId = Get-NormalizedIdentifierText $label
        if ($requirementId -match '^REQ-[A-Z0-9-]+$') {
            return [pscustomobject]@{
                RequirementId = $requirementId
                Title         = $title
            }
        }
    }

    if ($trimmed -match '^`(?<requirementId>REQ-[A-Z0-9-]+)`\s+(?<title>.+)$') {
        return [pscustomobject]@{
            RequirementId = $Matches['requirementId']
            Title         = $Matches['title'].Trim()
        }
    }

    if ($trimmed -match '^(?<requirementId>REQ-[A-Z0-9-]+)\s+(?<title>.+)$') {
        return [pscustomobject]@{
            RequirementId = $Matches['requirementId']
            Title         = $Matches['title'].Trim()
        }
    }

    return $null
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

    # Specification identifiers are terminal-free: SPEC-<DOMAIN>(-<GROUPING>...)
    return $ArtifactId -match '^SPEC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*$'
}

function Get-ArtifactIdComponents {
    param(
        [AllowNull()]
        [string]$ArtifactId
    )

    if ([string]::IsNullOrWhiteSpace($ArtifactId)) {
        return $null
    }

    if ($ArtifactId -match '^(?<prefix>SPEC|REQ|ARC|WI|VER)-(?<stem>[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*)(?:-(?<sequence>\d{4,}))?$') {
        $prefix = $Matches['prefix']
        $stem = $Matches['stem']
        $sequence = $Matches['sequence']

        [pscustomobject]@{
            ArtifactId        = $ArtifactId
            Prefix            = $prefix
            Stem              = $stem
            Sequence          = $sequence
            IsSequenceBearing = -not [string]::IsNullOrWhiteSpace($sequence)
            Segments          = @($stem -split '-')
        }
    }

    return $null
}

function Test-ArtifactIdFamily {
    param(
        [AllowNull()]
        [string]$ArtifactId,
        [Parameter(Mandatory)]
        [ValidateSet('specification', 'requirement', 'architecture', 'work_item', 'verification')]
        [string]$ArtifactType
    )

    switch ($ArtifactType) {
        'specification' { return Test-SpecificationArtifactId $ArtifactId }
        'requirement' { return $ArtifactId -match '^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$' }
        'architecture' { return $ArtifactId -match '^ARC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$' }
        'work_item' { return $ArtifactId -match '^WI-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$' }
        'verification' { return $ArtifactId -match '^VER-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$' }
    }
}

function Parse-BulletList {
    param([string]$Content)

    $items = @()
    foreach ($line in ($Content -split "\r?\n")) {
        if ($line -match '^\s*-\s+(?<item>.+?)\s*$') {
            $items += $Matches['item'].Trim()
        }
    }

    return $items
}

function Parse-LabeledListBlock {
    param([string]$Content)

    $result = [ordered]@{}
    $currentLabel = $null

    foreach ($line in ($Content -split "\r?\n")) {
        if ($line -match '^\s*(?:-\s*)?(?<label>[A-Za-z][A-Za-z ]*):\s*$') {
            $currentLabel = $Matches['label'].Trim()
            if (-not $result.Contains($currentLabel)) {
                $result[$currentLabel] = @()
            }
            continue
        }

        if ($line -match '^\s*-\s+(?<item>.+?)\s*$' -and $null -ne $currentLabel) {
            $result[$currentLabel] = @($result[$currentLabel]) + @($Matches['item'].Trim())
        }
    }

    return $result
}

function Normalize-Set {
    param([AllowNull()][object[]]$Values)

    @(
        $Values |
            ForEach-Object { if ($null -ne $_) { Get-NormalizedIdentifierText ($_.ToString()) } } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Select-Object -Unique |
            Sort-Object
    )
}

function Add-Error {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$Message
    )

    [void]$Errors.Add($Message)
}

function Get-ExpectedDomainFromPath {
    param([string]$RelativePath)

    if ($RelativePath -match '^(?:specs/(?:requirements|architecture|work-items|verification)|examples)/(?<domain>[^/]+)/') {
        return $Matches['domain']
    }

    return $null
}

function Get-Namespace {
    param([AllowNull()][string]$Identifier)

    if ([string]::IsNullOrWhiteSpace($Identifier)) {
        return $null
    }

    if ($Identifier -match '^SPEC-(?<namespace>[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*)$') {
        return $Matches['namespace']
    }

    if ($Identifier -match '^(?:REQ|ARC|WI|VER)-(?<namespace>[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*)-\d{4,}$') {
        return $Matches['namespace']
    }

    return $null
}

function Test-NamespacesMatch {
    param(
        [AllowNull()]
        [string]$Left,
        [AllowNull()]
        [string]$Right
    )

    $leftNamespace = Get-Namespace $Left
    $rightNamespace = Get-Namespace $Right
    if ($null -eq $leftNamespace -or $null -eq $rightNamespace) {
        return $false
    }

    return $leftNamespace -eq $rightNamespace
}

function Assert-SetEquals {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$FileLabel,
        [string]$Label,
        [object[]]$Left,
        [object[]]$Right
    )

    $leftSet = @(Normalize-Set $Left)
    $rightSet = @(Normalize-Set $Right)

    if ($leftSet.Count -ne $rightSet.Count -or (Compare-Object -ReferenceObject $leftSet -DifferenceObject $rightSet)) {
        Add-Error $Errors "${FileLabel}: ${Label} mismatch. Expected [$($rightSet -join ', ')], found [$($leftSet -join ', ')]."
    }
}

function Test-UniqueValues {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$FileLabel,
        [string]$Label,
        [object[]]$Values
    )

    $normalized = @(
        $Values |
            ForEach-Object {
                if ($null -ne $_) {
                    $_.ToString().Trim()
                }
            } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
    $unique = @($normalized | Select-Object -Unique)

    if ($normalized.Count -ne $unique.Count) {
        Add-Error $Errors "${FileLabel}: ${Label} contains duplicate values."
    }
}

function Add-ReferenceValidation {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$FileLabel,
        [string]$FieldLabel,
        [object[]]$Values,
        [string]$ExpectedPattern,
        [string]$SourceNamespace,
        [hashtable]$KnownArtifacts,
        [hashtable]$KnownRequirements,
        [string]$RequirementIdPattern = '^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$',
        [switch]$AllowUnresolvedRequirement
    )

    foreach ($value in @($Values)) {
        if ([string]::IsNullOrWhiteSpace([string]$value)) {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} contains an empty value."
            continue
        }

        $reference = Get-NormalizedIdentifierText ($value.ToString())
        if ($reference -notmatch $ExpectedPattern) {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} value '$reference' does not match the expected identifier family."
            continue
        }

        if ($SourceNamespace -and -not (Test-NamespacesMatch $SourceNamespace $reference)) {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} value '$reference' does not align with namespace '$SourceNamespace'."
        }

        if ($reference -match '^REQ-') {
            if (-not $AllowUnresolvedRequirement -and -not $KnownRequirements.ContainsKey($reference)) {
                Add-Error $Errors "${FileLabel}: ${FieldLabel} references unknown requirement '$reference'."
            }
        }
        elseif ($reference -match '^(SPEC|ARC|WI|VER)-') {
            if (-not $KnownArtifacts.ContainsKey($reference)) {
                Add-Error $Errors "${FileLabel}: ${FieldLabel} references unknown artifact '$reference'."
            }
        }
        else {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} references unsupported identifier '$reference'."
        }
    }
}

function Parse-RequirementSection {
    param(
        [object]$Section,
        [string]$FileLabel,
        [string]$SpecNamespace,
        [hashtable]$KnownArtifacts,
        [hashtable]$KnownRequirements,
        [System.Collections.Generic.List[string]]$Errors,
        [string]$RequirementIdPattern = '^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$',
        [string]$ArchitectureIdPattern = '^ARC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$',
        [string]$WorkItemIdPattern = '^WI-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$',
        [string]$VerificationIdPattern = '^VER-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$',
        [string[]]$AllowedTraceLabels = @('Satisfied By', 'Implemented By', 'Verified By', 'Derived From', 'Supersedes', 'Upstream Refs', 'Related'),
        [switch]$AllowExtensionLabels,
        [switch]$SkipReferenceValidation
    )

    $heading = $Section.Heading
    $headingParts = Get-RequirementHeadingParts $heading
    if ($null -eq $headingParts) {
        Add-Error $Errors "${FileLabel}: invalid requirement heading '$heading'."
        return $null
    }

    $requirementId = $headingParts.RequirementId
    $title = $headingParts.Title
    $namespace = Get-Namespace $requirementId

    if ([string]::IsNullOrWhiteSpace($title)) {
        Add-Error $Errors "${FileLabel}: requirement '$requirementId' is missing a short title."
    }

    if ($SpecNamespace -and $namespace -and $namespace -ne $SpecNamespace) {
        Add-Error $Errors "${FileLabel}: requirement '$requirementId' does not align with specification namespace '$SpecNamespace'."
    }

    $bodyLines = $Section.Content -split "\r?\n"
    $clauseLines = @()
    $traceLines = @()
    $notesLines = @()
    $mode = 'clause'

    foreach ($line in $bodyLines) {
        if ($line -match '^\s*Trace:\s*$') {
            $mode = 'trace'
            continue
        }

        if ($line -match '^\s*Notes:\s*$') {
            $mode = 'notes'
            continue
        }

        switch ($mode) {
            'clause' { $clauseLines += $line }
            'trace' { $traceLines += $line }
            'notes' { $notesLines += $line }
        }
    }

    $clause = ($clauseLines -join "`n").Trim()
    if ([string]::IsNullOrWhiteSpace($clause)) {
        Add-Error $Errors "${FileLabel}: requirement '$requirementId' is missing its normative clause."
    }
    else {
        $keywordMatches = [regex]::Matches($clause, '\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b')
        if ($keywordMatches.Count -ne 1) {
            Add-Error $Errors "${FileLabel}: requirement '$requirementId' clause must contain exactly one approved normative keyword."
        }
    }

    $trace = if ($traceLines.Count -gt 0) { Parse-LabeledListBlock -Content ($traceLines -join "`n") } else { [ordered]@{} }
    $unsupportedTraceLabels = @()
    foreach ($label in $trace.Keys) {
        if ($AllowedTraceLabels -notcontains $label) {
            $unsupportedTraceLabels += $label
            if (-not $AllowExtensionLabels) {
                Add-Error $Errors "${FileLabel}: requirement '$requirementId' trace block contains unsupported label '$label'."
            }
        }
    }

    if (-not $SkipReferenceValidation -and $trace.Contains('Satisfied By')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Satisfied By" -Values $trace['Satisfied By'] -ExpectedPattern $ArchitectureIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements
    }
    if (-not $SkipReferenceValidation -and $trace.Contains('Implemented By')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Implemented By" -Values $trace['Implemented By'] -ExpectedPattern $WorkItemIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements
    }
    if (-not $SkipReferenceValidation -and $trace.Contains('Verified By')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Verified By" -Values $trace['Verified By'] -ExpectedPattern $VerificationIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements
    }
    if (-not $SkipReferenceValidation -and $trace.Contains('Derived From')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Derived From" -Values $trace['Derived From'] -ExpectedPattern $RequirementIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements -AllowUnresolvedRequirement
    }
    if (-not $SkipReferenceValidation -and $trace.Contains('Supersedes')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Supersedes" -Values $trace['Supersedes'] -ExpectedPattern $RequirementIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements -AllowUnresolvedRequirement
    }
    if (-not $SkipReferenceValidation -and $trace.Contains('Related')) {
        foreach ($reference in @($trace['Related'])) {
            if ([string]::IsNullOrWhiteSpace($reference)) {
                Add-Error $Errors "${FileLabel}: requirement '$requirementId' / Related contains an empty value."
                continue
            }

            $normalized = Get-NormalizedIdentifierText ($reference.ToString())
            if ($normalized -match $RequirementIdPattern) {
                if (-not $KnownRequirements.ContainsKey($normalized)) {
                    Add-Error $Errors "${FileLabel}: requirement '$requirementId' / Related references unknown requirement '$normalized'."
                }
            }
            elseif ($normalized -match '^(SPEC|ARC|WI|VER)-') {
                if (-not $KnownArtifacts.ContainsKey($normalized)) {
                    Add-Error $Errors "${FileLabel}: requirement '$requirementId' / Related references unknown artifact '$normalized'."
                }
            }
            else {
                Add-Error $Errors "${FileLabel}: requirement '$requirementId' / Related value '$normalized' is not a supported identifier."
            }
        }
    }

    [pscustomobject]@{
        RequirementId = $requirementId
        Title         = $title
        Clause        = $clause
        Trace         = $trace
        Notes         = $notesLines
        NotesText     = ($notesLines -join "`n").Trim()
        UnsupportedTraceLabels = $unsupportedTraceLabels
        Namespace     = $namespace
    }
}

function Get-FrontMatterArray {
    param(
        [hashtable]$Metadata,
        [string]$Key
    )

    if ($Metadata.Contains($Key)) {
        return @($Metadata[$Key] | ForEach-Object { if ($null -ne $_) { Get-NormalizedIdentifierText ($_.ToString()) } })
    }

    return @()
}

Export-ModuleMember -Function `
    Remove-YamlQuotes, `
    Get-NormalizedIdentifierText, `
    Get-RequirementHeadingParts, `
    Get-FrontMatter, `
    ConvertFrom-SimpleFrontMatter, `
    Get-MarkdownSections, `
    ConvertTo-MarkdownAnchor, `
    Test-SpecificationArtifactId, `
    Get-ArtifactIdComponents, `
    Test-ArtifactIdFamily, `
    Parse-BulletList, `
    Parse-LabeledListBlock, `
    Normalize-Set, `
    Add-Error, `
    Get-ExpectedDomainFromPath, `
    Get-Namespace, `
    Test-NamespacesMatch, `
    Assert-SetEquals, `
    Test-UniqueValues, `
    Add-ReferenceValidation, `
    Parse-RequirementSection, `
    Get-FrontMatterArray
