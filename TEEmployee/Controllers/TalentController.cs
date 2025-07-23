using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;
using TEEmployee.Models.Profession;
using TEEmployee.Models.Talent;

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
        /// <summary>
        /// 人才資料庫
        /// </summary>
        /// <returns></returns>
        public ActionResult Talent()
        {
            return View();
        }
        /// <summary>
        /// 選擇人員
        /// </summary>
        /// <returns></returns>
        public ActionResult TalentOption()
        {
            return PartialView();
        }
        /// <summary>
        /// HighPerformers
        /// </summary>
        /// <returns></returns>
        public ActionResult TalentHighPerformers()
        {
            return PartialView();
        }
        /// <summary>
        /// 成功典範
        /// </summary>
        /// <returns></returns>
        public ActionResult TalentSuccessExample()
        {
            return PartialView();
        }
        /// <summary>
        /// 個人資料
        /// </summary>
        /// <returns></returns>
        public ActionResult TalentRecord()
        {
            return PartialView();
        }

        // -----------------------------------------
        // Web API
        // -----------------------------------------

        /// <summary>
        /// 取得年份
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetYears()
        {
            var ret = _service.GetYears();
            return Json(ret);
        }
        /// <summary>
        /// 上傳測評資料檔案
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportPDFFile(HttpPostedFileBase file)
        {
            if (file == null) return Json(new { Status = 0, Message = "No File Selected" });
            var ret = _service.ImportPDFFile(file, Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 上傳測評資料PDF
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadPDFFile(HttpPostedFileBase file, string year)
        {
            if (file == null) return Json(new { Status = 0, Message = "No File Selected" });
            string ret = _service.UploadPDFFile(file, year, Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 取得測評資料PDF
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPDF(string year, string empno)
        {
            string pdfPath = _service.GetPDF(year, empno);
            var fileBytes = _service.DownloadFile(pdfPath);
            string contentType = "application/octet-stream"; // byte
            FileContentResult result = null;
            try
            {
                result = File(fileBytes, contentType, empno + ".pdf");
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }
        /// <summary>
        /// High Performer
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult HighPerformer()
        {
            var ret = _service.HighPerformer();
            return Json(ret);
        }
        /// <summary>
        /// 取得群組
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetGroupList()
        {
            var ret = _service.GetGroupList(Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 儲存個人紀錄選項
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveChoice(List<Ability> users)
        {
            var ret = false;
            _service.SaveChoice(users);
            ret = true;

            return Json(ret);
        }
        /// <summary>
        /// 取得所有員工履歷
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAll()
        {
            var ret = _service.GetAll(Session["empno"].ToString());
            return Json(ret);
        }
        /// <summary>
        /// 取得所有員工職等職級
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetSenioritys()
        {
            var ret = _service.GetSenioritys();
            return Json(ret);
        }
        /// <summary>
        /// 條件篩選
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConditionFilter(ConditionFilter filter, string json)
        {
            List<CV> userCVs = JsonConvert.DeserializeObject<List<CV>>(json); //反序列化
            var ret = _service.ConditionFilter(filter, userCVs);
            return Json(ret);
        }
        /// <summary>
        /// 取得員工履歷
        /// </summary>
        /// <param name="empno"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get(string empno)
        {
            if(Session["leader"] is false)
            {
                empno = Session["empno"].ToString();
            }
            var ret = _service.Get(empno);

            // 對Admin隱藏年度績效
            if (Session["empno"].ToString() != Session["original_empno"].ToString())
            {
                foreach (CV item in ret)
                {
                    item.performance = "";
                }
            }

            return Json(ret);
        }
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="userCV"></param>
        /// <param name="planning"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 刪除人才資料庫
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTalent()
        {
            var ret = _service.DeleteTalent();
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

        // **************************** 上傳Word檔或文字檔解析 **************************** //

        ///// <summary>
        ///// 上傳年度績效檔案
        ///// </summary>
        ///// <param name="importFile"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult ImportFile(HttpPostedFileBase importFile)
        //{
        //    if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });
        //    var ret = _service.ImportFile(importFile);
        //    return Json(ret);
        //}
        ///// <summary>
        ///// 上傳員工履歷表多檔, 並解析Word後存到SQL
        ///// </summary>
        ///// <param name="files"></param>
        ///// <returns></returns>
        //public ActionResult Uploaded(HttpPostedFileBase[] files)
        //{
        //    _service.Uploaded(files);
        //    return RedirectToAction("Index", "Home");
        //}
        ///// <summary>
        ///// 上傳員工經歷文字檔
        ///// </summary>
        ///// <param name="file"></param>
        ///// <returns></returns>
        //public ActionResult UploadExperience(HttpPostedFileBase file)
        //{
        //    var ret = _service.UploadExperience(file);
        //    return RedirectToAction("Index", "Home");
        //}
    }
}