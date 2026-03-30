@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%.."
set "WEB_ROOT_FLOWBLOX=C:\RootDrive\Windows XP Workstation\Public\htdocs\flowbloxweb"

pushd "%REPO_ROOT%" >nul
if errorlevel 1 (
  echo [ERROR] Could not switch to repository root: "%REPO_ROOT%"
  exit /b 1
)

echo Publishing FlowBlox MSI release...
echo Web root: "%WEB_ROOT_FLOWBLOX%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Publish-FlowBloxMsiRelease.ps1" -WebRootFlowBlox "%WEB_ROOT_FLOWBLOX%" %*
if errorlevel 1 (
  echo.
  echo [ERROR] MSI publish failed.
  popd >nul
  exit /b 1
)

echo.
echo [OK] MSI publish completed.
popd >nul
exit /b 0
