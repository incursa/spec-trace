<#
.SYNOPSIS
Validates the SpecTrace reference repository.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RequirementIdPattern = '^REQ-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$'
$SpecificationIdPattern = '^SPEC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*$'
$ArchitectureIdPattern = '^ARC-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$'
$WorkItemIdPattern = '^WI-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$'
$VerificationIdPattern = '^VER-[A-Z][A-Z0-9]*(?:-[A-Z][A-Z0-9]*)*-\d{4,}$'

function Remove-YamlQuotes {
    param([AllowNull()][string]$Value)

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
    param([string]$Content)

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
    param([string]$Yaml)

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

    $result
}

function Get-MarkdownSections {
    param([string]$Body)

    $normalizedBody = $Body.Trim()
    $normalizedBody = [regex]::Replace($normalizedBody, '^#\s+[^\r\n]+\r?\n*', '', 1)
    $matches = [regex]::Matches(
        $normalizedBody,
        '(?ms)^##\s+(?<heading>[^\r\n]+)\r?\n(?<content>.*?)(?=^##\s+[^\r\n]+\r?\n|\z)'
    )

    $sections = @()
    foreach ($match in $matches) {
        $sections += [pscustomobject]@{
            Heading = $match.Groups['heading'].Value.Trim()
            Content = $match.Groups['content'].Value.Trim()
        }
    }

    [pscustomobject]@{
        Intro    = if ($matches.Count -gt 0) { $normalizedBody.Substring(0, $matches[0].Index).Trim() } else { $normalizedBody }
        Sections = $sections
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

    $items
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

    $result
}

function Normalize-Set {
    param([AllowNull()][object[]]$Values)

    @(
        $Values |
            ForEach-Object { if ($null -ne $_) { $_.ToString().Trim() } } |
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
        [AllowNull()][string]$Left,
        [AllowNull()][string]$Right
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
        [switch]$AllowUnresolvedRequirement
    )

    foreach ($value in @($Values)) {
        if ([string]::IsNullOrWhiteSpace([string]$value)) {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} contains an empty value."
            continue
        }

        $reference = $value.ToString().Trim()
        if ($reference -notmatch $ExpectedPattern) {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} value '$reference' does not match the expected identifier family."
            continue
        }

        if ($SourceNamespace -and -not (Test-NamespacesMatch $SourceNamespace $reference)) {
            Add-Error $Errors "${FileLabel}: ${FieldLabel} value '$reference' does not align with namespace '$SourceNamespace'."
        }

        if ($reference -match $RequirementIdPattern) {
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
        [System.Collections.Generic.List[string]]$Errors
    )

    $heading = $Section.Heading
    if ($heading -notmatch '^(?<requirementId>REQ-[A-Z0-9-]+)\s+(?<title>.+)$') {
        Add-Error $Errors "${FileLabel}: invalid requirement heading '$heading'."
        return $null
    }

    $requirementId = $Matches['requirementId']
    $title = $Matches['title'].Trim()
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
    $allowedTraceLabels = @('Satisfied By', 'Implemented By', 'Verified By', 'Derived From', 'Supersedes', 'Source Refs', 'Test Refs', 'Code Refs', 'Related')
    foreach ($label in $trace.Keys) {
        if ($allowedTraceLabels -notcontains $label) {
            Add-Error $Errors "${FileLabel}: requirement '$requirementId' trace block contains unsupported label '$label'."
        }
    }

    if ($trace.Contains('Satisfied By')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Satisfied By" -Values $trace['Satisfied By'] -ExpectedPattern $ArchitectureIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements
    }
    if ($trace.Contains('Implemented By')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Implemented By" -Values $trace['Implemented By'] -ExpectedPattern $WorkItemIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements
    }
    if ($trace.Contains('Verified By')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Verified By" -Values $trace['Verified By'] -ExpectedPattern $VerificationIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements
    }
    if ($trace.Contains('Derived From')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Derived From" -Values $trace['Derived From'] -ExpectedPattern $RequirementIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements -AllowUnresolvedRequirement
    }
    if ($trace.Contains('Supersedes')) {
        Add-ReferenceValidation -Errors $Errors -FileLabel $FileLabel -FieldLabel "requirement '$requirementId' / Supersedes" -Values $trace['Supersedes'] -ExpectedPattern $RequirementIdPattern -SourceNamespace $requirementId -KnownArtifacts $KnownArtifacts -KnownRequirements $KnownRequirements -AllowUnresolvedRequirement
    }
    if ($trace.Contains('Related')) {
        foreach ($reference in @($trace['Related'])) {
            if ([string]::IsNullOrWhiteSpace($reference)) {
                Add-Error $Errors "${FileLabel}: requirement '$requirementId' / Related contains an empty value."
                continue
            }

            $normalized = $reference.ToString().Trim()
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
        Namespace     = $namespace
    }
}

function Get-RequiredFiles {
    @(
        'README.md',
        'overview.md',
        'layout.md',
        'authoring.md',
        'AGENTS.md',
        'LLMS.txt',
        'CONTRIBUTING.md',
        'CHANGELOG.md',
        'SECURITY.md',
        'CODE_OF_CONDUCT.md',
        'LICENSE',
        'spec-template.md',
        'architecture-template.md',
        'work-item-template.md',
        'verification-template.md',
        'artifact-id-policy.json',
        'scripts/SpecTrace.Helpers.psm1',
        'scripts/Test-SpecTraceRepository.ps1',
        'schemas/README.md',
        'schemas/artifact-frontmatter.schema.json',
        'schemas/artifact-id-policy.schema.json',
        'schemas/requirement-clause.schema.json',
        'schemas/requirement-trace-fields.schema.json',
        'schemas/work-item-trace-fields.schema.json',
        'examples/README.md',
        'examples/payments/SPEC-PAY-ACH.md',
        'examples/payments/sample-architecture.md',
        'examples/payments/sample-work-item.md',
        'examples/payments/sample-verification.md',
        'examples/arithmetic/SPEC-MATH-DIV.md',
        'examples/arithmetic/sample-architecture.md',
        'examples/arithmetic/sample-work-item.md',
        'examples/arithmetic/sample-verification.md',
        'specs/requirements/spec-trace/_index.md',
        'specs/requirements/spec-trace/SPEC-STD.md',
        'specs/requirements/spec-trace/SPEC-ID.md',
        'specs/requirements/spec-trace/SPEC-LIN.md',
        'specs/requirements/spec-trace/SPEC-PRF.md',
        'specs/requirements/spec-trace/SPEC-LAY.md',
        'specs/requirements/spec-trace/SPEC-TPL.md',
        'specs/requirements/spec-trace/SPEC-SCH.md',
        'specs/requirements/spec-trace/SPEC-EXM.md',
        'scripts/Export-SpecTraceBundle.ps1',
        'scripts/Validate-SpecTrace.ps1'
    )
}

function Get-FrontMatterArray {
    param(
        [hashtable]$Metadata,
        [string]$Key
    )

    if ($Metadata.Contains($Key)) {
        return @($Metadata[$Key])
    }

    return @()
}

$errors = [System.Collections.Generic.List[string]]::new()
$knownArtifacts = @{}
$knownRequirements = @{}
$documents = @()

foreach ($relativePath in Get-RequiredFiles) {
    $fullPath = Join-Path $RootPath $relativePath
    if (-not (Test-Path -LiteralPath $fullPath)) {
        Add-Error $errors "missing required file: $relativePath"
        continue
    }

    $content = Get-Content -Raw -LiteralPath $fullPath
    if ([string]::IsNullOrWhiteSpace($content)) {
        Add-Error $errors "empty required file: $relativePath"
        continue
    }

    if ($relativePath -like '*.json') {
        try {
            $null = $content | ConvertFrom-Json -Depth 64
        }
        catch {
            Add-Error $errors "invalid JSON in ${relativePath}: $($_.Exception.Message)"
        }
    }
}

$artifactFiles = @()
foreach ($folder in @('specs', 'examples')) {
    $folderPath = Join-Path $RootPath $folder
    if (Test-Path -LiteralPath $folderPath) {
        $artifactFiles += Get-ChildItem -LiteralPath $folderPath -Recurse -File -Filter *.md
    }
}

foreach ($file in ($artifactFiles | Sort-Object FullName)) {
    $relativePath = [System.IO.Path]::GetRelativePath($RootPath, $file.FullName).Replace('\', '/')
    $content = Get-Content -Raw -LiteralPath $file.FullName
    $frontMatter = Get-FrontMatter -Content $content

    if ($null -eq $frontMatter) {
        continue
    }

    $metadata = ConvertFrom-SimpleFrontMatter -Yaml $frontMatter.Raw
    if (-not $metadata.Contains('artifact_type')) {
        continue
    }

    $artifactType = [string]$metadata['artifact_type']
    if ($artifactType -notin @('specification', 'architecture', 'work_item', 'verification')) {
        continue
    }

    $artifactId = if ($metadata.Contains('artifact_id')) { [string]$metadata['artifact_id'] } else { '' }
    $title = if ($metadata.Contains('title')) { [string]$metadata['title'] } else { '' }
    $domain = if ($metadata.Contains('domain')) { [string]$metadata['domain'] } else { '' }
    $status = if ($metadata.Contains('status')) { [string]$metadata['status'] } else { '' }
    $owner = if ($metadata.Contains('owner')) { [string]$metadata['owner'] } else { '' }
    $namespace = Get-Namespace $artifactId
    $expectedDomain = Get-ExpectedDomainFromPath -RelativePath $relativePath

    if ([string]::IsNullOrWhiteSpace($artifactId)) {
        Add-Error $errors "${relativePath}: missing artifact_id in front matter."
        continue
    }

    if ($knownArtifacts.ContainsKey($artifactId)) {
        Add-Error $errors "${relativePath}: duplicate artifact ID '$artifactId' also found in $($knownArtifacts[$artifactId].RelativePath)."
        continue
    }

    $typePattern = switch ($artifactType) {
        'specification' { $SpecificationIdPattern }
        'architecture' { $ArchitectureIdPattern }
        'work_item' { $WorkItemIdPattern }
        'verification' { $VerificationIdPattern }
    }

    $statusValues = switch ($artifactType) {
        'specification' { @('draft', 'proposed', 'approved', 'implemented', 'verified', 'superseded', 'retired') }
        'architecture' { @('draft', 'proposed', 'approved', 'implemented', 'verified', 'superseded', 'retired') }
        'work_item' { @('planned', 'in_progress', 'blocked', 'complete', 'cancelled', 'superseded') }
        'verification' { @('planned', 'passed', 'failed', 'blocked', 'waived', 'obsolete') }
    }

    if ($artifactId -notmatch $typePattern) {
        Add-Error $errors "${relativePath}: artifact_id '$artifactId' does not match the $artifactType identifier pattern."
    }

    if ([string]::IsNullOrWhiteSpace($title)) {
        Add-Error $errors "${relativePath}: front matter is missing title."
    }

    if ([string]::IsNullOrWhiteSpace($domain)) {
        Add-Error $errors "${relativePath}: front matter is missing domain."
    }

    if ([string]::IsNullOrWhiteSpace($owner)) {
        Add-Error $errors "${relativePath}: front matter is missing owner."
    }

    switch ($artifactType) {
        'specification' {
            if (-not $metadata.Contains('capability') -or [string]::IsNullOrWhiteSpace([string]$metadata['capability'])) {
                Add-Error $errors "${relativePath}: specification front matter is missing capability."
            }
        }
        'architecture' {
            if (-not $metadata.Contains('satisfies') -or @($metadata['satisfies']).Count -eq 0) {
                Add-Error $errors "${relativePath}: architecture front matter is missing satisfies."
            }
        }
        'work_item' {
            foreach ($requiredKey in @('addresses', 'design_links', 'verification_links')) {
                if (-not $metadata.Contains($requiredKey) -or @($metadata[$requiredKey]).Count -eq 0) {
                    Add-Error $errors "${relativePath}: work item front matter is missing $requiredKey."
                }
            }
        }
        'verification' {
            if (-not $metadata.Contains('verifies') -or @($metadata['verifies']).Count -eq 0) {
                Add-Error $errors "${relativePath}: verification front matter is missing verifies."
            }
        }
    }

    if ($expectedDomain -and $domain -ne $expectedDomain) {
        Add-Error $errors "${relativePath}: front matter domain '$domain' does not match the folder domain '$expectedDomain'."
    }

    if ($statusValues -notcontains $status) {
        Add-Error $errors "${relativePath}: status '$status' is not valid for artifact type '$artifactType'."
    }

    $sections = Get-MarkdownSections -Body $frontMatter.Body
    $doc = [pscustomobject]@{
        RelativePath      = $relativePath
        ArtifactId        = $artifactId
        ArtifactType      = $artifactType
        Domain            = $domain
        Namespace         = $namespace
        Metadata          = $metadata
        Sections          = @($sections.Sections)
        RequirementSections = @()
    }
    $knownArtifacts[$artifactId] = $doc
    $documents += $doc

    if ($artifactType -eq 'specification') {
        foreach ($section in $sections.Sections) {
            if ($section.Heading -match '^REQ-') {
                if ($section.Heading -notmatch '^(?<requirementId>REQ-[A-Z0-9-]+)\s+(?<title>.+)$') {
                    Add-Error $errors "${relativePath}: invalid requirement heading '$($section.Heading)'."
                    continue
                }

                $requirementId = $Matches['requirementId']
                $requirementNamespace = Get-Namespace $requirementId

                if ($requirementNamespace -and $namespace -and $requirementNamespace -ne $namespace) {
                    Add-Error $errors "${relativePath}: requirement '$requirementId' does not share namespace '$namespace' with artifact '$artifactId'."
                }

                if ($knownRequirements.ContainsKey($requirementId)) {
                    Add-Error $errors "${relativePath}: duplicate requirement ID '$requirementId' also found in $($knownRequirements[$requirementId].RelativePath)."
                    continue
                }

                $knownRequirements[$requirementId] = [pscustomobject]@{
                    RelativePath = $relativePath
                    ArtifactId   = $artifactId
                    Namespace    = $requirementNamespace
                    Section      = $section
                    Title        = ''
                    Trace        = $null
                }

                $doc.RequirementSections += $section
            }
        }

        if ($doc.RequirementSections.Count -eq 0) {
            Add-Error $errors "${relativePath}: specification must contain at least one REQ section."
        }
    }
}

foreach ($doc in $documents) {
    $artifactId = $doc.ArtifactId
    $artifactType = $doc.ArtifactType
    $metadata = $doc.Metadata
    $sections = $doc.Sections
    $relativePath = $doc.RelativePath
    $namespace = $doc.Namespace

    Test-UniqueValues -Errors $errors -FileLabel $relativePath -Label 'related_artifacts' -Values (Get-FrontMatterArray -Metadata $metadata -Key 'related_artifacts')

    $relatedArtifacts = Get-FrontMatterArray -Metadata $metadata -Key 'related_artifacts'
    if (@($relatedArtifacts).Count -gt 0) {
        Add-ReferenceValidation -Errors $errors -FileLabel $relativePath -FieldLabel 'related_artifacts' -Values $relatedArtifacts -ExpectedPattern '^(SPEC|ARC|WI|VER)-' -SourceNamespace $null -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements
    }

    if ($artifactType -eq 'specification') {
        foreach ($requirementSection in $doc.RequirementSections) {
            $requirement = Parse-RequirementSection -Section $requirementSection -FileLabel $relativePath -SpecNamespace $namespace -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements -Errors $errors
            if ($null -ne $requirement) {
                $knownRequirements[$requirement.RequirementId].Trace = $requirement.Trace
                $knownRequirements[$requirement.RequirementId].Title = $requirement.Title
            }
        }
    }

    if ($artifactType -eq 'architecture') {
        $satisfies = Get-FrontMatterArray -Metadata $metadata -Key 'satisfies'
        Test-UniqueValues -Errors $errors -FileLabel $relativePath -Label 'satisfies' -Values $satisfies
        Add-ReferenceValidation -Errors $errors -FileLabel $relativePath -FieldLabel 'satisfies' -Values $satisfies -ExpectedPattern $RequirementIdPattern -SourceNamespace $artifactId -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements

        $requirementsSatisfied = $sections | Where-Object { $_.Heading -eq 'Requirements Satisfied' } | Select-Object -First 1
        if ($null -eq $requirementsSatisfied) {
            Add-Error $errors "${relativePath}: architecture document is missing a Requirements Satisfied section."
        }
        else {
            Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Requirements Satisfied' -Left (Parse-BulletList -Content $requirementsSatisfied.Content) -Right $satisfies
        }
    }

    if ($artifactType -eq 'work_item') {
        $addresses = Get-FrontMatterArray -Metadata $metadata -Key 'addresses'
        $designLinks = Get-FrontMatterArray -Metadata $metadata -Key 'design_links'
        $verificationLinks = Get-FrontMatterArray -Metadata $metadata -Key 'verification_links'

        Test-UniqueValues -Errors $errors -FileLabel $relativePath -Label 'addresses' -Values $addresses
        Test-UniqueValues -Errors $errors -FileLabel $relativePath -Label 'design_links' -Values $designLinks
        Test-UniqueValues -Errors $errors -FileLabel $relativePath -Label 'verification_links' -Values $verificationLinks

        Add-ReferenceValidation -Errors $errors -FileLabel $relativePath -FieldLabel 'addresses' -Values $addresses -ExpectedPattern $RequirementIdPattern -SourceNamespace $artifactId -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements
        Add-ReferenceValidation -Errors $errors -FileLabel $relativePath -FieldLabel 'design_links' -Values $designLinks -ExpectedPattern $ArchitectureIdPattern -SourceNamespace $artifactId -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements
        Add-ReferenceValidation -Errors $errors -FileLabel $relativePath -FieldLabel 'verification_links' -Values $verificationLinks -ExpectedPattern $VerificationIdPattern -SourceNamespace $artifactId -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements

        $requirementsAddressed = $sections | Where-Object { $_.Heading -eq 'Requirements Addressed' } | Select-Object -First 1
        $designInputs = $sections | Where-Object { $_.Heading -eq 'Design Inputs' } | Select-Object -First 1
        $traceLinks = $sections | Where-Object { $_.Heading -eq 'Trace Links' } | Select-Object -First 1

        if ($null -eq $requirementsAddressed) {
            Add-Error $errors "${relativePath}: work item document is missing a Requirements Addressed section."
        }
        else {
            Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Requirements Addressed' -Left (Parse-BulletList -Content $requirementsAddressed.Content) -Right $addresses
        }

        if ($null -eq $designInputs) {
            Add-Error $errors "${relativePath}: work item document is missing a Design Inputs section."
        }
        else {
            Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Design Inputs' -Left (Parse-BulletList -Content $designInputs.Content) -Right $designLinks
        }

        if ($null -eq $traceLinks) {
            Add-Error $errors "${relativePath}: work item document is missing a Trace Links section."
        }
        else {
            $trace = Parse-LabeledListBlock -Content $traceLinks.Content
            foreach ($label in @('Addresses', 'Uses Design', 'Verified By')) {
                if (-not $trace.Contains($label)) {
                    Add-Error $errors "${relativePath}: Trace Links is missing label '$label'."
                }
            }

            if ($trace.Contains('Addresses')) {
                Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Trace Links / Addresses' -Left $trace['Addresses'] -Right $addresses
            }

            if ($trace.Contains('Uses Design')) {
                Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Trace Links / Uses Design' -Left $trace['Uses Design'] -Right $designLinks
            }

            if ($trace.Contains('Verified By')) {
                Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Trace Links / Verified By' -Left $trace['Verified By'] -Right $verificationLinks
            }
        }
    }

    if ($artifactType -eq 'verification') {
        $verifies = Get-FrontMatterArray -Metadata $metadata -Key 'verifies'
        Test-UniqueValues -Errors $errors -FileLabel $relativePath -Label 'verifies' -Values $verifies
        Add-ReferenceValidation -Errors $errors -FileLabel $relativePath -FieldLabel 'verifies' -Values $verifies -ExpectedPattern $RequirementIdPattern -SourceNamespace $artifactId -KnownArtifacts $knownArtifacts -KnownRequirements $knownRequirements

        $requirementsVerified = $sections | Where-Object { $_.Heading -eq 'Requirements Verified' } | Select-Object -First 1
        if ($null -eq $requirementsVerified) {
            Add-Error $errors "${relativePath}: verification document is missing a Requirements Verified section."
        }
        else {
            Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Requirements Verified' -Left (Parse-BulletList -Content $requirementsVerified.Content) -Right $verifies
        }

        $relatedArtifactsSection = $sections | Where-Object { $_.Heading -eq 'Related Artifacts' } | Select-Object -First 1
        if ($null -ne $relatedArtifactsSection -and @($relatedArtifacts).Count -gt 0) {
            Assert-SetEquals -Errors $errors -FileLabel $relativePath -Label 'Related Artifacts' -Left (Parse-BulletList -Content $relatedArtifactsSection.Content) -Right $relatedArtifacts
        }
    }
}

foreach ($entry in $knownRequirements.GetEnumerator()) {
    $requirementId = $entry.Key
    $record = $entry.Value
    $trace = if ($null -ne $record.Trace) { $record.Trace } else { [ordered]@{} }

    if ($trace.Contains('Satisfied By')) {
        foreach ($targetId in @($trace['Satisfied By'])) {
            $target = $knownArtifacts[$targetId]
            if ($null -ne $target) {
                $targetRequirements = Get-FrontMatterArray -Metadata $target.Metadata -Key 'satisfies'
                if ($targetRequirements -notcontains $requirementId) {
                    Add-Error $errors "$($record.RelativePath): requirement '$requirementId' is not reciprocated by architecture '$targetId'."
                }
            }
        }
    }

    if ($trace.Contains('Implemented By')) {
        foreach ($targetId in @($trace['Implemented By'])) {
            $target = $knownArtifacts[$targetId]
            if ($null -ne $target) {
                $targetRequirements = Get-FrontMatterArray -Metadata $target.Metadata -Key 'addresses'
                if ($targetRequirements -notcontains $requirementId) {
                    Add-Error $errors "$($record.RelativePath): requirement '$requirementId' is not reciprocated by work item '$targetId'."
                }
            }
        }
    }

    if ($trace.Contains('Verified By')) {
        foreach ($targetId in @($trace['Verified By'])) {
            $target = $knownArtifacts[$targetId]
            if ($null -ne $target) {
                $targetRequirements = Get-FrontMatterArray -Metadata $target.Metadata -Key 'verifies'
                if ($targetRequirements -notcontains $requirementId) {
                    Add-Error $errors "$($record.RelativePath): requirement '$requirementId' is not reciprocated by verification '$targetId'."
                }
            }
        }
    }
}

if ($errors.Count -gt 0) {
    Write-Error ($errors -join [Environment]::NewLine)
    exit 1
}

$specCount = @($documents | Where-Object { $_.ArtifactType -eq 'specification' }).Count
$requirementCount = $knownRequirements.Count
Write-Host "Validated $($documents.Count) artifacts across $specCount specifications and $requirementCount requirements."
