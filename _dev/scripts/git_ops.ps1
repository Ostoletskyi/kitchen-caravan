param(
  [Parameter(Mandatory=$true)][string]$ProjectRoot,
  [Parameter(Mandatory=$true)][ValidateSet("status","push","pullrebase","conflict")][string]$Mode
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Say([string]$m){ Write-Host $m }
function Run([string]$cmd){ Say ">> $cmd"; Invoke-Expression $cmd }

$git = Get-Command git -ErrorAction SilentlyContinue
if (-not $git) { Say "[ERROR] git not found in PATH."; exit 1 }
if (-not (Test-Path (Join-Path $ProjectRoot ".git"))) { Say "[ERROR] .git folder not found. Not a git repo?"; exit 1 }

Push-Location $ProjectRoot
try {
  switch ($Mode) {
    "status" {
      Run "git status"
      Run "git log --oneline -n 10"
      Run "git remote -v"
    }
    "push" {
      Run "git status"
      Say ""
      Say "[INFO] This menu does NOT auto-commit."
      Say "       If you have changes, run:"
      Say "       git add -A"
      Say "       git commit -m 'your message'"
      Say ""
      Run "git push"
    }
    "pullrebase" {
      Run "git status"
      Say ""
      Run "git fetch --all --prune"
      $branch = (git rev-parse --abbrev-ref HEAD).Trim()
      Say "[INFO] Current branch: $branch"
      Run "git pull --rebase"
      Run "git status"
    }
    "conflict" {
      Run "git status"
      Say ""
      Say "Conflict Helper:"
      Say "  A) Continue rebase   (git rebase --continue)"
      Say "  B) Abort rebase      (git rebase --abort)"
      Say "  C) Use mergetool     (git mergetool)"
      Say "  D) Open repo folder  (Explorer)"
      Say "  E) Show conflicts    (git diff --name-only --diff-filter=U)"
      Say "  X) Exit helper"
      $choice = Read-Host "Select A/B/C/D/E/X"
      switch ($choice.ToUpperInvariant()) {
        "A" { Run "git rebase --continue" }
        "B" { Run "git rebase --abort" }
        "C" { Run "git mergetool" }
        "D" { Start-Process explorer.exe $ProjectRoot | Out-Null }
        "E" { Run "git diff --name-only --diff-filter=U" }
        default { Say "[INFO] Exit." }
      }
      Run "git status"
    }
  }
} finally {
  Pop-Location
}
