@echo off
setlocal EnableExtensions

if "%~1"=="" (
  echo ERROR: Missing TargetDir argument.
  echo Usage: %~nx0 "<TargetDir>" ["win-x64"^|"win-x86"^|"win-arm64"^|"linux-x64"]
  exit /b 1
)

set "TARGET_DIR=%~1"
set "RID_OVERRIDE=%~2"

set "CURRENT_DIR=%~dp0"
for %%I in ("%CURRENT_DIR%..\") do set "ROOT_DIR=%%~fI\"

if "%RID_OVERRIDE%"=="" (
  call "%ROOT_DIR%FlowBlox\postbuild.bat" "%TARGET_DIR%"
) else (
  call "%ROOT_DIR%FlowBlox\postbuild.bat" "%TARGET_DIR%" "%RID_OVERRIDE%"
)

exit /b %ERRORLEVEL%