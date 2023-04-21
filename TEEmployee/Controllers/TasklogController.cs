using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.TaskLog;
using static TEEmployee.Models.TaskLog.TasklogService;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class TasklogController : Controller
    {
        private TasklogService _service;

        public TasklogController()
        {
            _service = new TasklogService();
        }

        // GET: Tasklog
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Details(Guid id)
        {
            ViewBag.id = id;
            return View();
        }
        public ActionResult UserList()
        {
            return PartialView();
        }

        public ActionResult UsersDetails()
        {
            return PartialView();
        }

        /*........................  Web api  ...........................*/

        [HttpPost]
        public JsonResult GetAllMonthlyRecord(string yymm)
        {
            // get the record base on the role
            var ret = _service.GetAllMonthlyRecord(Session["empno"].ToString(), yymm);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllMonthlyRecordData(string yymm)
        {
            // get the record base on the role
            var ret = _service.GetAllMonthlyRecordData(Session["empno"].ToString(), yymm);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetTasklogData(string empno, string yymm)
        {           
            var ret = _service.GetTasklogData(Session["empno"].ToString(), yymm);
            return Json(ret);
        }


        [HttpPost]
        public bool UpdateProjectTask(List<ProjectTask> projectTasks)
        {            
            var ret = _service.UpdateProjectTask(projectTasks, Session["empno"].ToString());
            return ret;
        }

        [HttpPost]
        public bool DeleteProjectTask(List<int> deletedIds)
        {
            var ret = _service.DeleteProjectTask(deletedIds, Session["empno"].ToString());
            return ret;
        }

        // Details
        [HttpPost]
        public JsonResult GetTasklogDataByGuid(string guid)
        {
            var ret = _service.GetTasklogDataByGuid(guid);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetUserByGuid(string guid)
        {
            var ret = _service.GetUserByGuid(guid);
            return Json(ret);
        }

        // 匯入上月資料 <-- 培文
        [HttpPost]
        public JsonResult GetLastMonthData(string empno, string yymm)
        {
            int insertIndex = yymm.Length - 2;
            // 轉換年月, format加上"-"
            string lastMonth = yymm.Insert(insertIndex, "-");

            // 民國年轉西元, 並減一個月
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new System.Globalization.TaiwanCalendar();
            DateTime dateTime = DateTime.Parse(lastMonth, culture).AddMonths(-1);
            
            // 將西元年轉成民國
            culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            lastMonth = dateTime.ToString("yyy-MM", culture).Replace("-", "");

            TasklogData ret = _service.GetTasklogData(Session["empno"].ToString(), lastMonth);
            // 
            // 更新撈回來的id與月份
            foreach(ProjectItem projectItem in ret.ProjectItems)
            {
                projectItem.yymm = yymm;
            }
            foreach (ProjectTask projectTask in ret.ProjectTasks)
            {
                projectTask.id = 0;
                projectTask.yymm = yymm;
            }

            return Json(ret);
        }

        // 多人詳細內容 <-- 培文
        public JsonResult GetMemberContent(List<MonthlyRecord> monthlyRecord, List<User> users)
        {
            var yymm = monthlyRecord[0].yymm;
            List<TasklogData> userTasklogDataList = new List<TasklogData>();
            foreach (User user in users)
            {
                userTasklogDataList.Add(_service.GetTasklogData(user.empno, yymm));
            }

            var ret = userTasklogDataList[0];
            
            return Json(userTasklogDataList);
        }

        //[HttpPost]
        //public JsonResult GetProjectTask(string yymm)
        //{            
        //    var ret = _service.GetProjectTask(Session["empno"].ToString(), yymm);
        //    return Json(ret);
        //}

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

    }
}