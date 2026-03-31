<#
.SYNOPSIS
Renders generated Markdown from canonical CUE artifacts.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$InputPath,

    [switch]$Check
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
    'generate-markdown',
    '--root',
    $resolvedRoot
)

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments += @('--input-path', $InputPath)
}

if ($Check) {
    $arguments += '--check'
}

& dotnet @arguments
exit $LASTEXITCODE
