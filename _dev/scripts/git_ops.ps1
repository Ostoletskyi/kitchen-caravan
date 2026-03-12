param(
  [Parameter(Mandatory = $true)]
  [Alias("Mode")]
  [ValidateSet("PUSH", "PULL_REBASE", "CONFLICT", "STATUS", "FETCH")]
  [string]$Action,

  [Parameter(Mandatory = $true)]
  [string]$ProjectRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ($PSVersionTable.PSVersion.Major -ge 7) {
  $PSNativeCommandUseErrorActionPreference = $false
}

function Say([string]$Message) {
  Write-Host $Message
}

function Warn([string]$Message) {
  Say "[WARN] $Message"
}

function Fail([string]$Message, [int]$Code = 1) {
  Say "[ERROR] $Message"
  exit $Code
}

function Ensure-ProjectRoot {
  param([string]$Root)

  if (-not (Test-Path -LiteralPath $Root)) {
    Fail "Project root not found: $Root"
  }
}

function Ensure-GitAvailable {
  if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Fail "git not found in PATH."
  }
}

function Format-ProcessArgument {
  param([string]$Value)

  if ($null -eq $Value) {
    return '""'
  }

  if ($Value -notmatch '[\s"]') {
    return $Value
  }

  return '"' + ($Value -replace '(\\*)"', '$1$1\"' -replace '(\\+)$', '$1$1') + '"'
}

function Invoke-Git {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Arguments,

    [switch]$AllowFailure,
    [switch]$Silent
  )

  $commandText = "git {0}" -f ($Arguments -join " ")
  if (-not $Silent) {
    Say ">> $commandText"
  }

  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = "git"
  $psi.WorkingDirectory = (Get-Location).Path
  $psi.UseShellExecute = $false
  $psi.RedirectStandardOutput = $true
  $psi.RedirectStandardError = $true
  $psi.CreateNoWindow = $true
  $psi.Arguments = (($Arguments | ForEach-Object { Format-ProcessArgument $_ }) -join " ")

  $process = New-Object System.Diagnostics.Process
  $process.StartInfo = $psi

  [void]$process.Start()
  $stdout = $process.StandardOutput.ReadToEnd()
  $stderr = $process.StandardError.ReadToEnd()
  $process.WaitForExit()

  $lines = @()
  if ($stdout) {
    $lines += ($stdout -split "\r\n|\n|\r")
  }
  if ($stderr) {
    $lines += ($stderr -split "\r\n|\n|\r")
  }
  $lines = @($lines | Where-Object { $_ -ne "" })
  $exitCode = $process.ExitCode

  if (-not $Silent) {
    foreach ($line in $lines) {
      Say $line
    }
  }

  if (-not $AllowFailure -and $exitCode -ne 0) {
    Fail "Command failed ($exitCode): $commandText" $exitCode
  }

  return [pscustomobject]@{
    ExitCode = $exitCode
    Output   = $lines
  }
}

function Ensure-GitRepository {
  param([string]$Root)

  Push-Location $Root
  try {
    $result = Invoke-Git -Arguments @("rev-parse", "--is-inside-work-tree") -AllowFailure -Silent
    if ($result.ExitCode -ne 0 -or -not ($result.Output -contains "true")) {
      Fail "Directory is not a git repository: $Root"
    }
  } finally {
    Pop-Location
  }
}

function Get-LatestLauncherLog {
  param([string]$Root)

  $logDir = Join-Path $Root "_dev\logs"
  if (-not (Test-Path -LiteralPath $logDir)) {
    return $null
  }

  return Get-ChildItem -LiteralPath $logDir -Filter "launcher_*.log" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
}

function Get-CurrentBranch {
  $result = Invoke-Git -Arguments @("branch", "--show-current") -AllowFailure -Silent
  if ($result.ExitCode -ne 0) {
    return ""
  }

  return (($result.Output | Select-Object -First 1) -as [string]).Trim()
}

function Get-UpstreamBranch {
  $result = Invoke-Git -Arguments @("rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}") -AllowFailure -Silent
  if ($result.ExitCode -ne 0) {
    return $null
  }

  return (($result.Output | Select-Object -First 1) -as [string]).Trim()
}

function Get-DefaultRemote {
  $result = Invoke-Git -Arguments @("remote") -AllowFailure -Silent
  if ($result.ExitCode -ne 0) {
    return $null
  }

  $remotes = @($result.Output | ForEach-Object { ($_ -as [string]).Trim() } | Where-Object { $_ })
  if ($remotes -contains "origin") {
    return "origin"
  }

  return $remotes | Select-Object -First 1
}

function Get-WorkingTreeChanges {
  $result = Invoke-Git -Arguments @("status", "--porcelain") -AllowFailure -Silent
  if ($result.ExitCode -ne 0) {
    return @()
  }

  return @($result.Output | Where-Object { $_ -and $_.ToString().Trim().Length -gt 0 })
}

function Get-StagedChanges {
  $result = Invoke-Git -Arguments @("diff", "--cached", "--name-only") -AllowFailure -Silent
  if ($result.ExitCode -ne 0) {
    return @()
  }

  return @($result.Output | ForEach-Object { ($_ -as [string]).Trim() } | Where-Object { $_ })
}

