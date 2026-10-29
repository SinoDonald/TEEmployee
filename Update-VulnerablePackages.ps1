# 在 Visual Studio「工具 > NuGet 套件管理員 > 套件管理器主控台」執行：
#   . .\Update-VulnerablePackages.ps1
# 預設專案請選 TEEmployee。執行前請先 git commit，方便比對與還原。
# 會一併處理：csproj HintPath / Content 檔、Scripts 與 Content 內的 js/css、Web.config bindingRedirect。

$p = "TEEmployee"

# 前端套件（NU1902 / NU1903）
Update-Package jQuery            -ProjectName $p -Version 3.7.1   # 停留在 3.x，bootstrap 4 需要 jQuery < 4
Update-Package bootstrap         -ProjectName $p -Version 4.6.2   # 4.x 最終版，避免 5.x 破壞性變更（會連帶升級 popper.js 1.16.1）
Update-Package jQuery.Validation -ProjectName $p                  # 最新版
Update-Package Moment.js         -ProjectName $p                  # 最新版（2.29.4 以上即修補）

# .NET 套件
Update-Package MimeKit           -ProjectName $p
Update-Package MailKit           -ProjectName $p                  # 會一併帶入相依的 BouncyCastle / System.Formats.Asn1 等
Update-Package Newtonsoft.Json   -ProjectName $p                  # 13.x，會自動改 Web.config redirect 為 13.0.0.0

# 重新產生 Web.config 的 bindingRedirect
Add-BindingRedirect -ProjectName $p
