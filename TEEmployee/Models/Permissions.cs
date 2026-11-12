using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    /// <summary>
    /// 權限設定集中管理
    ///
    /// 名單都放在 Web.config &lt;appSettings&gt;，換人時只要改 Web.config，不必改程式：
    ///   Admin          系統管理員 (清空資料庫、管理頁所有功能、切換身分…)
    ///   AdminPageUsers 可進入管理頁、匯入組織表的非管理員 (例：6846、7291、7596)
    ///   FacilityManagers 公用裝置管理員 (新增 / 移除裝置、修改 Teams 密碼、代改他人預約)
    ///   ForumAdmin     我要抱抱管理者 (可刪除貼文 / 回文)
    ///   TalentEditor   人才培育資料庫可「儲存」的資協
    ///   WishHandler    許願池處理人 (可修改需求狀態)
    ///
    /// 員編比對會忽略前面補的 0：「4125」與「04125」視為同一人。
    /// Web.config 沒有設定某個名單時，使用下方 Defaults 的預設值 (與舊程式寫死的員編相同)。
    /// </summary>
    public static class Permissions
    {
        private static readonly Dictionary<string, string> Defaults = new Dictionary<string, string>
        {
            { "Admin", "" },
            { "AdminPageUsers", "6846,7291,7596" },
            { "FacilityManagers", "7291,7596" },
            { "ForumAdmin", "7291" },
            { "TalentEditor", "4125" },
            { "WishHandler", "8477" },
        };

        // ------------------------------------------------------------------ 員編與名單

        /// <summary>員編正規化：去空白、去掉前面補的 0 (「04125」→「4125」)</summary>
        public static string NormalizeEmpno(string empno)
        {
            if (string.IsNullOrWhiteSpace(empno)) return "";
            string s = empno.Trim().TrimStart('0');
            return s.Length == 0 ? "0" : s;
        }

        /// <summary>兩個員編是否為同一人 (忽略前導 0)</summary>
        public static bool SameEmpno(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            return NormalizeEmpno(a) == NormalizeEmpno(b);
        }

        /// <summary>讀取 Web.config 名單 (逗號、分號或空白分隔)</summary>
        public static HashSet<string> GetList(string key)
        {
            string raw = ConfigurationManager.AppSettings[key];
            if (raw == null && Defaults.ContainsKey(key)) raw = Defaults[key];

            return new HashSet<string>((raw ?? "")
                .Split(new[] { ',', ';', ' ', '，', '、' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeEmpno)
                .Where(x => x.Length > 0));
        }

        /// <summary>員編是否在名單中 (完整比對，不會因為「698」包含在「6988」而誤判)</summary>
        public static bool InList(string key, string empno)
        {
            if (string.IsNullOrWhiteSpace(empno)) return false;
            return GetList(key).Contains(NormalizeEmpno(empno));
        }

        public static bool IsAdminEmpno(string empno)
        {
            return InList("Admin", empno);
        }

        // ------------------------------------------------------------------ 依目前登入者 (Session) 判斷

        public static string CurrentEmpno(HttpSessionStateBase session)
        {
            return session?["empno"] as string ?? "";
        }

        /// <summary>
        /// 目前是否為系統管理員。
        /// 以 Session["Admin"] 判斷 (登入時依 Admin 名單設定；管理員切換成別人身分時會暫時移除)
        /// </summary>
        public static bool IsAdmin(HttpSessionStateBase session)
        {
            return session != null && session["Admin"] is object;
        }

        /// <summary>部門主管</summary>
        public static bool IsDepartmentManager(HttpSessionStateBase session)
        {
            return session != null && session["leader"] is bool b && b;
        }

        /// <summary>可進入管理頁 (Home/Admin)、匯入組織表</summary>
        public static bool CanUseAdminPage(HttpSessionStateBase session)
        {
            return IsAdmin(session) || InList("AdminPageUsers", CurrentEmpno(session));
        }

        /// <summary>公用裝置管理：管理員、部門主管、FacilityManagers 名單</summary>
        public static bool IsFacilityManager(HttpSessionStateBase session)
        {
            return IsAdmin(session) || IsDepartmentManager(session) || InList("FacilityManagers", CurrentEmpno(session));
        }

        /// <summary>我要抱抱管理者：可刪除貼文 / 回文</summary>
        public static bool IsForumAdmin(HttpSessionStateBase session)
        {
            return IsAdmin(session) || InList("ForumAdmin", CurrentEmpno(session));
        }

        /// <summary>人才培育資料庫：可儲存規劃、優勢、劣勢等欄位 (只限資協)</summary>
        public static bool IsTalentEditor(HttpSessionStateBase session)
        {
            return InList("TalentEditor", CurrentEmpno(session));
        }

        /// <summary>許願池處理人：可修改需求狀態 (只限 WishHandler 名單，管理員也不行)</summary>
        public static bool IsWishHandler(HttpSessionStateBase session)
        {
            return InList("WishHandler", CurrentEmpno(session));
        }

        /// <summary>
        /// 管理員切換身分 (假冒) 中：Session["ImpersonatorAdmin"] = true。
        /// 假冒期間沒有管理員權限，但仍可使用「切換身分」切回自己。
        /// </summary>
        public static bool CanSwitchIdentity(HttpSessionStateBase session)
        {
            return IsAdmin(session) || (session != null && session["ImpersonatorAdmin"] is object);
        }

        // ------------------------------------------------------------------ 檔案路徑

        /// <summary>
        /// 安全地組出資料夾內的檔案路徑：只取檔名 (去掉 ..\ 與路徑)，並確認結果仍在資料夾內。
        /// 不合法時回傳 null。
        /// </summary>
        public static string SafeFilePath(string folder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName)) return null;

            string name = System.IO.Path.GetFileName(fileName.Replace('/', '\\'));
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == "..") return null;
            if (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0) return null;

            string root = System.IO.Path.GetFullPath(folder).TrimEnd('\\') + "\\";
            string full = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, name));
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
        }
    }
}
