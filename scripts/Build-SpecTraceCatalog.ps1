<#
.SYNOPSIS
Builds a repository-wide SpecTrace catalog from canonical JSON artifacts.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$InputPath,

    [Parameter()]
    [string]$JsonOutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path

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

& dotnet @arguments
exit $LASTEXITCODE
