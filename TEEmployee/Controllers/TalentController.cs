using System.Collections.Generic;
using System.Linq;
using System;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;
using TEEmployee.Models.Talent;
using System.IO;
using TEEmployee.Models.Profession;

namespace TEEmployee.Controllers
{
    public class TalentController : Controller
    {
        private TalentService _service;

        public TalentController()
        {
            _service = new TalentService();
        }

        // GET: Talent
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Talent()
        {
            return View();
        }
        public ActionResult TalentOption()
        {
            return PartialView();
        }
        public ActionResult TalentRecord()
        {
            return PartialView();
        }

        // -----------------------------------------
        // Web API
        // -----------------------------------------

        // 比對上傳的檔案更新時間
        [HttpPost]
        public JsonResult CompareLastestUpdate(List<string> filesInfo)
        {
            var ret = _service.CompareLastestUpdate(filesInfo);
            return Json(ret);
        }
        // 上傳員工履歷表多檔, 並解析Word後存到SQL
        public ActionResult Uploaded(HttpPostedFileBase[] files)
        {
            _service.Uploaded(files);
            return RedirectToAction("Index", "Home");
        }
        // 上傳年度績效檔案
        [HttpPost]
        public ActionResult ImportFile(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });
            var ret = _service.ImportFile(importFile);
            return Json(ret);
        }
        // 上傳測評資料檔案
        [HttpPost]
        public ActionResult ImportPDFFile(HttpPostedFileBase importPDFFile)
        {
            if (importPDFFile == null) return Json(new { Status = 0, Message = "No File Selected" });
            var ret = _service.ImportPDFFile(importPDFFile);
            return Json(ret);
        }
        // High Performer
        [HttpPost]
        public JsonResult HighPerformer(List<Skill> getAllScores)
        {
            var ret = _service.HighPerformer(getAllScores);
            return Json(ret);
        }
        // 取得群組
        [HttpPost]
        public JsonResult GetGroupList()
        {
            var ret = _service.GetGroupList(Session["empno"].ToString());
            return Json(ret);
        }
        // 取得所有員工履歷
        [HttpPost]
        public JsonResult GetAll()
        {
            var ret = _service.GetAll(Session["empno"].ToString());
            return Json(ret);
        }
        // 取得員工履歷
        [HttpPost]
        public JsonResult Get(string empno)
        {
            if(Session["empno"].ToString() != "4125")
            {
                empno = Session["empno"].ToString();
            }
            var ret = _service.GetAll(empno);
            return Json(ret);
        }
        // 儲存回覆
        [HttpPost]
        public JsonResult SaveResponse(CV userCV, string planning)
        {
            var ret = false;
            //if (Session["empno"].ToString() == "4125")
            //{
                _service.SaveResponse(userCV, planning);
                ret = true;
            //}
            
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

        
    }
}