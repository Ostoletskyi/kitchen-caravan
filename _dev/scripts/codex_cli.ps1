param(
  [Parameter(Mandatory=$true)][string]$ProjectRoot
)

$ProjectRoot = [string]$ProjectRoot

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Say([string]$m){ Write-Host $m }
function Exists([string]$name){ return [bool](Get-Command $name -ErrorAction SilentlyContinue) }

Say "============================"
Say "Codex CLI Mode"
Say "Project: $ProjectRoot"
Say "============================"
Say ""

$cmdCandidates = @("codex", "openai")
$cmd = $null
foreach ($c in $cmdCandidates) { if (Exists $c) { $cmd = $c; break } }

if (-not $cmd) {
  Say "[ERROR] No Codex-capable CLI found in PATH."
  Say "Tried: codex, openai"
  exit 1
}

Say "[OK] Found CLI: $cmd"
Say ""

function Menu {
  Say "Choose an action:"
  Say "  1) Open interactive shell in project root (recommended)"
  Say "  2) Show CLI help (--help)"
  Say "  3) Show git status + diff (review before Codex)"
  Say "  4) Create safe Codex branch (codex/session-YYYYMMDD_HHMM)"
  Say "  0) Back"
}

Push-Location $ProjectRoot
try {
  while ($true) {
    Menu
    $choice = Read-Host "Select 1/2/3/4/0"
    switch ($choice) {
      "1" {
        $pwsh = (Get-Command pwsh.exe -ErrorAction SilentlyContinue).Source
        if (-not $pwsh) { $pwsh = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source }
        if (-not $pwsh) { Say "[ERROR] PowerShell not found."; continue }

        Say "[INFO] Opening shell..."
        $banner = @"
cd `"$ProjectRoot`"
Write-Host '--- Codex CLI Shell ---'
Write-Host 'Tips:'
Write-Host '  git status'
Write-Host '  git diff'
Write-Host '  $cmd --help'
Write-Host '  (run your Codex CLI command here)'
"@
        Start-Process -FilePath $pwsh -ArgumentList @("-NoExit","-Command",$banner) -WorkingDirectory $ProjectRoot | Out-Null
      }
      "2" { Say ">> $cmd --help"; & $cmd --help }
      "3" {
        if (Get-Command git -ErrorAction SilentlyContinue) {
          Say ">> git status"; git status
          Say ""; Say ">> git diff"; git diff
        } else { Say "[WARN] git not found." }
      }
      "4" {
        if (-not (Get-Command git -ErrorAction SilentlyContinue)) { Say "[WARN] git not found."; continue }
        $stamp = Get-Date -Format "yyyyMMdd_HHmm"
        $branch = "codex/session-$stamp"
        Say ">> git checkout -b $branch"
        git checkout -b $branch
        git status
      }
      "0" { break }
      default { Say "Unknown choice." }
    }
    Say ""
  }
} finally { Pop-Location }
