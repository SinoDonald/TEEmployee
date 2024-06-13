using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.TaskLog;
using TEEmployee.Models.GSchedule;
using System.Web.Services.Description;
using TEEmployee.Models.Talent;
using Newtonsoft.Json;
using TEEmployee.Models.Profession;
using System.IO;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Notify(); // 重要通知
            //InsertProjectItem();
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
        /// <summary>
        /// Admin：人才資料庫, 上傳Word履歷、員工經歷(行政部資料)、KPI檔案
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin()
        {
            ViewBag.Message = "Hello there!!";

            return View();

            //if (Session["Admin"] is object)
            //    return View();
            //else
            //    return RedirectToAction("Index");
        }

        public ActionResult Document()
        {            
            return View();           
        }

        public ActionResult Question()
        {            
            return View();
        }

        public ActionResult History()
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

        /// <summary>
        /// 首頁通知
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Notify()
        {
            NotifyService _service = new NotifyService();
            List<bool> ret = _service.GetNotify(Session["empno"].ToString());
            Session["notify"] = ret;
            Session["notify_count"] = ret.Where(x => x==true).Count();

            return Json(ret);
        }
        /// <summary>
        /// 人才培訓資料庫
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TalentUpdate()
        {
            var ret = "";
            TalentService _service = new TalentService();
            _service.TalentUpdate();

            return Json(ret);
        }
        /// <summary>
        /// 檢視user.db
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReviewUserDB()
        {
            var ret = new UserRepository().GetAll().OrderBy(x => x.empno).ToList();
            return Json(ret);
        }
        /// <summary>
        /// 下載user.db
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadUserDB()
        {
            string filePath = new UserRepository().DownloadUserDB();
            ProfessionService _service = new ProfessionService();
            var fileBytes = _service.DownloadFile(filePath);
            string contentType = "application/octet-stream"; // byte
            FileContentResult result = null;
            try
            {
                result = File(fileBytes, contentType, "userDB.xlsx");
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
        /// <summary>
        /// 檢視profession.db
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReviewProfessionDB()
        {
            var ret = new ProfessionRepository().GetAll().OrderBy(x => x.id).ToList();
            return Json(ret);
        }
        /// <summary>
        /// 下載profession.db
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadProfessionDB()
        {
            string filePath = new ProfessionRepository().DownloadProfessionDB();
            ProfessionService _service = new ProfessionService();
            var fileBytes = _service.DownloadFile(filePath);
            string contentType = "application/octet-stream"; // byte
            FileContentResult result = null;
            try
            {
                result = File(fileBytes, contentType, "professionDB.xlsx");
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        [HttpPost]
        public bool InsertProjectItem()
        {
            bool ret = false;

            if (/*Session["Admin"] is object*/ true)
            {               

                // read new project items from txt file

                List<ProjectItem> projectItems = new List<ProjectItem>();
                List<ProjectItem> DBprojectItems = new List<ProjectItem>();

                using (TasklogService service = new TasklogService(false))
                {
                    projectItems = service.GetAllProjectItem();
                }

                if (projectItems.Count != 0) ret = true;
                else return false;
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
                    DBprojectItems = service.GetAllProjectItem().Where(x => x.yymm == projectItems[0].yymm).ToList();

                    List<ProjectItem> intersectProjectItems = DBprojectItems.Intersect(projectItems, new ProjectItemEqualityComparer()).ToList();
                    List<ProjectItem> exceptProjectItems = DBprojectItems.Except(intersectProjectItems, new ProjectItemEqualityComparer()).ToList();

                    service.DeleteProjectItem(exceptProjectItems);
                    service.UpsertProjectItem(projectItems);
                    //service.InsertProjectItem(projectItems);
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

            //if (Session["Admin"] is object)
            //{
            //    List<User> users = new List<User>();

            //    using (TasklogService service = new TasklogService())
            //    {
            //        ret = service.InsertUser();
            //    }
            //}

          
            List<User> users = new List<User>();

            using (TasklogService service = new TasklogService())
            {
                ret = service.InsertUser();
            }
            

            return ret;
        }


        [HttpPost]
        public bool InsertUserExtra(string usersJson)
        {
            bool ret = false;

            List<User> users = JsonConvert.DeserializeObject<List<User>>(usersJson);

            //if (Session["Admin"] is object)
            //{               
            //    using (TasklogService service = new TasklogService())
            //    {
            //        ret = service.InsertUserExtra(users);
            //    }
            //}


            using (TasklogService service = new TasklogService())
            {
                ret = service.InsertUserExtra(users);
            }
            

            return ret;
        }

        //[HttpPost]
        //public bool InsertUserExtra(List<User> users)
        //{
        //    bool ret = false;

        //    //if (Session["Admin"] is object)
        //    //{               
        //    //    using (TasklogService service = new TasklogService())
        //    //    {
        //    //        ret = service.InsertUserExtra(users);
        //    //    }
        //    //}


        //    using (TasklogService service = new TasklogService())
        //    {
        //        ret = service.InsertUserExtra(users);
        //    }


        //    return ret;
        //}


        [HttpPost]
        public bool CreateMonthlyRecord(string yymm)
        {
            bool ret = false;

            //if (Session["Admin"] is object)
            //{            
            //    using (TasklogService service = new TasklogService())
            //    {
            //        ret = service.CreateMonthlyRecord();
            //    }

            //}

            
            using (TasklogService service = new TasklogService())
            {
                ret = service.CreateMonthlyRecord(yymm);
            }            

            return ret;
        }


        [HttpPost]
        public JsonResult UpdateAllPercentComplete()
        {
            using (GScheduleService service = new GScheduleService())
            {
                var ret = service.UpdateAllPercentComplete();
                return Json(ret);
            }                
        }
                
        [HttpPost]
        public JsonResult UploadEmployeeFile(HttpPostedFileBase employeeFile)
        {
            using (TasklogService service = new TasklogService())
            {
                var ret = service.UploadEmployeeFile(employeeFile.InputStream);
                return Json(ret);
            }
        }

        [HttpPost]
        public JsonResult UploadProjectItemFile(HttpPostedFileBase projectItemFile)
        {
            using (TasklogService service = new TasklogService())
            {
                var ret = service.UploadProjectItemFile(projectItemFile.InputStream);
                return Json(ret);
            }
        }

    }
}