﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class AssessmentController : Controller
    {
        private AssessmentService _service;

        public AssessmentController()
        {
            _service = new AssessmentService();
        }
        // GET: Assessment
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Self()
        {
            return View();
        }
        /// <summary>
        /// 給予主管建議評估表
        /// </summary>
        /// <returns></returns>
        public ActionResult Manage()
        {
            return View();
        }
        /// <summary>
        /// 請選擇要給予建議的主管(可複選)
        /// </summary>
        /// <returns></returns>
        public ActionResult ManagerOption()
        {
            return PartialView();
        }
        /// <summary>
        /// 給予主管建議評估表表單填寫
        /// </summary>
        /// <returns></returns>
        public ActionResult ManagerSuggest()
        {
            return PartialView();
        }
        /// <summary>
        /// 設置可以評核的主管名單
        /// </summary>
        /// <returns></returns>
        public ActionResult SetManager()
        {
            return PartialView();
        }

        public ActionResult SelfChart()
        {
            return View();
        }
        public ActionResult Employee()
        {
            return View();
        }
        public ActionResult EmpList()
        {
            return PartialView();
        }
        public ActionResult AssessEmp()
        {
            return PartialView();
        }
        public ActionResult Compare()
        {
            return PartialView();
        }
        public ActionResult Review()
        {            
            return PartialView();
        }
        public ActionResult Feedback()
        {
            return View();
        }
        public ActionResult EmployeeList()
        {
            return PartialView();
        }
        public ActionResult AssessEmployee()
        {
            return PartialView();
        }
        public ActionResult ReviewEmployee()
        {
            return PartialView();
        }
        public ActionResult ChartMenu()
        {
            return View();
        }
        public ActionResult ChartEmployee()
        {
            return View();
        }
        public ActionResult ChartManager()
        {
            return View();
        }
        public ActionResult ChartPerformance()
        {
            return View();
        }

        // Web api---
        //[HttpPost]
        public JsonResult GetAllSelfAssessments()
        {
            var ret = _service.GetAllSelfAssessments();
            return Json(ret);            
        }

        [HttpPost]
        public JsonResult GetAllManageAssessments(string year, User manager)
        {
            _service = new AssessmentService("manage");
            var ret = _service.GetAllManageAssessments(year, manager, Session["empno"].ToString());
            return Json(ret);
        }

        //[HttpPost]
        //public bool CreateResponse(Response response)
        //{
        //    bool ret = _service.UpdateResponse(response);
        //    return ret;
        //}

        /// <summary>
        /// 要填寫的主管名單
        /// </summary>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UserManagers()
        {
            // 找到使用者各群組的主管們
            List<User> ret = new NotifyRepository().UserManagers(Session["empno"].ToString(), "");
            return Json(ret);
        }

        /// <summary>
        /// 建立回覆
        /// </summary>
        /// <param name="assessments"></param>
        /// <param name="state"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpPost]
        public bool CreateResponse(List<Assessment> assessments, string state, string year)
        {
            bool ret = _service.UpdateResponse(assessments, Session["empno"].ToString(), state, year);
            // 如果狀態為submit並修改成功, 則更新通知資料庫 <-- 培文
            if (state.Equals("submit") && ret == true)
            {
                NotifyService notifyService = new NotifyService();
                notifyService.UpdateDatabase(Session["empno"].ToString(), 1, "0"); // 使用者寄送後取消通知
                // 找到使用者各群組的主管們
                List<User> userManagers = notifyService.UserManagers(Session["empno"].ToString(), "freeback");
                foreach(User userManager in userManagers)
                {
                    notifyService.UpdateDatabase(userManager.empno, 3, "1");
                }
            }
            return ret;            
        }

        /// <summary>
        /// 建立主管回覆
        /// </summary>
        /// <param name="assessments"></param>
        /// <param name="state"></param>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        [HttpPost]
        public bool CreateManageResponse(List<Assessment> assessments, string state, string year, User manager)
        {
            _service = new AssessmentService("manage");
            bool ret = _service.UpdateManageResponse(assessments, state, year, manager, Session["empno"].ToString());
            // 如果狀態為sent並修改成功, 則更新通知資料庫 <-- 培文
            NotifyService notifyService = new NotifyService();
            if (state.Equals("sent") && ret == true)
            {
                // 檢查是否必填的主管都填寫完成
                bool isSent = notifyService.ManagerSuggest(Session["empno"].ToString());
                if(isSent == true)
                {
                    notifyService.UpdateDatabase(Session["empno"].ToString(), 2, "1"); // 尚有主管未填寫需通知
                }
                else
                {
                    notifyService.UpdateDatabase(Session["empno"].ToString(), 2, "0"); // 所有主管都填寫完則取消通知
                }
            }
            return ret;
        }

        //[HttpPost]
        //public JsonResult GetResponse()
        //{
        //    var ret = _service.GetSelfAssessmentResponse(Session["empno"].ToString());
        //    return Json(ret);
        //}

        //[HttpPost]
        //public JsonResult GetResponse(string UserId)
        //{
        //    if (UserId == null) UserId = Session["empno"].ToString();
        //    var ret = _service.GetSelfAssessmentResponse(UserId);
        //    return Json(ret);
        //}

        [HttpPost]
        public JsonResult GetResponse(User employee)
        {
            if (employee.empno is null)
                employee.empno = Session["empno"].ToString();            
            var ret = _service.GetSelfAssessmentResponse(employee.empno);
            return Json(ret);
        }

        /// <summary>
        /// 取得主管回覆
        /// </summary>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetManageResponse(string year, User manager)
        {
            _service = new AssessmentService("manage");
            var ret = _service.GetManageAssessmentResponse(year, manager.empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetResponseByYear(User employee, string year)
        {            
            if (employee.empno is null)
                employee.empno = Session["empno"].ToString();
            var ret = _service.GetSelfAssessmentResponse(employee.empno, year);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllResponses()
        {
            var ret = _service.GetAllSelfAssessmentResponses();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllCategorySelfAssessmentCharts()
        {
            var ret = _service.GetAllCategorySelfAssessmentCharts();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllEmployees()
        {
            var ret = _service.GetAllEmployees();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetManagers()
        {
            var ret = _service.GetManagers();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetScorePeople()
        {
            var ret = _service.GetScorePeople(Session["empno"].ToString());
            return Json(ret);
        }

        //[HttpPost]
        //public JsonResult SetScorePeople()
        //{
        //    var ret = _service.SetScorePeople();
        //    return Json(ret);
        //}

        //[HttpPost]
        //public JsonResult GetAllEmployeesWithState()
        //{
        //    var ret = _service.GetAllEmployeesWithState(Session["empno"].ToString(), Session["empname"].ToString());
        //    return Json(ret);
        //}

        [HttpPost]
        public JsonResult GetAllEmployeesWithStateByRole()
        {
            var ret = _service.GetAllEmployeesWithStateByRole(Session["empno"].ToString());
            return Json(ret);
        }


        [HttpPost]
        public bool CreateMResponse(List<Assessment> assessments, User employee)
        {
            bool ret = _service.UpdateMResponse(assessments, employee.empno, Session["empno"].ToString());
            return ret;
        }

        [HttpPost]
        public JsonResult GetMResponse(User employee)
        {            
            var ret = _service.GetSelfAssessmentMResponse(employee.empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetMixResponse(User employee)
        {
            var ret = _service.GetSelfAssessmentMixResponse(employee.empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateMixResponse(List<MixResponse> mixResponses, User employee)
        {
            var ret = _service.UpdateSelfAssessmentMixResponse(mixResponses, employee.empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetYearList()
        {
            var ret = _service.GetYearList(Session["empno"].ToString());
            return Json(ret);
        }            

        [HttpPost]
        public JsonResult GetManageYearList()
        {
            _service = new AssessmentService("manage");
            var ret = _service.GetManageYearList(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetFeedback(string empno)
        {
            var ret = _service.GetFeedback(empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllOtherFeedbacks(string empno)
        {
            var ret = _service.GetAllOtherFeedbacks(empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllFeedbacksForManager(string year, string empno)
        {
            var ret = _service.GetAllFeedbacksForManager(year, empno, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllFeedbacks(string year)
        {
            var ret = _service.GetAllFeedbacks(Session["empno"].ToString(), year);
            return Json(ret);
        }

        [HttpPost]
        public bool UpdateFeedback(List<string> feedbacks, string state, string empno)
        {
           bool ret = _service.UpdateFeedback(feedbacks, state, empno, Session["empno"].ToString());
            // 如果狀態為submit並修改成功, 則更新通知資料庫 <-- 培文
            if (state.Equals("submit") && ret == true)
            {
                NotifyService notifyService = new NotifyService();
                notifyService.UpdateDatabase(empno, 1, "1"); // 通知同仁已回覆
                // 檢查是否還有尚未回覆的同仁
                List<EmployeesWithState> employeesWithState = _service.GetAllEmployeesWithStateByRole(Session["empno"].ToString());
                int submit = employeesWithState.Where(x => x.State.Equals("submit")).Count();
                if(submit > 0)
                {
                    notifyService.UpdateDatabase(Session["empno"].ToString(), 3, "1"); // 尚需回覆同仁
                }
                else
                {
                    notifyService.UpdateDatabase(Session["empno"].ToString(), 3, "0"); // 取消通知
                }
            }
            return ret;
        }

        [HttpPost]
        public JsonResult GetReviewByYear(string year, string empno)
        {
            var ret = _service.GetReviewByYear(year, empno, Session["empno"].ToString());
            return Json(ret);
        }

        //==============
        // Chart
        //==============

        //employee chart

        [HttpPost]
        public JsonResult GetChartYearList(bool isManagerResponse)
        {
            if (isManagerResponse)
                _service = new AssessmentService("manage");
            var ret = _service.GetChartYearList();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetChartGroupList()
        {
            var ret = _service.GetChartGroupList(Session["empno"].ToString());
            return Json(ret);
        }
        
        [HttpPost]
        public JsonResult GetChartEmployeeData(string year)
        {
            var ret = _service.GetChartEmployeeData(Session["empno"].ToString(), year);
            return Json(ret);
        }

        // manager chart

        //[HttpPost]
        //public JsonResult GetChartManagerList(string year)
        //{
        //    var ret = _service.GetChartManagerList(Session["empno"].ToString(), year);
        //    return Json(ret);
        //}

        public JsonResult GetChartManagerData(string year)
        {
            _service = new AssessmentService("manage");
            var ret = _service.GetChartManagerData(Session["empno"].ToString(), year);
            return Json(ret);
        }

        //=============================
        // Manage Response New service
        //=============================

        [HttpPost]
        public JsonResult ManageResponseStateCheck()
        {
            _service = new AssessmentService("manage");
            var ret = _service.ManageResponseStateCheck(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllScoreManagers()
        {
            var ret = _service.GetAllScoreManagers();
            return Json(ret);
        }

        [HttpPost]
        public bool UpdateScoreManagers(List<User> selectedManagers)
        {
            _service = new AssessmentService("manage");
            var ret = _service.UpdateScoreManagers(selectedManagers);
            return ret;
        }

        //=============================
        // Feedback Notification
        //=============================
        
        [HttpPost]
        public JsonResult GetFeedbackNotification()
        {            
            var ret = _service.GetFeedbackNotification(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public bool UpdateFeedbackNotification(string empno)
        {
            var ret = _service.UpdateFeedbackNotification(Session["empno"].ToString(), empno);
            return ret;
        }

        //=============================
        // Performance Cluster
        //=============================

        [HttpPost]
        public ContentResult GetPerformanceChart(string year)
        {            
            var ret = _service.GetPerformanceChart(year, Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        [HttpPost]
        public JsonResult GetAllPerformance(string year)
        {
            var ret = _service.GetAllPerformance(year, Session["empno"].ToString());
            return Json(ret);
        }

        //=============================
        // Database reset
        //=============================

        [HttpPost]
        public JsonResult DeleteAll()
        {
            var ret1 = _service.DeleteAll();

            AssessmentService _service2 = new AssessmentService("manage");
            var ret2 = _service2.DeleteAll();

            var ret = ret1 || ret2;

            return Json(ret);
        }        

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}