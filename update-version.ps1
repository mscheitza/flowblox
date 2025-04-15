<# 
.SYNOPSIS
  Sets version + metadata centrally for multiple csproj files, and updates the MSIX appxmanifest.
.PARAMETER Version
  SemVer with 3 parts, e.g. "1.0.0". For MSIX, ".0" will be appended.
.PARAMETER Root
  Optional: Root path; default is the script directory.
#>

param(
  [Parameter(Mandatory = $true)]
  [ValidatePattern('^\d+\.\d+\.\d+$')]
  [string]$Version,
  [string]$Root = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

# Fallback for Root (robust for different invocation styles)
if ([string]::IsNullOrWhiteSpace($Root)) { $Root = Split-Path -Parent $MyInvocation.MyCommand.Path }
if ([string]::IsNullOrWhiteSpace($Root)) { $Root = Get-Location }

# ---- Configuration ----
$Projects = @(
  "FlowBlox\FlowBlox.csproj",
  "FlowBlox.Core\FlowBlox.Core.csproj",
  "FlowBlox.UICore\FlowBlox.UICore.csproj",
  "FlowBlox.SequenceDetection\FlowBlox.SequenceDetection.csproj"
)

$AppxManifestPath  = "FlowBlox.Packaging\Package.appxmanifest"
$MsixVersion       = "$Version.0"  # MSIX needs 4 parts

# Metadata for all csproj
$Meta = @{
  "Company"                  = "FlowBlox Community"
  "Product"                  = "FlowBlox"
  "Authors"                  = "Marcel Scheitza and contributors"
  "Copyright"                = "Copyright © 2025 Marcel Scheitza and contributors. Licensed under the MIT License."
  "PackageLicenseExpression" = "MIT"
  "Version"                  = $Version
  "AssemblyVersion"          = $MsixVersion
  "FileVersion"              = $MsixVersion
}

# ---- Helpers ----
function Load-Xml([string]$path) {
  if (-not (Test-Path $path)) { throw ("File not found: {0}" -f $path) }
  [xml]$xml = Get-Content -LiteralPath $path -Raw
  return $xml
}
function Save-Xml([xml]$xml, [string]$path) {
  $settings = New-Object System.Xml.XmlWriterSettings
  $settings.OmitXmlDeclaration = $true
  $settings.Encoding = New-Object System.Text.UTF8Encoding($true)
  $settings.Indent = $true
  $writer = [System.Xml.XmlWriter]::Create($path, $settings)
  $xml.Save($writer)
  $writer.Flush(); $writer.Close()
}
function Get-Or-Create-PropertyGroup([xml]$xml) {
  if (-not $xml) { throw "XML document is null." }

  $project = $xml.DocumentElement
  if (-not $project -or $project.Name -ne 'Project') {
    throw "No <Project> root found."
  }
  
  $pgNodes = $project.SelectNodes("./PropertyGroup")
  $count = if ($pgNodes) { $pgNodes.Count } else { 0 }
  Write-Host ("  Found existing PropertyGroup count: {0}" -f $count) -ForegroundColor Cyan

  if ($count -gt 0) {
    return [System.Xml.XmlElement]$pgNodes.Item(0)
  } else {
    Write-Host "  No PropertyGroup found, creating new one..." -ForegroundColor Magenta
    $pg = $xml.CreateElement("PropertyGroup")
    [void]$project.AppendChild($pg)
    return $pg
  }
}
function Set-Property([xml]$xml, [System.Xml.XmlElement]$pg, [string]$name, [string]$value) {
  $node = $pg.SelectSingleNode($name)
  if (-not $node) {
    Write-Host ("  Creating property <{0}> with value '{1}'" -f $name, $value) -ForegroundColor DarkGreen
    $node = $xml.CreateElement($name)
    [void]$pg.AppendChild($node)
  } else {
    Write-Host ("  Updating property <{0}> from '{1}' to '{2}'" -f $name, $node.InnerText, $value) -ForegroundColor Yellow
  }
  $node.InnerText = $value
}

# ---- Process csproj files ----
$updated = @()
foreach ($rel in $Projects) {
  $path = Join-Path $Root $rel
  try {
    Write-Host ("Updating {0} ..." -f $path) -ForegroundColor Cyan

    $xml = Load-Xml $path
    $pg  = Get-Or-Create-PropertyGroup $xml
    foreach ($k in $Meta.Keys) {
      Set-Property $xml $pg $k $Meta[$k]
    }
    Save-Xml $xml $path
    $updated += $path
  } catch {
    Write-Warning ("Error processing {0}: {1}" -f $path, $_.Exception.Message)
  }
}

# ---- Process appxmanifest: set Package/Identity/@Version ----
$manifestPath = Join-Path $Root $AppxManifestPath
try {
  $xml = Load-Xml $manifestPath
  # Use GetElementsByTagName("Identity") and take the first one
  $identity = $xml.GetElementsByTagName("Identity") | Select-Object -First 1
  if (-not $identity) { throw "Identity element not found in appxmanifest." }

  if ($identity.Attributes["Version"]) {
    $identity.Attributes["Version"].Value = $MsixVersion
  } else {
    $attr = $xml.CreateAttribute("Version")
    $attr.Value = $MsixVersion
    [void]$identity.Attributes.Append($attr)
  }

  Save-Xml $xml $manifestPath
  $updated += $manifestPath
} catch {
  Write-Warning ("Error processing {0}: {1}" -f $manifestPath, $_.Exception.Message)
}

# ---- Result ----
Write-Host "Updated:" -ForegroundColor Green
$updated | ForEach-Object { Write-Host ("  {0}" -f $_) }