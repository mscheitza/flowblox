param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$InstallerProjectPath = "FlowBlox.Installer\FlowBlox.Installer.wixproj",
    [string]$FlowBloxProjectPath = "FlowBlox\FlowBlox.csproj",
    [string]$PublishDir = "artifacts\installer\publish\win-x64",
    [string]$MsiOutputDir = "artifacts\installer\msi",
    [switch]$NoRestore,
    [switch]$NoPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path

$flowBloxProject = Join-Path $repoRoot $FlowBloxProjectPath
$installerProject = Join-Path $repoRoot $InstallerProjectPath
$publishDirResolved = Join-Path $repoRoot $PublishDir
$msiOutputDirResolved = Join-Path $repoRoot $MsiOutputDir

if (-not (Test-Path $flowBloxProject)) {
    throw "FlowBlox project not found: $flowBloxProject"
}

if (-not (Test-Path $installerProject)) {
    throw "Installer project not found: $installerProject"
}

[xml]$flowBloxXml = Get-Content $flowBloxProject
$assemblyVersionNode = $flowBloxXml.SelectSingleNode("/Project/PropertyGroup/AssemblyVersion")
$versionNode = $flowBloxXml.SelectSingleNode("/Project/PropertyGroup/Version")

$productVersion = if ($assemblyVersionNode -and -not [string]::IsNullOrWhiteSpace($assemblyVersionNode.InnerText)) {
    $assemblyVersionNode.InnerText
}
elseif ($versionNode -and -not [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
    "$($versionNode.InnerText).0"
}
else {
    "1.0.0.0"
}

if (-not $NoPublish) {
    Write-Host "Publishing FlowBlox for MSI payload..."
    $publishArgs = @(
        "publish", $flowBloxProject,
        "-c", $Configuration,
        "-r", $RuntimeIdentifier,
        "--self-contained", "false",
        "-o", $publishDirResolved
    )

    if ($NoRestore) {
        $publishArgs += "--no-restore"
    }

    Write-Host "dotnet $($publishArgs -join ' ')"
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }
}

if (-not (Test-Path $publishDirResolved)) {
    throw "Publish directory not found: $publishDirResolved"
}

if (-not (Test-Path $msiOutputDirResolved)) {
    New-Item -Path $msiOutputDirResolved -ItemType Directory | Out-Null
}

$outputName = "FlowBlox_${productVersion}_x64"

Write-Host "Building MSI via WiX..."
$buildArgs = @(
    "build", $installerProject,
    "-c", $Configuration,
    "/p:ProductVersion=$productVersion",
    "/p:SourceDir=$publishDirResolved",
    "/p:OutputPath=$msiOutputDirResolved\\",
    "/p:OutputName=$outputName"
)

if ($NoRestore) {
    $buildArgs += "--no-restore"
}

Write-Host "dotnet $($buildArgs -join ' ')"
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build (WiX) failed with exit code $LASTEXITCODE."
}

$expectedMsi = Join-Path $msiOutputDirResolved "$outputName.msi"
Write-Host ""
Write-Host "MSI build completed."
Write-Host "Version: $productVersion"
Write-Host "Output:  $expectedMsi"

