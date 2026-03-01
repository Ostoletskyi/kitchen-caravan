# KitchenCaravan DevOps Menu + Codex CLI (Drop-in)

## Install
Unzip into: `C:\Projects\KitchenCaravan` (overwrite existing files)

## Menu (English)
1) Push
2) Pull --rebase
3) Start Work
4) Conflict Helper
5) Status
6) Codex CLI Mode
0) Exit

## Fixes in this build
- Adds option 6 to the menu (Codex CLI Mode)
- Fixes Start Work failing under strict mode when parameters were not bound
- Improves Visual Studio detection with common fixed devenv.exe paths + vswhere fallback
