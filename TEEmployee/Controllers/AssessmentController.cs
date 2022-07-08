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
            //_service = new AssessmentService("manage");
            return View();
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


        // Web api---
        [HttpPost]
        public JsonResult GetAllSelfAssessments()
        {
            var ret = _service.GetAllSelfAssessments();
            return Json(ret);            
        }

        [HttpPost]
        public JsonResult GetAllManageAssessments()
        {
            _service = new AssessmentService("manage");
            var ret = _service.GetAllManageAssessments();
            return Json(ret);
        }

        //[HttpPost]
        //public bool CreateResponse(Response response)
        //{
        //    bool ret = _service.UpdateResponse(response);
        //    return ret;
        //}

        [HttpPost]
        public bool CreateResponse(List<Assessment> assessments)
        {
            bool ret = _service.UpdateResponse(assessments, Session["empno"].ToString());
            return ret;            
        }

        [HttpPost]
        public bool CreateManageResponse(List<Assessment> assessments)
        {
            _service = new AssessmentService("manage");
            bool ret = _service.UpdateManageResponse(assessments, Session["empno"].ToString());
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
        public JsonResult GetManageResponse()
        {
            _service = new AssessmentService("manage");
            var ret = _service.GetManageAssessmentResponse(Session["empno"].ToString());
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

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}