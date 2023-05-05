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
        public static HtmlString Versioned(this UrlHelper helper, string target)
        {
            
            if (!HttpContext.Current.IsDebuggingEnabled || true)
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