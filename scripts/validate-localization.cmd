@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0validate-localization.ps1" %*
exit /b %errorlevel%
