using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;

namespace TEEmployee.Controllers
{
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

        // Web api---
        [HttpPost]
        public JsonResult GetAllSelfAssessments()
        {
            var ret = _service.GetAllSelfAssessments();
            return Json(ret);
        }
        [HttpPost]
        public JsonResult GetManageAssessments()
        {
            var ret = _service.GetManageAssessments();
            return Json(ret);
        }

        [HttpPost]
        public bool CreateResponse(Response response)
        {
            bool ret = _service.UpdateResponse(response);
            return ret;
        }


        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}