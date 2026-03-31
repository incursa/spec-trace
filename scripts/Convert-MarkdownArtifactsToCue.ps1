<#
.SYNOPSIS
Migrates canonical SpecTrace Markdown artifacts to CUE.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$InputPath,

    [switch]$Force
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
    'migrate-markdown',
    '--root',
    $resolvedRoot
)

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments += @('--input-path', $InputPath)
}

& dotnet @arguments
exit $LASTEXITCODE