function Get-ConflictState {
  param([string]$Root)

  $gitDir = Join-Path $Root ".git"
  $inRebase = (Test-Path -LiteralPath (Join-Path $gitDir "rebase-merge")) -or (Test-Path -LiteralPath (Join-Path $gitDir "rebase-apply"))
  $inMerge = Test-Path -LiteralPath (Join-Path $gitDir "MERGE_HEAD")

  $unmerged = Invoke-Git -Arguments @("ls-files", "-u") -AllowFailure -Silent
  $unmergedFiles = @(
    $unmerged.Output |
      ForEach-Object {
        $line = ($_ -as [string]).Trim()
        if (-not $line) {
          return
        }

        $parts = $line -split '\s+'
        if ($parts.Length -ge 4) {
          return $parts[3]
        }
      } |
      Where-Object { $_ } |
      Sort-Object -Unique
  )

  return [pscustomobject]@{
    InRebase      = $inRebase
    InMerge       = $inMerge
    HasUnmerged   = $unmergedFiles.Count -gt 0
    UnmergedFiles = $unmergedFiles
  }
}

function Build-AutoCommitMessage {
  param([string]$Root)

  $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
  $logFile = Get-LatestLauncherLog $Root
  $fallback = "Auto: update ($timestamp)"

  if (-not $logFile) {
    return $fallback
  }

  $tail = @(Get-Content -LiteralPath $logFile.FullName -ErrorAction SilentlyContinue | Select-Object -Last 200)
  if ($tail.Count -eq 0) {
    return $fallback
  }

  $marker = $null
  for ($i = $tail.Length - 1; $i -ge 0; $i--) {
    $line = $tail[$i]
    if ($line -match '\[(INFO|BOOT)\]\s+(.+)$') {
      $candidate = $Matches[2].Trim()
      if ($candidate -and $candidate -notmatch '^(AUTO PUSH|PUSH|STATUS|PULL_REBASE|FETCH|CONFLICT|STARTWORK)$' -and
          $candidate -notmatch '^(Branch:|Upstream:|Ahead/Behind:|AUTO PUSH:|FETCH \+ PULL|FETCH --PRUNE|CONFLICT HELPER|Stash entries:|Commit message:)') {
        $marker = $candidate
        break
      }
    }
  }

  if (-not $marker) {
    $staged = Get-StagedChanges
    if ($staged.Count -gt 0) {
      $preview = ($staged | Select-Object -First 3) -join ", "
      if ($staged.Count -gt 3) {
        $preview += ", ..."
      }
      $marker = "update $preview"
    }
  }

  if (-not $marker) {
    $marker = "update"
  }

  return "Auto: $marker ($timestamp)"
}

function Show-RepositorySummary {
  $branch = Get-CurrentBranch
  $upstream = Get-UpstreamBranch

  if ($branch) {
    Say "[INFO] Branch: $branch"
  } else {
    Warn "Detached HEAD or branch name unavailable."
  }

  if ($upstream) {
    Say "[INFO] Upstream: $upstream"
    $aheadBehind = Invoke-Git -Arguments @("rev-list", "--left-right", "--count", "HEAD...@{u}") -AllowFailure -Silent
    if ($aheadBehind.ExitCode -eq 0 -and $aheadBehind.Output.Count -gt 0) {
      $counts = (($aheadBehind.Output | Select-Object -First 1) -as [string]).Trim() -split '\s+'
      if ($counts.Length -ge 2) {
        Say "[INFO] Ahead/Behind: $($counts[0])/$($counts[1])"
      }
    }
  } else {
    Warn "No upstream configured for the current branch."
  }
}

function Ensure-SafeForSync {
  param([string]$Root)

  $state = Get-ConflictState $Root
  if ($state.InRebase -or $state.InMerge -or $state.HasUnmerged) {
    Fail "Repository has an unfinished merge/rebase. Use Conflict Helper first."
  }
}

function Invoke-Push {
  param([string]$Root)

  Push-Location $Root
  try {
    Ensure-SafeForSync $Root
    Say "[INFO] AUTO PUSH: stage -> commit if needed -> push"
    Show-RepositorySummary

    Invoke-Git -Arguments @("add", "-A") | Out-Null

    $changes = Get-WorkingTreeChanges
    $staged = Get-StagedChanges

    if ($changes.Count -eq 0) {
      Say "[OK] Working tree clean. Nothing to commit."
    } elseif ($staged.Count -eq 0) {
      Warn "Changes were detected, but nothing is staged after 'git add -A'. Check .gitignore or file permissions."
    } else {
      $message = Build-AutoCommitMessage $Root
      Say "[INFO] Commit message: $message"
      Invoke-Git -Arguments @("commit", "-m", $message) | Out-Null
    }

    $branch = Get-CurrentBranch
    if (-not $branch) {
      Fail "Cannot push from detached HEAD."
    }

    $upstream = Get-UpstreamBranch
    if ($upstream) {
      Invoke-Git -Arguments @("push") | Out-Null
      return
    }

    $remote = Get-DefaultRemote
    if (-not $remote) {
      Fail "No git remote configured. Add a remote before pushing."
    }

    Warn "No upstream found. Setting upstream to $remote/$branch."
    Invoke-Git -Arguments @("push", "--set-upstream", $remote, $branch) | Out-Null
  } finally {
    Pop-Location
  }
}

