using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models.Promotion;

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
            var ret = _service.GetAll(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetByUser()
        {
            var ret = _service.GetByUser(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public JsonResult Update(Promotion promotion)
        {
            var ret = _service.Update(promotion);

            return Json(ret);
        }

        // Upload file from client side to server 
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file, FormCollection form)
        {            
            string jsonString = form.GetValue("promotion").AttemptedValue;
            Promotion promotion = JsonConvert.DeserializeObject<Promotion>(jsonString);
            var ret = _service.UploadFile(file, promotion);

            return Json(ret);
        }

        // Download file from server side to client 
        [HttpPost]
        public FileContentResult DownloadFile(Promotion promotion)
        {
            var fileBytes = _service.DownloadFile(promotion);
            string contentType = "application/octet-stream"; // byte 
            return File(fileBytes, contentType, promotion.filepath);
        }

        [HttpPost]
        public ContentResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());
            return Content(ret, "application/json");
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

    }
}