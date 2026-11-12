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
using TEEmployee.Models.Notify;

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
            // 只有管理員與 AdminPageUsers 名單可進入 (首頁連結的顯示條件相同)
            if (!Permissions.CanUseAdminPage(Session))
                return RedirectToAction("Index");

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

        /// <summary>
        /// 管理員切換身分 (模擬其他員工)。
        /// 切換後 Session 完全以該員工為準 (包含沒有管理員權限)，
        /// 但保留 Session["ImpersonatorAdmin"]，可再切換回自己；切回自己時恢復管理員權限。
        /// </summary>
        [HttpPost]
        public ActionResult Index(User fake)
        {
            if (Permissions.CanSwitchIdentity(Session) && fake != null && !string.IsNullOrWhiteSpace(fake.empno))
            {
                User user = new UserRepository().Get(fake.empno.Trim());

                if (user is null)
                    return RedirectToAction("Unauthorized", "Error");

                string original = Session["original_empno"] as string;
                bool backToSelf = Permissions.SameEmpno(user.empno, original);

                Session["empno"] = user.empno;
                Session["empname"] = user.name;
                Session["group"] = user.group ?? "";
                Session["group_one"] = user.group_one ?? "";
                Session["group_two"] = user.group_two ?? "";
                Session["group_three"] = user.group_three ?? "";
                Session["group_four"] = user.group_four ?? "";

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

                if (user.department_manager || user.group_manager || user.group_one_manager || user.group_two_manager || user.group_three_manager || user.group_four_manager)
                {
                    Session["role"] = "Manager";
                }

                // 管理員權限依「被切換成的員工」決定
                if (Permissions.IsAdminEmpno(user.empno))
                    Session["Admin"] = true;
                else
                    Session.Remove("Admin");

                if (backToSelf)
                    Session.Remove("ImpersonatorAdmin");
                else
                    Session["ImpersonatorAdmin"] = true;
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
        /// 更新通知
        /// </summary>
        [HttpPost]
        public JsonResult UpdateNotify()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員 (會重建全體員工的通知)
            var ret = new NotifyService().UpdateNotify(Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 刪除通知Log檔
        /// </summary>
        [HttpPost]
        public JsonResult DeleteNotifyLog()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = new NotifyService().DeleteNotifyLog();
            return Json(ret);
        }
        /// <summary>
        /// 檢視user.db
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReviewUserDB()
        {
            if (!Permissions.IsAdmin(Session)) return Json(new List<User>()); // 限系統管理員

            var ret = new UserRepository().GetAll().OrderBy(x => x.empno).ToList();
            return Json(ret);
        }
        /// <summary>
        /// 下載user.db
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadUserDB()
        {
            if (!Permissions.IsAdmin(Session)) return new HttpStatusCodeResult(403); // 限系統管理員

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
            if (!Permissions.IsAdmin(Session)) return Json(new List<object>()); // 限系統管理員

            var ret = new ProfessionRepository().GetAll().OrderBy(x => x.id).ToList();
            return Json(ret);
        }
        /// <summary>
        /// 下載profession.db
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadProfessionDB()
        {
            if (!Permissions.IsAdmin(Session)) return new HttpStatusCodeResult(403); // 限系統管理員

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
            if (!Permissions.IsAdmin(Session)) return false; // 限系統管理員

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
            if (!Permissions.CanUseAdminPage(Session)) return false; // 匯入組織表：管理員或 AdminPageUsers

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
            if (!Permissions.CanUseAdminPage(Session)) return false; // 匯入組織表：管理員或 AdminPageUsers

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
            if (!Permissions.IsAdmin(Session)) return false; // 限系統管理員

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
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            using (GScheduleService service = new GScheduleService())
            {
                var ret = service.UpdateAllPercentComplete();
                return Json(ret);
            }                
        }
                
        [HttpPost]
        public JsonResult UploadEmployeeFile(HttpPostedFileBase employeeFile)
        {
            if (!Permissions.CanUseAdminPage(Session)) return Json(false); // 匯入組織表：管理員或 AdminPageUsers

            using (TasklogService service = new TasklogService())
            {
                var ret = service.UploadEmployeeFile(employeeFile.InputStream);
                return Json(ret);
            }
        }

        [HttpPost]
        public JsonResult UploadProjectItemFile(HttpPostedFileBase projectItemFile)
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            using (TasklogService service = new TasklogService())
            {
                var ret = service.UploadProjectItemFile(projectItemFile.InputStream);
                return Json(ret);
            }
        }

        ///// <summary>
        ///// 人才培訓資料庫
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult TalentUpdate()
        //{
        //    var ret = "";
        //    TalentService _service = new TalentService();
        //    _service.TalentUpdate();

        //    return Json(ret);
        //}
    }
}