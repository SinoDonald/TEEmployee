using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Education2024;

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

        //public ActionResult Assign()
        //{
        //    return View();
        //}

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
        
        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}