<#
.SYNOPSIS
  執行自動化測試並開啟 HTML 報告

.EXAMPLE
  .\Run-Tests.ps1                         # 全部測試
  .\Run-Tests.ps1 -Only forum             # 只跑檔名含 forum 的測試
  .\Run-Tests.ps1 -Project smoke          # 只跑冒煙測試
  .\Run-Tests.ps1 -Headed                 # 顯示瀏覽器畫面
  .\Run-Tests.ps1 -Restart                # 重新編譯並重啟測試站台後再測
#>
[CmdletBinding()]
param(
    [string]$Only = '',
    [ValidateSet('', 'smoke', 'functional', 'destructive')]
    [string]$Project = '',
    [switch]$Headed,
    [switch]$Restart,
    [switch]$NoReport,
    [int]$Port = 5080
)

$ErrorActionPreference = 'Stop'
$OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$TestEnv = Split-Path -Parent $PSScriptRoot
$BaseUrl = "http://localhost:$Port"
Set-Location $TestEnv

function Fail($msg) { Write-Host "`n[錯誤] $msg" -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------- Node.js
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Fail '找不到 Node.js。請安裝 Node.js 18 以上 (https://nodejs.org)，再重新執行。'
}

# ---------------------------------------------------------------- 測試站台
$running = $false
try {
    $r = Invoke-WebRequest -Uri "$BaseUrl/DevTools/Health" -UseBasicParsing -TimeoutSec 5
    $running = ($r.StatusCode -eq 200)
} catch { }

if ($Restart -or -not $running) {
    Write-Host '啟動測試站台…' -ForegroundColor Cyan
    & (Join-Path $PSScriptRoot 'Start-TestSite.ps1') -Port $Port -NoBrowser
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

# ---------------------------------------------------------------- 套件
if (-not (Test-Path (Join-Path $TestEnv 'node_modules\@playwright\test'))) {
    Write-Host '安裝測試套件 (第一次需要幾分鐘)…' -ForegroundColor Cyan
    npm install --no-audit --no-fund
    if ($LASTEXITCODE -ne 0) { Fail 'npm install 失敗，請確認可以連到 npm 套件庫 (公司 Proxy 可能需要設定 npm config set proxy)。' }
}

# ---------------------------------------------------------------- 執行
$env:TE_BASE_URL = $BaseUrl
$pwArgs = @('playwright', 'test')
if ($Project) { $pwArgs += "--project=$Project" }
if ($Only)    { $pwArgs += $Only }
if ($Headed)  { $pwArgs += '--headed' }

Write-Host "`n執行：npx $($pwArgs -join ' ')" -ForegroundColor Cyan
npx @pwArgs
$code = $LASTEXITCODE

if (-not $NoReport) {
    Write-Host "`n開啟測試報告…" -ForegroundColor Cyan
    npx playwright show-report
}
exit $code
