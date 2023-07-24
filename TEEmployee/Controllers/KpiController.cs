using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Kpi;

namespace TEEmployee.Controllers
{
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

        /*........................  Web api  ...........................*/
        [HttpPost]
        public JsonResult GetAllKpiModels(int year)
        {
            var ret = _service.GetAllKpiModelsByRole(Session["empno"].ToString(), year);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertKpiModels()
        {
            var ret = _service.InsertKpiModels();

            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}