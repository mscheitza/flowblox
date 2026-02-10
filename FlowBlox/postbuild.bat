@echo off
setlocal EnableExtensions EnableDelayedExpansion

rem ------------------------------------------------------------
rem FlowBlox PostBuild (no labels, normalized paths)
rem Usage:
rem   postbuild.bat "<TargetDir>" ["win-x64"|"win-x86"|"win-arm64"|"linux-x64"]
rem ------------------------------------------------------------

if "%~1"=="" (
  echo ERROR: Missing TargetDir argument.
  echo Usage: %~nx0 "<TargetDir>" ["win-x64"^|"win-x86"^|"win-arm64"^|"linux-x64"]
  exit /b 1
)

set "TARGET_DIR=%~1"
set "RID_OVERRIDE=%~2"

rem Ensure trailing backslash
if not "%TARGET_DIR%\"=="%TARGET_DIR%" set "TARGET_DIR=%TARGET_DIR%\"

rem Normalize TargetDir (remove any .. segments)
for %%I in ("%TARGET_DIR%") do set "TARGET_DIR=%%~fI\"

set "CURRENT_DIR=%~dp0"

rem Compute and normalize repo root (one level above FlowBlox\)
for %%I in ("%CURRENT_DIR%..\") do set "ROOT_DIR=%%~fI\"

set "SRC_ONNX=%ROOT_DIR%FlowBloxResources\data\onnxruntimes"
set "DST_ONNX=%TARGET_DIR%data\onnxruntimes"

set "RID=win-x64"
if not "%RID_OVERRIDE%"=="" set "RID=%RID_OVERRIDE%"

echo.
echo [FlowBlox] PostBuild ONNX Runtime copy
echo   TargetDir : %TARGET_DIR%
echo   RootDir   : %ROOT_DIR%
echo   Source    : %SRC_ONNX%\
echo   Dest      : %DST_ONNX%\
echo   RID       : %RID%
echo.

if not exist "%SRC_ONNX%\" (
  echo [FlowBlox] WARN: Source folder not found: %SRC_ONNX%\
  exit /b 0
)

if not exist "%TARGET_DIR%\" (
  echo [FlowBlox] WARN: TargetDir not found: %TARGET_DIR%
  exit /b 0
)

if not exist "%DST_ONNX%\" mkdir "%DST_ONNX%" >nul 2>&1

rem ------------------------------------------------------------
rem Provider: CPU
rem Source:   %SRC_ONNX%\cpu\%RID%\
rem Dest:     %DST_ONNX%\cpu\%RID%\
rem ------------------------------------------------------------
set "SRC_DIR=%SRC_ONNX%\cpu\%RID%"
set "DST_DIR=%DST_ONNX%\cpu\%RID%"

if exist "%SRC_DIR%\" (
  dir /b "%SRC_DIR%\*.*" >nul 2>&1
  if errorlevel 1 (
    echo [FlowBlox] WARN: Provider 'cpu' source folder exists but is empty: %SRC_DIR%\
  ) else (
    if not exist "%DST_DIR%\" mkdir "%DST_DIR%" >nul 2>&1
    echo [FlowBlox] Copy provider 'cpu' (%RID%)
    echo           from %SRC_DIR%\
    echo             to %DST_DIR%\
    xcopy "%SRC_DIR%\*.*" "%DST_DIR%\" /E /I /Y /D >nul
    if errorlevel 1 echo [FlowBlox] WARN: Copy returned non-zero exit code for provider 'cpu'.
  )
) else (
  echo [FlowBlox] INFO: Provider 'cpu' source folder missing (skipping): %SRC_DIR%\
)

rem ------------------------------------------------------------
rem Provider: GPU (source folder name depends on RID)
rem Source: gpu-windows or gpu-linux -> Dest: gpu
rem ------------------------------------------------------------
set "GPU_SRC_PROVIDER=gpu-windows"
echo %RID% | findstr /I "^linux-" >nul
if %errorlevel%==0 set "GPU_SRC_PROVIDER=gpu-linux"

set "SRC_DIR=%SRC_ONNX%\%GPU_SRC_PROVIDER%\%RID%"
set "DST_DIR=%DST_ONNX%\gpu\%RID%"

if exist "%SRC_DIR%\" (
  dir /b "%SRC_DIR%\*.*" >nul 2>&1
  if errorlevel 1 (
    echo [FlowBlox] WARN: Provider 'gpu' source folder exists but is empty: %SRC_DIR%\
  ) else (
    if not exist "%DST_DIR%\" mkdir "%DST_DIR%" >nul 2>&1
    echo [FlowBlox] Copy provider 'gpu' (%RID%)
    echo           from %SRC_DIR%\
    echo             to %DST_DIR%\
    xcopy "%SRC_DIR%\*.*" "%DST_DIR%\" /E /I /Y /D >nul
    if errorlevel 1 echo [FlowBlox] WARN: Copy returned non-zero exit code for provider 'gpu'.
  )
) else (
  echo [FlowBlox] INFO: Provider 'gpu' source folder missing (skipping): %SRC_DIR%\
)

rem ------------------------------------------------------------
rem Provider: DirectML
rem ------------------------------------------------------------
set "SRC_DIR=%SRC_ONNX%\directml\%RID%"
set "DST_DIR=%DST_ONNX%\directml\%RID%"

if exist "%SRC_DIR%\" (
  dir /b "%SRC_DIR%\*.*" >nul 2>&1
  if errorlevel 1 (
    echo [FlowBlox] WARN: Provider 'directml' source folder exists but is empty: %SRC_DIR%\
  ) else (
    if not exist "%DST_DIR%\" mkdir "%DST_DIR%" >nul 2>&1
    echo [FlowBlox] Copy provider 'directml' (%RID%)
    echo           from %SRC_DIR%\
    echo             to %DST_DIR%\
    xcopy "%SRC_DIR%\*.*" "%DST_DIR%\" /E /I /Y /D >nul
    if errorlevel 1 echo [FlowBlox] WARN: Copy returned non-zero exit code for provider 'directml'.
  )
) else (
  echo [FlowBlox] INFO: Provider 'directml' source folder missing (skipping): %SRC_DIR%\
)

rem ------------------------------------------------------------
rem Provider: OpenVINO
rem ------------------------------------------------------------
set "SRC_DIR=%SRC_ONNX%\openvino\%RID%"
set "DST_DIR=%DST_ONNX%\openvino\%RID%"

if exist "%SRC_DIR%\" (
  dir /b "%SRC_DIR%\*.*" >nul 2>&1
  if errorlevel 1 (
    echo [FlowBlox] WARN: Provider 'openvino' source folder exists but is empty: %SRC_DIR%\
  ) else (
    if not exist "%DST_DIR%\" mkdir "%DST_DIR%" >nul 2>&1
    echo [FlowBlox] Copy provider 'openvino' (%RID%)
    echo           from %SRC_DIR%\
    echo             to %DST_DIR%\
    xcopy "%SRC_DIR%\*.*" "%DST_DIR%\" /E /I /Y /D >nul
    if errorlevel 1 echo [FlowBlox] WARN: Copy returned non-zero exit code for provider 'openvino'.
  )
) else (
  echo [FlowBlox] INFO: Provider 'openvino' source folder missing (skipping): %SRC_DIR%\
)

echo [FlowBlox] PostBuild done.
exit /b 0
