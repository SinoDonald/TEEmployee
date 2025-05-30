﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public JsonResult GetByUser(string empno)
        {
            if (string.IsNullOrEmpty(empno))
                empno = Session["empno"].ToString();

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

        [HttpPost]
        public JsonResult DeleteAll()
        {
            var ret = _service.DeleteAll(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public FileContentResult DownloadAuthExcel(string authStr)
        {            
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