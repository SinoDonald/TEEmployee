using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TEEmployee.Models
{
    public static class HtmlHelperExtensions
    {
        // 是否啟用 min.js 與版本號 (?v=) 輸出；原本以 if(false) 停用，改用 static readonly 避免 CS0162
        private static readonly bool EnableVersioning = false; // 原條件: !HttpContext.Current.IsDebuggingEnabled

        public static HtmlString Versioned(this UrlHelper helper, string target)
        {
            
            if (EnableVersioning)
            {
                var minTarget = target.Substring(0, target.Length - 2) + "min.js";

                if (File.Exists(HttpContext.Current.Server.MapPath(minTarget)))
                    target = minTarget;

                var file = HttpContext.Current.Server.MapPath(target);

                DateTime lastModifiedDate = File.GetLastWriteTime(file);

                string versionedUrl = $"{target}?v={lastModifiedDate.Ticks}";
                                
                return new HtmlString(helper.Content(versionedUrl));
            }

            return new HtmlString(helper.Content(target));

            //DateTime localDate = DateTime.Now;

            //if (target.StartsWith("~"))
            //{
            //    target = target.Substring(1);
            //}

            //string versionedUrl = $"{target}?v={localDate.Ticks}";

            //return new HtmlString(versionedUrl);            

        }

    }
}