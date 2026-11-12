using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
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

        // -----------------------------------------
        // 權限
        // -----------------------------------------

        /// <summary>新增 / 移除裝置、修改 Teams 密碼：管理員、部門主管、Web.config FacilityManagers 名單</summary>
        private bool CanManageDevices()
        {
            return Permissions.IsFacilityManager(Session);
        }

        private Facility FindReservation(long id)
        {
            return _facilityService.GetDevices().FirstOrDefault(x => x.id == id);
        }

        /// <summary>預約只有建立者本人 (modifiedUser)、管理員或公用裝置管理員可修改 / 刪除</summary>
        private bool CanEditReservation(long id)
        {
            if (Permissions.IsAdmin(Session) || Permissions.InList("FacilityManagers", Session["empno"] as string)) return true;
            var row = FindReservation(id);
            return row != null && Permissions.SameEmpno(row.modifiedUser, Session["empno"] as string);
        }

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
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var valid = new List<Facility>();
            foreach(Facility facility in facilitys)
            {
                // 日期格式異常的資料略過，避免一筆錯誤讓整個行事曆無法顯示
                if (DateTime.TryParseExact(facility.meetingDate + " " + facility.startTime, "yyyy/MM/dd HH:mm", culture, System.Globalization.DateTimeStyles.None, out DateTime start) &&
                    DateTime.TryParseExact(facility.meetingDate + " " + facility.endTime, "yyyy/MM/dd HH:mm", culture, System.Globalization.DateTimeStyles.None, out DateTime end))
                {
                    facility.start = start.ToString("s");
                    facility.end = end.ToString("s");
                    valid.Add(facility);
                }
            }
            facilitys = valid;
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
            if (!CanEditReservation(id)) return Json(false); // 只有建立者本人或管理員可刪除預約
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
            if (reserve == null) return Json("資料不完整");
            string me = Session["empno"].ToString();

            if (state == "edit")
            {
                // 只有建立者本人或管理員可修改；管理員代改時保留原建立者
                var existing = FindReservation(reserve.id);
                if (existing == null || !CanEditReservation(reserve.id)) return Json("沒有修改此預約的權限");
                reserve.modifiedUser = string.IsNullOrEmpty(existing.modifiedUser) ? me : existing.modifiedUser;
            }
            else
            {
                // 新增：建立者一律是目前登入者 (不採用前端傳來的值)
                reserve.modifiedUser = me;
            }

            string ret = _facilityService.Send(state, reserve);
            return Json(ret);
        }
        /// <summary>
        /// 修改Teams密碼
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Change(string deviceID, string password)
        {
            if (!CanManageDevices()) return Json("沒有管理裝置的權限"); // 限管理員、部門主管或 FacilityManagers 名單 (與借用頁顯示管理按鈕的條件相同)

            string ret = _facilityService.Change(deviceID, password);
            return Json(ret);
        }
        /// <summary>
        /// 新增裝置
        /// </summary>
        /// <param name="facility"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CreateDevice(Facility facility)
        {
            if (!CanManageDevices()) return Json("沒有管理裝置的權限"); // 限管理員、部門主管或 FacilityManagers 名單 (與借用頁顯示管理按鈕的條件相同)

            string ret = _facilityService.CreateDevice(facility);
            return Json(ret);
        }
        /// <summary>
        /// 移除裝置
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RemoveDevice(string deviceID)
        {
            if (!CanManageDevices()) return Json("沒有管理裝置的權限"); // 限管理員、部門主管或 FacilityManagers 名單 (與借用頁顯示管理按鈕的條件相同)

            string ret = _facilityService.RemoveDevice(deviceID);
            return Json(ret);
        }

        [HttpPost]
        public async Task<ActionResult> GetSensorResourceData()
        {
            //var ret = await _presenceSensorService.GetSensorResourceData();
            var ret = await _presenceSensorService.GetSensorResourceDataFromProxy();

            return Content(ret, "application/json");
        }
    }
}