<#
.SYNOPSIS
Validates canonical SpecTrace JSON artifacts.
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

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path

$arguments = @(
    'run',
    '--project',
    (Join-Path $resolvedRoot 'src\SpecTrace.Tool\SpecTrace.Tool.csproj'),
    '--',
    'validate',
    '--root',
    $resolvedRoot,
    '--profile',
    $Profile
)

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments += @('--input-path', $InputPath)
}

if (-not [string]::IsNullOrWhiteSpace($JsonReportPath)) {
    $arguments += @('--json-report', $JsonReportPath)
}

& dotnet @arguments
exit $LASTEXITCODE
