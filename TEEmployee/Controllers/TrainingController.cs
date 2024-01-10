using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Training;

namespace TEEmployee.Controllers
{
    public class TrainingController : Controller
    {
        private TrainingService _service;

        public TrainingController()
        {
            _service = new TrainingService();
        }
        // GET: Training
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Group()
        {
            return View();
        }

        /* =======================================
                        Web api
          ====================================== */

        [HttpPost]
        public ContentResult GetAllRecords()
        {            
            var ret = _service.GetAllRecordsJSON();
            return Content(ret, "application/json");
        }

        [HttpPost]
        public ContentResult GetAllRecordsByUser(string empno)
        {
            if (string.IsNullOrEmpty(empno))
            {
                empno = Session["empno"].ToString();
            }                

            var ret = _service.GetAllRecordsByUserJSON(empno, Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        // Upload file from client side to server 
        [HttpPost]
        public JsonResult UploadTrainingFile(HttpPostedFileBase trainingFile)
        {
            var ret = _service.UploadTrainingFile(trainingFile.InputStream);
            return Json(ret);
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}