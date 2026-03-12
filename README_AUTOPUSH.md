KitchenCaravan DevOps - Auto Push (Option 1)

Drop-in update:
- Replaces _dev/scripts/git_ops.ps1
- Makes option 1 (Push) fully automatic:
  git add -A
  git commit (auto message from latest _dev/logs/launcher_*.log or staged files)
  git push

Current behavior:
- Validates that the folder is a git repo before running.
- Refuses to push from unfinished merge/rebase states.
- If the branch has no upstream, it uses `git push --set-upstream`.
- If there is nothing to commit, it still performs a normal `git push`.

IMPORTANT:
Append the lines from GITIGNORE_APPEND.txt into your .gitignore
to prevent auto-add from including generated folders.
