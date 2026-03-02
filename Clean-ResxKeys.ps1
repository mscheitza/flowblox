<#
.SYNOPSIS
  Cleans up unused TEXT resources in .resx files by scanning .cs and .xaml files.

.DESCRIPTION
  - Collects ONLY textual <data> nodes from .resx (has <value>, no @type, no @mimetype)
  - Checks usage by iterating every key against cached file contents.
  - Usage patterns:
      A) Property access: Something.Key  OR  prefix:Something.Key (XAML)
      B) Quoted key: "Key" (and also 'Key')
      C) Split key: "Part1" ... "Part2" ... "Part3" (only if key contains underscores)

  Ignores:
   - non-text data nodes (files/blobs/icons)
   - only the auto-generated resx Designer.cs files corresponding to the scanned resx files

.NOTES
  Run without -Apply first, review CSV, then run with -Apply -Backup.
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)] [string]$ResxRoot,
  [Parameter(Mandatory=$true)] [string]$CodeRoot,

  [switch]$Apply,
  [switch]$Backup,

  [string]$ReportPath = (Join-Path (Get-Location) "resx-cleanup-report.csv"),

  [int]$MaxGap = 250,

  # 0 = unlimited. Limits how many TEXT keys are checked for usage (useful for quick tests).
  [int]$MaxTextKeysToCheck = 0,

  # If set, writes plain text lists of cached code file paths (one per cache group).
  [string]$CacheReportPath = (Join-Path (Get-Location) "resx-cleanup-cache-files.txt"),

  [switch]$PreviewPerFile,
  [switch]$OnlyWithDesigner,

  [string[]]$ExcludeDirPatterns = @("bin", "obj", ".git", ".vs", "packages", "node_modules")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ----------------- helpers -----------------

function Escape-Regex([string]$s) { [Regex]::Escape($s) }

function Get-FilesFiltered([string]$root, [string[]]$include, [string[]]$excludePatterns) {
  $files = Get-ChildItem -Path $root -Recurse -File -Include $include -ErrorAction SilentlyContinue
  if (-not $files) { return @() }

  $excludeRegex = if ($excludePatterns -and $excludePatterns.Count -gt 0) {
    ($excludePatterns | ForEach-Object { [Regex]::Escape($_) }) -join "|"
  } else { $null }

  if ($excludeRegex) {
    return $files | Where-Object { $_.FullName -notmatch "([\\/])($excludeRegex)([\\/])" }
  }
  return $files
}

function Read-AllTextSafe([string]$path) {
  try {
    return [IO.File]::ReadAllText($path, [Text.Encoding]::UTF8)
  } catch {
    return Get-Content -Path $path -Raw -ErrorAction SilentlyContinue
  }
}

