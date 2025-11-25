using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Models;
using TEEmployee.Models.Forum;
using TEEmployee.Models.Notify;

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
            UpdateDatabase("1"); // 我要抱抱通知

            return Json(ret);
        }

        [HttpPost]
        public JsonResult InsertReply(Reply reply)
        {
            var ret = _service.InsertReply(reply, Session["empno"].ToString());
            UpdateDatabase("1"); // 我要抱抱通知

            return Json(ret);
        }

        /// <summary>
        /// 我要抱抱通知
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDatabase(string notification)
        {
            bool ret = false;

            NotifyRepository _notifyRepository = new NotifyRepository();
            if (notification.Equals("0")) { ret = _notifyRepository.UpdateDatabase(Session["empno"].ToString(), 8, notification); }
            else
            {
                List<User> users = _notifyRepository.GetAll();
                foreach(User user in users) { ret = _notifyRepository.UpdateDatabase(user.empno, 8, notification); }
            }
            
            return Json(ret);
        }

        [HttpPost]
        public JsonResult DeletePost(Post post)
        {
            var ret = _service.DeletePost(post);
            return Json(ret);
        }
        /// <summary>
        /// 刪除回覆
        /// </summary>
        /// <param name="post"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteReply(int postId, int id)
        {
            var ret = _service.DeleteReply(postId, id);
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