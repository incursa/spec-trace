<#
.SYNOPSIS
Validates a SpecTrace repository tree against the hardened standard.

.DESCRIPTION
Scans Markdown artifacts and schema/support files, validates identifier shapes,
trace-link family typing, reciprocal consistency, profile-specific graph rules,
and selected schema/policy contracts. The default profile is `core` so the
validator stays lightweight on day one; `traceable` and `auditable` enable
stricter repository-level enforcement.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$InputPath,

    [Parameter()]
    [ValidateSet('core', 'traceable', 'auditable')]
    [string]$Profile = 'core',

    [Parameter()]
    [string]$JsonReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'SpecTrace.Helpers.psm1') -Force -DisableNameChecking

$ApprovedKeywordPattern = '\b(?:MUST NOT|SHALL NOT|SHOULD NOT|MUST|SHALL|SHOULD|MAY)\b'
$CanonicalTraceLabels = @('Satisfied By', 'Implemented By', 'Verified By', 'Derived From', 'Supersedes', 'Upstream Refs', 'Related')
$CanonicalWorkItemTraceLabels = @('Addresses', 'Uses Design', 'Verified By')
$ArtifactTypes = @('specification', 'architecture', 'work_item', 'verification')
$EvidenceStatusValues = @('observed', 'passed', 'failed', 'not_observed', 'not_collected', 'unsupported')
$EvidenceKindPattern = '^[a-z][a-z0-9_]*$'

$ArtifactFrontMatterKeys = @{
    specification = @('artifact_id', 'artifact_type', 'title', 'domain', 'capability', 'status', 'owner', 'tags', 'related_artifacts')
    architecture  = @('artifact_id', 'artifact_type', 'title', 'domain', 'status', 'owner', 'satisfies', 'related_artifacts')
    work_item     = @('artifact_id', 'artifact_type', 'title', 'domain', 'status', 'owner', 'addresses', 'design_links', 'verification_links', 'related_artifacts')
    verification  = @('artifact_id', 'artifact_type', 'title', 'domain', 'status', 'owner', 'verifies', 'related_artifacts')
}

$ArtifactStatusValues = @{
    specification = @('draft', 'proposed', 'approved', 'implemented', 'verified', 'superseded', 'retired')
    architecture  = @('draft', 'proposed', 'approved', 'implemented', 'verified', 'superseded', 'retired')
    work_item     = @('planned', 'in_progress', 'blocked', 'complete', 'cancelled', 'superseded')
    verification  = @('planned', 'passed', 'failed', 'blocked', 'waived', 'obsolete')
}

function New-Finding {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('error', 'warning')]
        [string]$Severity,

        [Parameter(Mandatory)]
        [string]$Code,

        [Parameter(Mandatory)]
        [string]$Message,

        [Parameter()]
        [string]$File = '',

        [Parameter()]
        [string]$ArtifactId = '',

        [Parameter()]
        [string]$RequirementId = ''
    )

    [pscustomobject]@{
        severity       = $Severity
        code           = $Code
        file           = $File
        artifact_id    = $ArtifactId
        requirement_id = $RequirementId
        message        = $Message
    }
}

function Add-Finding {
    param(
        [System.Collections.Generic.List[object]]$Findings,
        [ValidateSet('error', 'warning')]
        [string]$Severity,
        [string]$Code,
        [string]$Message,
        [string]$File = '',
        [string]$ArtifactId = '',
        [string]$RequirementId = ''
    )

    [void]$Findings.Add((New-Finding -Severity $Severity -Code $Code -Message $Message -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId))
}

function Get-RuleSeverity {
    param(
        [Parameter(Mandatory)]
        [string]$Rule,

        [Parameter(Mandatory)]
        [ValidateSet('core', 'traceable', 'auditable')]
        [string]$Profile
    )

    switch ($Rule) {
        'extension' {
            if ($Profile -eq 'core') { return 'warning' }
            return 'error'
        }
        'duplicate' {
            if ($Profile -eq 'core') { return 'warning' }
            return 'error'
        }
        'unresolved' {
            if ($Profile -eq 'core') { return 'warning' }
            return 'error'
        }
        'downstream' {
            if ($Profile -eq 'core') { return 'warning' }
            return 'error'
        }
        'coverage' {
            if ($Profile -eq 'auditable') { return 'error' }
            return 'warning'
        }
        'reciprocal' {
            if ($Profile -eq 'auditable') { return 'error' }
            return 'warning'
        }
        'orphan' {
            if ($Profile -eq 'auditable') { return 'error' }
            return 'warning'
        }
        'notes' {
            return 'warning'
        }
        default {
            return 'error'
        }
    }
}

function Get-ReferenceFamily {
    param([AllowNull()][string]$Identifier)

    if ([string]::IsNullOrWhiteSpace($Identifier)) {
        return $null
    }

    if ($Identifier -match '^REQ-') { return 'REQ' }
    if ($Identifier -match '^SPEC-') { return 'SPEC' }
    if ($Identifier -match '^ARC-') { return 'ARC' }
    if ($Identifier -match '^WI-') { return 'WI' }
    if ($Identifier -match '^VER-') { return 'VER' }
    return $null
}

