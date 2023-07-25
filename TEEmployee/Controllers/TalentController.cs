using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;
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
        public ActionResult TalentRecord()
        {
            return PartialView();
        }

        // -----------------------------------------
        // Web API
        // -----------------------------------------

        // 上傳員工履歷表多檔
        public ActionResult Uploaded(HttpPostedFileBase[] files)
        {
            if (files.Count() > 0)
            {
                try
                {
                    foreach (var uploadFile in files)
                    {
                        if (uploadFile.ContentLength > 0)
                        {
                            string savePath = Path.Combine(Server.MapPath("~/App_Data"), "Talent\\CV\\"); // 人員履歷表Word檔路徑
                            uploadFile.SaveAs(savePath + uploadFile.FileName);
                        }
                    }
                    List<User> userGroups = new UserRepository().UserGroups(); // 取得員工群組
                    new TalentRepository().SaveUserCV(userGroups); // 讀取Word人員履歷表
                }
                catch (Exception) { }
            }
            return RedirectToAction("Index", "Home");
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
        public JsonResult SaveResponse(CV userCV)
        {
            var ret = false;
            if (Session["empno"].ToString() == "4125")
            {
                _service.SaveResponse(userCV);
                ret = true;
            }
            
            return Json(ret);
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase files)
        {
            var num = HttpContext.Request.Files.Count;

            var file = HttpContext.Request.Files[0];

            //Cat ret = new Cat { Name = "Momk", Age = 66 };
            var ret = true;
            var jj = Json(ret);
            return jj;
        }

        //[HttpPost]
        //public ActionResult UploadFile(FormCollection form)
        //{
        //    var name = form.GetValue("firstName");
        //    bool res = true;
        //    return Json(res);
        //}

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

        //[HttpPost]
        //public FileContentResult DownloadMyPage(string name)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //    //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    string contentType = "application/octet-stream";


        //    using (var package = new ExcelPackage())
        //    {
        //        var sheet = package.Workbook.Worksheets.Add("Sheet 1");
        //        sheet.Cells["A1:C1"].Merge = true;
        //        sheet.Cells["A1"].Style.Font.Size = 18f;
        //        sheet.Cells["A1"].Style.Font.Bold = true;
        //        sheet.Cells["A1"].Value = name;
        //        var excelData = package.GetAsByteArray();
        //        var fileName = "MyWorkbook.xlsx";

        //        //Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

        //        return File(excelData, contentType, fileName);
        //    }
        //}
    }
}