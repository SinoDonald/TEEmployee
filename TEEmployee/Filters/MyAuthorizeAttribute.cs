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

            //if (filterContext.HttpContext.Session["empno"] == null)
            if (true)
            {
                //loginUser = "6112";
                //loginUser = "5526";
                //loginUser = "4125";
                //loginUser = "7291";
                //var ret = _userRepository.Get(loginUser);

                User ret;

                if (filterContext.HttpContext.Session["empno"] == null)
                {
                    ret = _userRepository.Get(loginUser);
                }
                else
                {
                    ret = _userRepository.Get(filterContext.HttpContext.Session["empno"].ToString());
                }

                if(ret != null) {

                    //Get Userinfo
                    //filterContext.HttpContext.Session["empno"] = loginUser;
                    filterContext.HttpContext.Session["empno"] = ret.empno;
                    filterContext.HttpContext.Session["empname"] = ret.name;
                    filterContext.HttpContext.Session["group"] = ret.group;
                    filterContext.HttpContext.Session["group_one"] = ret.group_one;
                    filterContext.HttpContext.Session["group_two"] = ret.group_two;
                    filterContext.HttpContext.Session["group_three"] = ret.group_three;

                    filterContext.HttpContext.Session["role"] = null;

                    if (ret.department_manager || ret.group_manager || ret.group_one_manager || ret.group_two_manager || ret.group_three_manager)
                    {
                        filterContext.HttpContext.Session["role"] = "Manager";
                    }

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