# KitchenCaravan DevOps Menu (Bootstrap)

## Install
Unzip into: C:\Projects\KitchenCaravan (overwrite existing files)

## Menu
1) Push
2) Pull --rebase
3) Start Work (bootstrap checks + open tools + open Codex)
4) Conflict Helper
5) Status
0) Exit

## Note
Option 3 runs _dev\scripts\bootstrap_start.ps1. It checks tools and prints install suggestions (winget/choco),
then opens Explorer, PowerShell in project root, Unity Hub, Visual Studio (if detected), and finally opens a guided Codex shell.


## Codex prompt helper
In the Codex window, choose option 2 to build a ready-to-paste Codex prompt from a compiler error. It saves the prompt under _dev\codex\ and copies it to clipboard.


## Unity Editor.log prompt
In the Codex window, choose option 6 to auto-build a Codex prompt from the latest compile errors found in Unity Editor.log.
Default path: %LOCALAPPDATA%\Unity\Editor\Editor.log
