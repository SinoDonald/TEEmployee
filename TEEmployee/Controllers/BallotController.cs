using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Ballot;
using TEEmployee.Models;
using TEEmployee.Filters;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class BallotController : Controller
    {
        private BallotService _service;

        public BallotController()
        {
            _service = new BallotService();
        }

        // GET: Ballot
        public ActionResult Index()
        {
            return View();
        }

        /* =======================================
                        Web api
          ====================================== */

        [HttpPost]
        public JsonResult GetEvents()
        {
            var ret = _service.GetEvents();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAllEmployeeCandidates()
        {
            var ret = _service.GetAllEmployeeCandidates();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetBallotByUserAndEvent(string event_name)
        {
            var ret = _service.GetBallotByUserAndEvent(Session["empno"].ToString(), event_name);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult CreateBallot(Ballot ballot)
        {
            if (ballot == null) return Json(new { success = false, message = "資料錯誤" });

            ballot.empno = Session["empno"].ToString();
            string error = _service.CreateBallot(ballot);
            return Json(new { success = error == null, message = error });
        }

        [HttpPost]
        public JsonResult DeleteAll()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.DeleteAll();
            return Json(ret);
        }

        [HttpPost]
        public FileContentResult DownloadEmployeeVoteExcel(string event_name)
        {
            if (!Permissions.CanDownloadBallot(Session)) return null; // 限 Admin 與部門主管

            var ev = BallotEvents.Find(event_name) ?? BallotEvents.Default(DateTime.Now);
            if (ev == null) return null;

            string contentType = "application/octet-stream"; // byte 
            var fileName = $"{ev.name}.xlsx";
            var excelData = _service.DownloadEmployeeVoteExcel(ev.code);

            return File(excelData, contentType, fileName);
        }
    }
}