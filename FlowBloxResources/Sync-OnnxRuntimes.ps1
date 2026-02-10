#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Info($msg)  { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn($msg)  { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err ($msg)  { Write-Host "[ERR ] $msg" -ForegroundColor Red }

function Ensure-Directory([string]$path) {
    if (-not (Test-Path $path)) { New-Item -ItemType Directory -Path $path -Force | Out-Null }
}

function Get-RepositoryRootPaths {
    # Script runs in FlowBloxResources
    $root = Resolve-Path -LiteralPath $PSScriptRoot
    $csproj = Join-Path $root "..\FlowBlox.Core\FlowBlox.Core.csproj"
    $csproj = Resolve-Path -LiteralPath $csproj -ErrorAction SilentlyContinue
    if (-not $csproj) {
        throw "Could not find FlowBlox.Core.csproj at '..\FlowBlox.Core\FlowBlox.Core.csproj' relative to '$root'."
    }
    return @{
        Root      = $root.Path
        Csproj    = $csproj.Path
        DataDir   = (Join-Path $root.Path "data")
        OrtOut    = (Join-Path $root.Path "data\onnxruntimes")
        GenAiOut  = (Join-Path $root.Path "data\onnxruntimesgenai")
    }
}

function Get-PackageVersionFromCsproj([string]$csprojPath, [string]$packageId) {
    [xml]$xml = Get-Content -LiteralPath $csprojPath -Raw

    # Handle common forms:
    # 1) <PackageReference Include="X" Version="1.2.3" />
    # 2) <PackageReference Include="X"><Version>1.2.3</Version></PackageReference>
    # 3) Central package mgmt (Directory.Packages.props) -> not handled (warn)

    $nsMgr = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $nsMgr.AddNamespace("msb", $xml.DocumentElement.NamespaceURI)

    $nodes = $xml.SelectNodes("//msb:PackageReference[@Include='$packageId' or @Update='$packageId']", $nsMgr)
    if (-not $nodes -or $nodes.Count -eq 0) {
        # Try without namespace (some csproj have none)
        $nodes = $xml.SelectNodes("//PackageReference[@Include='$packageId' or @Update='$packageId']")
    }
    if (-not $nodes -or $nodes.Count -eq 0) { return $null }

    foreach ($n in $nodes) {
        if ($n.Version -and $n.Version.Trim().Length -gt 0) { return $n.Version.Trim() }
        $v = $n.SelectSingleNode("msb:Version", $nsMgr)
        if (-not $v) { $v = $n.SelectSingleNode("Version") }
        if ($v -and $v.InnerText.Trim().Length -gt 0) { return $v.InnerText.Trim() }
    }
    return $null
}

function Get-NuGetGlobalPackagesFolder {
    # Default global packages folder:
    $default = Join-Path $env:USERPROFILE ".nuget\packages"
    if (Test-Path $default) { return $default }

    # Fallback: ask dotnet
    try {
        $out = & dotnet nuget locals global-packages -l 2>$null
        # typical: "global-packages: C:\Users\...\ .nuget\packages\"
        $m = [regex]::Match($out, "global-packages:\s*(.+)$")
        if ($m.Success) {
            $p = $m.Groups[1].Value.Trim()
            if (Test-Path $p) { return $p }
        }
    } catch { }

    throw "Could not determine NuGet global packages folder."
}

function Test-PackageInCache([string]$nugetRoot, [string]$packageId, [string]$version) {
    $pkgPath = Join-Path $nugetRoot (Join-Path $packageId.ToLowerInvariant() $version)
    return (Test-Path $pkgPath)
}

function Ensure-PackagesInCache([string]$csprojDir, [hashtable[]]$packages) {
    # Create a temp csproj that references all required packages and dotnet restore it.
    # This will populate the NuGet global cache without touching your real project refs.
    $tmpDir = Join-Path $csprojDir ".tmp_restore_runtimes"
    Ensure-Directory $tmpDir

    $tmpProj = Join-Path $tmpDir "RestoreRuntimes.csproj"

    $refs = $packages | ForEach-Object {
        "<PackageReference Include=""$($_.Id)"" Version=""$($_.Version)"" />"
    } | Out-String

    $projXml = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RestoreNoCache>true</RestoreNoCache>
    <RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources>
    <DisableImplicitNuGetFallbackFolder>true</DisableImplicitNuGetFallbackFolder>
  </PropertyGroup>
  <ItemGroup>
$refs
  </ItemGroup>
</Project>
"@

    Set-Content -LiteralPath $tmpProj -Value $projXml -Encoding UTF8

    Write-Info "Restoring helper project to populate NuGet cache..."
    Write-Info "  $tmpProj"

    # Restore
    $p = Start-Process -FilePath "dotnet" -ArgumentList @("restore", $tmpProj, "-v", "minimal") -NoNewWindow -PassThru -Wait
    if ($p.ExitCode -ne 0) {
        Write-Warn "dotnet restore returned exit code $($p.ExitCode). Some packages may still be missing."
    }

    # Cleanup project file (keep folder for troubleshooting if needed)
    try { Remove-Item -LiteralPath $tmpProj -Force -ErrorAction SilentlyContinue } catch { }
}

