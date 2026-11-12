#if DEBUG
// =====================================================================
//  測試工具（只在 Debug 編譯時存在；且必須 Web.config TestMode=true + 本機連線）
//  網址：/DevTools
//    - 切換測試身分（任一員工、可強制管理員 / 非管理員）
//    - 重置測試資料（把 App_Data 還原成種子資料）
//    - 提供自動化測試使用的 JSON API
// =====================================================================
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;

namespace TEEmployee.Controllers
{
    public class DevToolsController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 未啟用測試模式時，整個 DevTools 視同不存在
            if (!TestMode.IsActive(filterContext.HttpContext))
            {
                filterContext.Result = HttpNotFound();
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: DevTools
        public ActionResult Index(string returnUrl)
        {
            ViewBag.returnUrl = SafeReturnUrl(returnUrl);
            ViewBag.current = Request.Cookies[TestMode.EmpnoCookie]?.Value ?? "";
            ViewBag.adminOverride = TestMode.AdminOverride(HttpContext);
            ViewBag.seedPath = ConfigurationManager.AppSettings["TestSeedPath"] ?? "";
            return View();
        }

        /// <summary>
        /// 以指定員工登入。admin: "1" 強制管理員、"0" 強制非管理員、"" 依 Web.config
        /// GET /DevTools/Login?empno=1234&admin=&returnUrl=/Forum
        /// </summary>
        public ActionResult Login(string empno, string admin = "", string returnUrl = "")
        {
            if (string.IsNullOrWhiteSpace(empno)) return RedirectToAction("Index");

            SetCookie(TestMode.EmpnoCookie, empno.Trim());
            SetCookie(TestMode.AdminCookie, admin == "1" || admin == "0" ? admin : "");
            Session.Clear();

            return Redirect(SafeReturnUrl(returnUrl));
        }

        /// <summary>登出 (清除測試身分)</summary>
        public ActionResult Logout()
        {
            ExpireCookie(TestMode.EmpnoCookie);
            ExpireCookie(TestMode.AdminCookie);
            Session.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 所有員工與其角色旗標 (自動化測試用來挑選各角色代表)
        /// </summary>
        public JsonResult Users()
        {
            var users = new UserRepository().GetAll() ?? new List<User>();

            var ret = users.OrderBy(u => u.empno).Select(u => new
            {
                u.empno,
                u.name,
                u.group,
                u.group_one,
                u.group_two,
                u.group_three,
                u.group_four,
                u.department_manager,
                u.group_manager,
                u.group_one_manager,
                u.group_two_manager,
                u.group_three_manager,
                u.group_four_manager,
                u.project_manager,
                isAdmin = Permissions.IsAdminEmpno(u.empno),
                isForumAdmin = Permissions.IsAdminEmpno(u.empno) || Permissions.InList("ForumAdmin", u.empno),
                isTalentEditor = Permissions.InList("TalentEditor", u.empno),
                isWishHandler = Permissions.InList("WishHandler", u.empno),
                isAdminPageUser = Permissions.InList("AdminPageUsers", u.empno),
                isFacilityManager = Permissions.InList("FacilityManagers", u.empno),
                roleLabel = RoleLabel(u),
            }).ToList();

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        /// <summary>目前 Session 內容 (驗證身分切換是否正確)</summary>
        public JsonResult WhoAmI()
        {
            var keys = new[] { "empno", "original_empno", "empname", "group", "group_one", "group_two", "group_three", "group_four", "role", "leader", "group_leader", "Admin", "ImpersonatorAdmin" };
            var dict = new Dictionary<string, object>();
            foreach (var k in keys) dict[k] = Session[k];
            dict["identity"] = User?.Identity?.Name;
            dict["adminOverride"] = TestMode.AdminOverride(HttpContext);
            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        /// <summary>健康檢查 (測試腳本用來確認測試站台已啟動且為測試模式)</summary>
        public JsonResult Health()
        {
            return Json(new
            {
                ok = true,
                testMode = true,
                appData = Server.MapPath("~/App_Data"),
                seed = ConfigurationManager.AppSettings["TestSeedPath"] ?? "",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 重置測試資料：把 App_Data 還原成種子資料，然後重新啟動網站 (清除快取與 Session)
        /// </summary>
        [HttpPost]
        public JsonResult Reset()
        {
            string seed = ConfigurationManager.AppSettings["TestSeedPath"];
            string target = Server.MapPath("~/App_Data");

            if (string.IsNullOrWhiteSpace(seed) || !Directory.Exists(seed))
                return Json(new { ok = false, message = "找不到種子資料夾 (Web.config TestSeedPath)：" + seed });

            // 安全檢查：只允許重置「測試站台」的 App_Data，不能是專案原始碼資料夾
            if (!target.Replace('/', '\\').ToLowerInvariant().Contains("\\.runtime\\"))
                return Json(new { ok = false, message = "安全檢查失敗：目前網站不是 TestEnv\\.runtime 下的測試站台，拒絕重置。 " + target });

            try
            {
                SQLiteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var errors = new List<string>();
                MirrorDirectory(seed, target, errors);

                HttpRuntime.UnloadAppDomain();   // 讓 static 快取、Session 全部重來

                return Json(new { ok = errors.Count == 0, errors });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        // ------------------------------------------------------------------

        private static string RoleLabel(User u)
        {
            var tags = new List<string>();
            if (Permissions.IsAdminEmpno(u.empno)) tags.Add("系統管理員");
            if (u.department_manager) tags.Add("部門主管");
            if (u.group_manager) tags.Add("組長");
            if (u.group_one_manager || u.group_two_manager || u.group_three_manager || u.group_four_manager) tags.Add("小組主管");
            if (u.project_manager) tags.Add("計畫經理");
            if (Permissions.InList("TalentEditor", u.empno)) tags.Add("資協");
            if (Permissions.InList("WishHandler", u.empno)) tags.Add("許願池處理人");
            if (Permissions.InList("AdminPageUsers", u.empno)) tags.Add("管理頁使用者");
            if (Permissions.InList("FacilityManagers", u.empno)) tags.Add("公用裝置管理員");
            if (tags.Count == 0) tags.Add("一般員工");
            return string.Join("、", tags);
        }

        private string SafeReturnUrl(string url)
        {
            if (!string.IsNullOrEmpty(url) && Url.IsLocalUrl(url) && !url.StartsWith("/DevTools", StringComparison.OrdinalIgnoreCase))
                return url;
            return Url.Content("~/");
        }

        private void SetCookie(string name, string value)
        {
            Response.Cookies.Add(new HttpCookie(name, value) { HttpOnly = false, Path = "/" });
        }

        private void ExpireCookie(string name)
        {
            Response.Cookies.Add(new HttpCookie(name, "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });
        }

        /// <summary>讓 target 與 source 內容一致 (多的刪掉、不同的覆蓋)，檔案被鎖住時重試</summary>
        private static void MirrorDirectory(string source, string target, List<string> errors)
        {
            Directory.CreateDirectory(target);

            var sourceFiles = new HashSet<string>(Directory.GetFiles(source).Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);
            var sourceDirs = new HashSet<string>(Directory.GetDirectories(source).Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);

            foreach (var f in Directory.GetFiles(target))
                if (!sourceFiles.Contains(Path.GetFileName(f)))
                    Retry(() => System.IO.File.Delete(f), f, errors);

            foreach (var d in Directory.GetDirectories(target))
                if (!sourceDirs.Contains(Path.GetFileName(d)))
                    Retry(() => Directory.Delete(d, true), d, errors);

            foreach (var name in sourceFiles)
            {
                string src = Path.Combine(source, name), dst = Path.Combine(target, name);
                Retry(() => System.IO.File.Copy(src, dst, true), dst, errors);
            }

            foreach (var name in sourceDirs)
                MirrorDirectory(Path.Combine(source, name), Path.Combine(target, name), errors);
        }

        private static void Retry(Action action, string path, List<string> errors)
        {
            for (int i = 0; i < 10; i++)
            {
                try { action(); return; }
                catch (IOException) { Thread.Sleep(200); }
                catch (UnauthorizedAccessException) { Thread.Sleep(200); }
            }
            errors.Add(path);
        }
    }
}
#endif
