using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEEmployee.Filters;
using TEEmployee.Models;
using TEEmployee.Models.Forum;
using TEEmployee.Models.Notify;

namespace TEEmployee.Controllers
{
    [MyAuthorize]
    public class ForumController : Controller
    {
        private ForumService _service;

        public ForumController()
        {
            _service = new ForumService();
        }

        /// <summary>
        /// 我要抱抱管理權限：系統管理員 (Web.config Admin) 或 7291
        /// 與 View 中顯示刪除按鈕的條件一致
        /// </summary>
        private bool IsForumAdmin()
        {
            return Session["Admin"] is object || (Session["empno"] as string) == "7291";
        }

        // GET: Forum
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Post(int id)
        {
            ViewBag.id = id;
            ViewBag.isForumAdmin = IsForumAdmin();
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

        /// <summary>
        /// 新增貼文 (multipart/form-data：post.* 欄位 + images 檔案)
        /// </summary>
        [HttpPost]
        public JsonResult InsertPost(Post post, IEnumerable<HttpPostedFileBase> images)
        {
            try
            {
                int id = _service.InsertPost(post, Session["empno"].ToString(), images);
                UpdateDatabase("1"); // 我要抱抱通知

                return Json(new { ok = true, id });
            }
            catch (ForumImageException ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { ok = false, message = "發文失敗，請稍後再試。" });
            }
        }

        /// <summary>
        /// 新增回文 (multipart/form-data：reply.* 欄位 + images 檔案)
        /// </summary>
        [HttpPost]
        public JsonResult InsertReply(Reply reply, IEnumerable<HttpPostedFileBase> images)
        {
            try
            {
                int id = _service.InsertReply(reply, Session["empno"].ToString(), images);
                UpdateDatabase("1"); // 我要抱抱通知

                return Json(new { ok = true, id });
            }
            catch (ForumImageException ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { ok = false, message = "回覆失敗，請稍後再試。" });
            }
        }

        /// <summary>
        /// 讀取圖片 (圖片存在 App_Data，只能透過此 action 經登入驗證後取得)
        /// GET Forum/Image/5 原圖；Forum/Image/5?thumb=true 縮圖
        /// </summary>
        [HttpGet]
        public ActionResult Image(int id, bool thumb = false)
        {
            var file = _service.GetImageFile(id, thumb);
            if (file.path == null) return HttpNotFound();

            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.Cache.SetMaxAge(TimeSpan.FromDays(7));
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return File(file.path, file.contentType);
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
            if (!IsForumAdmin()) return Json(false);

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
            if (!IsForumAdmin()) return Json(false);

            var ret = _service.DeleteReply(postId, id);
            return Json(ret);
        }

        //=============================
        // Database reset
        //=============================

        [HttpPost]
        public JsonResult DeleteAll()
        {
            if (!(Session["Admin"] is object)) return Json(false);

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
