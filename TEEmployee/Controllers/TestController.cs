using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TEEmployee.Controllers
{
    public class TestController : Controller
    {
        public TestController()
        {

        }

        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        /*........................  Web api  ...........................*/

        // Upload file from client side to server 
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

        [HttpPost]
        public ActionResult UploadFiles(List<HttpPostedFileBase> files)
        {
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
            var name = form.GetValue("firstName");
            bool res = true;
            return Json(res);
        }


        // Create Excel file in server and pass it to client 
        [HttpPost]
        public FileContentResult DownloadMyPage(string name)
        {
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