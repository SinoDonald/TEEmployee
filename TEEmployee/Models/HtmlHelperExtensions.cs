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
        public static string Versioned(this HtmlHelper helper, string target)
        {

            DateTime localDate = DateTime.Now;

            if (target.StartsWith("~"))
            {
                target = target.Substring(1);
            }

            string versionedUrl = $"{target}?v={localDate.Ticks}";

            return versionedUrl;

        }

    }
}