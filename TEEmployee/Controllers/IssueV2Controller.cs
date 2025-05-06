using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.IssueV2;

namespace TEEmployee.Controllers
{
    public class IssueV2Controller : Controller
    {
        private IssueService _service;

        public IssueV2Controller()
        {
            _service = new IssueService();
        }

        // GET: IssueV2
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
        public JsonResult DeleteProject(Project project)
        {
            var ret = _service.DeleteProject(project);
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
        public JsonResult DeleteIssue(Issue issue)
        {
            var ret = _service.DeleteIssue(issue);
            return Json(ret);
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        [HttpPost]
        public JsonResult GetCategoriesByGroupOne(string group_one)
        {
            var ret = _service.GetCategoriesByGroupOne(group_one);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateCategory(CustomCategory category)
        {
            var ret = _service.CreateCategory(category);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteCategory(CustomCategory category)
        {
            var ret = _service.DeleteCategory(category);
            return Json(ret);
        }
    }
}