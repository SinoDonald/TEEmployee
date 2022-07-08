using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;

namespace TEEmployee.Filters
{
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        private IUserRepository _userRepository;
        public MyAuthorizeAttribute()
        {
            //_userRepository = new UserTxtRepository();
            _userRepository = new UserRepository();
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;
            return true;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext); 

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.HttpContext.Response.Redirect("~/Home/Unauthorized/" +
                filterContext.HttpContext.User.Identity.Name);
                filterContext.Result = new EmptyResult();
                return;
            }

            string loginUser = filterContext.HttpContext.User.Identity.Name;
            Match m = Regex.Match(loginUser, @"\\{0,1}(\d{4})@{0,1}");      
            if (m.Success)
                loginUser = m.Groups[1].ToString(); 
            //-------------------------------------------------------

            if (filterContext.HttpContext.Session["empno"] == null)
            {
                
                var ret = _userRepository.Get(loginUser);

                if(ret != null) {
                    
                    //Get Userinfo
                    filterContext.HttpContext.Session["empno"] = loginUser;
                    filterContext.HttpContext.Session["empname"] = ret.name;
                    //filterContext.HttpContext.Session["role"] = ret.Role;
                }
                else
                {
                    filterContext.HttpContext.Response.Redirect("~/Home/Unauthorized/"
                        //+filterContext.HttpContext.User.Identity.Name
                    );
                    filterContext.Result = new EmptyResult();
                    return;
                }
                
            }
        }
    }
}