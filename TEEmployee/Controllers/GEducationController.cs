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