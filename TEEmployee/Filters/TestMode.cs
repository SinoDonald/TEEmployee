#if DEBUG
// =====================================================================
//  測試模式（只在 Debug 編譯時存在，Release 版完全不會編進 DLL）
//
//  啟用條件（三者同時成立才會生效）：
//    1. Debug 編譯
//    2. Web.config <appSettings> 有 TestMode = true
//       （只有 TestEnv\Start-TestSite.ps1 產生的「測試站台」副本會加這個設定，
//         專案本身的 Web.config 不需要、也不應該加）
//    3. 連線來自本機 (Request.IsLocal)
//
//  作用：以 Cookie「te_test_empno」指定的員工編號登入，取代 Windows 驗證，
//        方便模擬不同身分／權限操作網站。
// =====================================================================
using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;

namespace TEEmployee.Filters
{
    public static class TestMode
    {
        public const string EmpnoCookie = "te_test_empno";
        public const string AdminCookie = "te_test_admin";   // "1" 強制管理員、"0" 強制非管理員、空白 = 依 Web.config
        private const string SessionMarker = "__test_identity";

        /// <summary>Web.config 是否開啟測試模式</summary>
        public static bool IsConfigured
        {
            get
            {
                return string.Equals(ConfigurationManager.AppSettings["TestMode"], "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>這個請求是否可使用測試模式</summary>
        public static bool IsActive(HttpContextBase ctx)
        {
            return IsConfigured && ctx != null && ctx.Request.IsLocal;
        }

        public static bool IsActive(HttpContext ctx)
        {
            return ctx != null && IsActive(new HttpContextWrapper(ctx));
        }

        /// <summary>
        /// Global.asax Application_PostAuthenticateRequest 呼叫：
        /// 依 Cookie 建立測試身分；沒有 Cookie 時導向身分切換頁
        /// </summary>
        public static void OnPostAuthenticateRequest(HttpContext ctx)
        {
            if (!IsActive(ctx)) return;

            var cookie = ctx.Request.Cookies[EmpnoCookie];
            string empno = cookie == null ? null : cookie.Value;

            if (!string.IsNullOrWhiteSpace(empno))
            {
                var principal = new GenericPrincipal(new GenericIdentity(empno.Trim(), "TestMode"), new string[0]);
                ctx.User = principal;
                System.Threading.Thread.CurrentPrincipal = principal;
                return;
            }

            // 尚未選擇身分：除了 DevTools 與靜態檔外，一律導向身分切換頁
            string path = ctx.Request.AppRelativeCurrentExecutionFilePath ?? "";
            bool isDevTools = path.StartsWith("~/DevTools", StringComparison.OrdinalIgnoreCase);
            bool isStatic = path.StartsWith("~/Content/", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("~/Scripts/", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("~/fonts/", StringComparison.OrdinalIgnoreCase)
                         || path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase);

            if (!isDevTools && !isStatic)
            {
                ctx.Response.Redirect("~/DevTools?returnUrl=" + HttpUtility.UrlEncode(ctx.Request.RawUrl), false);
                ctx.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// MyAuthorizeAttribute 一開始呼叫：切換身分時清掉舊 Session，讓 MyAuthorize 重新讀取使用者資料
        /// </summary>
        public static void BeforeAuthorize(HttpContextBase ctx)
        {
            if (!IsActive(ctx) || ctx.Session == null) return;

            string marker = CurrentMarker(ctx);
            if ((ctx.Session[SessionMarker] as string) != marker)
            {
                ctx.Session.Clear();
                ctx.Session[SessionMarker] = marker;
            }
        }

        /// <summary>
        /// MyAuthorizeAttribute 結束前呼叫：套用「強制管理員 / 強制非管理員」
        /// </summary>
        public static void AfterAuthorize(HttpContextBase ctx)
        {
            if (!IsActive(ctx) || ctx.Session == null) return;
            if (ctx.Session["empno"] == null) return;

            string mode = AdminOverride(ctx);
            if (mode == "1")
            {
                ctx.Session["Admin"] = true;
                foreach (var key in new[] { "group", "group_one", "group_two", "group_three", "group_four" })
                    if (ctx.Session[key] == null) ctx.Session[key] = "";
            }
            else if (mode == "0")
            {
                ctx.Session.Remove("Admin");
            }
        }

        public static string AdminOverride(HttpContextBase ctx)
        {
            var c = ctx.Request.Cookies[AdminCookie];
            return c == null ? "" : (c.Value ?? "");
        }

        private static string CurrentMarker(HttpContextBase ctx)
        {
            return (ctx.User != null ? ctx.User.Identity.Name : "") + "|" + AdminOverride(ctx);
        }
    }
}
#endif
