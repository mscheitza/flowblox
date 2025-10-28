set Mode=%1
set CurrentDir=%~dp0
pushd %CurrentDir%ApplicationDir
if /i %Mode%==DEBUG (
xcopy *.* "..\bin\Debug\net8.0-windows10.0.19041.0\" /S /Y
) else (
xcopy *.* "..\bin\Release\net8.0-windows10.0.19041.0\" /S /Y
)
exit