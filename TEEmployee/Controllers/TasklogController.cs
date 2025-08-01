﻿using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
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
        /// <summary>
        /// 查閱工作紀錄管控表
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 查閱工作紀錄管控表：員工名單
        /// </summary>
        /// <returns></returns>
        public ActionResult UserList()
        {
            return PartialView();
        }
        /// <summary>
        /// 查閱工作紀錄管控表：個人詳細內容
        /// </summary>
        /// <returns></returns>
        public ActionResult UserDetails()
        {
            return PartialView();
        }
        /// <summary>
        /// 查閱工作紀錄管控表：多人詳細內容
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 匯入上月資料
        /// </summary>
        /// <param name="yymm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetLastMonthData(string yymm)
        {
            int insertIndex = yymm.Length - 2;
            // 轉換年月, format加上"-"
            string thisMonth = yymm.Insert(insertIndex, "-");
            string lastMonth = yymm.Insert(insertIndex, "-");

            // 民國年轉西元, 並減一個月
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new System.Globalization.TaiwanCalendar();
            DateTime thisMonthDateTime = DateTime.Parse(lastMonth, culture);
            DateTime lastMonthDateTime = DateTime.Parse(lastMonth, culture).AddMonths(-1);
            
            // 將西元年轉成民國
            culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            thisMonth = thisMonthDateTime.ToString("yyy-MM", culture).Replace("-", "");
            lastMonth = lastMonthDateTime.ToString("yyy-MM", culture).Replace("-", "");

            // 儲存上月與本月的資料
            List<ProjectItem> projectItems = new List<ProjectItem>();
            List<ProjectTask> projectTasks = new List<ProjectTask>();

            TasklogData thisMonthData = _service.GetTasklogData(Session["empno"].ToString(), thisMonth);
            foreach (ProjectItem projectItem in thisMonthData.ProjectItems)
            {
                projectItems.Add(projectItem);
            }
            foreach (ProjectTask projectTask in thisMonthData.ProjectTasks)
            {
                projectTasks.Add(projectTask);
            }

            // 更新抓取回來的id與月份
            TasklogData lastMonthData = _service.GetTasklogData(Session["empno"].ToString(), lastMonth);
            foreach(ProjectItem projectItem in lastMonthData.ProjectItems)
            {
                // 上月與本月有同計畫編號時, 只留存本月的
                ProjectItem samePrjName = projectItems.Where(x => x.projno.Equals(projectItem.projno)).FirstOrDefault();
                if(samePrjName == null)
                {
                    projectItem.yymm = yymm;
                    projectItem.workHour = 0; // 工時不要帶入
                    projectItem.overtime = 0; // 加班不要帶入
                    projectItems.Add(projectItem);
                }
            }
            foreach (ProjectTask projectTask in lastMonthData.ProjectTasks)
            {
                projectTask.id = 0;
                projectTask.yymm = yymm;
                projectTask.realHour = 0; // 實際時數不要帶入
                projectTasks.Add(projectTask);
            }

            TasklogData ret = new TasklogData() { ProjectItems = projectItems, ProjectTasks = projectTasks };

            return Json(ret);
        }
        /// <summary>
        /// 取得使用者群組
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public JsonResult GetGroups(string json)
        {
            List<MonthlyRecordData> monthlyRecordData = JsonConvert.DeserializeObject<List<MonthlyRecordData>>(json); //反序列化
            List<string> ret = new List<string>();
            if(monthlyRecordData != null && monthlyRecordData.Count > 0)
            {
                List<string> groups = monthlyRecordData.Where(x => x.User.group != "").Select(x => x.User.group).Distinct().OrderBy(x => x).ToList();
                foreach (string group in groups)
                {
                    ret.Add(group);
                }
                groups = monthlyRecordData.Where(x => x.User.group_one != "").Select(x => x.User.group_one).Distinct().OrderBy(x => x).ToList();
                foreach (string group in groups)
                {
                    ret.Add(group);
                }
                groups = monthlyRecordData.Where(x => x.User.group_two != "").Select(x => x.User.group_two).Distinct().OrderBy(x => x).ToList();
                foreach (string group in groups)
                {
                    ret.Add(group);
                }
                groups = monthlyRecordData.Where(x => x.User.group_three != "").Select(x => x.User.group_three).Distinct().OrderBy(x => x).ToList();
                foreach (string group in groups)
                {
                    ret.Add(group);
                }

                ret = ret.OrderBy(x => x.Length).Distinct().ToList();
            }

            ret.Insert(0, "全部顯示");

            return Json(ret);
        }
        /// <summary>
        /// 群組篩選
        /// </summary>
        /// <param name="json"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public JsonResult GetGroupByName(string json, string groupName)
        {
            List<MonthlyRecordData> monthlyRecordData = JsonConvert.DeserializeObject<List<MonthlyRecordData>>(json); //反序列化
            List<MonthlyRecordData> ret = new List<MonthlyRecordData>();
            if (groupName == null)
            {
                ret = monthlyRecordData;
            }
            else if (groupName.Equals("全部顯示"))
            {
                ret = monthlyRecordData;
            }
            else
            {
                ret = monthlyRecordData.Where(x => x.User.group.Equals(groupName) || x.User.group_one.Equals(groupName) ||
                                                                        x.User.group_two.Equals(groupName) || x.User.group_three.Equals(groupName)).ToList();
            }

            return Json(ret);
        }
        /// <summary>
        /// 個人詳細內容
        /// </summary>
        /// <param name="startMonth"></param>
        /// <param name="endMonth"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public JsonResult GetUserContent(string startMonth, string endMonth, User user)
        {
            // 月份差距
            List<string> yymms = new List<string>();
            DateTime sDate = new DateTime();
            DateTime eDate = DateTime.Now;
            if (startMonth != null)
            {
                sDate = DateTime.Parse(startMonth);
            }
            if (endMonth != null)
            {
                eDate = DateTime.Parse(endMonth);
            }
            if (sDate > eDate)
            {
                sDate = DateTime.Parse(endMonth);
                eDate = DateTime.Parse(startMonth);
            }
            int monthsSpan = eDate.Year * 12 + eDate.Month - sDate.Year * 12 - sDate.Month; // 相差幾個月
            TaiwanCalendar tc = new TaiwanCalendar(); // 使用民國
            for (int i = 0; i <= monthsSpan; i++)
            {
                DateTime dt = eDate.AddMonths(-i);
                yymms.Add(tc.GetYear(dt).ToString("000") + tc.GetMonth(dt).ToString("00"));
            }

            List<MultiTasklogData> ret = new List<MultiTasklogData>();
            foreach (string yymm in yymms)
            {
                ret.Add(_service.GetMultiTasklogData(user, yymm));
            }

            return Json(ret);
        }
        /// <summary>
        /// 多人詳細內容
        /// </summary>
        /// <param name="yymm"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        public JsonResult GetMemberContent(string yymm, List<User> users)
        {
            List<MultiTasklogData> ret = new List<MultiTasklogData>();
            foreach (User user in users)
            {
                ret.Add(_service.GetMultiTasklogData(user, yymm));
            }
            
            return Json(ret);
        }

        //[HttpPost]
        //public JsonResult GetProjectTask(string yymm)
        //{            
        //    var ret = _service.GetProjectTask(Session["empno"].ToString(), yymm);
        //    return Json(ret);
        //}

        [HttpPost]
        public JsonResult AddProjectTypeColumn()
        {               
            try
            {
                _service.AddProjectTypeColumn();
                var response = new { Success = true };
                return Json(response);
            }
            catch
            {
                var response = new { Success = false };
                return Json(response);
            }
        }


        [HttpPost]
        public JsonResult AddCustomOrderColumn()
        {
            var ret = _service.AddCustomOrderColumn();
            return Json(ret);
        }


        [HttpPost]
        public JsonResult AddGenerateScheduleColumn()
        {
            var ret = _service.AddGenerateScheduleColumn();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertCustomUser()
        {
            var ret = _service.InsertCustomUser();
            return Json(ret);
        }

        //=============================
        // Database reset
        //=============================

        [HttpPost]
        public JsonResult DeleteAll()
        {
            // Only delete ProjectTask and MonthlyRecord
            var ret = _service.DeleteAll();
            return Json(ret);
        }


        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

    }
}