function Copy-RuntimeNative([string]$nugetRoot, [string]$packageId, [string]$version, [string]$rid, [string]$destDir) {
    $pkgBase = Join-Path $nugetRoot (Join-Path $packageId.ToLowerInvariant() $version)
    if (-not (Test-Path $pkgBase)) {
        Write-Warn "Package not found in cache: $packageId $version"
        return $false
    }

    $src = Join-Path $pkgBase (Join-Path "runtimes\$rid" "native")
    if (-not (Test-Path $src)) {
        Write-Warn "RID/native not found: $packageId $version -> runtimes\$rid\native"
        return $false
    }

    Ensure-Directory $destDir

    # Enumerate source items explicitly (works reliably; no wildcard + LiteralPath pitfalls)
	$items = @(Get-ChildItem -LiteralPath $src -Force -ErrorAction Stop)
	if ($items.Length -eq 0) {
		Write-Warn "Source folder is empty: $src"
		return $false
	}

	Write-Info "Copy: $packageId $version [$rid] -> $destDir"
	foreach ($it in $items) {
		Copy-Item -LiteralPath $it.FullName -Destination $destDir -Force -Recurse -ErrorAction Stop
	}

    return $true
}

function Write-Readme([string]$dataDir) {
    $readmePath = Join-Path $dataDir "README.txt"
    $txt = @"
FlowBlox Native ONNX Runtime Bundling
====================================

This folder contains native runtime binaries copied from the local NuGet Global Packages cache.

Why?
-----
FlowBlox deploys selected ONNX Runtime / ONNX Runtime GenAI native binaries (CPU/GPU/DirectML/OpenVINO)
independently from NuGet to support dynamic runtime/provider loading.

When to run this script?
------------------------
Run Sync-OnnxRuntimes.ps1 whenever you change the managed NuGet versions in:
  ../FlowBlox.Core/FlowBlox.Core.csproj

The script reads the managed versions:
  - Microsoft.ML.OnnxRuntime
  - Microsoft.ML.OnnxRuntimeGenAI

Then it:
  1) Ensures the corresponding runtime/provider packages are present in the NuGet global cache
     (via a temporary 'dotnet restore').
  2) Copies files from:
       %USERPROFILE%\.nuget\packages\<package>\<version>\runtimes\<rid>\native\*
     into:
       data\onnxruntimes\...
       data\onnxruntimesgenai\...

Notes
-----
- If a provider package or RID is not available, the script prints a warning and continues.
- GenAI GPU package names may vary between releases; warnings will show what is missing.

"@
    Set-Content -LiteralPath $readmePath -Value $txt -Encoding UTF8
    Write-Info "Wrote README: $readmePath"
}

