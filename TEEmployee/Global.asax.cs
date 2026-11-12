using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TEEmployee
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

#if DEBUG
        /// <summary>
        /// 測試模式：以 Cookie 指定的員工身分登入 (只在 Debug 編譯 + Web.config TestMode=true + 本機連線時生效)
        /// </summary>
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            TEEmployee.Filters.TestMode.OnPostAuthenticateRequest(Context);
        }
#endif
    }
}
