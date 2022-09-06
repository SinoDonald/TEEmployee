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
        public ActionResult Manage()
        {
            return View();
        }
        public ActionResult ManagerOption()
        {
            return PartialView();
        }
        public ActionResult ManagerSuggest()
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

        // Web api---
        [HttpPost]
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

        [HttpPost]
        public bool CreateResponse(List<Assessment> assessments, string state, string year)
        {
            bool ret = _service.UpdateResponse(assessments, Session["empno"].ToString(), state, year);
            return ret;            
        }

        [HttpPost]
        public bool CreateManageResponse(List<Assessment> assessments, string state, string year, User manager)
        {
            _service = new AssessmentService("manage");
            bool ret = _service.UpdateManageResponse(assessments, state, year, manager, Session["empno"].ToString());
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
        public JsonResult GetAllFeedbacks(string year)
        {
            var ret = _service.GetAllFeedbacks(Session["empno"].ToString(), year);
            return Json(ret);
        }

        [HttpPost]
        public bool UpdateFeedback(List<string> feedbacks, string state, string empno)
        {
           bool ret = _service.UpdateFeedback(feedbacks, state, empno, Session["empno"].ToString());
            return ret;
        }

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

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}