[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Arguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot 'src\SpecTrace.Rfc.Cli\SpecTrace.Rfc.Cli.csproj'
& dotnet run --project $projectPath -- @Arguments
exit $LASTEXITCODE
