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

$helpSections = [ordered]@{
    '1' = @{
        Title = 'Push'
        Text = @'
============================
Help: Push
============================

Назначение:
Отправляет ваши локальные коммиты в удалённый репозиторий.

Когда использовать:
Когда вы завершили логический кусок работы и хотите опубликовать изменения для команды.

Что важно помнить:
- Если есть незакоммиченные файлы, сначала проверьте Status и сделайте commit вручную.
- Перед Push обычно стоит выполнить Pull --rebase.
- Если upstream для ветки ещё не настроен, launcher должен настроить его автоматически.
'@
    }
    '2' = @{
        Title = 'Pull --rebase'
        Text = @'
============================
Help: Pull --rebase --autostash
============================

Назначение:
Синхронизирует вашу текущую ветку с удалённой и накладывает локальные коммиты поверх свежей истории.

Когда использовать:
В начале рабочего дня, перед Push, перед началом сложной задачи и после долгого перерыва в работе.

Что важно помнить:
- --rebase делает историю чище, чем обычный merge.
- --autostash временно убирает незакоммиченные изменения и возвращает их после синхронизации.
- Если возник конфликт, переходите в Conflict Helper.
'@
    }
    '3' = @{
        Title = 'Start Work'
        Text = @'
============================
Help: Start Work
============================

Назначение:
Запускает рабочее окружение проекта: bootstrap-проверки, инструменты, Codex и вспомогательные окна.

Когда использовать:
Обычно один раз в начале рабочей сессии.

Что важно помнить:
- Это пункт для старта окружения, а не для диагностики Git-состояния.
- Если вчера был конфликт или незавершённый rebase, сначала используйте Status или Conflict Helper.
'@
    }
    '4' = @{
        Title = 'Conflict Helper'
        Text = @'
============================
Help: Conflict Helper
============================

Назначение:
Помогает безопасно завершить конфликт после pull --rebase, merge или ручного разрешения файлов.

Когда использовать:
Когда Git уже сообщил о конфликте, rebase остановился или merge не завершён.

Что важно помнить:
- Используйте этот пункт только при реальном конфликтном состоянии.
- Здесь обычно доступны continue, abort или запуск mergetool.
- После завершения конфликта обязательно проверьте Status.
'@
    }
    '5' = @{
        Title = 'Status / Fetch / Exit'
        Text = @'
============================
Help: Status / Fetch / Exit
============================

Status
Назначение:
Показывает текущую ветку, upstream, ahead/behind, недавние коммиты, remotes и stash.

Когда использовать:
Перед Pull, перед Push, после конфликтов и в любой непонятной ситуации.

Fetch --all --prune --tags
Назначение:
Обновляет информацию об удалённых ветках и тегах без изменения рабочей директории.

Когда использовать:
Когда нужно безопасно подтянуть свежие данные с сервера без вливания изменений в текущую ветку.

Exit
Назначение:
Закрывает launcher без Git-операций и без запуска bootstrap-сценариев.
'@
    }
    '6' = @{
        Title = 'Типовой Workflow'
        Text = @'
============================
Help: Типовой Workflow
============================

Рекомендуемый порядок работы:
1. В начале дня выполните Status.
2. Затем выполните Pull --rebase --autostash.
3. После синхронизации используйте Start Work.
4. Во время работы периодически проверяйте Status.
5. Когда коммиты готовы, выполните Push.
6. Если возник конфликт, завершите его через Conflict Helper и снова проверьте Status.

Базовое правило:
Если не уверены, что делать дальше, начните со Status. Это самый безопасный диагностический шаг.
'@
    }
    '7' = @{
        Title = 'Кодировка Console'
        Text = @'
============================
Help: Кодировка Console
============================

Зачем это нужно:
Если русские буквы отображаются некорректно, проблема обычно в текущей кодировке консоли.

Что делать:
- В главном меню нажмите C, чтобы переключить кодировку.
- В экране помощи тоже можно нажать C для смены кодировки.
- Подбирайте вариант, при котором кириллица читается корректно в вашей консоли.

Доступные профили:
- UTF-8
- OEM 866
- Windows-1251
'@
    }
}

function Ensure-Path {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Clear-ScreenSafe {
    if ([Console]::IsOutputRedirected) {
        return
    }

    try {
        Clear-Host
    } catch {
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
    Clear-ScreenSafe
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

function Show-HelpSection {
    param(
        [Parameter(Mandatory = $true)][string]$SectionKey
    )

    $section = $helpSections[$SectionKey]
    if (-not $section) {
        return
    }

    while ($true) {
        Clear-ScreenSafe
        $encodingProfile = Get-CurrentEncodingProfile

        Write-Host $section.Text
        Write-Host ''
        Write-Host "Encoding: $($encodingProfile.Name) (code page $($encodingProfile.CodePage))"
        Write-Host 'C: switch encoding'
        Write-Host 'Enter, Esc or 0: back to help menu'

        if ([Console]::IsInputRedirected) {
            Read-Host 'Press Enter to continue' | Out-Null
            return
        }

        try {
            $key = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
        } catch {
            Read-Host 'Press Enter to continue' | Out-Null
            return
        }

        if ($key.VirtualKeyCode -eq 13 -or $key.VirtualKeyCode -eq 27 -or $key.Character -eq '0') {
            return
        }

        if ($key.Character -in @('c', 'C', 'с', 'С')) {
            Cycle-ConsoleEncoding
        }
    }
}

function Show-HelpManual {
    while ($true) {
        Clear-ScreenSafe
        $encodingProfile = Get-CurrentEncodingProfile

        Write-Host '=============================================='
        Write-Host 'Help / Manual'
        Write-Host "Encoding: $($encodingProfile.Name) (code page $($encodingProfile.CodePage))"
        Write-Host 'Выберите раздел справки'
        Write-Host '=============================================='
        Write-Host '1) Push'
        Write-Host '2) Pull --rebase --autostash'
        Write-Host '3) Start Work'
        Write-Host '4) Conflict Helper'
        Write-Host '5) Status / Fetch / Exit'
        Write-Host '6) Типовой Workflow'
        Write-Host '7) Кодировка Console'
        Write-Host 'C) Switch Encoding'
        Write-Host '0) Back'
        Write-Host '=============================================='

        $choice = (Read-Host 'Select help section [1,2,3,4,5,6,7,C,0]').Trim()
        switch ($choice) {
        '1' { Show-HelpSection -SectionKey '1' }
        '2' { Show-HelpSection -SectionKey '2' }
        '3' { Show-HelpSection -SectionKey '3' }
        '4' { Show-HelpSection -SectionKey '4' }
        '5' { Show-HelpSection -SectionKey '5' }
        '6' { Show-HelpSection -SectionKey '6' }
        '7' { Show-HelpSection -SectionKey '7' }
        'c' { Cycle-ConsoleEncoding }
        'C' { Cycle-ConsoleEncoding }
        '0' { return }
        default {
            Write-Host ''
            Write-Host '[WARN] Unknown help option.'
            Start-Sleep -Seconds 1
            continue
        }
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