function Test-UniqueStrings {
    param(
        [System.Collections.Generic.List[object]]$Findings,
        [string]$Profile,
        [string]$Code,
        [string]$File,
        [string]$Label,
        [object[]]$Values,
        [string]$ArtifactId = '',
        [string]$RequirementId = ''
    )

    $normalized = @(
        $Values |
            ForEach-Object { if ($null -ne $_) { $_.ToString().Trim() } } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($normalized.Count -ne @($normalized | Select-Object -Unique).Count) {
        Add-Finding -Findings $Findings -Severity (Get-RuleSeverity -Rule 'duplicate' -Profile $Profile) -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${Label} contains duplicate values."
    }
}

function Add-SetMismatchFinding {
    param(
        [System.Collections.Generic.List[object]]$Findings,
        [string]$Code,
        [string]$File,
        [string]$ExpectedLabel,
        [string]$ActualLabel,
        [object[]]$Expected,
        [object[]]$Actual,
        [string]$ArtifactId = '',
        [string]$RequirementId = ''
    )

    $expectedSet = @(Normalize-Set $Expected)
    $actualSet = @(Normalize-Set $Actual)

    if ($expectedSet.Count -ne $actualSet.Count -or (Compare-Object -ReferenceObject $expectedSet -DifferenceObject $actualSet)) {
        Add-Finding -Findings $Findings -Severity 'error' -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${ActualLabel} mismatch. Expected [$($expectedSet -join ', ')], found [$($actualSet -join ', ')]."
    }
}

function Test-ReferenceList {
    param(
        [System.Collections.Generic.List[object]]$Findings,
        [string]$Profile,
        [string]$File,
        [string]$ContextLabel,
        [object[]]$Values,
        [string[]]$AllowedFamilies,
        [hashtable]$KnownArtifacts,
        [hashtable]$KnownRequirements,
        [string]$Code,
        [string]$ArtifactId = '',
        [string]$RequirementId = '',
        [switch]$AllowUnresolvedRequirement
    )

    $normalized = @()
    foreach ($value in @($Values)) {
        if ([string]::IsNullOrWhiteSpace([string]$value)) {
            Add-Finding -Findings $Findings -Severity 'error' -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${ContextLabel} contains an empty value."
            continue
        }

        $reference = Get-NormalizedIdentifierText ($value.ToString())
        $normalized += $reference

        $family = Get-ReferenceFamily $reference
        if ($null -eq $family) {
            Add-Finding -Findings $Findings -Severity 'error' -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${ContextLabel} value '$reference' is not a supported identifier."
            continue
        }

        if ($AllowedFamilies -notcontains $family) {
            Add-Finding -Findings $Findings -Severity 'error' -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${ContextLabel} value '$reference' does not match the expected identifier family."
            continue
        }

        if ($family -eq 'REQ') {
            if (-not $AllowUnresolvedRequirement -and -not $KnownRequirements.ContainsKey($reference)) {
                Add-Finding -Findings $Findings -Severity (Get-RuleSeverity -Rule 'unresolved' -Profile $Profile) -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${ContextLabel} references unknown requirement '$reference'."
            }
        }
        else {
            if (-not $KnownArtifacts.ContainsKey($reference)) {
                Add-Finding -Findings $Findings -Severity (Get-RuleSeverity -Rule 'unresolved' -Profile $Profile) -Code $Code -File $File -ArtifactId $ArtifactId -RequirementId $RequirementId -Message "${ContextLabel} references unknown artifact '$reference'."
            }
        }
    }

    return ,$normalized
}

function Get-AllowedFrontMatterKeys {
    param([Parameter(Mandatory)][ValidateSet('specification', 'architecture', 'work_item', 'verification')][string]$ArtifactType)
    return $ArtifactFrontMatterKeys[$ArtifactType]
}

function Get-RequiredFrontMatterKeys {
    param([Parameter(Mandatory)][ValidateSet('specification', 'architecture', 'work_item', 'verification')][string]$ArtifactType)

    switch ($ArtifactType) {
        'specification' { return @('artifact_id', 'artifact_type', 'title', 'domain', 'capability', 'status', 'owner') }
        'architecture' { return @('artifact_id', 'artifact_type', 'title', 'domain', 'status', 'owner', 'satisfies') }
        'work_item' { return @('artifact_id', 'artifact_type', 'title', 'domain', 'status', 'owner', 'addresses', 'design_links', 'verification_links') }
        'verification' { return @('artifact_id', 'artifact_type', 'title', 'domain', 'status', 'owner', 'verifies') }
    }
}

function Get-StatusValues {
    param([Parameter(Mandatory)][ValidateSet('specification', 'architecture', 'work_item', 'verification')][string]$ArtifactType)
    return $ArtifactStatusValues[$ArtifactType]
}

function Resolve-RepositoryPath {
    param(
        [string]$BasePath,
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return (Resolve-Path -LiteralPath (Join-Path $BasePath $Path)).Path
}

function Get-ValidationMarkdownFiles {
    param(
        [string]$ResolvedRootPath,
        [string]$ResolvedInputPath
    )

    if ($ResolvedInputPath -eq $ResolvedRootPath) {
        $paths = @()
        foreach ($relative in @('specs', 'examples')) {
            $candidate = Join-Path $ResolvedRootPath $relative
            if (Test-Path -LiteralPath $candidate) {
                $paths += Get-ChildItem -LiteralPath $candidate -Recurse -File -Filter *.md
            }
        }

        foreach ($relative in @(
            'README.md',
            'overview.md',
            'layout.md',
            'authoring.md',
            'AGENTS.md',
            'spec-template.md',
            'architecture-template.md',
            'work-item-template.md',
            'verification-template.md',
            'CHANGELOG.md'
        )) {
            $candidate = Join-Path $ResolvedRootPath $relative
            if (Test-Path -LiteralPath $candidate) {
                $paths += Get-Item -LiteralPath $candidate
            }
        }

        return $paths
    }

    return @(Get-ChildItem -LiteralPath $ResolvedInputPath -Recurse -File -Filter *.md)
}

function Get-SchemaPaths {
    param([string]$ResolvedRootPath)

    @(
        (Join-Path $ResolvedRootPath 'artifact-id-policy.json')
        (Join-Path $ResolvedRootPath 'schemas/artifact-frontmatter.schema.json')
        (Join-Path $ResolvedRootPath 'schemas/artifact-id-policy.schema.json')
        (Join-Path $ResolvedRootPath 'schemas/evidence-snapshot.schema.json')
        (Join-Path $ResolvedRootPath 'schemas/requirement-clause.schema.json')
        (Join-Path $ResolvedRootPath 'schemas/requirement-trace-fields.schema.json')
        (Join-Path $ResolvedRootPath 'schemas/work-item-trace-fields.schema.json')
    )
}

function Get-EvidenceSnapshotFiles {
    param(
        [string]$ResolvedRootPath,
        [string]$ResolvedInputPath
    )

    $searchPath = if ($ResolvedInputPath -eq $ResolvedRootPath) { $ResolvedRootPath } else { $ResolvedInputPath }
    return @(Get-ChildItem -LiteralPath $searchPath -Recurse -File -Filter *.evidence.json)
}

function Write-JsonReport {
    param(
        [Parameter(Mandatory)]
        [object]$Report,

        [Parameter(Mandatory)]
        [string]$Path
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $directory = Split-Path -Parent $fullPath
    if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $Report | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $fullPath -Encoding utf8
}

$resolvedRootPath = Resolve-RepositoryPath -BasePath (Get-Location).Path -Path $RootPath
if ([string]::IsNullOrWhiteSpace($InputPath)) {
    $InputPath = $resolvedRootPath
}

$resolvedInputPath = Resolve-RepositoryPath -BasePath $resolvedRootPath -Path $InputPath
$findings = [System.Collections.Generic.List[object]]::new()
$artifacts = @{}
$requirements = @{}
$schemaObjects = @{}
$specGroupingCodes = @{}
$markdownFiles = Get-ValidationMarkdownFiles -ResolvedRootPath $resolvedRootPath -ResolvedInputPath $resolvedInputPath | Sort-Object FullName

foreach ($schemaPath in (Get-SchemaPaths -ResolvedRootPath $resolvedRootPath)) {
    $schemaLabel = [System.IO.Path]::GetRelativePath($resolvedRootPath, $schemaPath).Replace('\', '/')
    if (-not (Test-Path -LiteralPath $schemaPath)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-MISSING' -File $schemaLabel -Message "missing required schema/support file."
        continue
    }

    $schemaContent = Get-Content -Raw -LiteralPath $schemaPath
    if ([string]::IsNullOrWhiteSpace($schemaContent)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-EMPTY' -File $schemaLabel -Message "schema/support file is empty."
        continue
    }

    try {
        $schemaObjects[$schemaPath] = $schemaContent | ConvertFrom-Json -Depth 64
    }
    catch {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-JSON' -File $schemaLabel -Message "invalid JSON: $($_.Exception.Message)"
    }
}

$policyPath = Join-Path $resolvedRootPath 'artifact-id-policy.json'
if ($schemaObjects.ContainsKey($policyPath)) {
    $policy = $schemaObjects[$policyPath]

    if ($policy.artifact_id_templates.specification -ne 'SPEC-{domain}{grouping}') {
        Add-Finding -Findings $findings -Severity 'error' -Code 'POLICY-TEMPLATE' -File 'artifact-id-policy.json' -Message "specification template must remain terminal-free as SPEC-{domain}{grouping}."
    }
    if ($policy.artifact_id_templates.architecture -ne 'ARC-{domain}{grouping}-{sequence}') {
        Add-Finding -Findings $findings -Severity 'error' -Code 'POLICY-TEMPLATE' -File 'artifact-id-policy.json' -Message "architecture template must remain sequence-bearing as ARC-{domain}{grouping}-{sequence}."
    }
    if ($policy.artifact_id_templates.work_item -ne 'WI-{domain}{grouping}-{sequence}') {
        Add-Finding -Findings $findings -Severity 'error' -Code 'POLICY-TEMPLATE' -File 'artifact-id-policy.json' -Message "work-item template must remain sequence-bearing as WI-{domain}{grouping}-{sequence}."
    }
    if ($policy.artifact_id_templates.verification -ne 'VER-{domain}{grouping}-{sequence}') {
        Add-Finding -Findings $findings -Severity 'error' -Code 'POLICY-TEMPLATE' -File 'artifact-id-policy.json' -Message "verification template must remain sequence-bearing as VER-{domain}{grouping}-{sequence}."
    }
    if ($policy.requirement_id_template -ne 'REQ-{domain}{grouping}-{sequence}') {
        Add-Finding -Findings $findings -Severity 'error' -Code 'POLICY-TEMPLATE' -File 'artifact-id-policy.json' -Message "requirement template must remain REQ-{domain}{grouping}-{sequence}."
    }

    $registryCodes = @($policy.grouping_key_registry | ForEach-Object { $_.code })
    if ($registryCodes.Count -ne @($registryCodes | Select-Object -Unique).Count) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'POLICY-REGISTRY' -File 'artifact-id-policy.json' -Message "grouping-key registry contains duplicate codes."
    }

    foreach ($code in $registryCodes) {
        $specGroupingCodes[$code] = $true
    }
}

if ($schemaObjects.ContainsKey((Join-Path $resolvedRootPath 'schemas/artifact-frontmatter.schema.json'))) {
    $schema = $schemaObjects[(Join-Path $resolvedRootPath 'schemas/artifact-frontmatter.schema.json')]
    $schemaFile = 'schemas/artifact-frontmatter.schema.json'
    $expectedTypes = @('specification', 'architecture', 'work_item', 'verification')
    $actualTypes = @($schema.oneOf | ForEach-Object { $_.properties.artifact_type.const })

    if ($actualTypes.Count -ne $expectedTypes.Count -or (Compare-Object -ReferenceObject $expectedTypes -DifferenceObject $actualTypes)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-FRONTMATTER' -File $schemaFile -Message "front matter schema must target the four core artifact families only."
    }

    foreach ($branch in @($schema.oneOf)) {
        $patternNames = @($branch.patternProperties.PSObject.Properties.Name)
        if ($patternNames -notcontains '^x_[a-z][a-z0-9_]*$') {
            Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-FRONTMATTER' -File $schemaFile -Message "front matter schema must allow namespaced x_ extension keys."
        }
        if ($branch.additionalProperties -ne $false) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-FRONTMATTER' -File $schemaFile -Message "front matter schema must reject non-canonical additional properties."
        }
    }
}

if ($schemaObjects.ContainsKey((Join-Path $resolvedRootPath 'schemas/requirement-clause.schema.json'))) {
    $schema = $schemaObjects[(Join-Path $resolvedRootPath 'schemas/requirement-clause.schema.json')]
    $schemaFile = 'schemas/requirement-clause.schema.json'
    $expectedKeywords = @('MUST', 'MUST NOT', 'SHALL', 'SHALL NOT', 'SHOULD', 'SHOULD NOT', 'MAY')
    $actualKeywords = @($schema.properties.normative_keyword.enum)

    if ($actualKeywords.Count -ne $expectedKeywords.Count -or (Compare-Object -ReferenceObject $expectedKeywords -DifferenceObject $actualKeywords)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-CLAUSE' -File $schemaFile -Message "normative keyword enum is not aligned with the approved closed keyword set."
    }

    $clausePatternJson = $schema.properties.clause.allOf | ConvertTo-Json -Depth 8
    if ($clausePatternJson -notmatch 'SHOULD NOT') {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-CLAUSE' -File $schemaFile -Message "clause pattern logic must recognize SHOULD NOT as an approved keyword phrase."
    }
}

if ($schemaObjects.ContainsKey((Join-Path $resolvedRootPath 'schemas/requirement-trace-fields.schema.json'))) {
    $schema = $schemaObjects[(Join-Path $resolvedRootPath 'schemas/requirement-trace-fields.schema.json')]
    $schemaFile = 'schemas/requirement-trace-fields.schema.json'
    $expectedTraceKeys = @('Satisfied By', 'Implemented By', 'Verified By', 'Derived From', 'Supersedes', 'Upstream Refs', 'Related')
    $actualTraceKeys = @($schema.properties.PSObject.Properties.Name)
    if ($actualTraceKeys.Count -ne $expectedTraceKeys.Count -or (Compare-Object -ReferenceObject $expectedTraceKeys -DifferenceObject $actualTraceKeys)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-TRACE' -File $schemaFile -Message "requirement trace schema must expose the canonical label set only."
    }
}

if ($schemaObjects.ContainsKey((Join-Path $resolvedRootPath 'schemas/evidence-snapshot.schema.json'))) {
    $schema = $schemaObjects[(Join-Path $resolvedRootPath 'schemas/evidence-snapshot.schema.json')]
    $schemaFile = 'schemas/evidence-snapshot.schema.json'
    $expectedTopLevelKeys = @('snapshot_id', 'generated_at', 'producer', 'requirements')
    $actualTopLevelKeys = @($schema.properties.PSObject.Properties.Name)
    if ($actualTopLevelKeys.Count -ne $expectedTopLevelKeys.Count -or (Compare-Object -ReferenceObject $expectedTopLevelKeys -DifferenceObject $actualTopLevelKeys)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-EVIDENCE' -File $schemaFile -Message "evidence snapshot schema must expose the canonical top-level key set only."
    }

    $actualStatuses = @($schema.'$defs'.statusToken.enum)
    if ($actualStatuses.Count -ne $EvidenceStatusValues.Count -or (Compare-Object -ReferenceObject $EvidenceStatusValues -DifferenceObject $actualStatuses)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-EVIDENCE' -File $schemaFile -Message "evidence snapshot status enum is not aligned with the canonical evidence statuses."
    }

    if ($schema.'$defs'.kindToken.pattern -ne $EvidenceKindPattern) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-EVIDENCE' -File $schemaFile -Message "evidence snapshot kind token pattern is not aligned with the canonical lowercase evidence-kind format."
    }
}

if ($schemaObjects.ContainsKey((Join-Path $resolvedRootPath 'schemas/work-item-trace-fields.schema.json'))) {
    $schema = $schemaObjects[(Join-Path $resolvedRootPath 'schemas/work-item-trace-fields.schema.json')]
    $schemaFile = 'schemas/work-item-trace-fields.schema.json'
    $expectedTraceKeys = @('Addresses', 'Uses Design', 'Verified By')
    $actualTraceKeys = @($schema.properties.PSObject.Properties.Name)
    if ($actualTraceKeys.Count -ne $expectedTraceKeys.Count -or (Compare-Object -ReferenceObject $expectedTraceKeys -DifferenceObject $actualTraceKeys)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'SCHEMA-WORKITEM-TRACE' -File $schemaFile -Message "work-item trace schema must expose the canonical label set only."
    }
}

foreach ($file in $markdownFiles) {
    $relativePath = [System.IO.Path]::GetRelativePath($resolvedRootPath, $file.FullName).Replace('\', '/')
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
    if ($artifactType -notin $ArtifactTypes) {
        continue
    }

    $artifactId = if ($metadata.Contains('artifact_id')) { [string]$metadata['artifact_id'] } else { '' }
    $domain = if ($metadata.Contains('domain')) { [string]$metadata['domain'] } else { '' }
    $status = if ($metadata.Contains('status')) { [string]$metadata['status'] } else { '' }
    $owner = if ($metadata.Contains('owner')) { [string]$metadata['owner'] } else { '' }
    $title = if ($metadata.Contains('title')) { [string]$metadata['title'] } else { '' }
    $artifactNamespace = Get-Namespace $artifactId
    $pathDomain = Get-ExpectedDomainFromPath -RelativePath $relativePath
    $sections = @((Get-MarkdownSections -Body $content).Sections)

    if ([string]::IsNullOrWhiteSpace($artifactId)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'ARTIFACT-ID' -File $relativePath -Message "missing artifact_id in front matter."
        continue
    }

    if ($artifactId -match '[<>]') {
        continue
    }

    if ($artifacts.ContainsKey($artifactId)) {
        Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'duplicate' -Profile $Profile) -Code 'ARTIFACT-DUPLICATE' -File $relativePath -ArtifactId $artifactId -Message "duplicate artifact ID also found in $($artifacts[$artifactId].RelativePath)."
        continue
    }

    if (-not (Test-ArtifactIdFamily -ArtifactId $artifactId -ArtifactType $artifactType)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'ARTIFACT-ID' -File $relativePath -ArtifactId $artifactId -Message "artifact_id does not match the $artifactType identifier pattern."
    }

    if ([string]::IsNullOrWhiteSpace($title)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'FRONTMATTER' -File $relativePath -ArtifactId $artifactId -Message "front matter is missing title."
    }

    if ([string]::IsNullOrWhiteSpace($domain)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'FRONTMATTER' -File $relativePath -ArtifactId $artifactId -Message "front matter is missing domain."
    }

    if ([string]::IsNullOrWhiteSpace($owner)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'FRONTMATTER' -File $relativePath -ArtifactId $artifactId -Message "front matter is missing owner."
    }

    if ($pathDomain -and $domain -ne $pathDomain) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'ARTIFACT-DOMAIN' -File $relativePath -ArtifactId $artifactId -Message "front matter domain '$domain' does not match the directory domain '$pathDomain'."
    }

    foreach ($key in @($metadata.Keys)) {
        if (($key -notmatch '^x_[a-z][a-z0-9_]*$') -and ((Get-AllowedFrontMatterKeys -ArtifactType $artifactType) -notcontains $key)) {
            Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'extension' -Profile $Profile) -Code 'FRONTMATTER-EXTENSION' -File $relativePath -ArtifactId $artifactId -Message "front matter key '$key' is not canonical for '$artifactType'."
        }
    }

    foreach ($requiredKey in (Get-RequiredFrontMatterKeys -ArtifactType $artifactType)) {
        if (-not $metadata.Contains($requiredKey) -or [string]::IsNullOrWhiteSpace([string]$metadata[$requiredKey])) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'FRONTMATTER' -File $relativePath -ArtifactId $artifactId -Message "front matter is missing required key '$requiredKey'."
        }
    }

    if ((Get-StatusValues -ArtifactType $artifactType) -notcontains $status) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'FRONTMATTER' -File $relativePath -ArtifactId $artifactId -Message "status '$status' is not valid for artifact type '$artifactType'."
    }

    $record = [pscustomobject]@{
        RelativePath = $relativePath
        ArtifactId   = $artifactId
        ArtifactType  = $artifactType
        Domain       = $domain
        Namespace    = $artifactNamespace
        Metadata     = $metadata
        Content      = $content
        Sections     = $sections
        Trace        = [ordered]@{}
    }

    $artifacts[$artifactId] = $record

    if ($artifactType -eq 'specification') {
        if ([string]::IsNullOrWhiteSpace([string]$metadata['capability'])) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'FRONTMATTER' -File $relativePath -ArtifactId $artifactId -Message "specification front matter is missing capability."
        }

        if ($relativePath -like 'specs/requirements/spec-trace/SPEC-*.md') {
            $specSegments = @((($artifactId -replace '^SPEC-', '') -split '-'))
            if ($specSegments.Count -ge 1) {
                $registryCode = $specSegments[$specSegments.Count - 1]
                if ($specGroupingCodes.Count -gt 0 -and -not $specGroupingCodes.ContainsKey($registryCode)) {
                    Add-Finding -Findings $findings -Severity 'error' -Code 'SPEC-GROUPING' -File $relativePath -ArtifactId $artifactId -Message "specification grouping code '$registryCode' is not listed in artifact-id-policy.json."
                }
            }
        }

        $requirementCountInFile = 0
        foreach ($section in $sections) {
            $headingParts = Get-RequirementHeadingParts $section.Heading
            if ($null -ne $headingParts) {
                $requirementCountInFile++
                $requirementId = $headingParts.RequirementId
                $requirementNamespace = Get-Namespace $requirementId

                if ($requirements.ContainsKey($requirementId)) {
                    Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'duplicate' -Profile $Profile) -Code 'REQ-DUPLICATE' -File $relativePath -ArtifactId $artifactId -RequirementId $requirementId -Message "duplicate requirement ID also found in $($requirements[$requirementId].RelativePath)."
                    continue
                }

                if ($artifactNamespace -and $requirementNamespace -and $artifactNamespace -ne $requirementNamespace) {
                    Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-NAMESPACE' -File $relativePath -ArtifactId $artifactId -RequirementId $requirementId -Message "requirement namespace '$requirementNamespace' does not align with specification namespace '$artifactNamespace'."
                }

                $requirements[$requirementId] = [pscustomobject]@{
                    RelativePath         = $relativePath
                    ArtifactId           = $artifactId
                    RequirementId        = $requirementId
                    ArtifactNamespace    = $artifactNamespace
                    RequirementNamespace = $requirementNamespace
                    Section              = $section
                    Title                = $headingParts.Title
                    Parsed               = $null
                    Trace                = [ordered]@{}
                }
            }
        }

        if ($requirementCountInFile -eq 0) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-CLAUSE' -File $relativePath -ArtifactId $artifactId -Message "specification must contain at least one REQ section."
        }
    }
}

