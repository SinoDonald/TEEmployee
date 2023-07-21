using System;
using System.Collections.Generic;
using System.Configuration;
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
                filterContext.HttpContext.Response.Redirect("~/Error/Unauthorized/" +
                filterContext.HttpContext.User.Identity.Name);
                filterContext.Result = new EmptyResult();
                return;
            }

            string loginUser = filterContext.HttpContext.User.Identity.Name;
            Match m = Regex.Match(loginUser, @"\\{0,1}(\d{4})@{0,1}");
            if (m.Success)
                loginUser = m.Groups[1].ToString();
            loginUser = "7596";
            //-------------------------------------------------------

            if (filterContext.HttpContext.Session["empno"] == null)
            {
                
                var ret = _userRepository.Get(loginUser);

                // Fake User v1

                //User ret;

                //if (filterContext.HttpContext.Session["empno"] == null)
                //{
                //    ret = _userRepository.Get(loginUser);
                //}
                //else
                //{
                //    ret = _userRepository.Get(filterContext.HttpContext.Session["empno"].ToString());
                //}

                if (ret != null) {
                                     
                    //Get Userinfo
                    //filterContext.HttpContext.Session["empno"] = loginUser;
                    filterContext.HttpContext.Session["empno"] = ret.empno;
                    filterContext.HttpContext.Session["empname"] = ret.name;
                    filterContext.HttpContext.Session["group"] = ret.group;
                    filterContext.HttpContext.Session["group_one"] = ret.group_one;
                    filterContext.HttpContext.Session["group_two"] = ret.group_two;
                    filterContext.HttpContext.Session["group_three"] = ret.group_three;

                    filterContext.HttpContext.Session["role"] = null;
                    filterContext.HttpContext.Session["leader"] = null;
                    filterContext.HttpContext.Session["group_leader"] = null;

                    // 首頁通知 <-- 培文
                    filterContext.HttpContext.Session["notify"] = new List<bool>();
                    filterContext.HttpContext.Session["notify_count"] = 0;

                    if (ret.department_manager)
                    {
                        filterContext.HttpContext.Session["leader"] = true;
                    }

                    if (ret.group_manager)
                    {
                        filterContext.HttpContext.Session["group_leader"] = true;
                    }

                    if (ret.project_manager)
                    {
                        filterContext.HttpContext.Session["role"] = "PM";
                    }

                    if (ret.department_manager || ret.group_manager || ret.group_one_manager || ret.group_two_manager || ret.group_three_manager)
                    {
                        filterContext.HttpContext.Session["role"] = "Manager";
                    }

                    if (ConfigurationManager.AppSettings["Admin"].Contains(ret.empno))
                    {
                        filterContext.HttpContext.Session["Admin"] = true;
                    }


                    //filterContext.HttpContext.Session["role"] = ret.Role;
                }
                else
                {
                    filterContext.HttpContext.Response.Redirect("~/Error/Unauthorized/"
                        //+filterContext.HttpContext.User.Identity.Name
                    );
                    filterContext.Result = new EmptyResult();
                    return;
                }
                
            }


            // fake user v1

            //if (true)
            //{
                
            //    User ret;

            //    if (filterContext.HttpContext.Session["empno"] == null)
            //    {
            //        ret = _userRepository.Get(loginUser);
            //    }
            //    else
            //    {
            //        ret = _userRepository.Get(filterContext.HttpContext.Session["empno"].ToString());
            //    }

            //    if (ret != null)
            //    {

            //        //Get Userinfo
            //        //filterContext.HttpContext.Session["empno"] = loginUser;
            //        filterContext.HttpContext.Session["empno"] = ret.empno;
            //        filterContext.HttpContext.Session["empname"] = ret.name;
            //        filterContext.HttpContext.Session["group"] = ret.group;
            //        filterContext.HttpContext.Session["group_one"] = ret.group_one;
            //        filterContext.HttpContext.Session["group_two"] = ret.group_two;
            //        filterContext.HttpContext.Session["group_three"] = ret.group_three;

            //        filterContext.HttpContext.Session["role"] = null;
            //        filterContext.HttpContext.Session["leader"] = null;

            //        if (ret.department_manager)
            //        {
            //            filterContext.HttpContext.Session["leader"] = true;
            //        }

            //        if (ret.project_manager)
            //        {
            //            filterContext.HttpContext.Session["role"] = "PM";
            //        }

            //        if (ret.department_manager || ret.group_manager || ret.group_one_manager || ret.group_two_manager || ret.group_three_manager)
            //        {
            //            filterContext.HttpContext.Session["role"] = "Manager";
            //        }

            //        //filterContext.HttpContext.Session["role"] = ret.Role;
            //    }
            //    else
            //    {
            //        filterContext.HttpContext.Response.Redirect("~/Home/Unauthorized/"
            //        //+filterContext.HttpContext.User.Identity.Name
            //        );
            //        filterContext.Result = new EmptyResult();
            //        return;
            //    }

            //}


        }
    }
}