param(
  [Parameter(Mandatory=$true)][string]$ProjectRoot
)

$ProjectRoot = [string]$ProjectRoot

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

try {
  [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
  $OutputEncoding = [Console]::OutputEncoding
} catch {}

function Say([string]$m){ Write-Host $m }

function Find-UnityHub {
  $candidates = @(
    "$env:ProgramFiles\Unity Hub\Unity Hub.exe",
    "$env:ProgramFiles(x86)\Unity Hub\Unity Hub.exe",
    "$env:LocalAppData\Programs\Unity Hub\Unity Hub.exe"
  )
  foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
  return $null
}

function Find-VSWhere {
  $candidates = @(
    "$env:ProgramFiles(x86)\Microsoft Visual Studio\Installer\vswhere.exe",
    "$env:ProgramFiles\Microsoft Visual Studio\Installer\vswhere.exe"
  )
  foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
  return $null
}

function Find-Devenv {
  $fixedCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
    "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\devenv.exe",
    "C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\devenv.exe",
    "C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\devenv.exe"
  )
  foreach ($p in $fixedCandidates) { if (Test-Path $p) { return $p } }

  $vswhere = Find-VSWhere
  if (-not $vswhere) { return $null }
  try {
    $installPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($installPath) {
      $devenv = Join-Path $installPath "Common7\IDE\devenv.exe"
      if (Test-Path $devenv) { return $devenv }
    }
  } catch {}
  return $null
}

function Find-Solution([string]$root) {
  $direct = Get-ChildItem -Path $root -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($direct) { return $direct.FullName }
  $deep = Get-ChildItem -Path $root -Filter *.sln -File -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($deep) { return $deep.FullName }
  return $null
}

if (-not $ProjectRoot -or -not (Test-Path $ProjectRoot)) {
  Say "[ERROR] ProjectRoot is missing or invalid: '$ProjectRoot'"
  exit 1
}

Say "[INFO] Opening project folder..."
Start-Process explorer.exe $ProjectRoot | Out-Null

Say "[INFO] Tool check (optional):"
if (Get-Command git -ErrorAction SilentlyContinue) { git --version }
if (Get-Command dotnet -ErrorAction SilentlyContinue) { dotnet --version }

Say "[INFO] Opening PowerShell window in project root..."
$pwsh = (Get-Command pwsh.exe -ErrorAction SilentlyContinue).Source
if (-not $pwsh) { $pwsh = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source }
if ($pwsh) {
  Start-Process -FilePath $pwsh -ArgumentList @("-NoExit","-Command","cd `"$ProjectRoot`"") -WorkingDirectory $ProjectRoot | Out-Null
} else {
  Say "[WARN] PowerShell executable not found."
}

$hub = Find-UnityHub
if ($hub) {
  Say "[INFO] Starting Unity Hub..."
  Start-Process -FilePath $hub | Out-Null
} else {
  Say "[WARN] Unity Hub not found."
}

$devenv = Find-Devenv
$sln = Find-Solution $ProjectRoot

if ($devenv -and $sln) {
  Say "[INFO] Starting Visual Studio..."
  Start-Process -FilePath $devenv -ArgumentList @("`"$sln`"") -WorkingDirectory $ProjectRoot | Out-Null
} else {
  if (-not $devenv) { Say "[WARN] Visual Studio not found (devenv.exe)." }
  if (-not $sln) { Say "[WARN] Solution (.sln) not found." }
}