foreach ($entry in ($requirements.GetEnumerator() | Sort-Object Key)) {
    $requirementId = $entry.Key
    $record = $entry.Value
    $parseErrors = [System.Collections.Generic.List[string]]::new()
    $parsed = Parse-RequirementSection -Section $record.Section -FileLabel $record.RelativePath -SpecNamespace $record.ArtifactNamespace -KnownArtifacts $artifacts -KnownRequirements $requirements -Errors $parseErrors -AllowedTraceLabels $CanonicalTraceLabels -AllowExtensionLabels -SkipReferenceValidation

    foreach ($parseError in $parseErrors) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-CLAUSE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message $parseError
    }

    if ($null -eq $parsed) {
        continue
    }

    $record.Parsed = $parsed
    $trace = $parsed.Trace
    $record.Trace = $trace

    if ($parsed.Clause -notmatch $ApprovedKeywordPattern) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-CLAUSE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "requirement clause must contain exactly one approved normative keyword."
    }
    else {
        $keywordMatches = [regex]::Matches($parsed.Clause, $ApprovedKeywordPattern)
        if ($keywordMatches.Count -ne 1) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-CLAUSE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "requirement clause must contain exactly one approved normative keyword."
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($parsed.NotesText) -and $parsed.NotesText -match $ApprovedKeywordPattern) {
        Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'notes' -Profile $Profile) -Code 'REQ-NOTES' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "Notes contain approved normative keyword language and should remain explanatory only."
    }

    foreach ($label in @($parsed.UnsupportedTraceLabels)) {
        if ($label -notmatch '^x_[a-z][a-z0-9_]*$') {
            Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'extension' -Profile $Profile) -Code 'REQ-EXTENSION' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "trace label '$label' is not canonical and is not namespaced as a local extension."
        }
    }

    $satisfiedBy = @()
    $implementedBy = @()
    $verifiedBy = @()

    if ($trace.Contains('Satisfied By')) {
        $satisfiedBy = Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "requirement '$requirementId' / Satisfied By" -Values $trace['Satisfied By'] -AllowedFamilies @('ARC') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'REQ-TRACE' -ArtifactId $record.ArtifactId -RequirementId $requirementId
    }

    if ($trace.Contains('Implemented By')) {
        $implementedBy = Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "requirement '$requirementId' / Implemented By" -Values $trace['Implemented By'] -AllowedFamilies @('WI') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'REQ-TRACE' -ArtifactId $record.ArtifactId -RequirementId $requirementId
    }

    if ($trace.Contains('Verified By')) {
        $verifiedBy = Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "requirement '$requirementId' / Verified By" -Values $trace['Verified By'] -AllowedFamilies @('VER') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'REQ-TRACE' -ArtifactId $record.ArtifactId -RequirementId $requirementId
    }

    if ($trace.Contains('Derived From')) {
        $derivedFrom = Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "requirement '$requirementId' / Derived From" -Values $trace['Derived From'] -AllowedFamilies @('REQ') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'REQ-TRACE' -ArtifactId $record.ArtifactId -RequirementId $requirementId -AllowUnresolvedRequirement
        $record.Trace['Derived From'] = $derivedFrom
    }

    if ($trace.Contains('Supersedes')) {
        $supersedes = Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "requirement '$requirementId' / Supersedes" -Values $trace['Supersedes'] -AllowedFamilies @('REQ') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'REQ-TRACE' -ArtifactId $record.ArtifactId -RequirementId $requirementId -AllowUnresolvedRequirement
        $record.Trace['Supersedes'] = $supersedes
    }

    if ($trace.Contains('Upstream Refs')) {
        $sourceRefs = @($trace['Upstream Refs'])
        Test-UniqueStrings -Findings $findings -Profile $Profile -Code 'REQ-TRACE' -File $record.RelativePath -Label 'Upstream Refs' -Values $sourceRefs -ArtifactId $record.ArtifactId -RequirementId $requirementId
        $record.Trace['Upstream Refs'] = $sourceRefs
    }

    if ($trace.Contains('Related')) {
        $related = @($trace['Related'])
        Test-UniqueStrings -Findings $findings -Profile $Profile -Code 'REQ-TRACE' -File $record.RelativePath -Label 'Related' -Values $related -ArtifactId $record.ArtifactId -RequirementId $requirementId

        foreach ($reference in $related) {
            if ([string]::IsNullOrWhiteSpace([string]$reference)) {
                Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-TRACE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "Related contains an empty value."
                continue
            }

            $normalizedReference = Get-NormalizedIdentifierText ($reference.ToString())
            $family = Get-ReferenceFamily $normalizedReference
            if ($family -eq 'REQ') {
                if (-not $requirements.ContainsKey($normalizedReference)) {
                    Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'unresolved' -Profile $Profile) -Code 'REQ-TRACE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "Related references unknown requirement '$normalizedReference'."
                }
            }
            elseif ($family -in @('SPEC', 'ARC', 'WI', 'VER')) {
                if (-not $artifacts.ContainsKey($normalizedReference)) {
                    Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'unresolved' -Profile $Profile) -Code 'REQ-TRACE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "Related references unknown artifact '$normalizedReference'."
                }
            }
            else {
                Add-Finding -Findings $findings -Severity 'error' -Code 'REQ-TRACE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "Related value '$normalizedReference' is not a supported identifier."
            }
        }

        $record.Trace['Related'] = $related
    }

    $record.Trace['Satisfied By'] = $satisfiedBy
    $record.Trace['Implemented By'] = $implementedBy
    $record.Trace['Verified By'] = $verifiedBy

    if ((@($satisfiedBy).Count + @($implementedBy).Count + @($verifiedBy).Count) -eq 0) {
        Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'downstream' -Profile $Profile) -Code 'REQ-DOWNSTREAM' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "requirement has no downstream trace links."
    }

    if (@($verifiedBy).Count -eq 0) {
        Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'coverage' -Profile $Profile) -Code 'REQ-COVERAGE' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "requirement has no verification coverage."
    }
}

