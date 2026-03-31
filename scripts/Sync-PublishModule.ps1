<#
.SYNOPSIS
Synchronizes the reusable CUE publish submodule from the canonical root sources.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path
$publishRoot = Join-Path $resolvedRoot 'publish'
New-Item -ItemType Directory -Force -Path $publishRoot | Out-Null

$copyMap = @(
    @{ Source = 'LICENSE'; Destination = 'LICENSE' },
    @{ Source = 'model\model.cue'; Destination = 'model\model.cue' },
    @{ Source = 'model\README.md'; Destination = 'model\README.md' },
    @{ Source = 'spec-template.cue'; Destination = 'spec-template.cue' },
    @{ Source = 'architecture-template.cue'; Destination = 'architecture-template.cue' },
    @{ Source = 'work-item-template.cue'; Destination = 'work-item-template.cue' },
    @{ Source = 'verification-template.cue'; Destination = 'verification-template.cue' }
)

$allowedPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($relativePath in @(
    'README.md',
    'cue.mod\module.cue'
)) {
    [void]$allowedPaths.Add(($relativePath -replace '[\\/]', '\'))
}

foreach ($entry in $copyMap) {
    $sourcePath = Join-Path $resolvedRoot $entry.Source
    $destinationPath = Join-Path $publishRoot $entry.Destination
    $destinationDirectory = Split-Path -Parent $destinationPath

    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Source file '$sourcePath' was not found."
    }

    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
    Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
    [void]$allowedPaths.Add(($entry.Destination -replace '[\\/]', '\'))
}

# Keep publish/ as a curated mirror so stale tracked files cannot leak into releases.
foreach ($existingFile in Get-ChildItem -LiteralPath $publishRoot -File -Recurse) {
    $relativePath = ([System.IO.Path]::GetRelativePath($publishRoot, $existingFile.FullName) -replace '[\\/]', '\')
    if (-not $allowedPaths.Contains($relativePath)) {
        Remove-Item -LiteralPath $existingFile.FullName -Force
    }
}

foreach ($directory in Get-ChildItem -LiteralPath $publishRoot -Directory -Recurse | Sort-Object FullName -Descending) {
    if (-not (Get-ChildItem -LiteralPath $directory.FullName -Force | Select-Object -First 1)) {
        Remove-Item -LiteralPath $directory.FullName -Force
    }
}

Write-Output $publishRoot
