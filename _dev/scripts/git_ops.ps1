param(
  [Parameter(Mandatory=$true)]
  [Alias("Mode")]
  [ValidateSet("PUSH","PULL_REBASE","CONFLICT","STATUS")]
  [string]$Action,

  [Parameter(Mandatory=$true)][string]$ProjectRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Say([string]$m){ Write-Host $m }
function Run([string]$cmd){
  Say ">> $cmd"
  try { Invoke-Expression $cmd } catch { Say ("[WARN] Command failed: " + $_.Exception.Message) }
}

function Ensure-ProjectRoot {
  param([string]$Root)
  if (-not (Test-Path -LiteralPath $Root)) {
    Say "[ERROR] Project root not found: $Root"
    exit 1
  }
}

function Get-LatestLauncherLog {
  param([string]$Root)
  $logDir = Join-Path $Root "_dev\logs"
  if (-not (Test-Path -LiteralPath $logDir)) { return $null }
  return (Get-ChildItem -LiteralPath $logDir -Filter "launcher_*.log" -ErrorAction SilentlyContinue |
          Sort-Object LastWriteTime -Descending |
          Select-Object -First 1)
}

function Build-AutoCommitMessage {
  param([string]$Root)

  $ts = Get-Date -Format "yyyy-MM-dd HH:mm"
  $f = Get-LatestLauncherLog $Root
  if (-not $f) { return "Auto: commit before push ($ts)" }

  $tail = Get-Content -LiteralPath $f.FullName -ErrorAction SilentlyContinue | Select-Object -Last 160
  $marker = $null

  # PowerShell 5.1 compatible reverse iteration
  $arr = @($tail)
  for ($i = $arr.Length - 1; $i -ge 0; $i--) {
    $line = $arr[$i]
    if ($line -match '\[(INFO|BOOT)\]\s+([A-Z0-9_ -]{4,})') {
      $marker = $Matches[2].Trim()
      break
    }
  }

  if (-not $marker) { $marker = "Work session" }
  return "Auto: $marker ($ts)"
}

function Detect-ConflictState {
  param([string]$Root)
  $gitDir = Join-Path $Root ".git"
  $inRebase = (Test-Path -LiteralPath (Join-Path $gitDir "rebase-merge")) -or (Test-Path -LiteralPath (Join-Path $gitDir "rebase-apply"))
  $inMerge  = (Test-Path -LiteralPath (Join-Path $gitDir "MERGE_HEAD"))
  $hasUnmerged = $false
  try {
    $u = git ls-files -u 2>$null
    if ($u) { $hasUnmerged = $true }
  } catch {}
  return [pscustomobject]@{ InRebase=$inRebase; InMerge=$inMerge; HasUnmerged=$hasUnmerged }
}

function AutoCommitAndPush {
  param([string]$Root)

  Push-Location $Root
  try {
    Say "[INFO] AUTO PUSH: git add -A -> commit (if needed) -> push"
    Run "git add -A"

    $porcelain = (git status --porcelain 2>$null)
    if (-not $porcelain -or $porcelain.Count -eq 0) {
      Say "[OK] Working tree clean. Nothing to commit."
      Run "git push"
      return
    }

    $cached = (git diff --cached --name-only 2>$null)
    if (-not $cached -or $cached.Count -eq 0) {
      Say "[OK] No staged changes to commit."
      Run "git push"
      return
    }

    $msg = Build-AutoCommitMessage $Root
    Say "[INFO] Commit message: $msg"
    Run ('git commit -m "{0}"' -f ($msg.Replace('"','\"')))

    Run "git --no-pager status -sb"
    Run "git push"
  } finally {
    Pop-Location
  }
}

function PullRebase {
  param([string]$Root)
  Push-Location $Root
  try {
    Say "[INFO] PULL --REBASE"
    Run "git --no-pager status"
    Run "git pull --rebase"
    Run "git --no-pager status -sb"
  } finally { Pop-Location }
}

function ConflictHelper {
  param([string]$Root)
  Push-Location $Root
  try {
    Say "[INFO] CONFLICT"
    Run "git --no-pager status"
    $state = Detect-ConflictState $Root
    if (-not $state.InRebase -and -not $state.InMerge -and -not $state.HasUnmerged) {
      Say "[INFO] No merge/rebase in progress and no unmerged files detected."
      Say "      Nothing to resolve right now."
      return
    }

    Say ""
    Run "git --no-pager diff --name-only --diff-filter=U"
    Say ""
    Say "Choose:"
    Say "  A) Continue rebase (git rebase --continue)"
    Say "  B) Abort rebase (git rebase --abort)"
    Say "  C) Abort merge (git merge --abort)"
    Say "  D) Open mergetool (git mergetool)"
    Say "  E) Open repo folder (Explorer)"
    Say "  X) Back"
    $c = (Read-Host "Select A/B/C/D/E/X").Trim().ToUpperInvariant()

    switch ($c) {
      "A" { Run "git rebase --continue" }
      "B" { Run "git rebase --abort" }
      "C" { Run "git merge --abort" }
      "D" { Run "git mergetool" }
      "E" { try { Start-Process explorer.exe $Root | Out-Null } catch {} }
      default { }
    }
  } finally { Pop-Location }
}

function Status {
  param([string]$Root)
  Push-Location $Root
  try {
    Run "git --no-pager status"
    Run "git --no-pager log --oneline -n 10"
    Run "git remote -v"
  } finally { Pop-Location }
}

Ensure-ProjectRoot $ProjectRoot

switch ($Action) {
  "PUSH"        { AutoCommitAndPush $ProjectRoot }
  "PULL_REBASE" { PullRebase $ProjectRoot }
  "CONFLICT"    { ConflictHelper $ProjectRoot }
  "STATUS"      { Status $ProjectRoot }
  default       { Say "[ERROR] Unknown action: $Action"; exit 2 }
}
