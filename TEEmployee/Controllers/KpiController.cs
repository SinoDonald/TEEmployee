using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Kpi;
using TEEmployee.Models;
using TEEmployee.Filters;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class KpiController : Controller
    {
        private KpiService _service;

        public KpiController()
        {
            _service = new KpiService();
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
            return Json(Session["empno"].ToString());
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

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}