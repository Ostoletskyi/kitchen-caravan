param(
  [Parameter(Mandatory=$true)][string]$ProjectRoot,
  [Parameter(Mandatory=$true)][string]$LogFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Log([string]$msg) {
  $stamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
  $line = "[$stamp] $msg"
  Write-Host $line
  Add-Content -Path $LogFile -Value $line
}

function Read-Config([string]$path) {
  if (Test-Path $path) {
    return (Get-Content $path -Raw | ConvertFrom-Json)
  }
  return $null
}

function Command-Exists([string]$name) {
  return [bool](Get-Command $name -ErrorAction SilentlyContinue)
}

function Find-VSWhere() {
  $candidates = @(
    "$env:ProgramFiles(x86)\Microsoft Visual Studio\Installer\vswhere.exe",
    "$env:ProgramFiles\Microsoft Visual Studio\Installer\vswhere.exe"
  )
  foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
  return $null
}

function Find-Devenv([string]$explicitPath) {
  if ($explicitPath -and (Test-Path $explicitPath)) { return $explicitPath }

  $vswhere = Find-VSWhere
  if (-not $vswhere) { return $null }

  try {
    $installPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($installPath) {
      $devenv = Join-Path $installPath "Common7\IDE\devenv.exe"
      if (Test-Path $devenv) { return $devenv }
    }
  } catch { }
  return $null
}

function Find-UnityHub([string]$explicitPath) {
  if ($explicitPath -and (Test-Path $explicitPath)) { return $explicitPath }

  $candidates = @(
    "$env:ProgramFiles\Unity Hub\Unity Hub.exe",
    "$env:ProgramFiles(x86)\Unity Hub\Unity Hub.exe",
    "$env:LocalAppData\Programs\Unity Hub\Unity Hub.exe"
  )
  foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
  return $null
}

function Find-Solution([string]$root, [string]$relativePath) {
  if ($relativePath) {
    $p = Join-Path $root $relativePath
    if (Test-Path $p) { return $p }
  }

  $direct = Get-ChildItem -Path $root -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($direct) { return $direct.FullName }

  $deep = Get-ChildItem -Path $root -Filter *.sln -File -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($deep) { return $deep.FullName }

  return $null
}

function Start-WindowsTerminalTabs($root, $tabs) {
  $wt = (Get-Command wt.exe -ErrorAction SilentlyContinue)
  if ($wt) {
    Log "Opening Windows Terminal tabs..."

    $pwshPath = (Get-Command pwsh.exe -ErrorAction SilentlyContinue).Source
    if (-not $pwshPath) {
      $pwshPath = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source
    }
    if (-not $pwshPath) {
      Log "WARN: No PowerShell executable found. Skipping terminal launch."
      return
    }

    # Build a single argument string with robust quoting (Windows Terminal is picky).
    $parts = @()
    $first = $true

    foreach ($t in $tabs) {
      $title = [string]$t.title
      $cmd = [string]$t.command

      # Escape quotes for PowerShell string interpolation
      $title = $title.Replace('"', '\"')
      $cmd = $cmd.Replace('"', '\"')

      $oneTab = "new-tab --title \"$title\" \"$pwshPath\" -NoExit -Command \"$cmd\""
      if ($first) {
        $parts += $oneTab
        $first = $false
      } else {
        $parts += "; $oneTab"
      }
    }

    $argString = ($parts -join " ")
    try {
      Start-Process -FilePath "wt.exe" -ArgumentList $argString -WorkingDirectory $root | Out-Null
    } catch {
      Log ("WARN: Failed to launch Windows Terminal. Falling back to classic PowerShell windows. Details: " + $_.Exception.Message)
      foreach ($t in $tabs) {
        Start-Process -FilePath $pwshPath -ArgumentList @("-NoExit","-Command",$t.command) -WorkingDirectory $root | Out-Null
      }
    }
  } else {
    Log "Windows Terminal not found. Opening classic PowerShell windows..."
    foreach ($t in $tabs) {
      Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoExit","-Command",$t.command) -WorkingDirectory $root | Out-Null
    }
  }
}

function Run-GitOps($root, $cfg) {
  if (-not (Command-Exists "git")) {
    Log "WARN: git not found in PATH."
    return
  }

  Log "Git status:"
  Push-Location $root
  try {
    & git status | ForEach-Object { Log "  $_" }

    if ($cfg -and $cfg.git -and $cfg.git.autoPull -eq $true) {
      $remote = $cfg.git.remote; if (-not $remote) { $remote = "origin" }
      $branch = $cfg.git.branch

      if (-not $branch -or $branch.Trim().Length -eq 0) {
        $branch = (& git rev-parse --abbrev-ref HEAD).Trim()
      }

      Log "Git pull --rebase from $remote/$branch ..."
      & git pull --rebase $remote $branch | ForEach-Object { Log "  $_" }
    } else {
      Log "Git autoPull disabled (config)."
    }
  } finally {
    Pop-Location
  }
}

function Check-Tools() {
  $checks = @(
    @{ name="git"; cmd="git --version" },
    @{ name="dotnet"; cmd="dotnet --info" },
    @{ name="node"; cmd="node -v" },
    @{ name="npm"; cmd="npm -v" }
  )

  foreach ($c in $checks) {
    $name = $c.name
    $cmd = $c.cmd
    if (Command-Exists $name) {
      Log "OK: $name found."
      try {
        $out = Invoke-Expression $cmd 2>$null
        if ($out) { ($out | Select-Object -First 5) | ForEach-Object { Log "  $_" } }
      } catch {
        Log "WARN: $name exists but command failed: $cmd"
      }
    } else {
      Log "WARN: $name not found in PATH."
    }
  }
}

function Open-Explorer($root) {
  Log "Opening project folder in Explorer..."
  Start-Process -FilePath "explorer.exe" -ArgumentList @("$root") | Out-Null
}

function Start-Unity($root, $cfg) {
  $hubPath = Find-UnityHub ($cfg.unity.unityHubPath)
  if (-not $hubPath) {
    Log "WARN: Unity Hub not found."
    return
  }

  Log "Starting Unity Hub: $hubPath"
  Start-Process -FilePath $hubPath | Out-Null

  if ($cfg.unity.openProject -eq $true) {
    Log "Unity openProject enabled. (Auto-open is version-dependent; Hub started.)"
  }
}

function Start-VisualStudio($root, $cfg) {
  $devenv = Find-Devenv ($cfg.visualStudio.devenvPath)
  if (-not $devenv) {
    Log "WARN: Visual Studio devenv.exe not found (vswhere missing or VS not installed)."
    return
  }

  $sln = Find-Solution $root ($cfg.visualStudio.solutionRelativePath)
  if (-not $sln) {
    Log "WARN: No .sln file found. Skipping Visual Studio open."
    return
  }

  Log "Starting Visual Studio: $devenv"
  Log "Opening solution: $sln"
  Start-Process -FilePath $devenv -ArgumentList @("`"$sln`"") -WorkingDirectory $root | Out-Null
}

try {
  $configPath = Join-Path $ProjectRoot "_dev\dev.config.json"
  $cfg = Read-Config $configPath
  if (-not $cfg) { throw "Config not found: $configPath" }

  Log "Bootstrap begin."
  Open-Explorer $ProjectRoot

  Log "Checking tools..."
  Check-Tools

  Log "Running Git operations..."
  Run-GitOps $ProjectRoot $cfg

  if ($cfg.openWindowsTerminal -eq $true -and $cfg.terminalsToOpen) {
    Start-WindowsTerminalTabs $ProjectRoot $cfg.terminalsToOpen
  } else {
    Log "Terminal auto-open disabled (config)."
  }

  Start-Unity $ProjectRoot $cfg
  Start-VisualStudio $ProjectRoot $cfg

  Log "Bootstrap end."
  exit 0
}
catch {
  Log "ERROR: $($_.Exception.Message)"
  Log "Stack: $($_.ScriptStackTrace)"
  exit 1
}
