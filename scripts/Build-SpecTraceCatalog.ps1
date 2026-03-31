<#
.SYNOPSIS
Builds a repository-wide SpecTrace catalog from canonical CUE artifacts.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$InputPath,

    [Parameter()]
    [string]$JsonOutputPath,

    [Parameter()]
    [string]$CueOutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path
& (Join-Path $PSScriptRoot 'Resolve-Cue.ps1') -RootPath $resolvedRoot | Out-Null

$arguments = @(
    'run',
    '--project',
    (Join-Path $resolvedRoot 'src\SpecTrace.Tool\SpecTrace.Tool.csproj'),
    '--',
    'build-catalog',
    '--root',
    $resolvedRoot
)

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments += @('--input-path', $InputPath)
}

if (-not [string]::IsNullOrWhiteSpace($JsonOutputPath)) {
    $arguments += @('--json-out', $JsonOutputPath)
}

if (-not [string]::IsNullOrWhiteSpace($CueOutputPath)) {
    $arguments += @('--cue-out', $CueOutputPath)
}

& dotnet @arguments
exit $LASTEXITCODE
