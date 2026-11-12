using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models.Promotion;
using TEEmployee.Models;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class PromotionController : Controller
    {
        private PromotionService _service;

        public PromotionController()
        {
            _service = new PromotionService();
        }

        // GET: Promotion
        public ActionResult Index()
        {
            return View();
        }

        /*........................  Web api  ...........................*/

        [HttpPost]
        public JsonResult GetAll()
        {
            if (!Permissions.IsAdmin(Session)) return Json(new List<object>()); // 限系統管理員

            // PromotionRepository.GetAll 尚未實作 (會丟 NotImplementedException)，前端也沒有使用，先回傳空清單
            return Json(new List<object>());

            var ret = _service.GetAll(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetByUser(string empno)
        {
            if (string.IsNullOrEmpty(empno))
                empno = Session["empno"].ToString();

            // 只能看本人，或依角色可管理的員工 (部門主管：全部；組長：同部門)
            if (!_service.CanAccess(Session["empno"].ToString(), empno))
                return Json(new List<Promotion>());

            // Hide sensitive data for Admin and Group Manager
            bool isSensitive = false;

            if (Session["empno"].ToString() != Session["original_empno"].ToString())
            {
                isSensitive = true;
            }

            if (Session["group_leader"] is true && Session["empno"].ToString() != empno)
            {
                isSensitive = true;
            }

            var ret = _service.GetByUser(empno, isSensitive);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult Update(Promotion promotion)
        {
            if (promotion == null || !_service.CanAccess(Session["empno"].ToString(), promotion.empno)) return Json(false); // 只能修改本人或所管理員工

            var ret = _service.Update(promotion);

            return Json(ret);
        }

        // Upload file from client side to server 
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file, FormCollection form)
        {            
            string jsonString = form.GetValue("promotion").AttemptedValue;
            Promotion promotion = JsonConvert.DeserializeObject<Promotion>(jsonString);
            if (promotion == null || !_service.CanAccess(Session["empno"].ToString(), promotion.empno))
                return Json(false); // 只能上傳本人或所管理員工的附件
            var ret = _service.UploadFile(file, promotion);

            return Json(ret);
        }

        // Download file from server side to client 
        [HttpPost]
        public FileContentResult DownloadFile(Promotion promotion)
        {
            if (promotion == null || !_service.CanAccess(Session["empno"].ToString(), promotion.empno))
                return null; // 只能下載本人或所管理員工的附件

            var fileBytes = _service.DownloadFile(promotion);
            if (fileBytes == null) return null;
            string contentType = "application/octet-stream"; // byte 
            return File(fileBytes, contentType, System.IO.Path.GetFileName(promotion.filepath ?? "download"));
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        [HttpPost]
        public JsonResult DeleteAll()
        {
            if (!Permissions.IsAdmin(Session)) return Json(false); // 限系統管理員

            var ret = _service.DeleteAll(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public FileContentResult DownloadAuthExcel(string authStr)
        {
            if (!(Permissions.IsAdmin(Session) || Permissions.IsDepartmentManager(Session) || Session["group_leader"] is true)) return null; // 限主管或管理員
            
            string contentType = "application/octet-stream"; // byte 
            var fileName = $"{DateTime.Now.Year}升等名單.xlsx";
            var excelData = _service.DownloadAuthExcel(authStr);
            
            return File(excelData, contentType, fileName);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

    }
}