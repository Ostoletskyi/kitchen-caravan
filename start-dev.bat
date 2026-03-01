@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

REM ===============================
REM KitchenCaravan DevOps Launcher
REM ===============================

set "PROJECT_ROOT=C:\Projects\KitchenCaravan"
set "DEV_DIR=%PROJECT_ROOT%\_dev"
set "SCRIPTS=%DEV_DIR%\scripts"
set "LOGS=%DEV_DIR%\logs"

if not exist "%PROJECT_ROOT%" (
  echo [ERROR] Project root not found: "%PROJECT_ROOT%"
  pause
  exit /b 1
)

if not exist "%DEV_DIR%"  mkdir "%DEV_DIR%"
if not exist "%SCRIPTS%"  mkdir "%SCRIPTS%"
if not exist "%LOGS%"     mkdir "%LOGS%"

set "STAMP=%DATE:~-4%%DATE:~3,2%%DATE:~0,2%_%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%"
set "STAMP=%STAMP: =0%"
set "LOG_FILE=%LOGS%\launcher_%STAMP%.log"

echo ==================================================  > "%LOG_FILE%"
echo KitchenCaravan Launcher started at %DATE% %TIME%    >> "%LOG_FILE%"
echo Project: %PROJECT_ROOT%                             >> "%LOG_FILE%"
echo ==================================================  >> "%LOG_FILE%"

:MENU
cls
echo ===============================================
echo KitchenCaravan DevOps Menu
echo Project: %PROJECT_ROOT%
echo Log: %LOG_FILE%
echo ===============================================
echo 1) Push (commit first if needed)
echo 2) Pull --rebase (sync)
echo 3) Start Work (open tools + Unity + VS)
echo 4) Conflict Helper (continue/abort/mergetool)
echo 5) Status (git status + last commits)
echo 6) Codex CLI Mode
echo 0) Exit
echo ===============================================
set /p CHOICE=Select an option: 

if "%CHOICE%"=="1" goto PUSH
if "%CHOICE%"=="2" goto PULLREBASE
if "%CHOICE%"=="3" goto STARTWORK
if "%CHOICE%"=="4" goto CONFLICT
if "%CHOICE%"=="5" goto STATUS
if "%CHOICE%"=="6" goto CODEX
if "%CHOICE%"=="0" goto END

echo Invalid choice.
pause
goto MENU

:STATUS
echo [INFO] STATUS >> "%LOG_FILE%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPTS%\git_ops.ps1" -ProjectRoot "%PROJECT_ROOT%" -Mode status 1>> "%LOG_FILE%" 2>>&1
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 25"
pause
goto MENU

:PUSH
echo [INFO] PUSH >> "%LOG_FILE%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPTS%\git_ops.ps1" -ProjectRoot "%PROJECT_ROOT%" -Mode push 1>> "%LOG_FILE%" 2>>&1
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 25"
pause
goto MENU

:PULLREBASE
echo [INFO] PULLREBASE >> "%LOG_FILE%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPTS%\git_ops.ps1" -ProjectRoot "%PROJECT_ROOT%" -Mode pullrebase 1>> "%LOG_FILE%" 2>>&1
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 25"
pause
goto MENU

:CONFLICT
echo [INFO] CONFLICT >> "%LOG_FILE%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPTS%\git_ops.ps1" -ProjectRoot "%PROJECT_ROOT%" -Mode conflict 1>> "%LOG_FILE%" 2>>&1
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 30"
pause
goto MENU

:CODEX
echo [INFO] CODEX >> "%LOG_FILE%"
REM Run Codex menu interactively (no output redirection) in a separate window
start "Codex CLI" powershell -NoProfile -ExecutionPolicy Bypass -NoExit -File "%SCRIPTS%\codex_cli.ps1" -ProjectRoot "%PROJECT_ROOT%"
echo [INFO] Codex CLI window started. >> "%LOG_FILE%"
pause
goto MENU

:STARTWORK
echo [INFO] STARTWORK >> "%LOG_FILE%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPTS%\start_work.ps1" -ProjectRoot "%PROJECT_ROOT%" 1>> "%LOG_FILE%" 2>>&1
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 35"
pause
goto MENU

:END
echo Bye.
exit /b 0
