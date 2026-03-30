@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%.."

pushd "%REPO_ROOT%" >nul
if errorlevel 1 (
  echo [ERROR] Could not switch to repository root: "%REPO_ROOT%"
  exit /b 1
)

echo Building FlowBlox MSI installer...
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Build-FlowBloxMsi.ps1" %*
if errorlevel 1 (
  echo.
  echo [ERROR] MSI build failed.
  popd >nul
  exit /b 1
)

echo.
echo [OK] MSI build completed.
popd >nul
exit /b 0
