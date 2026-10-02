FlowBlox Native ONNX Runtime Bundling
====================================

This README describes the native runtime binaries stored below this folder in:
  data\onnxruntimes\...
  data\onnxruntimesgenai\...

Sync-OnnxRuntimes.ps1 copies these binaries from the local NuGet Global Packages cache.

Why?
-----
FlowBlox deploys selected ONNX Runtime / ONNX Runtime GenAI native binaries (CPU/CUDA/DirectML/OpenVINO)
independently from NuGet to support dynamic runtime/provider loading.

When to run this script?
------------------------
Run Sync-OnnxRuntimes.ps1 whenever you change the managed NuGet versions in:
  ../FlowBlox.Core/FlowBlox.Core.csproj

The script reads the managed versions:
  - Microsoft.ML.OnnxRuntime.Managed
  - Microsoft.ML.OnnxRuntimeGenAI.Managed

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
- The sync fails if a required provider package, RID, or native file set is unavailable.
- All GenAI provider packages must depend on the same ONNX Runtime version selected by FlowBlox.Core.
- Each runtime output folder contains a runtime-manifest.json with the synchronized package and layout versions.
- Matching manifests keep the existing folders and enable incremental copying. A version or layout change rebuilds only the affected runtime output folder.

