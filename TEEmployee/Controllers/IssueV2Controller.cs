using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.IssueV2;
using TEEmployee.Models;
using TEEmployee.Filters;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
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
            // 只回傳自己可存取群組的計畫
            var allowed = _service.GetAllowedGroups(Session["empno"].ToString());
            var ret = _service.GetAllProjects().Where(p => allowed.Contains(p.group_one)).ToList();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetProjectsByGroupOne(string group_one)
        {
            if (!(_service.CanAccessGroup(Session["empno"].ToString(), group_one))) return Json(new List<Project>()); // 只能存取自己可管理群組的資料

            var ret = _service.GetProjectsByGroupOne(group_one);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateProject(Project project)
        {
            if (!(project != null && _service.CanAccessGroup(Session["empno"].ToString(), project.group_one))) return Json(false); // 只能存取自己可管理群組的資料

            var ret = _service.CreateProject(project);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateProject(Project project)
        {
            if (!(project != null && _service.CanAccessGroup(Session["empno"].ToString(), _service.FindProject(project.id)?.group_one))) return Json(false); // 只能存取自己可管理群組的資料

            var ret = _service.UpdateProject(project);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteProject(Project project)
        {
            if (!(project != null && _service.CanAccessGroup(Session["empno"].ToString(), _service.FindProject(project.id)?.group_one))) return Json(false); // 只能存取自己可管理群組的資料

            var ret = _service.DeleteProject(project);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateIssue(Issue issue)
        {
            if (!(issue != null && _service.CanAccessGroup(Session["empno"].ToString(), _service.FindProject(issue.project_id)?.group_one))) return Json(false); // 只能存取自己可管理群組的資料

            var ret = _service.CreateIssue(issue);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateIssue(Issue issue)
        {
            if (!(issue != null && _service.CanAccessGroup(Session["empno"].ToString(), _service.FindProjectOfIssue(issue.id)?.group_one))) return Json(false); // 只能存取自己可管理群組的資料

            var ret = _service.UpdateIssue(issue);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteIssue(Issue issue)
        {
            if (!(issue != null && _service.CanAccessGroup(Session["empno"].ToString(), _service.FindProjectOfIssue(issue.id)?.group_one))) return Json(false); // 只能存取自己可管理群組的資料

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
            if (!(_service.CanAccessGroup(Session["empno"].ToString(), group_one))) return Json(new List<CustomCategory>()); // 只能存取自己可管理群組的資料

            var ret = _service.GetCategoriesByGroupOne(group_one);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateCategory(CustomCategory category)
        {
            if (!(category != null && _service.CanAccessGroup(Session["empno"].ToString(), category.group_one))) return Json(false); // 只能存取自己可管理群組的資料

            var ret = _service.CreateCategory(category);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteCategory(CustomCategory category)
        {
            if (category == null || !_service.CanAccessGroup(Session["empno"].ToString(), category.group_one) || !_service.GetCategoriesByGroupOne(category.group_one).Any(x => x.id == category.id && !x.isFromProfession)) return Json(false); // 只能刪除自己可管理群組的自訂類別

            var ret = _service.DeleteCategory(category);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult AlterTableAddProjectFinishedColumn()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.AlterTableAddProjectFinishedColumn();
            return Json(ret);
        }
    }
}