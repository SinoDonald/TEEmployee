using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Ballot;

namespace TEEmployee.Controllers
{
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
            ballot.empno = Session["empno"].ToString();
            var ret = _service.CreateBallot(ballot);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeleteAll()
        {
            var ret = _service.DeleteAll();
            return Json(ret);
        }

        [HttpPost]
        public FileContentResult DownloadEmployeeVoteExcel()
        {
            string contentType = "application/octet-stream"; // byte 
            var fileName = $"績優員工票選2025.xlsx";
            var excelData = _service.DownloadEmployeeVoteExcel();

            return File(excelData, contentType, fileName);
        }
    }
}