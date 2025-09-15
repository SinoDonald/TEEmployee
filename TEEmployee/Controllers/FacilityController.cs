using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.Facility;
using TEEmployee.Models.Talent;

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

        /// <summary>
        /// 取得當前使用者員編
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CurrentUser()
        {
            User ret = new UserRepository().Get(Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 取得所有公用裝置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDevices()
        {
            var ret = _facilityService.GetDevices().ToList();
            return Json(ret);
        }
        /// <summary>
        /// 取得裝置行事曆
        /// </summary>
        /// <param name="deviceID"></param>
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
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(int id)
        {
            bool ret = _facilityService.Delete(id);
            return Json(ret);
        }
        /// <summary>
        /// 修改與新增
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reserve"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Send(string state, Facility reserve)
        {
            string ret = _facilityService.Send(state, reserve);
            return Json(ret);
        }

        [HttpPost]
        public async Task<ActionResult> GetSensorResourceData()
        {
            var ret = await _presenceSensorService.GetSensorResourceData();
            return Content(ret, "application/json");
        }
    }
}