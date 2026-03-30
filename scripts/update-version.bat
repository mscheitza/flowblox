@echo off
set VERSION=%~1
if "%VERSION%"=="" (
  set /p VERSION=Bitte Versionsnummer eingeben, z.B. 1.2.3: 
)
if "%VERSION%"=="" (
  echo [ERROR] Keine Versionsnummer angegeben.
  echo Usage: %~n0 ^<Version^>
  echo Beispiel: %~n0 1.2.3
  exit /b 1
)
powershell -ExecutionPolicy Bypass -File "%~dp0update-version.ps1" -Version "%VERSION%"