foreach ($entry in ($artifacts.GetEnumerator() | Sort-Object Key)) {
    $artifactId = $entry.Key
    $record = $entry.Value
    $artifactType = $record.ArtifactType
    $metadata = $record.Metadata
    $sections = $record.Sections

    if ($artifactType -eq 'architecture') {
        $satisfies = @(Get-FrontMatterArray -Metadata $metadata -Key 'satisfies')
        $requirementsSatisfied = $sections | Where-Object { $_.Heading -eq 'Requirements Satisfied' } | Select-Object -First 1
        if ($null -eq $requirementsSatisfied) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'ARCH-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "architecture document is missing a Requirements Satisfied section."
        }
        else {
            Add-SetMismatchFinding -Findings $findings -Code 'ARCH-TRACE' -File $record.RelativePath -ExpectedLabel 'Requirements Satisfied' -ActualLabel 'Requirements Satisfied' -Expected $satisfies -Actual (Parse-BulletList -Content $requirementsSatisfied.Content) -ArtifactId $artifactId
        }
    }

    if ($artifactType -eq 'work_item') {
        $addresses = @(Get-FrontMatterArray -Metadata $metadata -Key 'addresses')
        $designLinks = @(Get-FrontMatterArray -Metadata $metadata -Key 'design_links')
        $verificationLinks = @(Get-FrontMatterArray -Metadata $metadata -Key 'verification_links')
        $requirementsAddressed = $sections | Where-Object { $_.Heading -eq 'Requirements Addressed' } | Select-Object -First 1
        $designInputs = $sections | Where-Object { $_.Heading -eq 'Design Inputs' } | Select-Object -First 1
        $traceLinks = $sections | Where-Object { $_.Heading -eq 'Trace Links' } | Select-Object -First 1

        if ($null -eq $requirementsAddressed) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'WORK-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "work item document is missing a Requirements Addressed section."
        }
        else {
            Add-SetMismatchFinding -Findings $findings -Code 'WORK-TRACE' -File $record.RelativePath -ExpectedLabel 'Requirements Addressed' -ActualLabel 'Requirements Addressed' -Expected $addresses -Actual (Parse-BulletList -Content $requirementsAddressed.Content) -ArtifactId $artifactId
        }

        if ($null -eq $designInputs) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'WORK-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "work item document is missing a Design Inputs section."
        }
        else {
            Add-SetMismatchFinding -Findings $findings -Code 'WORK-TRACE' -File $record.RelativePath -ExpectedLabel 'Design Inputs' -ActualLabel 'Design Inputs' -Expected $designLinks -Actual (Parse-BulletList -Content $designInputs.Content) -ArtifactId $artifactId
        }

        if ($null -eq $traceLinks) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'WORK-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "work item document is missing a Trace Links section."
        }
        else {
            $trace = Parse-LabeledListBlock -Content $traceLinks.Content
            foreach ($label in $CanonicalWorkItemTraceLabels) {
                if (-not $trace.Contains($label)) {
                    Add-Finding -Findings $findings -Severity 'error' -Code 'WORK-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "Trace Links is missing label '$label'."
                }
            }

            foreach ($label in @($trace.Keys)) {
                if (($CanonicalWorkItemTraceLabels -notcontains $label) -and ($label -notmatch '^x_[a-z][a-z0-9_]*$')) {
                    Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'extension' -Profile $Profile) -Code 'WORK-EXTENSION' -File $record.RelativePath -ArtifactId $artifactId -Message "Trace Links label '$label' is not canonical and is not namespaced as a local extension."
                }
            }

            if ($trace.Contains('Addresses')) {
                Add-SetMismatchFinding -Findings $findings -Code 'WORK-TRACE' -File $record.RelativePath -ExpectedLabel 'Addresses' -ActualLabel 'Trace Links / Addresses' -Expected $addresses -Actual $trace['Addresses'] -ArtifactId $artifactId
                Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "work item '$artifactId' / Addresses" -Values $trace['Addresses'] -AllowedFamilies @('REQ') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'WORK-TRACE' -ArtifactId $artifactId | Out-Null
            }

            if ($trace.Contains('Uses Design')) {
                Add-SetMismatchFinding -Findings $findings -Code 'WORK-TRACE' -File $record.RelativePath -ExpectedLabel 'Uses Design' -ActualLabel 'Trace Links / Uses Design' -Expected $designLinks -Actual $trace['Uses Design'] -ArtifactId $artifactId
                Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "work item '$artifactId' / Uses Design" -Values $trace['Uses Design'] -AllowedFamilies @('ARC') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'WORK-TRACE' -ArtifactId $artifactId | Out-Null
            }

            if ($trace.Contains('Verified By')) {
                Add-SetMismatchFinding -Findings $findings -Code 'WORK-TRACE' -File $record.RelativePath -ExpectedLabel 'Verified By' -ActualLabel 'Trace Links / Verified By' -Expected $verificationLinks -Actual $trace['Verified By'] -ArtifactId $artifactId
                Test-ReferenceList -Findings $findings -Profile $Profile -File $record.RelativePath -ContextLabel "work item '$artifactId' / Verified By" -Values $trace['Verified By'] -AllowedFamilies @('VER') -KnownArtifacts $artifacts -KnownRequirements $requirements -Code 'WORK-TRACE' -ArtifactId $artifactId | Out-Null
            }
        }
    }

    if ($artifactType -eq 'verification') {
        $verifies = @(Get-FrontMatterArray -Metadata $metadata -Key 'verifies')
        $requirementsVerified = $sections | Where-Object { $_.Heading -eq 'Requirements Verified' } | Select-Object -First 1
        $statusSection = $sections | Where-Object { $_.Heading -eq 'Status' } | Select-Object -First 1
        $relatedArtifactsSection = $sections | Where-Object { $_.Heading -eq 'Related Artifacts' } | Select-Object -First 1

        if ($null -eq $requirementsVerified) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'VER-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "verification document is missing a Requirements Verified section."
        }
        else {
            Add-SetMismatchFinding -Findings $findings -Code 'VER-TRACE' -File $record.RelativePath -ExpectedLabel 'Requirements Verified' -ActualLabel 'Requirements Verified' -Expected $verifies -Actual (Parse-BulletList -Content $requirementsVerified.Content) -ArtifactId $artifactId
        }

        if ($null -eq $statusSection) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'VER-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "verification document is missing a Status section."
        }
        else {
            $statusText = ($statusSection.Content -split "\r?\n" | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
            if ($null -ne $statusText -and $statusText -ne $metadata['status']) {
                Add-Finding -Findings $findings -Severity 'error' -Code 'VER-TRACE' -File $record.RelativePath -ArtifactId $artifactId -Message "Status section does not reflect the artifact-level status '$($metadata['status'])'."
            }
        }

        if ($null -ne $relatedArtifactsSection -and $metadata.Contains('related_artifacts')) {
            Add-SetMismatchFinding -Findings $findings -Code 'VER-TRACE' -File $record.RelativePath -ExpectedLabel 'Related Artifacts' -ActualLabel 'Related Artifacts' -Expected @(Get-FrontMatterArray -Metadata $metadata -Key 'related_artifacts') -Actual (Parse-BulletList -Content $relatedArtifactsSection.Content) -ArtifactId $artifactId
        }
    }
}

