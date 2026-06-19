<#
.SYNOPSIS
Converts the QUIC RFC text corpus into SpecTrace artifacts.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$SpecTraceRoot,

    [Parameter()]
    [string]$QuicRoot = 'C:\src\incursa\quic-dotnet',

    [Parameter()]
    [string]$PublishCanonicalRoot,

    [Parameter()]
    [string[]]$RfcNumbers,

    [Parameter()]
    [ValidateRange(1, 16)]
    [int]$ThrottleLimit = 2,

    [Parameter()]
    [ValidateRange(1, 300)]
    [int]$PollSeconds = 20,

    [Parameter()]
    [ValidateRange(1, 100)]
    [int]$ExtractBatchSize = 10,

    [Parameter()]
    [ValidateRange(1, 100)]
    [int]$AuditBatchSize = 25,

    [Parameter()]
    [ValidateRange(1, 100)]
    [int]$NormalizeBatchSize = 25,

    [Parameter()]
    [ValidateRange(0, 10)]
    [int]$MaxBatchRetries = 1,

    [Parameter()]
    [ValidateRange(1, 3600)]
    [int]$BatchTimeoutSeconds = 300,

    [Parameter()]
    [ValidateSet('off', 'codex')]
    [string]$AiMode = 'codex',

    [Parameter()]
    [string]$CodexCommand = 'codex',

    [Parameter()]
    [string]$Model = 'gpt-5.4-mini',

    [Parameter()]
    [string]$ReasoningEffort = 'high',

    [Parameter()]
    [string]$RetryReasoningEffort = 'xhigh',

    [Parameter()]
    [ValidateSet('candidate-units', 'functional', 'normative', 'all')]
    [string]$ExtractionScope = 'candidate-units',

    [Parameter()]
    [ValidateSet('off', 'figures')]
    [string]$DeterministicExtraction = 'off',

    [Parameter()]
    [ValidateSet('core', 'traceable', 'auditable')]
    [string]$ValidateProfile = 'core',

    [Parameter()]
    [switch]$PublishCanonical
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

if ([string]::IsNullOrWhiteSpace($SpecTraceRoot)) {
    $SpecTraceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}
else {
    $SpecTraceRoot = (Resolve-Path -LiteralPath $SpecTraceRoot).Path
}

$QuicRoot = (Resolve-Path -LiteralPath $QuicRoot).Path
$PublishCanonicalRoot = if ([string]::IsNullOrWhiteSpace($PublishCanonicalRoot)) {
    $QuicRoot
}
else {
    $resolved = [System.IO.Path]::GetFullPath($PublishCanonicalRoot)
    New-Item -ItemType Directory -Path $resolved -Force | Out-Null
    $resolved
}
$InputRoot = Join-Path $QuicRoot 'specs\rfcs'
$WorkRoot = Join-Path $SpecTraceRoot '.work-rfc-batch'
$PublishRoot = Join-Path $PublishCanonicalRoot 'specs\requirements\quic'
$SpecRfcScript = Join-Path $SpecTraceRoot 'tools\SpecTrace.Rfc\spec-rfc.ps1'

if (-not (Test-Path -LiteralPath $InputRoot)) {
    throw "RFC input folder was not found: $InputRoot"
}

if (-not (Test-Path -LiteralPath $SpecRfcScript)) {
    throw "SpecTrace RFC wrapper was not found: $SpecRfcScript"
}

New-Item -ItemType Directory -Path $WorkRoot -Force | Out-Null

$inputs = Get-ChildItem -LiteralPath $InputRoot -Filter 'rfc*.txt' | Sort-Object Name
$inputs = @($inputs)
if ($RfcNumbers) {
    $wanted = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($value in $RfcNumbers) {
        if ($value -match '(\d+)') {
            [void]$wanted.Add($Matches[1])
        }
    }

    if ($wanted.Count -eq 0) {
        throw "No RFC numbers could be parsed from -RfcNumbers."
    }

    $inputs = $inputs | Where-Object {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
        $name -match '^rfc(?<num>\d+)$' -and $wanted.Contains($Matches.num)
    }
}

$inputs = @($inputs)

