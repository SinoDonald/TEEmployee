using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Education;

namespace TEEmployee.Controllers
{
    public class EducationController : Controller
    {
        private EducationService _service;

        public EducationController()
        {
            _service = new EducationService();
        }
        // GET: Training
        public ActionResult Index()
        {
            return View();
        }

        /* =======================================
                       Web api
         ====================================== */

        [HttpPost]
        public JsonResult GetAllCourses()
        {
            var ret = _service.GetAllCourses();
            return Json(ret);
        }
                
        [HttpPost]
        public JsonResult UploadCourseFile(HttpPostedFileBase courseFile)
        {
            var ret = _service.UploadCourseFile(courseFile.InputStream);
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}