@echo off
setlocal EnableExtensions

if "%~1"=="" (
  echo ERROR: Missing TargetDir argument.
  echo Usage: %~nx0 "<TargetDir>" ["win-x64"|"win-x86"|"win-arm64"|"linux-x64"]
  exit /b 1
)

set "TARGET_DIR=%~1"

rem Remove trailing backslash to avoid \" quote-escaping issues
if "%TARGET_DIR:~-1%"=="\" set "TARGET_DIR=%TARGET_DIR:~0,-1%"

set "RID_OVERRIDE=%~2"
set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%postbuild_onnx.ps1"

if not exist "%PS_SCRIPT%" (
  echo ERROR: PowerShell script not found:
  echo   %PS_SCRIPT%
  exit /b 1
)

echo.
echo [FlowBlox] PostBuild (PowerShell)
echo   TargetDir : %TARGET_DIR%
if not "%RID_OVERRIDE%"=="" echo   RID       : %RID_OVERRIDE%
echo.

rem Build common PowerShell args safely (no carets, no multiline)
set "COMMON_ARGS=-NoProfile -ExecutionPolicy Bypass -File "%PS_SCRIPT%" -TargetDir "%TARGET_DIR%""
if not "%RID_OVERRIDE%"=="" set "COMMON_ARGS=%COMMON_ARGS% -Rid "%RID_OVERRIDE%""

rem Copy ONNX Runtime
powershell %COMMON_ARGS% -DataSubfolder onnxruntimes
if errorlevel 1 (
  echo [FlowBlox] ERROR: PowerShell postbuild failed for onnxruntimes.
  exit /b 1
)

rem Copy ONNX Runtime GenAI
powershell %COMMON_ARGS% -DataSubfolder onnxruntimesgenai
if errorlevel 1 (
  echo [FlowBlox] ERROR: PowerShell postbuild failed for onnxruntimesgenai.
  exit /b 1
)

echo.
echo [FlowBlox] PostBuild done.
exit /b 0
