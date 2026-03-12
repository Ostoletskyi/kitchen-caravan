# KitchenCaravan DevOps Menu (Bootstrap)

## Install
Unzip into: C:\Projects\KitchenCaravan (overwrite existing files)

## Menu
1) Push
2) Pull --rebase --autostash
3) Start Work (bootstrap checks + open tools + open Codex)
4) Conflict Helper
5) Status
6) Fetch --all --prune --tags
0) Exit

## Note
Option 3 runs _dev\scripts\bootstrap_start.ps1. It checks tools and prints install suggestions (winget/choco),
then opens Explorer, PowerShell in project root, Unity Hub, Visual Studio (if detected), and finally opens a guided Codex shell.

## Git behavior
- `start-dev.bat` now detects the project root from its own location instead of a hardcoded path.
- All git actions validate that `git` exists and that the folder is really a git repo before running.
- Push auto-detects missing upstream and uses `git push --set-upstream` when needed.
- Pull does `git fetch --prune --tags --all` first, then `git pull --rebase --autostash`.
- Conflict Helper only offers actions that match the current state (rebase vs merge).
- Status shows branch, upstream, ahead/behind, recent commits, remotes, and stash count.


## Codex prompt helper
In the Codex window, choose option 2 to build a ready-to-paste Codex prompt from a compiler error. It saves the prompt under _dev\codex\ and copies it to clipboard.


## Unity Editor.log prompt
In the Codex window, choose option 6 to auto-build a Codex prompt from the latest compile errors found in Unity Editor.log.
Default path: %LOCALAPPDATA%\Unity\Editor\Editor.log
