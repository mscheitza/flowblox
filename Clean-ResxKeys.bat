@echo off

echo ============================================
echo RESX CLEANUP TOOL
echo ============================================
echo.

set RESX_ROOT=C:\Projekte\VisualStudio\flowblox
set CODE_ROOT=C:\Projekte\VisualStudio\flowblox
set SCRIPT=%~dp0Clean-ResxKeys.ps1

echo ResxRoot: %RESX_ROOT%
echo CodeRoot: %CODE_ROOT%
echo Script:   %SCRIPT%
echo.

echo Step 1: Dry run + preview
echo --------------------------------------------

powershell -ExecutionPolicy Bypass -File "%SCRIPT%" -ResxRoot "%RESX_ROOT%" -CodeRoot "%CODE_ROOT%" -PreviewPerFile

echo.
pause

echo Step 2: APPLY cleanup (with backup)
echo --------------------------------------------

powershell -ExecutionPolicy Bypass -File "%SCRIPT%" -ResxRoot "%RESX_ROOT%" -CodeRoot "%CODE_ROOT%" -Apply -Backup

echo.
pause