if (-not $inputs) {
    throw "No RFC input files were selected from $InputRoot"
}

function Get-RfcNumber {
    param([System.IO.FileInfo]$File)

    if ($File.Name -notmatch '^rfc(?<num>\d+)\.txt$') {
        throw "Input file name must follow rfc####.txt: $($File.Name)"
    }

    return $Matches.num
}

$jobScript = {
    param(
        [string]$SpecRfcScript,
        [string]$QuicRoot,
        [string]$WorkRoot,
        [string]$PublishRoot,
        [string]$InputPath,
        [bool]$PublishCanonical,
        [int]$ExtractBatchSize,
        [int]$AuditBatchSize,
        [int]$NormalizeBatchSize,
        [int]$MaxBatchRetries,
        [int]$BatchTimeoutSeconds,
        [string]$AiMode,
        [string]$CodexCommand,
        [string]$Model,
        [string]$ReasoningEffort,
        [string]$RetryReasoningEffort,
        [string]$ExtractionScope,
        [string]$DeterministicExtraction,
        [string]$ValidateProfile
    )

    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'

    $repoRoot = Split-Path -Path (Split-Path -Path (Split-Path -Path $SpecRfcScript -Parent) -Parent) -Parent

    $inputLeaf = [System.IO.Path]::GetFileName($InputPath)
    if ($inputLeaf -notmatch '^rfc(?<num>\d+)\.txt$') {
        throw "Unexpected RFC input file name: $InputPath"
    }

    $rfcNumber = $Matches.num
    $rfcTag = "RFC$rfcNumber"
    $workDir = Join-Path $WorkRoot "rfc$rfcNumber"
    $rawOutDir = Join-Path $workDir 'raw'
    $batchOutDir = Join-Path $workDir 'batches'
    $sourcePath = Join-Path $workDir 'source.json'
    $ledgerPath = Join-Path $workDir 'source-ledger.jsonl'
    $candidatesPath = Join-Path $workDir 'candidates.jsonl'
    $auditPath = Join-Path $workDir 'review-decisions.jsonl'
    $auditReportPath = Join-Path $workDir 'coverage-audit.md'
    $normalizedPath = Join-Path $workDir 'review-decisions.normalized.jsonl'
    $stablePath = Join-Path $workDir "SPEC-QUIC-RFC$rfcNumber.stable.json"
    $canonicalPath = Join-Path $PublishRoot "SPEC-QUIC-RFC$rfcNumber.json"
    $logPath = Join-Path $workDir 'batch.log'

    New-Item -ItemType Directory -Path $workDir -Force | Out-Null
    New-Item -ItemType Directory -Path $rawOutDir -Force | Out-Null
    New-Item -ItemType Directory -Path $batchOutDir -Force | Out-Null
    Set-Content -LiteralPath $logPath -Value @()

    function Write-Log {
        param([string]$Message)

        Add-Content -LiteralPath $logPath -Value ("[{0}] {1}" -f $rfcTag, $Message)
    }

    function Invoke-SpecRfcStep {
        param(
            [string]$Name,
            [string[]]$Arguments
        )

        Write-Log "$Name start"
        & $SpecRfcScript @Arguments 2>&1 | ForEach-Object {
            Add-Content -LiteralPath $logPath -Value ("[{0}] {1}" -f $rfcTag, $_)
        }

        if ($LASTEXITCODE -ne 0) {
            throw "$Name failed with exit code $LASTEXITCODE"
        }

        Write-Log "$Name done"
    }

    $result = [ordered]@{
        RfcNumber       = $rfcNumber
        InputPath       = $InputPath
        WorkDir         = $workDir
        StablePath      = $stablePath
        CanonicalPath   = $null
        Status          = 'failed'
        RequirementCount = $null
        LogPath         = $logPath
        Error           = $null
    }

    try {
        Write-Log "input $InputPath"
        Invoke-SpecRfcStep 'ingest' @(
            'ingest'
            '--source', $InputPath
            '--out', $sourcePath
        )

        Invoke-SpecRfcStep 'segment' @(
            'segment'
            '--source', $sourcePath
            '--out', $ledgerPath
        )

        Invoke-SpecRfcStep 'extract' @(
            'extract'
            '--ledger', $ledgerPath
            '--out', $candidatesPath
            '--extraction-scope', $ExtractionScope
            '--deterministic-extraction', $DeterministicExtraction
            '--ai-mode', $AiMode
            '--batch-size', $ExtractBatchSize.ToString()
            '--adaptive-min-batch-size', '1'
            '--max-batch-retries', $MaxBatchRetries.ToString()
            '--batch-timeout-seconds', $BatchTimeoutSeconds.ToString()
            '--model', $Model
            '--reasoning-effort', $ReasoningEffort
            '--retry-reasoning-effort', $RetryReasoningEffort
            '--raw-out-dir', $rawOutDir
            '--batch-out-dir', $batchOutDir
            '--resume'
            '--codex', $CodexCommand
        )

        Invoke-SpecRfcStep 'coverage-audit' @(
            'coverage-audit'
            '--ledger', $ledgerPath
            '--candidates', $candidatesPath
            '--out', $auditPath
            '--report-out', $auditReportPath
            '--ai-mode', $AiMode
            '--batch-size', $AuditBatchSize.ToString()
            '--adaptive-min-batch-size', '1'
            '--max-batch-retries', $MaxBatchRetries.ToString()
            '--batch-timeout-seconds', $BatchTimeoutSeconds.ToString()
            '--model', $Model
            '--reasoning-effort', $ReasoningEffort
            '--retry-reasoning-effort', $RetryReasoningEffort
            '--raw-out-dir', $rawOutDir
            '--batch-out-dir', $batchOutDir
            '--resume'
            '--codex', $CodexCommand
        )

        Invoke-SpecRfcStep 'normalize' @(
            'normalize'
            '--ledger', $ledgerPath
            '--review', $auditPath
            '--out', $normalizedPath
            '--ai-mode', $AiMode
            '--batch-size', $NormalizeBatchSize.ToString()
            '--adaptive-min-batch-size', '1'
            '--max-batch-retries', $MaxBatchRetries.ToString()
            '--batch-timeout-seconds', $BatchTimeoutSeconds.ToString()
            '--model', $Model
            '--reasoning-effort', $ReasoningEffort
            '--retry-reasoning-effort', $RetryReasoningEffort
            '--raw-out-dir', $rawOutDir
            '--batch-out-dir', $batchOutDir
            '--resume'
            '--codex', $CodexCommand
        )

        Invoke-SpecRfcStep 'assemble' @(
            'assemble'
            '--ledger', $ledgerPath
            '--review', $normalizedPath
            '--spec-id', "SPEC-QUIC-RFC$rfcNumber"
            '--domain', 'quic'
            '--capability', "quic-rfc$rfcNumber"
            '--title', "QUIC RFC $rfcNumber Requirements"
            '--owner', 'protocol-team'
            '--purpose', "Capture QUIC RFC $rfcNumber requirements."
            '--id-style', 'namespace'
            '--out', $stablePath
        )

        if ($PublishCanonical) {
            Copy-Item -LiteralPath $stablePath -Destination $canonicalPath -Force
            Write-Log "published $canonicalPath"
        }

        $validationRoot = $repoRoot
        $validationPath = if ($PublishCanonical) { $canonicalPath } else { $stablePath }
        Invoke-SpecRfcStep 'validate' @(
            'validate'
            '--root', $validationRoot
            '--input-path', $validationPath
            '--profile', $ValidateProfile
        )

        $requirementCount = ((Get-Content -LiteralPath $stablePath -Raw | ConvertFrom-Json).requirements).Count
        $result.Status = 'success'
        $result.CanonicalPath = if ($PublishCanonical) { $canonicalPath } else { $null }
        $result.RequirementCount = $requirementCount
        Write-Log "complete $requirementCount requirement(s)"
    }
    catch {
        $result.Error = $_.Exception.Message
        Write-Log "failed $($result.Error)"
    }

    [pscustomobject]$result
}

