<#
.SYNOPSIS
  建立並啟動「測試站台」：網站副本 + 測試資料副本 + 測試登入，供手動模擬與自動化測試使用。

.DESCRIPTION
  1. 以 Debug 編譯 TEEmployee（可用 -NoBuild 略過）
  2. 把網站複製到 TestEnv\.runtime\site（不含 App_Data）
  3. 建立種子資料 TestEnv\.runtime\seed\App_Data（第一次，或加 -RefreshSeed 時從專案 App_Data 複製）
  4. 把種子資料複製到測試站台的 App_Data（每次啟動都還原，可用 -KeepData 保留上次的測試資料）
  5. 在測試站台的 Web.config 開啟 TestMode（專案原本的 Web.config 不會被修改）
  6. 用 IIS Express 在 http://localhost:<Port> 啟動（匿名 + 測試登入，不需要 Windows 帳號）

  專案原始碼與專案 App_Data 都不會被修改。

.EXAMPLE
  .\Start-TestSite.ps1
  .\Start-TestSite.ps1 -NoBuild -KeepData
  .\Start-TestSite.ps1 -RefreshSeed      # 重新從專案 App_Data 取一份新的種子資料
#>
[CmdletBinding()]
param(
    [int]$Port = 5080,
    [switch]$NoBuild,
    [switch]$RefreshSeed,
    [switch]$KeepData,
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
$OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$TestEnv   = Split-Path -Parent $PSScriptRoot                    # ...\TEEmployee\TestEnv
$RepoRoot  = Split-Path -Parent $TestEnv                         # ...\TEEmployee
$ProjDir   = Join-Path $RepoRoot 'TEEmployee'                    # ...\TEEmployee\TEEmployee
$Csproj    = Join-Path $ProjDir 'TEEmployee.csproj'
$Runtime   = Join-Path $TestEnv '.runtime'
$SiteDir   = Join-Path $Runtime 'site'
$SeedDir   = Join-Path $Runtime 'seed\App_Data'
$ConfigDir = Join-Path $Runtime 'iisexpress'
$HostCfg   = Join-Path $ConfigDir 'applicationhost.config'
$PidFile   = Join-Path $Runtime 'iisexpress.pid'
$SiteName  = 'TEEmployeeTest'
$BaseUrl   = "http://localhost:$Port"

function Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Fail($msg) { Write-Host "`n[錯誤] $msg" -ForegroundColor Red; exit 1 }

if (-not (Test-Path $Csproj)) { Fail "找不到專案檔：$Csproj" }
New-Item -ItemType Directory -Force -Path $Runtime, $ConfigDir | Out-Null

# ---------------------------------------------------------------- 0. 停止舊的測試站台
& (Join-Path $PSScriptRoot 'Stop-TestSite.ps1') -Quiet

# ---------------------------------------------------------------- 1. 編譯 (Debug)
if (-not $NoBuild) {
    Step '以 Debug 編譯 TEEmployee'
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = $null
    if (Test-Path $vswhere) {
        $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
    if (-not $msbuild) { Fail '找不到 MSBuild。請先安裝 Visual Studio，或在 Visual Studio 中以 Debug 建置後加上 -NoBuild 再執行。' }

    & $msbuild $Csproj /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { Fail 'MSBuild 編譯失敗，請先在 Visual Studio 確認可以建置。' }
}

$dll = Join-Path $ProjDir 'bin\TEEmployee.dll'
if (-not (Test-Path $dll)) { Fail "找不到 $dll，請先建置專案。" }

# ---------------------------------------------------------------- 2. 複製網站 (不含 App_Data)
Step "複製網站到 $SiteDir"
$rc = robocopy $ProjDir $SiteDir /MIR /XD obj .vs App_Data _site node_modules /XF *.user /NFL /NDL /NJH /NJS /NP /R:2 /W:1
if ($LASTEXITCODE -ge 8) { Fail "robocopy 複製網站失敗 (代碼 $LASTEXITCODE)" }

# ---------------------------------------------------------------- 3. 種子資料
if ($RefreshSeed -or -not (Test-Path $SeedDir)) {
    Step "建立種子資料（從專案 App_Data 複製）→ $SeedDir"
    $src = Join-Path $ProjDir 'App_Data'
    robocopy $src $SeedDir /MIR /NFL /NDL /NJH /NJS /NP /R:2 /W:1 | Out-Null
    if ($LASTEXITCODE -ge 8) { Fail "robocopy 複製種子資料失敗 (代碼 $LASTEXITCODE)" }
} else {
    Write-Host "（沿用既有種子資料：$SeedDir；要更新請加 -RefreshSeed）"
}

# ---------------------------------------------------------------- 4. 測試資料
$SiteData = Join-Path $SiteDir 'App_Data'
if ($KeepData -and (Test-Path $SiteData)) {
    Step '保留上次的測試資料 (-KeepData)'
} else {
    Step '還原測試資料（種子 → 測試站台 App_Data）'
    robocopy $SeedDir $SiteData /MIR /NFL /NDL /NJH /NJS /NP /R:2 /W:1 | Out-Null
    if ($LASTEXITCODE -ge 8) { Fail "robocopy 還原測試資料失敗 (代碼 $LASTEXITCODE)" }
}

# ---------------------------------------------------------------- 5. 測試站台的 Web.config
Step '設定測試站台 Web.config（TestMode=true）'
$webConfigPath = Join-Path $SiteDir 'Web.config'
[xml]$wc = Get-Content -Raw -Encoding UTF8 $webConfigPath

function Set-AppSetting([xml]$doc, [string]$key, [string]$value) {
    $node = $doc.configuration.appSettings.add | Where-Object { $_.key -eq $key }
    if (-not $node) {
        $node = $doc.CreateElement('add')
        $node.SetAttribute('key', $key)
        $doc.configuration.appSettings.AppendChild($node) | Out-Null
    }
    $node.SetAttribute('value', $value)
}
Set-AppSetting $wc 'TestMode' 'true'
Set-AppSetting $wc 'TestSeedPath' $SeedDir

# 顯示詳細錯誤 (方便找 bug)
$sw = $wc.configuration.'system.web'
$ce = $sw.SelectSingleNode('customErrors')
if (-not $ce) { $ce = $wc.CreateElement('customErrors'); $sw.AppendChild($ce) | Out-Null }
$ce.SetAttribute('mode', 'Off')

# DevTools 允許未登入存取 (用來選擇測試身分)
$loc = $wc.configuration.SelectSingleNode("location[@path='DevTools']")
if (-not $loc) {
    $frag = $wc.CreateDocumentFragment()
    $frag.InnerXml = '<location path="DevTools"><system.web><authorization><allow users="*" /></authorization></system.web></location>'
    $wc.configuration.AppendChild($frag) | Out-Null
}
$wc.Save($webConfigPath)

# ---------------------------------------------------------------- 6. IIS Express 設定
Step '建立 IIS Express 設定（匿名驗證）'
$iisDir = Join-Path $env:ProgramFiles 'IIS Express'
if (-not (Test-Path (Join-Path $iisDir 'iisexpress.exe'))) { $iisDir = Join-Path ${env:ProgramFiles(x86)} 'IIS Express' }
$iisExe = Join-Path $iisDir 'iisexpress.exe'
$appcmd = Join-Path $iisDir 'appcmd.exe'
if (-not (Test-Path $iisExe)) { Fail '找不到 IIS Express（通常隨 Visual Studio 安裝）。' }

$template = Join-Path $iisDir 'config\templates\PersonalWebServer\applicationhost.config'
if (-not (Test-Path $template)) { Fail "找不到 IIS Express 範本：$template" }
Copy-Item $template $HostCfg -Force

& $appcmd add site /name:$SiteName "/bindings:http/*:${Port}:localhost" "/physicalPath:$SiteDir" "/apphostconfig:$HostCfg" | Out-Null
& $appcmd set app "$SiteName/" /applicationPool:Clr4IntegratedAppPool "/apphostconfig:$HostCfg" | Out-Null
& $appcmd set config $SiteName -section:system.webServer/security/authentication/anonymousAuthentication /enabled:true /commit:apphost "/apphostconfig:$HostCfg" | Out-Null
& $appcmd set config $SiteName -section:system.webServer/security/authentication/windowsAuthentication /enabled:false /commit:apphost "/apphostconfig:$HostCfg" | Out-Null

# ---------------------------------------------------------------- 7. 啟動
Step "啟動 IIS Express → $BaseUrl"
# 以最小化視窗執行：視窗內可看到每個請求的記錄；關閉該視窗即停止測試站台
$p = Start-Process -FilePath $iisExe -ArgumentList "/config:`"$HostCfg`"", "/site:$SiteName" `
        -WindowStyle Minimized -PassThru
Set-Content -Path $PidFile -Value $p.Id

$ok = $false
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds 1
    try {
        $r = Invoke-WebRequest -Uri "$BaseUrl/DevTools/Health" -UseBasicParsing -TimeoutSec 10
        if ($r.StatusCode -eq 200 -and $r.Content -match '"testMode":true') { $ok = $true; break }
    } catch { }
    if ($p.HasExited) { break }
}
if (-not $ok) {
    Fail "測試站台沒有正常啟動。請確認專案是用 Debug 編譯（TestMode 只存在 Debug 版），並查看工作列上 IIS Express 視窗的錯誤訊息。"
}

Write-Host "`n測試站台已啟動：$BaseUrl/DevTools" -ForegroundColor Green
Write-Host "停止：TestEnv\scripts\Stop-TestSite.ps1"
if (-not $NoBrowser) { Start-Process "$BaseUrl/DevTools" }

exit 0
