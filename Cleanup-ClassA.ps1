# ============================================================================
# TEEmployee - delete "Class A" leftover files (confirmed unused)
#
# How to run (any one):
#   - Right-click this file > "Run with PowerShell"
#   - PowerShell:  cd C:\Prj\ASP.Net\TEEmployee ; .\Cleanup-ClassA.ps1
#   - VS Package Manager Console:  . .\Cleanup-ClassA.ps1
#
# Before running:
#   1. Commit and push first (so everything can be restored from Git).
#   2. Stop debugging in VS (Shift+F5) so bin\*.dll is not locked.
#
# Add -WhatIf to only preview:  .\Cleanup-ClassA.ps1 -WhatIf
# ============================================================================
param([switch]$WhatIf)

$root = $PSScriptRoot
if (-not $root) { $root = (Get-Location).Path }

if (-not (Test-Path (Join-Path $root 'TEEmployee.sln'))) {
    Write-Host "TEEmployee.sln not found in $root" -ForegroundColor Red
    Write-Host "Please put this script in the solution folder (C:\Prj\ASP.Net\TEEmployee)." -ForegroundColor Red
    return
}

$items = @(
    # Finished one-time scripts
    'RemoveUnusedPackages.ps1',
    'Update-Remaining.ps1',
    'Update-VulnerablePackages.ps1',

    # VS upgrade report (2025/04)
    'UpgradeLog.htm',

    # Backup of the project file
    'TEEmployee\TEEmployee.csproj.good',

    # Old package versions (not referenced by any packages.config)
    'packages\System.Buffers.4.4.0',
    'packages\System.Buffers.4.6.0',
    'packages\System.Memory.4.5.3',
    'packages\System.Memory.4.6.0',
    'packages\System.Numerics.Vectors.4.4.0',
    'packages\System.Numerics.Vectors.4.6.0',
    'packages\System.Runtime.CompilerServices.Unsafe.4.5.2',
    'packages\System.Runtime.CompilerServices.Unsafe.6.1.0',
    'packages\System.Threading.Tasks.Extensions.4.6.0',

    # Leftover DLLs in bin
    'TEEmployee\bin\DocumentFormat.OpenXml.Framework.dll',
    'TEEmployee\bin\Microsoft.AspNetCore.Metadata.dll'
)

$targets = @()
foreach ($rel in $items) {
    $p = Join-Path $root $rel
    if (Test-Path -LiteralPath $p) { $targets += $p }
    else { Write-Host "  (skip, not found) $rel" -ForegroundColor DarkGray }
}

if ($targets.Count -eq 0) {
    Write-Host "Nothing to delete." -ForegroundColor Green
    return
}

Write-Host ""
Write-Host "The following $($targets.Count) item(s) will be deleted:" -ForegroundColor Yellow
$targets | ForEach-Object { Write-Host "  $_" }
Write-Host ""

if ($WhatIf) {
    Write-Host "Preview only (-WhatIf). Nothing was deleted." -ForegroundColor Cyan
    return
}

$answer = Read-Host "Type Y to delete"
if ($answer -ne 'Y' -and $answer -ne 'y') {
    Write-Host "Cancelled. Nothing was deleted." -ForegroundColor Cyan
    return
}

$failed = @()
foreach ($p in $targets) {
    try {
        Remove-Item -LiteralPath $p -Recurse -Force -ErrorAction Stop
        Write-Host "Deleted: $p" -ForegroundColor Green
    } catch {
        $failed += $p
        Write-Host "FAILED : $p  ($($_.Exception.Message))" -ForegroundColor Red
    }
}

Write-Host ""
if ($failed.Count) {
    Write-Host "$($failed.Count) item(s) could not be deleted (file in use?). Stop debugging / close VS and run again." -ForegroundColor Yellow
} else {
    Write-Host "Done. Next: Rebuild Solution in VS, then commit the deletions in SourceTree." -ForegroundColor Green
    Write-Host "You can delete this script (Cleanup-ClassA.ps1) afterwards." -ForegroundColor Green
}