function Invoke-PullRebase {
  param([string]$Root)

  Push-Location $Root
  try {
    Ensure-SafeForSync $Root
    Say "[INFO] FETCH + PULL --REBASE --AUTOSTASH"
    Show-RepositorySummary
    Invoke-Git -Arguments @("status", "--short", "--branch") | Out-Null
    Invoke-Git -Arguments @("fetch", "--prune", "--tags", "--all") | Out-Null
    Invoke-Git -Arguments @("pull", "--rebase", "--autostash") | Out-Null
    Invoke-Git -Arguments @("status", "--short", "--branch") | Out-Null
  } finally {
    Pop-Location
  }
}

function Invoke-Fetch {
  param([string]$Root)

  Push-Location $Root
  try {
    Ensure-SafeForSync $Root
    Say "[INFO] FETCH --PRUNE --TAGS --ALL"
    Show-RepositorySummary
    Invoke-Git -Arguments @("fetch", "--prune", "--tags", "--all") | Out-Null
    Invoke-Git -Arguments @("remote", "-v") | Out-Null
  } finally {
    Pop-Location
  }
}

function Invoke-ConflictHelper {
  param([string]$Root)

  Push-Location $Root
  try {
    Say "[INFO] CONFLICT HELPER"
    Invoke-Git -Arguments @("status", "--short", "--branch") | Out-Null

    $state = Get-ConflictState $Root
    if (-not $state.InRebase -and -not $state.InMerge -and -not $state.HasUnmerged) {
      Say "[INFO] No merge/rebase in progress and no unmerged files detected."
      return
    }

    if ($state.UnmergedFiles.Count -gt 0) {
      Say "[INFO] Unmerged files:"
      foreach ($file in $state.UnmergedFiles) {
        Say "  $file"
      }
    }

    Say ""
    Say "Choose:"
    if ($state.InRebase) {
      Say "  C) Continue rebase"
      Say "  A) Abort rebase"
    }
    if ($state.InMerge) {
      Say "  M) Abort merge"
    }
    Say "  T) Open mergetool"
    Say "  E) Open repo folder"
    Say "  S) Show status again"
    Say "  X) Back"

    $choiceRaw = Read-Host "Select option"
    if ($null -eq $choiceRaw) {
      $choice = "X"
    } else {
      $choice = $choiceRaw.Trim().ToUpperInvariant()
    }
    switch ($choice) {
      "C" {
        if ($state.InRebase) {
          Invoke-Git -Arguments @("rebase", "--continue") | Out-Null
        } else {
          Warn "Rebase is not in progress."
        }
      }
      "A" {
        if ($state.InRebase) {
          Invoke-Git -Arguments @("rebase", "--abort") | Out-Null
        } else {
          Warn "Rebase is not in progress."
        }
      }
      "M" {
        if ($state.InMerge) {
          Invoke-Git -Arguments @("merge", "--abort") | Out-Null
        } else {
          Warn "Merge is not in progress."
        }
      }
      "T" { Invoke-Git -Arguments @("mergetool") | Out-Null }
      "E" {
        try {
          Start-Process explorer.exe $Root | Out-Null
        } catch {
          Warn "Could not open Explorer: $($_.Exception.Message)"
        }
      }
      "S" { Invoke-Git -Arguments @("status", "--short", "--branch") | Out-Null }
      default { }
    }
  } finally {
    Pop-Location
  }
}

function Invoke-Status {
  param([string]$Root)

  Push-Location $Root
  try {
    Show-RepositorySummary
    Invoke-Git -Arguments @("status", "--short", "--branch") | Out-Null
    Invoke-Git -Arguments @("log", "--oneline", "--decorate", "-n", "10") | Out-Null
    Invoke-Git -Arguments @("remote", "-v") | Out-Null
    $stash = Invoke-Git -Arguments @("stash", "list") -AllowFailure -Silent
    if ($stash.ExitCode -eq 0) {
      $count = @($stash.Output | Where-Object { $_ }).Count
      Say "[INFO] Stash entries: $count"
    }
  } finally {
    Pop-Location
  }
}

Ensure-ProjectRoot $ProjectRoot
Ensure-GitAvailable
Ensure-GitRepository $ProjectRoot

switch ($Action.ToUpperInvariant()) {
  "PUSH"        { Invoke-Push $ProjectRoot }
  "PULL_REBASE" { Invoke-PullRebase $ProjectRoot }
  "CONFLICT"    { Invoke-ConflictHelper $ProjectRoot }
  "STATUS"      { Invoke-Status $ProjectRoot }
  "FETCH"       { Invoke-Fetch $ProjectRoot }
  default       { Fail "Unknown action: $Action" 2 }
}
