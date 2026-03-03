param(
  [Parameter(Mandatory=$true)][string]$ProjectRoot,
  [Parameter(Mandatory=$true)][string]$LogFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

try {
  [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
  $OutputEncoding = [Console]::OutputEncoding
} catch {}

function LogLine([string]$m){
  $ts = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
  $line = '[' + $ts + '] ' + $m
  Write-Host $line
  try { Add-Content -Path $LogFile -Value $line -Encoding UTF8 } catch {}
}

function HasCmd([string]$name){
  return [bool](Get-Command $name -ErrorAction SilentlyContinue)
}

function SuggestInstall([string]$tool, [string]$hint){
  LogLine ('[MISSING] ' + $tool)
  LogLine ('          ' + $hint)
}

function Find-UnityHub {
  $pf = $env:ProgramFiles
  $pfx86 = [Environment]::GetEnvironmentVariable('ProgramFiles(x86)')

  $candidates = @()
  if ($pf)   { $candidates += (Join-Path $pf 'Unity Hub\\Unity Hub.exe') }
  if ($pfx86){ $candidates += (Join-Path $pfx86 'Unity Hub\\Unity Hub.exe') }
  $candidates += (Join-Path $env:LocalAppData 'Programs\\Unity Hub\\Unity Hub.exe')

  foreach ($p in $candidates) {
    if ($p -and (Test-Path $p)) { return $p }
  }
  return $null
}

function Find-VSWhere {
  $candidates = @(
    (Join-Path $env:ProgramFiles(x86) 'Microsoft Visual Studio\Installer\vswhere.exe'),
    (Join-Path $env:ProgramFiles 'Microsoft Visual Studio\Installer\vswhere.exe')
  )
  foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
  return $null
}

function Find-Devenv {
  $fixed = @(
    'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe',
    'C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe',
    'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe'
  )
  foreach ($p in $fixed) { if (Test-Path $p) { return $p } }

  $vswhere = Find-VSWhere
  if ($vswhere) {
    try {
      $installPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
      if ($installPath) {
        $devenv = Join-Path $installPath 'Common7\IDE\devenv.exe'
        if (Test-Path $devenv) { return $devenv }
      }
    } catch {}
  }
  return $null
}

function Find-Solution([string]$root){
  $direct = Get-ChildItem -Path $root -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($direct) { return $direct.FullName }
  $deep = Get-ChildItem -Path $root -Filter *.sln -File -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($deep) { return $deep.FullName }
  return $null
}

function Get-PwshPath {
  $p = (Get-Command pwsh.exe -ErrorAction SilentlyContinue).Source
  if (-not $p) { $p = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source }
  return $p
}

function Open-PwshWindow([string]$root){
  $pwsh = Get-PwshPath
  if (-not $pwsh) { return $false }
  Start-Process -FilePath $pwsh -WorkingDirectory $root -ArgumentList @('-NoExit','-Command', ('Set-Location -LiteralPath "' + $root + '"')) | Out-Null
  return $true
}

function Open-CodexShell([string]$root){
  $pwsh = Get-PwshPath
  if (-not $pwsh) { return $false }

  $codex = Get-Command codex -ErrorAction SilentlyContinue
  if (-not $codex) { return $false }

  $shellScript = Join-Path (Split-Path -Parent $PSCommandPath) 'codex_shell.ps1'
  if (-not (Test-Path $shellScript)) { return $false }

  Start-Process -FilePath $pwsh -WorkingDirectory $root -ArgumentList @(
    '-NoExit',
    '-ExecutionPolicy','Bypass',
    '-File', $shellScript,
    '-ProjectRoot', $root
  ) | Out-Null
  return $true
}

# ---------------- Main ----------------
if (-not (Test-Path -LiteralPath $ProjectRoot)) {
  LogLine ('[ERROR] Project root not found: ' + $ProjectRoot)
  exit 1
}

LogLine ('[BOOT] Project: ' + $ProjectRoot)
LogLine '[BOOT] Checking tools...'

if (HasCmd 'git') { LogLine ('[OK] git: ' + (git --version)) }
else { SuggestInstall 'git' 'winget install --id Git.Git -e   (or: choco install git -y)' }

if (HasCmd 'dotnet') { LogLine ('[OK] dotnet: ' + (& dotnet --version)) }
else { SuggestInstall '.NET SDK' 'winget install --id Microsoft.DotNet.SDK.8 -e   (or install the SDK you need)' }

if (HasCmd 'pwsh') { try { LogLine ('[OK] pwsh: ' + (pwsh -v)) } catch { LogLine '[OK] pwsh: installed' } }
else { SuggestInstall 'PowerShell 7' 'winget install --id Microsoft.PowerShell -e   (or: choco install powershell-core -y)' }

$codexCmd = Get-Command codex -ErrorAction SilentlyContinue
if ($codexCmd) { LogLine ('[OK] codex: ' + $codexCmd.Source) }
else { SuggestInstall 'codex CLI' 'If installed via npm: npm i -g codex (or your codex package). Ensure %APPDATA%\npm is in PATH.' }

$hub = Find-UnityHub
if ($hub) { LogLine ('[OK] Unity Hub: ' + $hub) }
else { SuggestInstall 'Unity Hub' 'Install Unity Hub from Unity.com, then re-run Start Work.' }

$devenv = Find-Devenv
if ($devenv) { LogLine ('[OK] Visual Studio: ' + $devenv) }
else { SuggestInstall 'Visual Studio' 'Install VS Community with "Game development with Unity" workload.' }

LogLine '[BOOT] Opening tools...'
Start-Process explorer.exe $ProjectRoot | Out-Null

if (Open-PwshWindow $ProjectRoot) { LogLine '[OK] Opened PowerShell in project root.' }
else { LogLine '[WARN] Could not open PowerShell window.' }

if ($hub) { Start-Process -FilePath $hub | Out-Null; LogLine '[OK] Unity Hub started.' }

$sln = Find-Solution $ProjectRoot
if ($devenv -and $sln) {
  LogLine ('[BOOT] Solution: ' + $sln)
  Start-Process -FilePath $devenv -WorkingDirectory $ProjectRoot -ArgumentList @('"' + $sln + '"') | Out-Null
  LogLine ('[OK] Visual Studio started: ' + $sln)
} elseif (-not $sln) {
  LogLine '[WARN] No .sln found.'
}

LogLine '[BOOT] Ensuring Codex helper dir...'
$codexDir = Join-Path (Split-Path -Parent $LogFile) '..\codex'
try { $codexDir = (Resolve-Path $codexDir).Path } catch { }
try { New-Item -ItemType Directory -Force -Path $codexDir | Out-Null } catch {}
LogLine ('[BOOT] Codex helper dir: ' + $codexDir)

LogLine '[BOOT] Opening Codex shell...'
if (Open-CodexShell $ProjectRoot) { LogLine '[OK] Codex shell window started.' }
else { LogLine '[WARN] Codex shell could not be started (codex not on PATH?).' }

LogLine '[BOOT] Done.'
