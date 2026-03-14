[CmdletBinding()]
param(
    [string]$ProjectRoot = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
)

$ErrorActionPreference = 'Stop'

$script:EncodingProfiles = @(
    @{
        Name = 'UTF-8'
        CodePage = 65001
        Encoding = New-Object System.Text.UTF8Encoding($false)
    },
    @{
        Name = 'OEM 866'
        CodePage = 866
        Encoding = [System.Text.Encoding]::GetEncoding(866)
    },
    @{
        Name = 'Windows-1251'
        CodePage = 1251
        Encoding = [System.Text.Encoding]::GetEncoding(1251)
    }
)
$script:CurrentEncodingIndex = 0

$devDir = Join-Path $ProjectRoot '_dev'
$scriptsDir = Join-Path $devDir 'scripts'
$logsDir = Join-Path $devDir 'logs'
$gitOps = Join-Path $scriptsDir 'git_ops.ps1'
$bootstrap = Join-Path $scriptsDir 'bootstrap_start.ps1'

$helpText = @'
============================
KitchenCaravan DevOps Guide
============================

1) Push
Назначение:
Отправляет ваши локальные коммиты в удалённый репозиторий. Перед отправкой убедитесь, что все нужные изменения уже закоммичены.

Когда использовать:
Когда вы завершили логический кусок работы и хотите сохранить его в общей ветке или опубликовать изменения для команды.

Что важно помнить:
Если есть незакоммиченные файлы, сначала проверьте Status, затем сделайте commit вручную. Если upstream для ветки ещё не настроен, launcher должен настроить его автоматически.

2) Pull --rebase --autostash
Назначение:
Синхронизирует вашу текущую ветку с удалённой: сначала получает свежие изменения, затем аккуратно накладывает ваши локальные коммиты поверх новых коммитов с сервера.

Когда использовать:
В начале рабочего дня, перед Push, перед началом сложной задачи и всегда, когда нужно убедиться, что вы работаете на актуальной истории ветки.

Что важно помнить:
Параметр --rebase держит историю чище, чем обычный merge. Параметр --autostash временно прячет незакоммиченные изменения, если это возможно, и возвращает их после синхронизации.

3) Start Work
Назначение:
Запускает рабочее окружение: проверяет bootstrap-инструменты, открывает нужные окна и подготавливает Codex / helper-сценарии для старта работы с проектом.

Когда использовать:
Обычно один раз в начале сессии. Это основной пункт для старта рабочего дня, если нужно быстро поднять все DevOps-инструменты проекта.

Что важно помнить:
Этот пункт не заменяет понимание состояния репозитория. Если вчера были конфликты или незавершённый rebase, сначала используйте Status или Conflict Helper.

4) Conflict Helper
Назначение:
Помогает безопасно завершить ситуацию с конфликтами после pull --rebase, merge или ручного разрешения файлов.

Когда использовать:
Когда Git сообщает о конфликте, rebase остановился, merge не завершён или нужно открыть configured mergetool.

Что важно помнить:
Используйте этот пункт только когда действительно есть конфликтное состояние. Он помогает сделать continue, abort или открыть инструмент слияния, в зависимости от текущего состояния репозитория.

5) Status
Назначение:
Показывает состояние репозитория: текущую ветку, связь с upstream, отставание или опережение, недавние коммиты, remotes и stash.

Когда использовать:
Это самый безопасный диагностический пункт. Используйте его перед Pull, перед Push, после разрешения конфликтов и в любой непонятной ситуации.

Что важно помнить:
Если не уверены, что делать дальше, начните со Status. Он даёт минимум риска и максимум контекста.

6) Fetch --all --prune --tags
Назначение:
Обновляет информацию о всех удалённых ветках и тегах без изменения вашей рабочей директории и без попытки влить изменения в текущую ветку.

Когда использовать:
Когда нужно просто подтянуть свежую информацию с сервера, проверить новые ветки, обновить теги или очистить локальные ссылки на уже удалённые remote-ветки.

Что важно помнить:
Fetch безопаснее Pull, потому что он не меняет ваши файлы. Это хороший шаг перед анализом состояния репозитория.

7) Help / Manual
Назначение:
Показывает эту встроенную инструкцию прямо в консоли, чтобы под рукой всегда была памятка по командам launcher и безопасному рабочему процессу.

