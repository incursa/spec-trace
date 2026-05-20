<#
.SYNOPSIS
Resolves a SpecTrace topic-view selection into a machine-readable JSON result.
#>
[CmdletBinding(DefaultParameterSetName = 'Path')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Path')]
    [string]$TopicViewPath,

    [Parameter(Mandatory, ParameterSetName = 'Json')]
    [string]$TopicViewJson,

    [Parameter()]
    [string]$RootPath = (Get-Location).Path,

    [Parameter()]
    [string]$InputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path

$arguments = @(
    'run',
    '--project',
    (Join-Path $resolvedRoot 'src\SpecTrace.Tool\SpecTrace.Tool.csproj'),
    '--',
    'resolve-topic-view',
    '--root',
    $resolvedRoot
)

if (-not [string]::IsNullOrWhiteSpace($InputPath)) {
    $arguments += @('--input-path', $InputPath)
}

if ($PSCmdlet.ParameterSetName -eq 'Path') {
    $resolvedTopicViewPath = if ([System.IO.Path]::IsPathRooted($TopicViewPath)) {
        (Resolve-Path -LiteralPath $TopicViewPath).Path
    }
    else {
        (Resolve-Path -LiteralPath (Join-Path $resolvedRoot $TopicViewPath)).Path
    }

    $arguments += @('--topic-view-path', $resolvedTopicViewPath)
}
else {
    $arguments += @('--topic-view-json', $TopicViewJson)
}

& dotnet @arguments
exit $LASTEXITCODE
