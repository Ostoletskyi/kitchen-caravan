KitchenCaravan DevOps - Auto Push (Option 1)

Drop-in update:
- Replaces _dev/scripts/git_ops.ps1
- Makes option 1 (Push) fully automatic:
  git add -A
  git commit (auto message from latest _dev/logs/launcher_*.log)
  git push

IMPORTANT:
Append the lines from GITIGNORE_APPEND.txt into your .gitignore
to prevent auto-add from including generated folders.