foreach ($entry in ($requirements.GetEnumerator() | Sort-Object Key)) {
    $requirementId = $entry.Key
    $record = $entry.Value
    $trace = if ($null -ne $record.Trace) { $record.Trace } else { [ordered]@{} }

    foreach ($targetId in @($trace['Satisfied By'])) {
        if ($artifacts.ContainsKey($targetId)) {
            $target = $artifacts[$targetId]
            $reciprocal = @(Get-FrontMatterArray -Metadata $target.Metadata -Key 'satisfies')
            if ($reciprocal -notcontains $requirementId) {
                Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'reciprocal' -Profile $Profile) -Code 'REQ-RECIPROCAL' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "architecture '$targetId' does not reciprocate requirement '$requirementId'."
            }
        }
    }

    foreach ($targetId in @($trace['Implemented By'])) {
        if ($artifacts.ContainsKey($targetId)) {
            $target = $artifacts[$targetId]
            $reciprocal = @(Get-FrontMatterArray -Metadata $target.Metadata -Key 'addresses')
            if ($reciprocal -notcontains $requirementId) {
                Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'reciprocal' -Profile $Profile) -Code 'REQ-RECIPROCAL' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "work item '$targetId' does not reciprocate requirement '$requirementId'."
            }
        }
    }

    foreach ($targetId in @($trace['Verified By'])) {
        if ($artifacts.ContainsKey($targetId)) {
            $target = $artifacts[$targetId]
            $reciprocal = @(Get-FrontMatterArray -Metadata $target.Metadata -Key 'verifies')
            if ($reciprocal -notcontains $requirementId) {
                Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'reciprocal' -Profile $Profile) -Code 'REQ-RECIPROCAL' -File $record.RelativePath -ArtifactId $record.ArtifactId -RequirementId $requirementId -Message "verification artifact '$targetId' does not reciprocate requirement '$requirementId'."
            }
        }
    }
}

