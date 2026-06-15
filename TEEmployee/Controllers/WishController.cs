using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.Forum;
using TEEmployee.Models.Profession;
using TEEmployee.Models.Training;
using TEEmployee.Models.Wish;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class WishController : Controller
    {
        private WishService _service;

        public WishController()
        {
            _service = new WishService();
        }

        // GET: Wish
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

        //[HttpPost]
        //public JsonResult InsertWish(Wish wish)
        //{
        //    var ret = _service.InsertWish(wish, Session["empno"].ToString());

        //    return Json(ret);
        //}

        [HttpPost]
        public JsonResult InsertWish(HttpPostedFileBase file, FormCollection form)
        {
            string jsonString = form.GetValue("wish").AttemptedValue;
            Wish wish = JsonConvert.DeserializeObject<Wish>(jsonString);
            var ret = _service.InsertWish(file, wish, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetAuthorization()
        {
            var ret = _service.GetAuthorization(Session["empno"].ToString());

            return Json(ret);
        }

        [HttpPost]
        public JsonResult UpdateWishStatus(Wish wish)
        {
            var ret = _service.UpdateWishStatus(wish, Session["empno"].ToString());
            return Json(ret);
        }


        [HttpPost]
        public FileContentResult DownloadFile(Wish wish)
        {
            var fileBytes = _service.DownloadFile(wish);
            string contentType = "application/octet-stream"; // byte 
            return File(fileBytes, contentType, wish.filepath);
        }


        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}