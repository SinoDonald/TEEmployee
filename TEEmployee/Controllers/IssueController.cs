using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Issue;
using TEEmployee.Models.Training;

namespace TEEmployee.Controllers
{
    public class IssueController : Controller
    {
        private IssueService _service;

        public IssueController()
        {
            _service = new IssueService();
        }

        // GET: Issue
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            return View();
        }

        /* =======================================
                        Web api
          ====================================== */

        [HttpPost]
        public JsonResult GetAllProjects()
        {
            var ret = _service.GetAllProjects();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetProjectsByGroupOne(string group_one)
        {
            var ret = _service.GetProjectsByGroupOne(group_one);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateProject(Project project)
        {
            var ret = _service.CreateProject(project);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateIssue(Issue issue)
        {
            var ret = _service.CreateIssue(issue);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateIssue(Issue issue)
        {
            var ret = _service.UpdateIssue(issue);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateControlledItem(ControlledItem item)
        {
            var ret = _service.CreateControlledItem(item);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateControlledItem(ControlledItem item)
        {
            var ret = _service.UpdateControlledItem(item);
            return Json(ret);
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }
    }
}