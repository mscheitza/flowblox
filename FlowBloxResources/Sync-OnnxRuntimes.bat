@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%Sync-OnnxRuntimes.ps1"

if not exist "%PS_SCRIPT%" (
  echo [ERROR] Script not found: "%PS_SCRIPT%"
  pause
  exit /b 1
)

echo Running Sync-OnnxRuntimes.ps1...
powershell -NoProfile -ExecutionPolicy Bypass -File "%PS_SCRIPT%"
set "EXITCODE=%ERRORLEVEL%"

if not "%EXITCODE%"=="0" (
  echo.
  echo [ERROR] Sync-OnnxRuntimes.ps1 failed with exit code %EXITCODE%.
  pause
  exit /b %EXITCODE%
)

echo.
echo Sync-OnnxRuntimes.ps1 completed successfully.
pause
exit /b 0
