@echo off
setlocal

chcp 65001 >nul
set "PYTHONIOENCODING=utf-8"

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "PROJECT_ROOT=%%~fI"
set "MENU_SCRIPT=%PROJECT_ROOT%\_dev\scripts\dev_menu.ps1"

if not exist "%MENU_SCRIPT%" (
  echo [ERROR] Missing script: "%MENU_SCRIPT%"
  pause
  exit /b 1
)

rem Prefer PowerShell 7 if available, otherwise fall back to Windows PowerShell
set "PS_EXE="
where pwsh >nul 2>nul
if %ERRORLEVEL%==0 (
  set "PS_EXE=pwsh"
) else (
  set "PS_EXE=powershell"
)

"%PS_EXE%" -NoLogo -NoProfile -ExecutionPolicy Bypass -Command ^
  "try { chcp 65001 ^| Out-Null } catch {} ;" ^
  "[Console]::InputEncoding  = [System.Text.UTF8Encoding]::new($true) ;" ^
  "[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($true) ;" ^
  "$OutputEncoding          = [System.Text.UTF8Encoding]::new($true) ;" ^
  "& '%MENU_SCRIPT%' -ProjectRoot '%PROJECT_ROOT%'"

set "SCRIPT_EXIT=%ERRORLEVEL%"

if not "%SCRIPT_EXIT%"=="0" (
  echo.
  echo [WARN] DevOps launcher finished with exit code %SCRIPT_EXIT%.
  pause
)

exit /b %SCRIPT_EXIT%