$referencedArtifacts = @{}
foreach ($record in $requirements.Values) {
    foreach ($targetId in @($record.Trace['Satisfied By'])) { $referencedArtifacts[$targetId] = $true }
    foreach ($targetId in @($record.Trace['Implemented By'])) { $referencedArtifacts[$targetId] = $true }
    foreach ($targetId in @($record.Trace['Verified By'])) { $referencedArtifacts[$targetId] = $true }
}

foreach ($entry in ($artifacts.GetEnumerator() | Sort-Object Key)) {
    $artifactId = $entry.Key
    $record = $entry.Value
    if ($record.ArtifactType -in @('architecture', 'work_item', 'verification') -and -not $referencedArtifacts.ContainsKey($artifactId)) {
        Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'orphan' -Profile $Profile) -Code 'ARTIFACT-ORPHAN' -File $record.RelativePath -ArtifactId $artifactId -Message "artifact has no downstream requirement trace links."
    }
}

$evidenceSnapshots = @()
$evidenceByRequirement = @{}
$evidenceKinds = @{}

foreach ($file in (Get-EvidenceSnapshotFiles -ResolvedRootPath $resolvedRootPath -ResolvedInputPath $resolvedInputPath | Sort-Object FullName)) {
    $relativePath = [System.IO.Path]::GetRelativePath($resolvedRootPath, $file.FullName).Replace('\', '/')
    $content = Get-Content -Raw -LiteralPath $file.FullName
    if ([string]::IsNullOrWhiteSpace($content)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-EMPTY' -File $relativePath -Message "evidence snapshot file is empty."
        continue
    }

    try {
        $snapshot = $content | ConvertFrom-Json -Depth 64
    }
    catch {
        Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-JSON' -File $relativePath -Message "invalid JSON: $($_.Exception.Message)"
        continue
    }

    if ([string]::IsNullOrWhiteSpace([string]$snapshot.snapshot_id)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-SHAPE' -File $relativePath -Message "evidence snapshot is missing snapshot_id."
        continue
    }
    if ([string]::IsNullOrWhiteSpace([string]$snapshot.generated_at)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-SHAPE' -File $relativePath -Message "evidence snapshot '$($snapshot.snapshot_id)' is missing generated_at."
    }
    if ($null -eq $snapshot.producer -or [string]::IsNullOrWhiteSpace([string]$snapshot.producer.name) -or [string]::IsNullOrWhiteSpace([string]$snapshot.producer.version)) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-SHAPE' -File $relativePath -Message "evidence snapshot '$($snapshot.snapshot_id)' must include producer name and version."
    }
    if ($null -eq $snapshot.requirements -or @($snapshot.requirements).Count -eq 0) {
        Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-SHAPE' -File $relativePath -Message "evidence snapshot '$($snapshot.snapshot_id)' must include at least one requirement observation."
        continue
    }

    $seenRequirementIds = @{}
    foreach ($requirementEvidence in @($snapshot.requirements)) {
        $requirementId = [string]$requirementEvidence.requirement_id
        if ([string]::IsNullOrWhiteSpace($requirementId)) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-SHAPE' -File $relativePath -Message "evidence snapshot '$($snapshot.snapshot_id)' contains a requirement observation without requirement_id."
            continue
        }

        if ($seenRequirementIds.ContainsKey($requirementId)) {
            Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'duplicate' -Profile $Profile) -Code 'EVIDENCE-DUPLICATE' -File $relativePath -RequirementId $requirementId -Message "evidence snapshot '$($snapshot.snapshot_id)' contains duplicate requirement observations for '$requirementId'."
        }
        else {
            $seenRequirementIds[$requirementId] = $true
        }

        if (-not $requirements.ContainsKey($requirementId)) {
            Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'unresolved' -Profile $Profile) -Code 'EVIDENCE-TRACE' -File $relativePath -RequirementId $requirementId -Message "evidence snapshot '$($snapshot.snapshot_id)' references unknown requirement '$requirementId'."
        }

        if ($null -eq $requirementEvidence.observations -or @($requirementEvidence.observations).Count -eq 0) {
            Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-SHAPE' -File $relativePath -RequirementId $requirementId -Message "evidence snapshot '$($snapshot.snapshot_id)' must include at least one observation for '$requirementId'."
            continue
        }

        foreach ($observation in @($requirementEvidence.observations)) {
            $kind = [string]$observation.kind
            $status = [string]$observation.status

            if ([string]::IsNullOrWhiteSpace($kind) -or $kind -notmatch $EvidenceKindPattern) {
                Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-KIND' -File $relativePath -RequirementId $requirementId -Message "evidence snapshot '$($snapshot.snapshot_id)' has invalid evidence kind '$kind'."
                continue
            }
            if ($EvidenceStatusValues -notcontains $status) {
                Add-Finding -Findings $findings -Severity 'error' -Code 'EVIDENCE-STATUS' -File $relativePath -RequirementId $requirementId -Message "evidence snapshot '$($snapshot.snapshot_id)' has invalid evidence status '$status' for '$requirementId'."
                continue
            }

            $evidenceKinds[$kind] = $true
            if (-not $evidenceByRequirement.ContainsKey($requirementId)) {
                $evidenceByRequirement[$requirementId] = [ordered]@{}
            }
            if (-not $evidenceByRequirement[$requirementId].Contains($kind)) {
                $evidenceByRequirement[$requirementId][$kind] = [ordered]@{
                    statuses = @()
                    refs = @()
                }
            }

            $bucket = $evidenceByRequirement[$requirementId][$kind]
            $bucket.statuses = @($bucket.statuses + $status | Select-Object -Unique)

            $refs = @($observation.refs | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })
            if ($refs.Count -ne @($refs | Select-Object -Unique).Count) {
                Add-Finding -Findings $findings -Severity (Get-RuleSeverity -Rule 'duplicate' -Profile $Profile) -Code 'EVIDENCE-DUPLICATE' -File $relativePath -RequirementId $requirementId -Message "evidence snapshot '$($snapshot.snapshot_id)' has duplicate refs for '$requirementId' / '$kind'."
            }
            if ($refs.Count -gt 0) {
                $bucket.refs = @($bucket.refs + $refs | Select-Object -Unique)
            }
        }
    }

    $evidenceSnapshots += [ordered]@{
        relative_path = $relativePath
        snapshot_id = [string]$snapshot.snapshot_id
        generated_at = [string]$snapshot.generated_at
        producer = [ordered]@{
            name = [string]$snapshot.producer.name
            version = [string]$snapshot.producer.version
        }
    }
}

