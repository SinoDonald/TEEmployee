using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Unauthorized()
        {           
            return View();
        }

        [HttpPost]
        public ActionResult Index(User fake)
        {
            if (Session["Admin"] is object)
            {
                User user = new UserRepository().Get(fake.empno);

                Session["empno"] = user.empno;
                Session["empname"] = user.name;
                Session["group"] = user.group;
                Session["group_one"] = user.group_one;
                Session["group_two"] = user.group_two;
                Session["group_three"] = user.group_three;

                Session["role"] = null;
                Session["leader"] = null;

                if (user.department_manager)
                {
                    Session["leader"] = true;
                }

                if (user.project_manager)
                {
                    Session["role"] = "PM";
                }

                if (user.department_manager || user.group_manager || user.group_one_manager || user.group_two_manager || user.group_three_manager)
                {
                    Session["role"] = "Manager";
                }


            }



            return RedirectToAction("Index");
        }
    }
}