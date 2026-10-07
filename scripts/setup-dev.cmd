@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup-dev.ps1" %*
exit /b %errorlevel%
