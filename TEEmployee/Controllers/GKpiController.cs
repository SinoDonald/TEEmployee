using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.GKpi;
using TEEmployee.Models;
using TEEmployee.Filters;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class GKpiController : Controller
    {
        private GKpiService _service;

        public GKpiController()
        {
            _service = new GKpiService();
        }

        // GET: Kpi
        public ActionResult Index()
        {
            // 沒有對應的 View，導向 KPI 填寫頁
            return RedirectToAction("Fillin");
        }

        public ActionResult Fillin()
        {
            return View();
        }

        public ActionResult Feedback()
        {
            return View();
        }

        /*........................  Web api  ...........................*/
        [HttpPost]
        public JsonResult GetAllKpiModels(int year)
        {
            var ret = _service.GetAllKpiModelsByRole(Session["empno"].ToString(), year);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateKpiItems(List<KpiItem> items, List<KpiItem> removedItems)
        {
            var ret = _service.UpdateKpiItems(items, removedItems, Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertKpiModels()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.InsertKpiModels();

            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAuth()
        {
            return Json(new List<string> { Session["empno"].ToString(), Session["empname"].ToString() });
        }

        // DLC

        [HttpPost]
        public JsonResult GetEmployeeKpiModels(int year)
        {
            var ret = _service.GetEmployeeKpiModelsByRole(Session["empno"].ToString(), year);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetManagerKpiModels(int year)
        {
            var ret = _service.GetManagerKpiModelsByRole(Session["empno"].ToString(), year);

            return Json(ret);
        }

        // Upload file from client side to server 
        [HttpPost]
        public ActionResult UploadKpiFile(HttpPostedFileBase kpifiles, string year)
        {
            var ret = _service.UploadKpiFile(kpifiles.InputStream, Session["empno"].ToString(), year);

            //var jj = Json(ret);
            var jj = Json((ret is null) ? "succeed" : "fail");

            return jj;
        }

        [HttpPost]
        public ActionResult DeleteKpiModels(string year)
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.DeleteKpiModels(year);
            return Json(ret);
        }

        [HttpPost]
        public ActionResult DeleteSolitaryKpiModels()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.DeleteSolitaryKpiModels();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateCustomField()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.UpdateCustomField();
            return Json(ret);
        }

        //=============================
        // Database reset
        //=============================

        [HttpPost]
        public JsonResult DeleteAll()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.DeleteAll();
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}