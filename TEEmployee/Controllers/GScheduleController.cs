using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models.GSchedule;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class GScheduleController : Controller
    {
        private GScheduleService _service;

        public GScheduleController()
        {
            _service = new GScheduleService();
        }

        // GET: Schedule
        public ActionResult Index()
        {            
            return View();
        }
        public ActionResult Group()
        {
            return PartialView();
        }
        public ActionResult Personal()
        {
            return PartialView();
        }
        public ActionResult Future()
        {
            return PartialView();
        }
        public ActionResult Project()
        {
            return PartialView();
        }
        /*........................  Web api  ...........................*/

        [HttpPost]
        public JsonResult GetAllSchedules()
        {
            // get the record base on the role
            var ret = _service.GetAllGroupSchedules(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateSchedule(Schedule schedule)
        {
            var ret = _service.UpdateSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertSchedule(Schedule schedule)
        {          
            var ret = _service.InsertSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteSchedule(Schedule schedule)
        {
            var ret = _service.DeleteSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateAllPercentComplete()
        {
            var ret = _service.UpdateAllPercentComplete();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllFutures()
        {
            // get the record base on the role
            var ret = _service.GetAllFutures(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllProjectSchedules()
        {
            var ret = _service.GetAllProjectSchedules(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertProjectSchedule(ProjectSchedule project)
        {
            var ret = _service.InsertProjectSchedule(project, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteProjectSchedule(ProjectSchedule project)
        {
            var ret = _service.DeleteProjectSchedule(project, Session["empno"].ToString());
            return Json(ret);
        }

        // Upload file from client side to server 
        [HttpPost]
        public ActionResult UploadProjectSchedule(HttpPostedFileBase file, FormCollection form)
        {
            string jsonString = form.GetValue("projectSchedule").AttemptedValue;
             ProjectSchedule projectSchedule = JsonConvert.DeserializeObject<ProjectSchedule>(jsonString);
            var ret = _service.UploadProjectSchedule(file, projectSchedule);

            return Json(ret);
        }
        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}