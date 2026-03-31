<#
.SYNOPSIS
Validates generated SpecTrace evidence snapshots against the canonical JSON Schema and repository requirement catalog.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string[]]$EvidencePath = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path

$arguments = @(
    'run',
    '--project',
    (Join-Path $resolvedRoot 'src\SpecTrace.Tool\SpecTrace.Tool.csproj'),
    '--',
    'validate-evidence',
    '--root',
    $resolvedRoot
)

foreach ($path in $EvidencePath) {
    if (-not [string]::IsNullOrWhiteSpace($path)) {
        $arguments += @('--evidence-path', $path)
    }
}

& dotnet @arguments
exit $LASTEXITCODE