Когда использовать:
Когда забыли назначение пункта меню, порядок действий или хотите быстро освежить типовой сценарий работы.

0) Exit
Назначение:
Закрывает launcher без выполнения Git-операций и без запуска bootstrap-сценариев.

Когда использовать:
Когда работа завершена или вы открыли launcher по ошибке.

----------------------------------------
Типовой безопасный порядок работы
----------------------------------------
1. В начале дня выполните Status, чтобы понять текущее состояние ветки и наличие незавершённых операций.
2. Затем выполните Pull --rebase --autostash, чтобы синхронизироваться с удалённым репозиторием.
3. После синхронизации используйте Start Work, если нужно поднять инструменты, Codex и вспомогательное окружение.
4. Во время работы периодически смотрите Status, особенно перед commit и перед отправкой изменений.
5. Когда работа завершена и коммиты готовы, выполните Push.
6. Если во время синхронизации возник конфликт, не пушьте сразу: сначала завершите его через Conflict Helper, потом снова проверьте Status.

----------------------------------------
Когда какую команду выбирать
----------------------------------------
Status:
Когда нужно понять, что происходит в репозитории прямо сейчас.

Fetch:
Когда нужно получить свежую информацию с сервера, но без изменения локальной ветки.

Pull --rebase:
Когда нужно встроить свежие удалённые изменения в свою рабочую ветку перед продолжением разработки.

Push:
Когда ваши локальные коммиты готовы к публикации.

Conflict Helper:
Когда Git уже сообщил о конфликте или незавершённом rebase/merge.

Start Work:
Когда вы начинаете рабочую сессию и хотите быстро развернуть всё нужное окружение проекта.
'@

function Ensure-Path {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Set-ConsoleEncoding {
    param([int]$Index)

    if ($Index -lt 0 -or $Index -ge $script:EncodingProfiles.Count) {
        return
    }

    $profile = $script:EncodingProfiles[$Index]
    $script:CurrentEncodingIndex = $Index

    cmd /c "chcp $($profile.CodePage)" > $null
    [Console]::InputEncoding = $profile.Encoding
    [Console]::OutputEncoding = $profile.Encoding
    $global:OutputEncoding = $profile.Encoding
}

function Get-CurrentEncodingProfile {
    return $script:EncodingProfiles[$script:CurrentEncodingIndex]
}

function Cycle-ConsoleEncoding {
    $nextIndex = ($script:CurrentEncodingIndex + 1) % $script:EncodingProfiles.Count
    Set-ConsoleEncoding -Index $nextIndex
}

function Get-BranchName {
    try {
        $branch = git -C $ProjectRoot branch --show-current 2>$null
        if ($LASTEXITCODE -eq 0 -and $branch) {
            return ($branch | Select-Object -First 1).Trim()
        }
    } catch {
    }

    return $null
}

function Show-Menu {
    Clear-Host
    $encodingProfile = Get-CurrentEncodingProfile
    Write-Host '=============================================='
    Write-Host 'KitchenCaravan DevOps Menu'
    Write-Host "Project: $ProjectRoot"
    Write-Host "Log: $script:LogFile"
    Write-Host "Encoding: $($encodingProfile.Name) (code page $($encodingProfile.CodePage))"

    $branch = Get-BranchName
    if ($branch) {
        Write-Host "Branch: $branch"
    } else {
        Write-Host 'Branch: <unavailable>'
    }

    Write-Host '=============================================='
    Write-Host '1) Push (commit first if needed)'
    Write-Host '2) Pull --rebase --autostash (sync)'
    Write-Host '3) Start Work (bootstrap + tools + Codex + prompt helper)'
    Write-Host '4) Conflict Helper (continue/abort/mergetool)'
    Write-Host '5) Status (branch + commits + remotes + stash)'
    Write-Host '6) Fetch --all --prune --tags'
    Write-Host '7) Help / Manual'
    Write-Host 'C) Cycle Console Encoding'
    Write-Host '0) Exit'
    Write-Host '=============================================='
}

function Invoke-GitOps {
    param(
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][int]$TailLines
    )

    Add-Content -LiteralPath $script:LogFile -Value "[INFO] $Mode"
    & powershell -NoProfile -ExecutionPolicy Bypass -File $gitOps -ProjectRoot $ProjectRoot -Mode $Mode *>> $script:LogFile
    $exitCode = $LASTEXITCODE

    Write-Host '--- Last lines of log ---'
    Get-Content -LiteralPath $script:LogFile -Tail $TailLines

    if ($exitCode -ne 0) {
        Write-Host ''
        Write-Host "[WARN] Operation finished with exit code $exitCode."
    }

    Read-Host 'Press Enter to continue' | Out-Null
}

