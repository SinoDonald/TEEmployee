using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.GEducation;

namespace TEEmployee.Controllers
{
    public class GEducationController : Controller
    {
        private GEducationService _service;

        public GEducationController()
        {
            _service = new GEducationService();
        }

        // GET: GEducation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Assign()
        {
            return View();
        }

        public ActionResult Curriculum()
        {
            return View();
        }

        public ActionResult Digital()
        {
            return View();
        }

        /* =======================================
                     Web api
       ====================================== */

        //[HttpPost]
        //public JsonResult GetAllCourses()
        //{
        //    var ret = _service.GetAllCourses();
        //    return Json(ret);
        //}

        [HttpPost]
        public JsonResult GetAllChapters()
        {
            var ret = _service.GetAllChapters();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UploadCourseFile(HttpPostedFileBase courseFile)
        {
            var ret = _service.UploadCourseFile(courseFile.InputStream);
            return Json(ret);
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        [HttpPost]
        public JsonResult UpsertRecords(List<Record> records)
        {
            var ret = _service.UpsertRecords(records, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateRecordCompleted(Record record)
        {
            var ret = _service.UpdateRecordCompleted(record, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateChapterDigitalized(Chapter chapter)
        {
            var ret = _service.UpdateChapterDigitalized(chapter, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllRecordsByUser(string empno)
        {
            var ret = _service.GetAllRecordsByUser(empno);
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

    }
}