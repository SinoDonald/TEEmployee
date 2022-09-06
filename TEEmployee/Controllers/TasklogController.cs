using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models.TaskLog;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class TasklogController : Controller
    {
        private TasklogService _service;

        public TasklogController()
        {
            _service = new TasklogService();
        }

        // GET: Tasklog
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Details(Guid id)
        {
            ViewBag.id = id;
            return View();
        }

        /*........................  Web api  ...........................*/

        [HttpPost]
        public JsonResult GetAllMonthlyRecord(string yymm)
        {
            // get the record base on the role
            var ret = _service.GetAllMonthlyRecord(Session["empno"].ToString(), yymm);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetTasklogData(string empno, string yymm)
        {           
            var ret = _service.GetTasklogData(Session["empno"].ToString(), yymm);
            return Json(ret);
        }


        [HttpPost]
        public bool UpdateProjectTask(List<ProjectTask> projectTasks)
        {            
            var ret = _service.UpdateProjectTask(projectTasks, Session["empno"].ToString());
            return ret;
        }

        [HttpPost]
        public bool DeleteProjectTask(List<int> deletedIds)
        {
            var ret = _service.DeleteProjectTask(deletedIds, Session["empno"].ToString());
            return ret;
        }

        // Details
        [HttpPost]
        public JsonResult GetTasklogDataByGuid(string guid)
        {
            var ret = _service.GetTasklogDataByGuid(guid);
            return Json(ret);
        }

        //[HttpPost]
        //public JsonResult GetProjectTask(string yymm)
        //{            
        //    var ret = _service.GetProjectTask(Session["empno"].ToString(), yymm);
        //    return Json(ret);
        //}

    }
}