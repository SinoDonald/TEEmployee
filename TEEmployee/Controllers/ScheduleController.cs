using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models.Schedule;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class ScheduleController : Controller
    {
        private ScheduleService _service;

        public ScheduleController()
        {
            _service = new ScheduleService();
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
        public ActionResult Individual()
        {
            return PartialView();
        }
        public ActionResult Future()
        {
            return PartialView();
        }


        /*........................  Web api  ...........................*/

        [HttpPost]
        public JsonResult GetAllSchedules(string yymm)
        {
            // get the record base on the role
            var ret = _service.GetAllSchedules();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllOwnedSchedules()
        {            
            var ret = _service.GetAllOwnedSchedules(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllReferredSchedules()
        {
            var ret = _service.GetAllReferredSchedules(Session["empno"].ToString());
            return Json(ret);
        }
    }
}