$pending = [System.Collections.Generic.Queue[System.IO.FileInfo]]::new()
foreach ($input in $inputs) {
    $pending.Enqueue($input)
}

$active = [System.Collections.Generic.List[object]]::new()
$results = [System.Collections.Generic.List[object]]::new()
$seenLineCounts = @{}

function Start-RfcJob {
    param([System.IO.FileInfo]$InputFile)

    $rfcNumber = Get-RfcNumber -File $InputFile
    $workDir = Join-Path $WorkRoot "rfc$rfcNumber"
    $logPath = Join-Path $workDir 'batch.log'

    $job = Start-Job -Name "RFC$rfcNumber" -ScriptBlock $jobScript -ArgumentList @(
        $SpecRfcScript
        $QuicRoot
        $WorkRoot
        $PublishRoot
        $InputFile.FullName
        [bool]$PublishCanonical
        $ExtractBatchSize
        $AuditBatchSize
        $NormalizeBatchSize
        $MaxBatchRetries
        $BatchTimeoutSeconds
        $AiMode
        $CodexCommand
        $Model
        $ReasoningEffort
        $RetryReasoningEffort
        $ExtractionScope
        $DeterministicExtraction
        $ValidateProfile
    )

    $active.Add([pscustomobject]@{
        Job     = $job
        Rfc     = $rfcNumber
        Input   = $InputFile.FullName
        LogPath = $logPath
    }) | Out-Null
    $seenLineCounts[$job.Id] = 0
}

