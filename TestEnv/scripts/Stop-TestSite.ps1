<#
.SYNOPSIS
  停止測試站台 (IIS Express TEEmployeeTest)
#>
[CmdletBinding()]
param([switch]$Quiet)

$TestEnv = Split-Path -Parent $PSScriptRoot
$PidFile = Join-Path $TestEnv '.runtime\iisexpress.pid'
$stopped = 0

if (Test-Path $PidFile) {
    $procId = Get-Content $PidFile -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($procId) {
        $p = Get-Process -Id $procId -ErrorAction SilentlyContinue
        if ($p -and $p.ProcessName -like 'iisexpress*') { Stop-Process -Id $procId -Force; $stopped++ }
    }
    Remove-Item $PidFile -ErrorAction SilentlyContinue
}

# 保險：找出所有以 /site:TEEmployeeTest 啟動的 IIS Express
Get-CimInstance Win32_Process -Filter "Name like 'iisexpress%'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match '/site:TEEmployeeTest' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue; $stopped++ }

if (-not $Quiet) {
    if ($stopped) { Write-Host '測試站台已停止。' -ForegroundColor Green }
    else { Write-Host '測試站台沒有在執行。' }
}
