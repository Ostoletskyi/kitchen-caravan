param(
  [Parameter(Mandatory=$true)][string]$ProjectRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Say([string]$m){ Write-Host $m }
function Run([string]$cmd){
  Say ">> $cmd"
  try { Invoke-Expression $cmd } catch { Say ("[WARN] Command failed: " + $_.Exception.Message) }
}

function Get-CommandPath {
  param([string[]]$Names)

  foreach ($name in $Names) {
    $cmd = Get-Command $name -ErrorAction SilentlyContinue
    if ($cmd) {
      return $cmd.Source
    }
  }

  return $null
}

function Read-MenuChoice {
  param([string]$Prompt)

  $raw = Read-Host $Prompt
  if ($null -eq $raw) {
    return ""
  }

  return $raw.Trim()
}

function Get-Git { return (Get-Command git -ErrorAction SilentlyContinue) }
function Get-Codex { return (Get-Command codex -ErrorAction SilentlyContinue) }

function Get-CurrentBranch {
  try { return (git rev-parse --abbrev-ref HEAD).Trim() } catch { return "" }
}

function Get-RemoteUrl {
  param([string]$RemoteName="origin")
  try {
    $u = (git remote get-url $RemoteName 2>$null)
    if ($u) { return $u.Trim() }
  } catch {}
  return ""
}

function Normalize-GitHubRepoUrl {
  param([string]$RemoteUrl)
  if (-not $RemoteUrl) { return "" }

  if ($RemoteUrl -match '^https?://') {
    $u = $RemoteUrl
    if ($u.EndsWith(".git")) { $u = $u.Substring(0, $u.Length-4) }
    return $u
  }

  if ($RemoteUrl -match '^git@github\.com:(.+)$') {
    $path = $Matches[1]
    if ($path.EndsWith(".git")) { $path = $path.Substring(0, $path.Length-4) }
    return ("https://github.com/" + $path)
  }

  return ""
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

  return [pscustomobject]@{
    InRebase = $inRebase
    InMerge = $inMerge
    HasUnmerged = $hasUnmerged
  }
}

function Open-GitHub-Help {
  param([string]$Root)

  $branch = Get-CurrentBranch
  $remote = Get-RemoteUrl "origin"
  $repoUrl = Normalize-GitHubRepoUrl $remote

  if (-not $repoUrl) {
    Say "[WARN] Could not parse GitHub repo URL from 'origin'. Remote was:"
    Say "       $remote"
    return
  }

  if (-not $branch) { $branch = "main" }

  $treeUrl = "$repoUrl/tree/$branch"
  $prUrl   = "$repoUrl/pull/new/$branch"

  Say "[INFO] Opening GitHub pages:"
  Say "  Branch: $treeUrl"
  Say "  PR:     $prUrl"
  try { Start-Process $treeUrl | Out-Null } catch {}
  try { Start-Process $prUrl   | Out-Null } catch {}
}

function Smart-Conflict-Resolver {
  param([string]$Root)

  Say ""
  Say "--- Smart Conflict Resolver ---"
  $state = Detect-ConflictState $Root

  Run "git --no-pager status"
  Say ""

  if (-not $state.InRebase -and -not $state.InMerge -and -not $state.HasUnmerged) {
    Say "[OK] No local merge/rebase in progress and no unmerged files."
    Say "     Opening GitHub branch + PR page in browser (for web-based conflict resolution if PR has conflicts)."
    Open-GitHub-Help $Root
    return
  }

  $det = @()
  if ($state.InRebase) { $det += "REBASE" }
  if ($state.InMerge)  { $det += "MERGE" }
  if ($state.HasUnmerged) { $det += "UNMERGED_FILES" }

  Say ("[INFO] Local conflict state detected: " + ($det -join " "))
  Say ""
  Run "git --no-pager diff --name-only --diff-filter=U"
  Say ""
  Say "What you can do now:"
  Say "  1) Open mergetool (git mergetool)"
  Say "  2) Continue rebase (git rebase --continue)"
  Say "  3) Abort rebase (git rebase --abort)"
  Say "  4) Abort merge (git merge --abort)"
  Say "  5) Open repo folder (Explorer)"
  Say "  X) Back"
  $c = (Read-MenuChoice "Select 1/2/3/4/5/X").ToUpperInvariant()
  if (-not $c) {
    return
  }
  switch ($c) {
    "1" { Run "git mergetool" }
    "2" { Run "git rebase --continue" }
    "3" { Run "git rebase --abort" }
    "4" { Run "git merge --abort" }
    "5" { try { Start-Process explorer.exe $Root | Out-Null } catch {} }
    default { }
  }
}

if (-not (Test-Path -LiteralPath $ProjectRoot)) {
  Say "[ERROR] Project root not found: $ProjectRoot"
  exit 1
}

$git = Get-Git
if (-not $git) {
  Say "[ERROR] git not found in PATH."
  exit 1
}

Push-Location $ProjectRoot
try {
  Say "--- Codex CLI (guided) ---"
  $codexCmd = Get-Codex
  if ($codexCmd) { Say "Detected: codex at: $($codexCmd.Source)" }
  else { Say "[WARN] codex not found in PATH. Install via npm or your preferred method." }
  Say ""

  while ($true) {
    Say "Choose an action:"
    Say "  1) Open interactive shell in project root (recommended)"
    Say "  2) Build a Codex prompt from pasted compile error (save + clipboard)"
    Say "  3) Show git status + diff (review before Codex)"
    Say "  4) Create safe Codex branch (codex/session-YYYYMMDD_HHMM)"
    Say "  5) Start Codex now (run: codex)"
    Say "  6) Build a Codex prompt from latest Unity Editor.log compile errors"
    Say "  7) Smart conflict resolver (local mergetool OR open GitHub PR page)"
    Say "  0) Back"

    $sel = Read-MenuChoice "Select 1/2/3/4/5/6/7/0"
    if (-not $sel) {
      break
    }
    switch ($sel) {
      "1" {
        $pwsh = Get-CommandPath @("pwsh.exe", "powershell.exe", "pwsh", "powershell")
        if (-not $pwsh) { Say "[ERROR] No PowerShell found."; continue }
        Start-Process -FilePath $pwsh -ArgumentList @("-NoExit","-Command","Set-Location -LiteralPath `"$ProjectRoot`"") -WorkingDirectory $ProjectRoot | Out-Null
      }
      "2" {
        Say ""
        Say "[INFO] Paste the compile error now, then press ENTER twice."
        $buf = New-Object System.Collections.Generic.List[string]
        while ($true) {
          $line = Read-Host
          if ([string]::IsNullOrWhiteSpace($line)) { break }
          $buf.Add($line)
        }
        $text = ($buf -join "`n").Trim()
        if (-not $text) { Say "[WARN] Nothing pasted."; continue }

        $outDir = Join-Path $ProjectRoot "_dev\codex"
        New-Item -ItemType Directory -Force -Path $outDir | Out-Null
        $ts = Get-Date -Format "yyyyMMdd_HHmmss"
        $outPath = Join-Path $outDir ("issue_prompt_" + $ts + ".txt")
        $prompt = @"
You are Codex 5.3 acting as a senior Unity engineer. Fix this compile error in the current repo.
Provide a unified diff patch (git apply compatible). Keep changes minimal.

ERROR:
$text
"@
        Set-Content -Path $outPath -Value $prompt -Encoding UTF8
        try { Set-Clipboard -Value $prompt } catch {}
        Say "[OK] Prompt saved: $outPath"
        Say "[OK] Prompt copied to clipboard."
      }
      "3" {
        Run "git --no-pager status"
        Say ""
        Run "git --no-pager diff"
      }
      "4" {
        $ts = Get-Date -Format "yyyyMMdd_HHmm"
        $branch = "codex/session-$ts"
        Run "git checkout -b $branch"
        Run "git --no-pager status"
      }
      "5" {
        if (-not $codexCmd) { Say "[WARN] codex not found. Install first."; continue }
        Say ""
        Say "[INFO] Starting Codex in project root..."
        Run "codex"
      }
      "6" {
        $logPath = Join-Path $env:LOCALAPPDATA "Unity\Editor\Editor.log"
        if (-not (Test-Path -LiteralPath $logPath)) {
          Say "[WARN] Unity Editor.log not found at: $logPath"
          continue
        }
        Say "[INFO] Using log: $logPath"
        $lines = Get-Content -LiteralPath $logPath -ErrorAction SilentlyContinue
        if (-not $lines) { Say "[WARN] Could not read log."; continue }

        $tail = $lines | Select-Object -Last 2000
        $errs = @()
        foreach ($l in $tail) { if ($l -match 'error\s+CS\d{4}\b') { $errs += $l } }
        $errs = $errs | Select-Object -Unique

        if (-not $errs -or $errs.Length -eq 0) {
          Say "[WARN] No compile errors (CS****) found in the last part of the log."
          Say "Tip: trigger a compile in Unity, then retry."
          continue
        }

        $outDir = Join-Path $ProjectRoot "_dev\codex"
        New-Item -ItemType Directory -Force -Path $outDir | Out-Null
        $ts = Get-Date -Format "yyyyMMdd_HHmmss"
        $outPath = Join-Path $outDir ("issue_prompt_" + $ts + ".txt")

        $joined = ($errs -join "`n")
        $prompt = @"
You are Codex 5.3 acting as a senior Unity engineer. Fix these Unity compile errors in the current repo.
Provide a unified diff patch (git apply compatible). Keep changes minimal.

ERRORS:
$joined
"@
        Set-Content -Path $outPath -Value $prompt -Encoding UTF8
        try { Set-Clipboard -Value $prompt } catch {}
        Say "[OK] Prompt saved: $outPath"
        Say "[OK] Prompt copied to clipboard."
        Say "Now paste the prompt into Codex and run it."
      }
      "7" {
        Smart-Conflict-Resolver $ProjectRoot
      }
      "0" { break }
      default { Say "[WARN] Unknown choice." }
    }
    Say ""
  }
} finally {
  Pop-Location
}