$errorCount = @($findings | Where-Object { $_.severity -eq 'error' }).Count
$warningCount = @($findings | Where-Object { $_.severity -eq 'warning' }).Count
$specCount = @($artifacts.Values | Where-Object { $_.ArtifactType -eq 'specification' }).Count
$requirementCount = $requirements.Count
$artifactCount = $artifacts.Count
$markdownCount = $markdownFiles.Count

$coverageDimensions = [ordered]@{
    upstream_refs  = [ordered]@{ present = 0; missing = 0 }
    satisfied_by   = [ordered]@{ present = 0; missing = 0 }
    implemented_by = [ordered]@{ present = 0; missing = 0 }
    verified_by    = [ordered]@{ present = 0; missing = 0 }
}

$evidenceKindDimensions = [ordered]@{}
foreach ($kind in ($evidenceKinds.Keys | Sort-Object)) {
    $statusCounts = [ordered]@{}
    foreach ($status in $EvidenceStatusValues) {
        $statusCounts[$status] = 0
    }
    $evidenceKindDimensions[$kind] = [ordered]@{
        present = 0
        missing = 0
        status_counts = $statusCounts
    }
}

$requirementDimensions = @()
foreach ($entry in ($requirements.GetEnumerator() | Sort-Object Key)) {
    $requirementId = $entry.Key
    $record = $entry.Value
    $trace = if ($null -ne $record.Trace) { $record.Trace } else { [ordered]@{} }
    $evidence = if ($evidenceByRequirement.ContainsKey($requirementId)) { $evidenceByRequirement[$requirementId] } else { [ordered]@{} }

    $hasSourceRefs = @($trace['Upstream Refs']).Count -gt 0
    $hasSatisfiedBy = @($trace['Satisfied By']).Count -gt 0
    $hasImplementedBy = @($trace['Implemented By']).Count -gt 0
    $hasVerifiedBy = @($trace['Verified By']).Count -gt 0

    foreach ($dimension in @(
        @{ name = 'upstream_refs'; present = $hasSourceRefs },
        @{ name = 'satisfied_by'; present = $hasSatisfiedBy },
        @{ name = 'implemented_by'; present = $hasImplementedBy },
        @{ name = 'verified_by'; present = $hasVerifiedBy }
    )) {
        if ($dimension.present) {
            $coverageDimensions[$dimension.name]['present']++
        }
        else {
            $coverageDimensions[$dimension.name]['missing']++
        }
    }

    foreach ($kind in $evidenceKindDimensions.Keys) {
        if ($evidence.Contains($kind)) {
            $evidenceKindDimensions[$kind]['present']++
            foreach ($status in @($evidence[$kind].statuses)) {
                if ($evidenceKindDimensions[$kind].status_counts.Contains($status)) {
                    $evidenceKindDimensions[$kind].status_counts[$status]++
                }
            }
        }
        else {
            $evidenceKindDimensions[$kind]['missing']++
        }
    }

    $requirementDimensions += [ordered]@{
        requirement_id = $requirementId
        upstream_refs = $hasSourceRefs
        satisfied_by = $hasSatisfiedBy
        implemented_by = $hasImplementedBy
        verified_by = $hasVerifiedBy
        evidence_kinds = $evidence
    }
}

