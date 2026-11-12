using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;
using TEEmployee.Filters;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class TestController : Controller
    {
        public TestController()
        {

        }

        // GET: Test
        public ActionResult Index()
        {
            if (!Permissions.IsAdmin(Session)) return RedirectToAction("Index", "Home"); // 限系統管理員

            return View();
        }

        /*........................  Web api  ...........................*/

        // Upload file from client side to server 
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase files)
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var num = HttpContext.Request.Files.Count;

            var file = HttpContext.Request.Files[0];

            //Cat ret = new Cat { Name = "Momk", Age = 66 };
            var ret = true;
            var jj = Json(ret);
            return jj;
        }

        [HttpPost]
        public ActionResult UploadFiles(List<HttpPostedFileBase> files)
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var num = HttpContext.Request.Files.Count;

            var file = HttpContext.Request.Files[0];


            //Cat ret = new Cat { Name = "Momk", Age = 66 };
            var ret = true;
            var jj = Json(ret);
            return jj;
        }

        // Submit form data except for file to server
        [HttpPost]
        public ActionResult SubmitForm(FormCollection form)
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var name = form.GetValue("firstName");
            bool res = true;
            return Json(res);
        }


        // Create Excel file in server and pass it to client 
        [HttpPost]
        public FileContentResult DownloadMyPage(string name)
        {
            if (!Permissions.IsAdmin(Session)) return null; // 限系統管理員

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string contentType = "application/octet-stream"; // byte 

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Sheet 1");
                sheet.Cells["A1:C1"].Merge = true;
                sheet.Cells["A1"].Style.Font.Size = 18f;
                sheet.Cells["A1"].Style.Font.Bold = true;
                sheet.Cells["A1"].Value = name;
                var excelData = package.GetAsByteArray();  // byte or stream
                var fileName = "MyWorkbook.xlsx";

                //Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

                return File(excelData, contentType, fileName);
            }
        }

        protected override void Dispose(bool disposing)
        {
            //_service.Dispose();
            base.Dispose(disposing);
        }
    }
}