# 補完尚未更新的 3 個有弱點套件
# 用法：套件管理器主控台  PM> . .\Update-Remaining.ps1

$p = 'TEEmployee'
Update-Package jQuery            -ProjectName $p -Version 3.7.1   # 3.x 最新；bootstrap 4 不能用 jQuery 4
Update-Package jQuery.Validation -ProjectName $p
Update-Package Newtonsoft.Json   -ProjectName $p
