using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace TEEmployee.Models
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// 在 JS / CSS 網址後面加上 ?v=檔案最後修改時間，檔案一更新網址就跟著變，
        /// 使用者瀏覽器不會繼續使用舊版快取 (不用再請大家按 Ctrl+F5)。
        /// 注意：不再自動改用 .min.js (舊的 .min.js 沒有跟著更新，改用會載入舊程式)。
        /// 用法：&lt;script src="@TEEmployee.Models.HtmlHelperExtensions.Versioned(Url, "~/Scripts/WebApp/Xxx/XxxCtrl.js")"&gt;&lt;/script&gt;
        /// </summary>
        public static HtmlString Versioned(this UrlHelper helper, string target)
        {
            try
            {
                var file = HttpContext.Current?.Server.MapPath(target);
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    long ticks = File.GetLastWriteTimeUtc(file).Ticks;
                    return new HtmlString(helper.Content(target) + "?v=" + ticks);
                }
            }
            catch (Exception)
            {
                // 取不到檔案時間時不加版本號，維持原本行為，不影響頁面顯示
            }

            return new HtmlString(helper.Content(target));
        }
    }
}
