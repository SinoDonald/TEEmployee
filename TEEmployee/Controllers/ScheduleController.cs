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
        public ActionResult Test()
        {
            return PartialView();
        }

        public ActionResult TF2()
        {
            return PartialView();
        }

        public ActionResult ShowTime()
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

        [HttpPost]
        public JsonResult GetAllEmployeesByRole()
        {
            var ret = _service.GetAllEmployeesByRole(Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllOwnedSubGroups()
        {
            var ret = _service.GetAllOwnedSubGroups(Session["empno"].ToString());
            return Json(ret);
        }
        

        [HttpPost]
        public JsonResult UpdateOwnedSchedules(List<Schedule> schedules)
        {
            var ret = _service.UpdateOwnedSchedules(schedules, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertSingleSchedule(Schedule schedule)
        {
            var ret = _service.InsertSingleSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateSingleSchedule(Schedule schedule, List<int> deletedMilestones)
        {
            var ret = _service.UpdateSingleSchedule(schedule, deletedMilestones, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteSingleSchedule(Schedule schedule)
        {
            var ret = _service.DeleteSingleSchedule(schedule, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteOwnedSchedules(List<int> schedules, List<int> milestones)
        {
            var ret = _service.DeleteOwnedSchedules(schedules, milestones);
            return Json(ret);
        }
        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}