function Normalize-FullPath([string]$p) {
  if (-not $p) { return $null }
  return ([IO.Path]::GetFullPath($p).TrimEnd('\','/'))
}

function StartsWithPath([string]$fullPath, [string]$prefixPath) {
  if (-not $fullPath -or -not $prefixPath) { return $false }
  $fp = Normalize-FullPath $fullPath
  $pp = Normalize-FullPath $prefixPath
  return $fp.StartsWith($pp, [StringComparison]::OrdinalIgnoreCase)
}

function Cache-CodeFiles([System.IO.FileInfo[]]$files) {
  $cache = @{}
  $notCached = New-Object System.Collections.Generic.List[string]

  foreach ($cf in $files) {
    $t = Read-AllTextSafe $cf.FullName
    if ($t) { $cache[$cf.FullName] = $t }
    else { $notCached.Add($cf.FullName) | Out-Null }
  }

  return [pscustomobject]@{
    Cache = $cache
    NotCached = $notCached
  }
}

function Write-CacheReport([string]$path, [hashtable]$cache, [System.Collections.Generic.List[string]]$notCached) {
  try {
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("CACHED FILES: $($cache.Count)") | Out-Null
    $lines.Add("NOT CACHED (empty/unreadable): $($notCached.Count)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("=== CACHED ===") | Out-Null
    foreach ($p in ($cache.Keys | Sort-Object)) { $lines.Add($p) | Out-Null }
    $lines.Add("") | Out-Null
    $lines.Add("=== NOT CACHED ===") | Out-Null
    foreach ($p in ($notCached | Sort-Object)) { $lines.Add($p) | Out-Null }

    $lines | Set-Content -Path $path -Encoding UTF8
    Write-Host "Cache report written: $path"
  } catch {
    Write-Host "WARN: Could not write cache report: $($_.Exception.Message)"
  }
}

function Is-TextResxDataNode($node) {
  if ($null -eq $node) { return $false }
  $hasValue = ($node.SelectSingleNode("value") -ne $null)
  $hasType  = $node.HasAttribute("type")
  $hasMime  = $node.HasAttribute("mimetype")
  return ($hasValue -and -not $hasType -and -not $hasMime)
}

function Extract-TextResxKeys([string]$resxPath) {
  [xml]$xml = Get-Content -Path $resxPath -Raw
  $nodes = @($xml.SelectNodes("//data[@name]"))
  foreach ($n in $nodes) {
    if (Is-TextResxDataNode $n) {
      $name = $n.GetAttribute("name")
      if ($name) { $name }
    }
  }
}

function Get-ResxDesignerPath([IO.FileInfo]$resxFile) {
  # Handles culture suffix: Resources.de.resx -> Resources.Designer.cs
  $base = $resxFile.BaseName
  if ($base -match '^(?<root>.+)\.[a-z]{2}(-[A-Z]{2})?$') {
    $base = $Matches['root']
  }
  return Join-Path $resxFile.DirectoryName ($base + ".Designer.cs")
}

function Has-MatchingDesigner([IO.FileInfo]$resxFile) {
  $designer = Get-ResxDesignerPath $resxFile
  return ($designer -and (Test-Path -LiteralPath $designer))
}

function Build-SplitRegex([string]$key, [int]$maxGap) {
  $parts = $key -split "_"
  if ($parts.Count -lt 2) { return $null }
  $gap = ".{0,$maxGap}"
  $quotedParts = $parts | ForEach-Object { '"' + (Escape-Regex $_) + '"' }
  return ($quotedParts -join $gap)
}

function ContainsInvariant([string]$haystack, [string]$needle) {
  if ([string]::IsNullOrEmpty($haystack) -or [string]::IsNullOrEmpty($needle)) { return $false }
  return ($haystack.IndexOf($needle, [StringComparison]::Ordinal) -ge 0)
}

function Build-KeyMatchers([string]$key, [int]$maxGap) {
  $kEsc = Escape-Regex $key

  # Property access:
  #   Something.Key
  #   prefix:Something.Key
  $rxProp = [regex]("(?<![A-Za-z0-9_])(?:[A-Za-z_][A-Za-z0-9_]*:)?[A-Za-z_][A-Za-z0-9_]*\." + $kEsc + "(?![A-Za-z0-9_])")

  $rxSplit = $null
  if ($key.Contains("_")) {
    $splitPattern = Build-SplitRegex -key $key -maxGap $maxGap
    if ($splitPattern) { $rxSplit = [regex]$splitPattern }
  }

  return [pscustomobject]@{
    Key = $key
    QuotedDbl = '"' + $key + '"'
    QuotedSgl = "'" + $key + "'"
    RxProp = $rxProp
    RxSplit = $rxSplit
  }
}

function Is-KeyUsedInTextWithMatchers($m, [string]$text) {
  if (-not $text) { return $false }

  if (ContainsInvariant $text $m.QuotedDbl) { return $true }
  if (ContainsInvariant $text $m.QuotedSgl) { return $true }
  if ($m.RxProp.IsMatch($text)) { return $true }
  if ($null -ne $m.RxSplit -and $m.RxSplit.IsMatch($text)) { return $true }

  $cm = [regex]::Match($text, '(?m)^\s*(public|internal|private|protected)?\s*(partial\s+)?class\s+(?<n>[A-Za-z_][A-Za-z0-9_]*)\b')
  if ($cm.Success) {
    $className = $cm.Groups['n'].Value
    if ($className) {
      $prefix = $className + "_"
      if ($m.Key.StartsWith($prefix, [StringComparison]::Ordinal)) {
        $alias = $m.Key.Substring($prefix.Length)
        $aliasQuotedDbl = '"' + $alias + '"'
        $aliasQuotedSgl = "'" + $alias + "'"

        if (ContainsInvariant $text $aliasQuotedDbl) { return $true }
        if (ContainsInvariant $text $aliasQuotedSgl) { return $true }
      }
    }
  }

  return $false
}

function Is-StronglyTypedResxDesigner([string]$path) {
  if (-not (Test-Path -LiteralPath $path)) { return $false }

  $t = Read-AllTextSafe $path
  if (-not $t) { return $false }

  if ($t -match '\bResourceManager\b' -and $t -match '\bGetString\b') { return $true }
  return $false
}

# ----------------- collect resx -----------------

$resxFiles = Get-FilesFiltered -root $ResxRoot -include @("*.resx") -excludePatterns $ExcludeDirPatterns
if ($OnlyWithDesigner) { $resxFiles = @($resxFiles | Where-Object { Has-MatchingDesigner $_ }) }

if (-not $resxFiles -or @($resxFiles).Count -eq 0) {
  throw "No .resx files found under: $ResxRoot (after filtering)."
}

Write-Host "Resx files found: $(@($resxFiles).Count)"

$allTextKeys = New-Object 'System.Collections.Generic.HashSet[string]'
$resxKeyToFiles = @{}
$resxFileToTextKeys = @{}

foreach ($rf in $resxFiles) {
  $keys = @(Extract-TextResxKeys -resxPath $rf.FullName)
  $resxFileToTextKeys[$rf.FullName] = $keys

  foreach ($k in $keys) {
    [void]$allTextKeys.Add($k)
    if (-not $resxKeyToFiles.ContainsKey($k)) {
      $resxKeyToFiles[$k] = New-Object System.Collections.Generic.List[string]
    }
    $resxKeyToFiles[$k].Add($rf.FullName) | Out-Null
  }
}

Write-Host "Unique TEXT resx keys found: $($allTextKeys.Count)"
Write-Host "(Non-text resources like blobs/files/icons are ignored and will NOT be removed.)"

# ----------------- scan & cache code -----------------

$codeFiles = Get-FilesFiltered -root $CodeRoot -include @("*.cs","*.xaml") -excludePatterns $ExcludeDirPatterns

if (-not $codeFiles -or @($codeFiles).Count -eq 0) {
  throw "No .cs/.xaml files found under: $CodeRoot (after filtering)."
}

Write-Host "Code files found (before excluding resx designer files): $(@($codeFiles).Count)"

# Exclude only the auto-generated strongly typed resx designer files (computed from resx list)
$designerPathsToExclude = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)

foreach ($rf in $resxFiles) {
  $p = Get-ResxDesignerPath $rf
  if ($p -and (Test-Path -LiteralPath $p)) {
    if (Is-StronglyTypedResxDesigner $p) {
      [void]$designerPathsToExclude.Add([IO.Path]::GetFullPath($p))
    }
  }
}

if ($designerPathsToExclude.Count -gt 0) {
  $codeFiles = @($codeFiles | Where-Object {
    -not $designerPathsToExclude.Contains([IO.Path]::GetFullPath($_.FullName))
  })
}

Write-Host "Excluded resx designer files from scan: $($designerPathsToExclude.Count)"
Write-Host "Code files after exclusion: $(@($codeFiles).Count)"

if (-not $codeFiles -or @($codeFiles).Count -eq 0) {
  throw "No .cs/.xaml files left to scan after excluding resx designer files. Check CodeRoot/ResxRoot or exclusion logic."
}

Write-Host "Caching code files (grouped)..."

$root = Normalize-FullPath $CodeRoot

$flowBloxRoot = Normalize-FullPath (Join-Path $root "FlowBlox")
$uiCoreRoot   = Normalize-FullPath (Join-Path $root "FlowBlox.UICore")
$uiCoreResx   = Normalize-FullPath (Join-Path $uiCoreRoot "Resources")

$codeFilesFlowBlox = @($codeFiles | Where-Object { StartsWithPath $_.FullName $flowBloxRoot })
$codeFilesUICore   = @($codeFiles | Where-Object { StartsWithPath $_.FullName $uiCoreRoot })
$codeFilesGlobal   = $codeFiles

Write-Host "Code files in FlowBlox:        $(@($codeFilesFlowBlox).Count)"
Write-Host "Code files in FlowBlox.UICore: $(@($codeFilesUICore).Count)"
Write-Host "Code files global:             $(@($codeFilesGlobal).Count)"

$cacheFlowBlox = Cache-CodeFiles -files $codeFilesFlowBlox
$cacheUICore   = Cache-CodeFiles -files $codeFilesUICore
$cacheGlobal   = Cache-CodeFiles -files $codeFilesGlobal

Write-Host "Cached texts (FlowBlox):        $($cacheFlowBlox.Cache.Count)"
Write-Host "Cached texts (FlowBlox.UICore): $($cacheUICore.Cache.Count)"
Write-Host "Cached texts (Global):          $($cacheGlobal.Cache.Count)"

Write-CacheReport -path ($CacheReportPath -replace '\.txt$', '.FlowBlox.txt') -cache $cacheFlowBlox.Cache -notCached $cacheFlowBlox.NotCached
Write-CacheReport -path ($CacheReportPath -replace '\.txt$', '.UICore.txt')   -cache $cacheUICore.Cache   -notCached $cacheUICore.NotCached
Write-CacheReport -path ($CacheReportPath -replace '\.txt$', '.Global.txt')   -cache $cacheGlobal.Cache   -notCached $cacheGlobal.NotCached

# ----------------- usage detection -----------------

$keyMatchers = @{}
foreach ($k in $allTextKeys) {
  $keyMatchers[$k] = Build-KeyMatchers -key $k -maxGap $MaxGap
}

$usedKeys = New-Object 'System.Collections.Generic.HashSet[string]'

$keysToCheck = @($allTextKeys)
if ($MaxTextKeysToCheck -gt 0 -and $keysToCheck.Count -gt $MaxTextKeysToCheck) {
  $keysToCheck = @(
    $keysToCheck |
    Sort-Object |
    Select-Object -First $MaxTextKeysToCheck
  )
  Write-Host "Limiting usage detection to first $MaxTextKeysToCheck TEXT keys (sorted) for quick testing."
}

$total = $keysToCheck.Count
$current = 0

foreach ($k in $keysToCheck) {
  $current++
  $percent = if ($total -gt 0) { [int](($current / $total) * 100) } else { 100 }

  Write-Progress `
    -Activity "Scanning TEXT resource keys" `
    -Status "$current of $total ($percent%)" `
    -PercentComplete $percent `
    -CurrentOperation $k

  $m = $keyMatchers[$k]

  $filesForKey = @($resxKeyToFiles[$k] | Select-Object -Unique)
  $useCache = $cacheGlobal.Cache

  if ($filesForKey.Count -gt 0) {
    $allInFlowBlox = $true
    $allInUICoreResources = $true

    foreach ($rfPath in $filesForKey) {
      if (-not (StartsWithPath $rfPath $flowBloxRoot)) { $allInFlowBlox = $false }
      if (-not (StartsWithPath $rfPath $uiCoreResx))   { $allInUICoreResources = $false }
    }

    if ($allInFlowBlox) {
      $useCache = $cacheFlowBlox.Cache
    } elseif ($allInUICoreResources) {
      $useCache = $cacheUICore.Cache
    }
  }

  foreach ($kv in $useCache.GetEnumerator()) {
    if (Is-KeyUsedInTextWithMatchers -m $m -text $kv.Value) {
      [void]$usedKeys.Add($k)
      break
    }
  }
}

Write-Progress -Activity "Scanning TEXT resource keys" -Completed

$unusedCount = $keysToCheck.Count - $usedKeys.Count
Write-Host "Checked TEXT keys: $($keysToCheck.Count)"
Write-Host "Used TEXT keys:    $($usedKeys.Count)"
Write-Host "Unused TEXT keys:  $unusedCount"

# ----------------- report -----------------

$report = New-Object System.Collections.Generic.List[object]
foreach ($k in $keysToCheck) {
  $isUsed = $usedKeys.Contains($k)
  $files = ($resxKeyToFiles[$k] | Select-Object -Unique) -join ";"
  $report.Add([pscustomobject]@{
    Key       = $k
    Used      = $isUsed
    ResxFiles = $files
  }) | Out-Null
}

$report | Export-Csv -Path $ReportPath -NoTypeInformation -Encoding UTF8
Write-Host "CSV report written: $ReportPath"

# ----------------- preview per resx -----------------

if ($PreviewPerFile) {
  $unusedCheckedKeys = New-Object 'System.Collections.Generic.HashSet[string]'
  foreach ($k in $keysToCheck) {
    if (-not $usedKeys.Contains($k)) { [void]$unusedCheckedKeys.Add($k) }
  }

  foreach ($rf in $resxFiles) {
    $keys = @($resxFileToTextKeys[$rf.FullName])
    $toDelete = @($keys | Where-Object { $unusedCheckedKeys.Contains($_) })

    if ($toDelete.Count -gt 0) {
      Write-Host ""
      Write-Host "---- PREVIEW: $($rf.FullName) ----"
      $toDelete | Sort-Object | ForEach-Object { Write-Host "  would delete: $_" }
    }
  }
}

if (-not $Apply) {
  Write-Host "Dry-run only. Re-run with -Apply (and ideally -Backup) to actually remove unused TEXT keys."
  return
}

# ----------------- apply cleanup (TEXT nodes only) -----------------

$unusedCheckedKeysForApply = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($k in $keysToCheck) {
  if (-not $usedKeys.Contains($k)) { [void]$unusedCheckedKeysForApply.Add($k) }
}

foreach ($rf in $resxFiles) {
  [xml]$xml = Get-Content -Path $rf.FullName -Raw
  $dataNodes = @($xml.SelectNodes("//data[@name]"))

  $removed = 0
  foreach ($n in $dataNodes) {
    if (-not (Is-TextResxDataNode $n)) { continue }

    $name = $n.GetAttribute("name")
    if ($name -and $unusedCheckedKeysForApply.Contains($name)) {
      [void]$n.ParentNode.RemoveChild($n)
      $removed++
    }
  }

  if ($removed -gt 0) {
    if ($Backup) {
      Copy-Item -Path $rf.FullName -Destination ($rf.FullName + ".bak") -Force
    }
    $settings = New-Object System.Xml.XmlWriterSettings
	$settings.Indent = $true
	$settings.Encoding = New-Object System.Text.UTF8Encoding($true)
	$settings.NewLineHandling = [System.Xml.NewLineHandling]::Entitize

	$writer = [System.Xml.XmlWriter]::Create($rf.FullName, $settings)
	$xml.Save($writer)
	$writer.Close()
    Write-Host "Updated $($rf.Name): removed $removed unused TEXT keys"
  }
}

Write-Host "Done."