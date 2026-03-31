<#
.SYNOPSIS
Compatibility wrapper for core SpecTrace validation.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$InputPath,

    [Parameter()]
    [string]$JsonReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$arguments = @{
    RootPath = $RootPath
    Profile = 'core'
}

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments['InputPath'] = $InputPath
}

if (-not [string]::IsNullOrWhiteSpace($JsonReportPath)) {
    $arguments['JsonReportPath'] = $JsonReportPath
}

& (Join-Path $PSScriptRoot 'Test-SpecTraceRepository.ps1') @arguments
exit $LASTEXITCODE