function Flush-JobLogs {
    foreach ($item in $active) {
        if (-not (Test-Path -LiteralPath $item.LogPath)) {
            continue
        }

        $lines = Get-Content -LiteralPath $item.LogPath
        $seen = $seenLineCounts[$item.Job.Id]
        for ($index = $seen; $index -lt $lines.Count; $index++) {
            Write-Host $lines[$index]
        }

        $seenLineCounts[$item.Job.Id] = $lines.Count
    }
}

function Remove-CompletedJobs {
    $completed = @($active | Where-Object { $_.Job.State -in @('Completed', 'Failed', 'Stopped') })
    foreach ($item in $completed) {
        $output = Receive-Job -Job $item.Job -ErrorAction SilentlyContinue
        if ($null -ne $output) {
            foreach ($record in @($output)) {
                $results.Add($record) | Out-Null
            }
        }

        Remove-Job -Job $item.Job -Force
        [void]$active.Remove($item)
        $seenLineCounts.Remove($item.Job.Id) | Out-Null
    }
}

Write-Host "SpecTrace root: $SpecTraceRoot"
Write-Host "QUIC root: $QuicRoot"
Write-Host "Inputs: $(@($inputs).Count)"
Write-Host "Work root: $WorkRoot"
if ($PublishCanonical) {
    New-Item -ItemType Directory -Path $PublishRoot -Force | Out-Null
    Write-Host "Canonical output root: $PublishRoot"
}

while ($pending.Count -gt 0 -or $active.Count -gt 0) {
    while ($pending.Count -gt 0 -and $active.Count -lt $ThrottleLimit) {
        Start-RfcJob -InputFile $pending.Dequeue()
        $last = $active[$active.Count - 1]
        Write-Host "Queued RFC $($last.Rfc) -> $($last.Input)"
    }

    Flush-JobLogs
    Remove-CompletedJobs

    $running = @($active | Where-Object { $_.Job.State -eq 'Running' }).Count
    $queued = $pending.Count
    $done = $results.Count
    $failed = @($results | Where-Object { $_.Status -eq 'failed' }).Count
    Write-Host ("Progress: queued={0} running={1} done={2} failed={3}" -f $queued, $running, $done, $failed)

    if ($pending.Count -gt 0 -or $active.Count -gt 0) {
        Start-Sleep -Seconds $PollSeconds
    }
}

$sortedResults = $results | Sort-Object RfcNumber
$summaryPath = Join-Path $WorkRoot 'batch-summary.json'
$sortedResults | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath

Write-Host ""
Write-Host "Batch complete."
Write-Host "Summary: $summaryPath"
$sortedResults | Format-Table RfcNumber, Status, RequirementCount, StablePath, CanonicalPath -AutoSize
