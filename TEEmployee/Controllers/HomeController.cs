using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.TaskLog;

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

        public ActionResult Admin()
        {
            ViewBag.Message = "Hello there!!";

            if (Session["Admin"] is object)
                return View();
            else
                return RedirectToAction("Index");
        }

        public ActionResult Document()
        {            
            return View();           
        }
        //public ActionResult Unauthorized()
        //{           
        //    return View();
        //}

        [HttpPost]
        public ActionResult Index(User fake)
        {
            if (Session["Admin"] is object)
            {
                User user = new UserRepository().Get(fake.empno);

                if (user is null)                    
                    return RedirectToAction("Unauthorized", "Error");

                Session["empno"] = user.empno;
                Session["empname"] = user.name;
                Session["group"] = user.group;
                Session["group_one"] = user.group_one;
                Session["group_two"] = user.group_two;
                Session["group_three"] = user.group_three;

                Session["role"] = null;
                Session["leader"] = null;
                Session["group_leader"] = null;

                if (user.department_manager)
                {
                    Session["leader"] = true;
                }

                if (user.group_manager)
                {
                    Session["group_leader"] = true;
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


        [HttpPost]
        public bool InsertProjectItem()
        {
            bool ret = false;

            if (Session["Admin"] is object)
            {               

                // read new project items from txt file

                List<ProjectItem> projectItems = new List<ProjectItem>();

                using (TasklogService service = new TasklogService(false))
                {
                    projectItems = service.GetAllProjectItem();
                }

                if (projectItems.Count != 0) ret = true;

                //List<MonthlyRecord> monthlyRecords = new List<MonthlyRecord>();
                //foreach (var item in projectItems)
                //{
                //    MonthlyRecord monthlyRecord = new MonthlyRecord()
                //    {
                //        empno = item.empno,
                //        yymm = item.yymm,
                //        guid = Guid.NewGuid()
                //    };

                //    monthlyRecords.Add(monthlyRecord);
                //}


                //using (TasklogService service = new TasklogService())
                //{
                //    foreach (var item in projectItems)
                //    {
                //        MonthlyRecord monthlyRecord = new MonthlyRecord()
                //        {
                //            empno = item.empno,
                //            yymm = item.yymm,
                //            guid = Guid.NewGuid()
                //        };

                //        service.InsertProjectItem(item);
                //        service.UpsertMonthlyRecord(monthlyRecord);
                //    }

                //}


                // insert new project items into database

                using (TasklogService service = new TasklogService())
                {                    
                        service.InsertProjectItem(projectItems);
                        //service.UpsertMonthlyRecord(monthlyRecords);
                }

            }

            return ret;
        }

        [HttpPost]
        public bool UpdateUser()
        {
            // update user from txt file

            bool ret = false;

            if (Session["Admin"] is object)
            {
                List<User> users = new List<User>();

                using (TasklogService service = new TasklogService())
                {
                    ret = service.InsertUser();
                }
            }

            return ret;
        }


        [HttpPost]
        public bool InsertUserExtra(List<User> users)
        {
            bool ret = false;

            if (Session["Admin"] is object)
            {               
                using (TasklogService service = new TasklogService())
                {
                    ret = service.InsertUserExtra(users);
                }
            }

            return ret;
        }


        [HttpPost]
        public bool CreateMonthlyRecord()
        {
            bool ret = false;

            if (Session["Admin"] is object)
            {            
                using (TasklogService service = new TasklogService())
                {
                    ret = service.CreateMonthlyRecord();
                }

            }

            return ret;
        }


    }
}