$report = [ordered]@{
    root_path = $resolvedRootPath
    input_path = $resolvedInputPath
    profile = $Profile
    success = ($errorCount -eq 0)
    counts = [ordered]@{
        markdown_files = $markdownCount
        evidence_files = $evidenceSnapshots.Count
        artifacts = $artifactCount
        specifications = $specCount
        requirements = $requirementCount
        warnings = $warningCount
        errors = $errorCount
    }
    coverage_dimensions = $coverageDimensions
    evidence_kind_dimensions = $evidenceKindDimensions
    requirement_dimensions = $requirementDimensions
    evidence_snapshots = $evidenceSnapshots
    findings = @($findings)
}

if (-not [string]::IsNullOrWhiteSpace($JsonReportPath)) {
    Write-JsonReport -Report $report -Path $JsonReportPath
}

if ($errorCount -gt 0) {
    Write-Host "SpecTrace validation failed under '$Profile' profile."
    foreach ($finding in $findings) {
        $artifactSuffix = if ([string]::IsNullOrWhiteSpace($finding.artifact_id)) { '' } else { " ($($finding.artifact_id))" }
        $requirementSuffix = if ([string]::IsNullOrWhiteSpace($finding.requirement_id)) { '' } else { " [$($finding.requirement_id)]" }
        Write-Host ($finding.severity.ToUpperInvariant() + ' [' + $finding.code + '] ' + $finding.file + $artifactSuffix + $requirementSuffix + ': ' + $finding.message)
    }
    Write-Host ("Summary: {0} errors, {1} warnings, {2} artifacts, {3} specifications, {4} requirements." -f $errorCount, $warningCount, $artifactCount, $specCount, $requirementCount)
    Write-Host ("Coverage: upstream {0}/{4}, design {1}/{4}, work {2}/{4}, verification {3}/{4}." -f $coverageDimensions.upstream_refs.present, $coverageDimensions.satisfied_by.present, $coverageDimensions.implemented_by.present, $coverageDimensions.verified_by.present, $requirementCount)
    if ($evidenceKindDimensions.Count -gt 0) {
        $evidenceSummary = ($evidenceKindDimensions.GetEnumerator() | Sort-Object Key | ForEach-Object { "{0} {1}/{2}" -f $_.Key, $_.Value.present, $requirementCount }) -join ', '
        Write-Host ("Evidence kinds: {0}." -f $evidenceSummary)
    }
    exit 1
}

if ($warningCount -gt 0) {
    Write-Host "SpecTrace validation passed under '$Profile' profile with $warningCount warning(s)."
    foreach ($finding in ($findings | Where-Object { $_.severity -eq 'warning' })) {
        $artifactSuffix = if ([string]::IsNullOrWhiteSpace($finding.artifact_id)) { '' } else { " ($($finding.artifact_id))" }
        $requirementSuffix = if ([string]::IsNullOrWhiteSpace($finding.requirement_id)) { '' } else { " [$($finding.requirement_id)]" }
        Write-Host ('WARNING [' + $finding.code + '] ' + $finding.file + $artifactSuffix + $requirementSuffix + ': ' + $finding.message)
    }
}
else {
    Write-Host "Validated $artifactCount artifacts across $specCount specifications and $requirementCount requirements."
}

Write-Host ("Coverage: upstream {0}/{4}, design {1}/{4}, work {2}/{4}, verification {3}/{4}." -f $coverageDimensions.upstream_refs.present, $coverageDimensions.satisfied_by.present, $coverageDimensions.implemented_by.present, $coverageDimensions.verified_by.present, $requirementCount)
if ($evidenceKindDimensions.Count -gt 0) {
    $evidenceSummary = ($evidenceKindDimensions.GetEnumerator() | Sort-Object Key | ForEach-Object { "{0} {1}/{2}" -f $_.Key, $_.Value.present, $requirementCount }) -join ', '
    Write-Host ("Evidence kinds: {0}." -f $evidenceSummary)
}
