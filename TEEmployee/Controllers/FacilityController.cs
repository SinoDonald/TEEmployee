using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using TEEmployee.Filters;
using TEEmployee.Models;
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
        /// 取得所有公用裝置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDevices()
        {
            var ret = _facilityService.GetDevices().Select(x => x.deviceID).Distinct().ToList();
            return Json(ret);
        }
        /// <summary>
        /// 取得裝置行事曆
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEvents(string deviceID)
        {
            List<Facility> facilitys = _facilityService.GetDevices().Where(x => x.deviceID.Equals(deviceID)).ToList();
            foreach(Facility facility in facilitys)
            {
                facility.start = DateTime.ParseExact(facility.meetingDate + " " + facility.startTime, "yyyy/MM/dd HH:mm", System.Globalization.CultureInfo.InvariantCulture).ToString("s");
                facility.end = DateTime.ParseExact(facility.meetingDate + " " + facility.endTime, "yyyy/MM/dd HH:mm", System.Globalization.CultureInfo.InvariantCulture).ToString("s");
            }
            return Json(facilitys, JsonRequestBehavior.AllowGet);
        }
    }
}