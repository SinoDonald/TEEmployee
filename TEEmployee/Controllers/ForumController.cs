using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models.Forum;

namespace TEEmployee.Controllers
{
    public class ForumController : Controller
    {
        private ForumService _service;

        public ForumController()
        {
            _service = new ForumService();
        }

        // GET: Forum
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Post(int id)
        {
            ViewBag.id = id;
            return View();
        }


        /* =======================================
                        Web api
          ====================================== */

        [HttpPost]
        public JsonResult GetAllPosts()
        {
            var ret = _service.GetAllPosts();
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetPost(int id)
        {
            var ret = _service.GetPost(id);
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertPost(Post post)
        {
            var ret = _service.InsertPost(post, Session["empno"].ToString());
            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertReply(Reply reply)
        {
            var ret = _service.InsertReply(reply, Session["empno"].ToString());
            return Json(ret);
        }

        //=============================
        // Database reset
        //=============================

        [HttpPost]
        public JsonResult DeleteAll()
        {
            var ret = _service.DeleteAll();
            return Json(ret);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

    }
}