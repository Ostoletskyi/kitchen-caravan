@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

REM ===============================
REM KitchenCaravan DevOps Launcher
REM ===============================

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "PROJECT_ROOT=%%~fI"
set "DEV_DIR=%PROJECT_ROOT%\_dev"
set "SCRIPTS=%DEV_DIR%\scripts"
set "LOGS=%DEV_DIR%\logs"
set "GIT_OPS=%SCRIPTS%\git_ops.ps1"
set "BOOTSTRAP=%SCRIPTS%\bootstrap_start.ps1"

if not exist "%PROJECT_ROOT%" (
  echo [ERROR] Project root not found: "%PROJECT_ROOT%"
  pause
  exit /b 1
)

if not exist "%DEV_DIR%" mkdir "%DEV_DIR%"
if not exist "%SCRIPTS%" mkdir "%SCRIPTS%"
if not exist "%LOGS%" mkdir "%LOGS%"

if not exist "%GIT_OPS%" (
  echo [ERROR] Missing script: "%GIT_OPS%"
  pause
  exit /b 1
)

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
set "CURRENT_BRANCH="
for /f "usebackq delims=" %%B in (`powershell -NoProfile -Command "$b=(git -C '%PROJECT_ROOT%' branch --show-current 2^>^$null); if ($LASTEXITCODE -eq 0 -and $b) { $b }"`) do set "CURRENT_BRANCH=%%B"
if defined CURRENT_BRANCH (
  echo Branch: %CURRENT_BRANCH%
) else (
  echo Branch: ^<unavailable^>
)
echo ===============================================
echo 1^) Push ^(commit first if needed^)
echo 2^) Pull --rebase --autostash ^(sync^)
echo 3^) Start Work ^(bootstrap + tools + Codex + prompt helper^)
echo 4^) Conflict Helper ^(continue/abort/mergetool^)
echo 5^) Status ^(branch + commits + remotes + stash^)
echo 6^) Fetch --all --prune --tags
echo 0^) Exit
echo ===============================================
choice /C 1234560 /N /M "Select an option [1,2,3,4,5,6,0]: "

if errorlevel 7 goto END
if errorlevel 6 goto FETCH
if errorlevel 5 goto STATUS
if errorlevel 4 goto CONFLICT
if errorlevel 3 goto STARTWORK
if errorlevel 2 goto PULLREBASE
if errorlevel 1 goto PUSH

goto MENU

:RUN_GIT_OPS
set "ACTION=%~1"
set "TAIL_LINES=%~2"
echo [INFO] %ACTION% >> "%LOG_FILE%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%GIT_OPS%" -ProjectRoot "%PROJECT_ROOT%" -Mode %ACTION% 1>> "%LOG_FILE%" 2>>&1
set "SCRIPT_EXIT=%ERRORLEVEL%"
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail %TAIL_LINES%"
if not "%SCRIPT_EXIT%"=="0" (
  echo.
  echo [WARN] Operation finished with exit code %SCRIPT_EXIT%.
)
pause
goto MENU

:STATUS
call :RUN_GIT_OPS STATUS 40

:PUSH
call :RUN_GIT_OPS PUSH 50

:PULLREBASE
call :RUN_GIT_OPS PULL_REBASE 50

:CONFLICT
call :RUN_GIT_OPS CONFLICT 60

:FETCH
call :RUN_GIT_OPS FETCH 40

:STARTWORK
echo [INFO] STARTWORK >> "%LOG_FILE%"
if not exist "%BOOTSTRAP%" (
  echo [ERROR] Missing script: "%BOOTSTRAP%"
  pause
  goto MENU
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%BOOTSTRAP%" -ProjectRoot "%PROJECT_ROOT%" -LogFile "%LOG_FILE%"
set "SCRIPT_EXIT=%ERRORLEVEL%"
echo --- Last lines of log ---
powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 80"
if not "%SCRIPT_EXIT%"=="0" (
  echo.
  echo [WARN] Start Work finished with exit code %SCRIPT_EXIT%.
)
pause
goto MENU

:END
echo Bye.
exit /b 0