function Invoke-StartWork {
    Add-Content -LiteralPath $script:LogFile -Value '[INFO] STARTWORK'

    if (-not (Test-Path -LiteralPath $bootstrap)) {
        Write-Host "[ERROR] Missing script: $bootstrap"
        Read-Host 'Press Enter to continue' | Out-Null
        return
    }

    & powershell -NoProfile -ExecutionPolicy Bypass -File $bootstrap -ProjectRoot $ProjectRoot -LogFile $script:LogFile
    $exitCode = $LASTEXITCODE

    Write-Host '--- Last lines of log ---'
    Get-Content -LiteralPath $script:LogFile -Tail 80

    if ($exitCode -ne 0) {
        Write-Host ''
        Write-Host "[WARN] Start Work finished with exit code $exitCode."
    }

    Read-Host 'Press Enter to continue' | Out-Null
}

function Show-HelpManual {
    while ($true) {
        Clear-Host
        $encodingProfile = Get-CurrentEncodingProfile

        Write-Host '=============================================='
        Write-Host "Help / Manual"
        Write-Host "Encoding: $($encodingProfile.Name) (code page $($encodingProfile.CodePage))"
        Write-Host 'F8 or C: switch encoding and redraw help'
        Write-Host 'Enter or Esc: return to menu'
        Write-Host '=============================================='
        Write-Host ''
        Write-Host $helpText
        Write-Host ''

        if ([Console]::IsInputRedirected) {
            return
        }

        try {
            $key = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
        } catch {
            Read-Host 'Press Enter to continue' | Out-Null
            return
        }

        if ($key.VirtualKeyCode -eq 13 -or $key.VirtualKeyCode -eq 27) {
            return
        }

        if ($key.VirtualKeyCode -eq 119 -or $key.Character -in @('c', 'C', 'с', 'С')) {
            Cycle-ConsoleEncoding
        }
    }
}

if (-not (Test-Path -LiteralPath $ProjectRoot)) {
    Write-Host "[ERROR] Project root not found: $ProjectRoot"
    exit 1
}

Ensure-Path -Path $devDir
Ensure-Path -Path $scriptsDir
Ensure-Path -Path $logsDir
Set-ConsoleEncoding -Index 0

if (-not (Test-Path -LiteralPath $gitOps)) {
    Write-Host "[ERROR] Missing script: $gitOps"
    exit 1
}

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$script:LogFile = Join-Path $logsDir "launcher_$stamp.log"

[System.IO.File]::WriteAllLines(
    $script:LogFile,
    [string[]]@(
        '=================================================='
        "KitchenCaravan Launcher started at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        "Project: $ProjectRoot"
        '=================================================='
    ),
    (New-Object System.Text.UTF8Encoding($false))
)

while ($true) {
    Show-Menu
    $choice = (Read-Host 'Select an option [1,2,3,4,5,6,7,0,C]').Trim()

    switch ($choice) {
        '1' { Invoke-GitOps -Mode 'PUSH' -TailLines 50 }
        '2' { Invoke-GitOps -Mode 'PULL_REBASE' -TailLines 50 }
        '3' { Invoke-StartWork }
        '4' { Invoke-GitOps -Mode 'CONFLICT' -TailLines 60 }
        '5' { Invoke-GitOps -Mode 'STATUS' -TailLines 40 }
        '6' { Invoke-GitOps -Mode 'FETCH' -TailLines 40 }
        '7' { Show-HelpManual }
        'c' { Cycle-ConsoleEncoding }
        'C' { Cycle-ConsoleEncoding }
        '0' {
            Write-Host 'Bye.'
            exit 0
        }
        default {
            Write-Host ''
            Write-Host '[WARN] Unknown option.'
            Start-Sleep -Seconds 1
        }
    }
}
