using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models.Facility;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class FacilityController : Controller
    {
        private FacilityService _facilityService;
        private PresenceSensorService _presenceSensorService;

        public FacilityController()
        {
            _facilityService = new FacilityService();
            _presenceSensorService = new PresenceSensorService();
        }

        // GET: Facility
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 部門公用電腦借用
        /// </summary>
        /// <returns></returns>
        public ActionResult Borrow()
        {
            return View();
        }

        // -----------------------------------------
        // Web API
        // -----------------------------------------

        [HttpPost]
        public async Task<ActionResult> GetSensorResourceData()
        {
            var ret = await _presenceSensorService.GetSensorResourceData();
            return Content(ret, "application/json");
        }

        /// <summary>
        /// 取得裝置行事曆
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEvents(DateTime start, DateTime end)
        {
            Facility[] ret = new Facility[] { };
            List<Facility> events = new List<Facility>();
            start = DateTime.Today.AddDays(-14);
            end = DateTime.Today.AddDays(-11);

            for (var i = 1; i <= 5; i++)
            {
                Facility facility = new Facility();
                facility.id = i;
                facility.title = "Event " + i;
                facility.start = start.ToString();
                facility.end = end.ToString();
                facility.allDay = false;
                events.Add(facility);

                start = start.AddDays(7);
                end = end.AddDays(7);
            }

            //return Json(events.ToArray(), JsonRequestBehavior.AllowGet);
            return Json(events.ToArray());
        }
    }
}