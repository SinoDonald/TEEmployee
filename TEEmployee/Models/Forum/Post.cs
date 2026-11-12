using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TEEmployee.Models.Forum
{
    /// <summary>
    /// 代表一則貼文實體
    /// </summary>
    public class Post
    {
        /// <summary>
        /// 貼文的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 貼文標題
        /// </summary>
        [AllowHtml]   // 允許輸入 < > 等字元 (前端以 ng-bind 顯示，不會執行)
        public string title { get; set; }
        /// <summary>
        /// 貼文內容
        /// </summary>
        [AllowHtml]   // 允許輸入 < > 等字元 (前端以 ng-bind 顯示，不會執行)
        public string content { get; set; }
        /// <summary>
        /// 貼文日期
        /// </summary>
        public string postDate { get; set; }
        /// <summary>
        /// 回文數量
        /// </summary>
        public int count { get; set; }
        public bool anonymous { get; set; }
        public List<Reply> replies { get; set; }
        public string latestName { get; set; }
        public string latestDate { get; set; }
        /// <summary>
        /// 貼文本身附加的圖片
        /// </summary>
        public List<PostImage> images { get; set; }
        /// <summary>
        /// 貼文本身附加的圖片數量 (列表頁顯示圖示用)
        /// </summary>
        public int imageCount { get; set; }
    }

    /// <summary>
    /// 貼文或回文附加的圖片。
    /// 回傳前端前會清空 fileName / thumbName / empno，避免洩漏實體路徑與上傳者 (匿名)。
    /// </summary>
    public class PostImage
    {
        public int id { get; set; }
        /// <summary>所屬貼文Id</summary>
        public int postId { get; set; }
        /// <summary>所屬回文Id；null 表示附加在貼文本身</summary>
        public int? replyId { get; set; }
        /// <summary>原圖相對路徑 (相對於 App_Data/ForumImages)</summary>
        public string fileName { get; set; }
        /// <summary>縮圖相對路徑</summary>
        public string thumbName { get; set; }
        public string contentType { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public long size { get; set; }
        /// <summary>上傳者 (僅供管理，不輸出到前端)</summary>
        public string empno { get; set; }
        public string createdAt { get; set; }
    }
}
