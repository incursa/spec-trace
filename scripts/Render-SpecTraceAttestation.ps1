<#
.SYNOPSIS
Generates a derived SpecTrace attestation snapshot as HTML and JSON.
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
    [ValidateSet('html', 'json', 'both')]
    [string]$Emit = 'both',

    [Parameter()]
    [string]$OutDir = 'artifacts/spec-trace/attestation',

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
    'generate-attestation',
    '--root',
    $resolvedRoot,
    '--profile',
    $Profile,
    '--emit',
    $Emit,
    '--out-dir',
    $OutDir
)

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments += @('--input-path', $InputPath)
}

foreach ($path in $EvidencePath) {
    if (-not [string]::IsNullOrWhiteSpace($path)) {
        $arguments += @('--evidence-path', $path)
    }
}

& dotnet @arguments
exit $LASTEXITCODE
