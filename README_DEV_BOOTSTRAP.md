# KitchenCaravan Dev Bootstrap Pack (Drop-in)

## What this does
- Creates `_dev/` folder structure
- Runs basic tool checks (git/dotnet/node/npm)
- Runs `git status` (optional `git pull --rebase` via config)
- Opens terminals from the project root (Windows Terminal if available)
- Launches Unity Hub
- Launches Visual Studio with the solution if found

## Install
1. Unzip into the project root: `C:\Projects\KitchenCaravan`
2. Run: `start-dev.bat`

## Configuration
Edit: `_dev\dev.config.json`

- Enable git pull:
  - `git.autoPull = true`
- Pin a solution file:
  - `visualStudio.solutionRelativePath = "KitchenCaravan.sln"`
- Pin Unity Hub path if auto-detect fails:
  - `unity.unityHubPath = "C:\\Program Files\\Unity Hub\\Unity Hub.exe"`

## Logs
Created in `_dev\logs\`


## Notes
- `terminalsToOpen[].command` should contain only the PowerShell commands to run (no `pwsh -Command ...` wrapper).
