<#
.SYNOPSIS
Ensures the pinned CUE CLI used by this repository is available.
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,

    [Parameter()]
    [string]$Version = 'v0.16.0'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path -LiteralPath $RootPath).Path
$toolDirectory = Join-Path $resolvedRoot '.tools\cue\bin'
$downloadDirectory = Join-Path $resolvedRoot '.tools\cue\downloads'
$localCue = Join-Path $toolDirectory $(if ($IsWindows) { 'cue.exe' } else { 'cue' })

function Test-CueVersion {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$ExpectedVersion
    )

    try {
        $versionOutput = & $Path version 2>$null
        return $LASTEXITCODE -eq 0 -and ($versionOutput -match "cue version $([regex]::Escape($ExpectedVersion))")
    }
    catch {
        return $false
    }
}

if (Test-Path -LiteralPath $localCue) {
    if (Test-CueVersion -Path $localCue -ExpectedVersion $Version) {
        Write-Output $localCue
        exit 0
    }
}

$globalCue = Get-Command cue -ErrorAction SilentlyContinue
if ($null -ne $globalCue -and (Test-CueVersion -Path $globalCue.Source -ExpectedVersion $Version)) {
    Write-Output $globalCue.Source
    exit 0
}

function Get-CueReleaseAssetName {
    param(
        [Parameter(Mandatory)]
        [string]$PinnedVersion
    )

    $osToken = if ($IsWindows) {
        'windows'
    }
    elseif ($IsMacOS) {
        'darwin'
    }
    elseif ($IsLinux) {
        'linux'
    }
    else {
        throw "Unsupported operating system for automatic CUE download."
    }

    $architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    $archToken = switch ($architecture) {
        ([System.Runtime.InteropServices.Architecture]::X64) { 'amd64' }
        ([System.Runtime.InteropServices.Architecture]::Arm64) { 'arm64' }
        default { throw "Unsupported architecture '$architecture' for automatic CUE download." }
    }

    $extension = if ($IsWindows) { 'zip' } else { 'tar.gz' }
    return "cue_${PinnedVersion}_${osToken}_${archToken}.${extension}"
}

function Expand-CueArchive {
    param(
        [Parameter(Mandatory)]
        [string]$ArchivePath,

        [Parameter(Mandatory)]
        [string]$DestinationPath
    )

    if ($ArchivePath.EndsWith('.zip', [System.StringComparison]::OrdinalIgnoreCase)) {
        Expand-Archive -LiteralPath $ArchivePath -DestinationPath $DestinationPath -Force
        return
    }

    if ($ArchivePath.EndsWith('.tar.gz', [System.StringComparison]::OrdinalIgnoreCase)) {
        & tar -xzf $ArchivePath -C $DestinationPath
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to extract '$ArchivePath' with tar."
        }

        return
    }

    throw "Unsupported archive format '$ArchivePath'."
}

function Get-RequiredCueBinary {
    param(
        [Parameter(Mandatory)]
        [string]$SearchRoot
    )

    $binaryName = if ($IsWindows) { 'cue.exe' } else { 'cue' }
    $candidate = Get-ChildItem -Path $SearchRoot -Recurse -File -Filter $binaryName |
        Select-Object -First 1

    if ($null -eq $candidate) {
        throw "The downloaded archive did not contain '$binaryName'."
    }

    return $candidate.FullName
}

$assetName = Get-CueReleaseAssetName -PinnedVersion $Version
$headers = @{
    Accept       = 'application/vnd.github+json'
    'User-Agent' = 'incursa-spec-trace'
}
$release = Invoke-RestMethod `
    -Uri "https://api.github.com/repos/cue-lang/cue/releases/tags/$Version" `
    -Headers $headers

$asset = $release.assets |
    Where-Object { $_.name -eq $assetName } |
    Select-Object -First 1

if ($null -eq $asset) {
    throw "Could not find CUE asset '$assetName' in release '$Version'."
}

New-Item -ItemType Directory -Force -Path $toolDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $downloadDirectory | Out-Null

$archivePath = Join-Path $downloadDirectory $asset.name
Invoke-WebRequest `
    -Uri $asset.browser_download_url `
    -Headers @{ 'User-Agent' = 'incursa-spec-trace' } `
    -OutFile $archivePath

$extractFolderName = if ($asset.name.EndsWith('.tar.gz', [System.StringComparison]::OrdinalIgnoreCase)) {
    $asset.name.Substring(0, $asset.name.Length - '.tar.gz'.Length)
}
else {
    [System.IO.Path]::GetFileNameWithoutExtension($asset.name)
}

$extractRoot = Join-Path $downloadDirectory $extractFolderName
if (Test-Path -LiteralPath $extractRoot) {
    Remove-Item -LiteralPath $extractRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
Expand-CueArchive -ArchivePath $archivePath -DestinationPath $extractRoot
$downloadedCue = Get-RequiredCueBinary -SearchRoot $extractRoot
Copy-Item -LiteralPath $downloadedCue -Destination $localCue -Force

if (-not (Test-Path -LiteralPath $localCue)) {
    throw "Expected CUE CLI at '$localCue' after download, but it was not found."
}

if (-not (Test-CueVersion -Path $localCue -ExpectedVersion $Version)) {
    throw "Downloaded CUE CLI at '$localCue' does not report version '$Version'."
}

Write-Output $localCue
