using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.EducationV2;

namespace TEEmployee.Controllers
{
    public class EducationV2Controller : Controller
    {
        private EducationService _service;

        public EducationV2Controller()
        {
            _service = new EducationService();
        }

        // GET: EducationV2
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AssignMenu()
        {
            return View();
        }

        public ActionResult Assign()
        {
            return View();
        }

        public ActionResult Unassign()
        {
            return View();
        }

        public ActionResult Record()
        {
            return View();
        }

        /* =======================================
                        Web api
          ====================================== */

        [HttpPost]
        public JsonResult GetAllContents()
        {
            var ret = _service.GetAllContents();
            return Json(ret);
        }

        public JsonResult GetAllValidContents()
        {
            var ret = _service.GetAllValidContents();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UploadCourseFile(HttpPostedFileBase courseFile)
        {
            var ret = _service.UploadCourseFile(courseFile.InputStream, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        public JsonResult GetAssignmentsByAssigner(List<string> empnos)
        {
            var ret = _service.GetAssignmentsByAssigner(empnos, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpsertAssignments(List<Assignment> assignments)
        {
            var ret = _service.UpsertAssignments(assignments, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteAssignments(List<Assignment> assignments)
        {
            var ret = _service.DeleteAssignments(assignments, Session["empno"].ToString());
            return Json(ret);
        }

        public JsonResult GetAssignmentsWithRecord(List<string> empnos)
        {
            var ret = _service.GetAssignmentsWithRecord(empnos);
            return Json(ret);
        }
    }
}