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
            var user = _service.GetAuthorization(Session["empno"].ToString());

            // isHandler：可修改需求狀態 (只限 Web.config WishHandler 名單)，前端依此開放編輯
            return Json(new
            {
                empno = user?.empno,
                name = user?.name,
                isHandler = Permissions.IsWishHandler(Session),
            });
        }

        [HttpPost]
        public JsonResult UpdateWishStatus(Wish wish)
        {
            if (!Permissions.IsWishHandler(Session)) return Json(false); // 限許願池處理人 (WishHandler)
            var ret = _service.UpdateWishStatus(wish);
            return Json(ret);
        }


        [HttpPost]
        public FileContentResult DownloadFile(Wish wish)
        {
            var fileBytes = _service.DownloadFile(wish);
            if (fileBytes == null) return null;
            string contentType = "application/octet-stream"; // byte 
            return File(fileBytes, contentType, System.IO.Path.GetFileName(wish.filepath));
        }


        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}