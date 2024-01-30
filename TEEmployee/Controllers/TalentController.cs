using Newtonsoft.Json;
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
        public ActionResult Talent()
        {
            return View();
        }
        public ActionResult TalentOption()
        {
            return PartialView();
        }
        public ActionResult TalentHighPerformers()
        {
            return PartialView();
        }
        public ActionResult TalentSuccessExample()
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
        // 上傳員工經歷文字檔
        public ActionResult UploadExperience(HttpPostedFileBase file)
        {
            var ret = _service.UploadExperience(file);
            return RedirectToAction("Index", "Home");
        }
        // 上傳年度績效檔案
        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });
            var ret = _service.ImportFile(importFile);
            return Json(ret);
        }
        // 上傳測評資料檔案
        [HttpPost]
        public ActionResult ImportPDFFile(HttpPostedFileBase file)
        {
            if (file == null) return Json(new { Status = 0, Message = "No File Selected" });
            var ret = _service.ImportPDFFile(file, Session["empno"].ToString());
            return Json(ret);
        }
        // High Performer
        [HttpPost]
        public JsonResult HighPerformer()
        {
            var ret = _service.HighPerformer();
            return Json(ret);
        }
        // 取得群組
        [HttpPost]
        public JsonResult GetGroupList()
        {
            var ret = _service.GetGroupList(Session["empno"].ToString());
            return Json(ret);
        }
        // 儲存選項
        [HttpPost]
        public JsonResult SaveChoice(List<Ability> users)
        {
            var ret = false;
            _service.SaveChoice(users);
            ret = true;

            return Json(ret);
        }
        // 取得所有員工履歷
        [HttpPost]
        public JsonResult GetAll()
        {
            var ret = _service.GetAll(Session["empno"].ToString());
            return Json(ret);
        }
        // 取得所有員工職等職級
        [HttpPost]
        public JsonResult GetSenioritys()
        {
            var ret = _service.GetSenioritys();
            return Json(ret);
        }
        // 條件篩選
        [HttpPost]
        public JsonResult ConditionFilter(ConditionFilter filter, string json)
        {
            List<CV> userCVs = JsonConvert.DeserializeObject<List<CV>>(json); //反序列化
            var ret = _service.ConditionFilter(filter, userCVs);
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