function Sync-OrtAndGenAiRuntimes {
    $paths = Get-RepositoryRootPaths
    $csprojPath = $paths.Csproj
    $csprojDir  = Split-Path -Parent $csprojPath

    Write-Info "Using csproj: $csprojPath"

    $ortVer   = Get-PackageVersionFromCsproj $csprojPath "Microsoft.ML.OnnxRuntime.Managed"
	$genaiVer = Get-PackageVersionFromCsproj $csprojPath "Microsoft.ML.OnnxRuntimeGenAI.Managed"

    if (-not $ortVer)   { throw "Could not find PackageReference version for Microsoft.ML.OnnxRuntime in csproj." }
    if (-not $genaiVer) { throw "Could not find PackageReference version for Microsoft.ML.OnnxRuntimeGenAI in csproj." }

    Write-Info "Detected versions:"
    Write-Info "  ONNX Runtime     : $ortVer"
    Write-Info "  ONNX Runtime GenAI: $genaiVer"

    $nugetRoot = Get-NuGetGlobalPackagesFolder
    Write-Info "NuGet global cache: $nugetRoot"

    # --- Define required packages based on managed versions
    # ORT provider packages (same version as managed ORT)
    $ortPackages = @(
        @{ Id = "Microsoft.ML.OnnxRuntime";             	Version = $ortVer },
        @{ Id = "Microsoft.ML.OnnxRuntime.Gpu.Windows";     Version = $ortVer },
		@{ Id = "Microsoft.ML.OnnxRuntime.Gpu.Linux";     	Version = $ortVer },
        @{ Id = "Microsoft.ML.OnnxRuntime.DirectML";    	Version = $ortVer },
        @{ Id = "Microsoft.ML.OnnxRuntime.OpenVINO";    	Version = $ortVer }
    )

    # GenAI packages (same version as managed GenAI)
    # CUDA package naming can vary; we try common candidates later for copying.
    $genaiPackages = @(
        @{ Id = "Microsoft.ML.OnnxRuntimeGenAI";            Version = $genaiVer },
        @{ Id = "Microsoft.ML.OnnxRuntimeGenAI.DirectML";   Version = $genaiVer },
        @{ Id = "Microsoft.ML.OnnxRuntimeGenAI.Cuda";       Version = $genaiVer }
    )

    # --- Ensure all packages exist in cache; if not, restore once.
    $missing = @()
    foreach ($p in ($ortPackages + $genaiPackages)) {
        if (-not (Test-PackageInCache $nugetRoot $p.Id $p.Version)) {
            $missing += $p
        }
    }

    if ($missing.Count -gt 0) {
        Write-Warn "Some packages are missing in NuGet global cache. Running 'dotnet restore' helper..."
        $missing | ForEach-Object { Write-Warn "  missing: $($_.Id) $($_.Version)" }
        Ensure-PackagesInCache $csprojDir ($ortPackages + $genaiPackages)
    } else {
        Write-Info "All required packages already present in NuGet cache."
    }

    # --- Create output dirs
    Ensure-Directory $paths.DataDir
    Ensure-Directory $paths.OrtOut
    Ensure-Directory $paths.GenAiOut

    # --- Copy maps (RID -> destination)
    $ridMapCpu = @(
        @{ Rid = "win-x64";    Dest = "cpu\win-x64" },
        @{ Rid = "win-arm64";  Dest = "cpu\win-arm64" },
        @{ Rid = "linux-x64";  Dest = "cpu\linux-x64" },
        @{ Rid = "linux-arm64";Dest = "cpu\linux-arm64" },
        @{ Rid = "osx-x64";    Dest = "cpu\osx-x64" },
        @{ Rid = "osx-arm64";  Dest = "cpu\osx-arm64" }
    )

    $ridMapWin = @(
        @{ Rid = "win-x64";    Dest = "win-x64" },
        @{ Rid = "win-arm64";  Dest = "win-arm64" }
    )

    $ridMapGpu = @(
        @{ Rid = "win-x64";    Dest = "win-x64" },
        @{ Rid = "linux-x64";  Dest = "linux-x64" }
    )

    # --- Copy ONNX Runtime
    Write-Info "=== Copying ONNX Runtime native runtimes ==="
    $anyCopied = $false

    foreach ($m in $ridMapCpu) {
        $dest = Join-Path $paths.OrtOut $m.Dest
        $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntime" $ortVer $m.Rid $dest) -or $anyCopied
    }

    foreach ($m in $ridMapWin) {
        $dest = Join-Path (Join-Path $paths.OrtOut "directml") $m.Dest
        $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntime.DirectML" $ortVer $m.Rid $dest) -or $anyCopied
    }

    foreach ($m in $ridMapGpu) {
        $destWin = Join-Path (Join-Path $paths.OrtOut "gpu-windows") "win-x64"
        $destLin = Join-Path (Join-Path $paths.OrtOut "gpu-linux") "linux-x64"
        if ($m.Rid -eq "win-x64")   { $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntime.Gpu.Windows" $ortVer $m.Rid $destWin) -or $anyCopied }
        if ($m.Rid -eq "linux-x64") { $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntime.Gpu.Linux" $ortVer $m.Rid $destLin) -or $anyCopied }
    }

    # OpenVINO (commonly win-x64 only)
    $destOv = Join-Path (Join-Path $paths.OrtOut "openvino") "win-x64"
    $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntime.OpenVINO" $ortVer "win-x64" $destOv) -or $anyCopied

    # --- Copy GenAI
    Write-Info "=== Copying ONNX Runtime GenAI native runtimes ==="
    foreach ($m in $ridMapCpu) {
        $dest = Join-Path $paths.GenAiOut $m.Dest
        $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntimeGenAI" $genaiVer $m.Rid $dest) -or $anyCopied
    }

    foreach ($m in $ridMapWin) {
        $dest = Join-Path (Join-Path $paths.GenAiOut "directml") $m.Dest
        $anyCopied = (Copy-RuntimeNative $nugetRoot "Microsoft.ML.OnnxRuntimeGenAI.DirectML" $genaiVer $m.Rid $dest) -or $anyCopied
    }

    # GenAI CUDA: try common candidates
    $cudaCandidates = @(
        "Microsoft.ML.OnnxRuntimeGenAI.Cuda",
        "Microsoft.ML.OnnxRuntimeGenAI.Gpu",
        "Microsoft.ML.OnnxRuntimeGenAI.CUDA" # just in case casing differs
    )

    foreach ($cand in $cudaCandidates) {
        $pkgExists = Test-PackageInCache $nugetRoot $cand $genaiVer
        if ($pkgExists) {
            $destCudaWin = Join-Path (Join-Path $paths.GenAiOut "cuda") "win-x64"
            $destCudaLin = Join-Path (Join-Path $paths.GenAiOut "cuda") "linux-x64"
            $anyCopied = (Copy-RuntimeNative $nugetRoot $cand $genaiVer "win-x64"   $destCudaWin) -or $anyCopied
            $anyCopied = (Copy-RuntimeNative $nugetRoot $cand $genaiVer "linux-x64" $destCudaLin) -or $anyCopied
            break
        }
    }

    if (-not $anyCopied) {
        Write-Warn "No files were copied. Check package versions and availability in NuGet cache."
    }

    Write-Readme $paths.DataDir
    Write-Info "Done."
}

# Entry
Sync-OrtAndGenAiRuntimes
