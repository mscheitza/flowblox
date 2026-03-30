param(
    [string]$WebRootFlowBlox = "C:\RootDrive\Windows XP Workstation\Public\htdocs\flowbloxweb",
    [string]$AppSubDirectory = "app",
    [string]$MsiPath = "",
    [string]$MsiSourceDir = "artifacts\installer\msi",
    [string]$BaseDownloadUrl = "https://flowblox.net/app/",
    [string]$ManifestFileName = "FlowBloxInstallerUpdates.xml"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-RepoPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        if (Test-Path $Path) {
            return (Resolve-Path $Path).Path
        }

        return [System.IO.Path]::GetFullPath($Path)
    }

    $repoRoot = Split-Path -Parent $PSScriptRoot
    $candidate = Join-Path $repoRoot $Path
    if (Test-Path $candidate) {
        return (Resolve-Path $candidate).Path
    }

    return [System.IO.Path]::GetFullPath($candidate)
}

function Ensure-Directory {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        New-Item -Path $Path -ItemType Directory | Out-Null
    }
}

$webRootFlowBloxResolved = Resolve-RepoPath -Path $WebRootFlowBlox
$appRoot = Join-Path $webRootFlowBloxResolved $AppSubDirectory
Ensure-Directory -Path $appRoot

if ([string]::IsNullOrWhiteSpace($MsiPath)) {
    $msiSourceDirResolved = Resolve-RepoPath -Path $MsiSourceDir
    $selectedMsi = Get-ChildItem -Path $msiSourceDirResolved -Filter "*.msi" -File |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if ($null -eq $selectedMsi) {
        throw "No MSI found in '$msiSourceDirResolved'."
    }

    $msiResolved = $selectedMsi.FullName
}
else {
    $msiResolved = Resolve-RepoPath -Path $MsiPath
}

$msiName = [System.IO.Path]::GetFileName($msiResolved)
if ($msiName -notmatch '^FlowBlox_(?<version>\d+\.\d+\.\d+\.\d+)_x64\.msi$') {
    throw "MSI file name must follow 'FlowBlox_<version>_x64.msi'. Got: $msiName"
}

$version = $Matches['version']
$releaseFolderName = "FlowBlox_${version}_x64"
$releaseFolderPath = Join-Path $appRoot $releaseFolderName
Ensure-Directory -Path $releaseFolderPath

$targetMsiPath = Join-Path $releaseFolderPath $msiName
Copy-Item -Path $msiResolved -Destination $targetMsiPath -Force

$base = $BaseDownloadUrl.TrimEnd('/') + '/'
$installerUrl = "$base$releaseFolderName/$msiName"
$manifestPath = Join-Path $appRoot $ManifestFileName

$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<FlowBloxInstallerUpdate>
  <LatestVersion>$version</LatestVersion>
  <InstallerUrl>$installerUrl</InstallerUrl>
  <PublishedUtc>$( (Get-Date).ToUniversalTime().ToString("o") )</PublishedUtc>
</FlowBloxInstallerUpdate>
"@

Set-Content -Path $manifestPath -Value $xml -Encoding UTF8

Write-Host "Published MSI release:"
Write-Host "  App root:      $appRoot"
Write-Host "  Release folder:$releaseFolderPath"
Write-Host "  MSI:           $targetMsiPath"
Write-Host "  Manifest:      $manifestPath"
Write-Host "  InstallerUrl:  $installerUrl"


