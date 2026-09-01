@echo off
setlocal EnableExtensions

set "MODE=%~1"
set "CURRENT_DIR=%~dp0"
set "SOURCE_DIR=%CURRENT_DIR%ApplicationDir"

if /i "%MODE%"=="DEBUG" (
  set "TARGET_DIR=%CURRENT_DIR%bin\Debug\net8.0-windows10.0.19041.0\"
) else (
  set "TARGET_DIR=%CURRENT_DIR%bin\Release\net8.0-windows10.0.19041.0\"
)

if not exist "%SOURCE_DIR%" (
  echo %~nx0(10^): error FB0400: Source folder not found: "%SOURCE_DIR%"
  exit /b 1
)

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

pushd "%SOURCE_DIR%"
xcopy *.* "%TARGET_DIR%" /S /Y /D /C
if errorlevel 1 (
  echo %~nx0(20^): warning FB0401: ApplicationDir copy completed with xcopy warnings. Build continues.
)
popd

exit /b 0