using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.GKpi;

namespace TEEmployee.Controllers
{
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
            return View();
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
        public ActionResult UploadKpiFile(HttpPostedFileBase kpifiles)
        {
            var ret = _service.UploadKpiFile(kpifiles.InputStream);

            //var jj = Json(ret);
            var jj = Json((ret is null) ? "succeed" : "fail");

            return jj;
        }

        [HttpPost]
        public ActionResult DeleteKpiModels(string year)
        {
            var ret = _service.DeleteKpiModels(year);
            return Json(ret);
        }

        [HttpPost]
        public ActionResult DeleteSolitaryKpiModels()
        {
            var ret = _service.DeleteSolitaryKpiModels();
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}