using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TEEmployee.Models.Forum
{
    /// <summary>
    /// 代表一則回文實體
    /// </summary>
    public class Reply
    {
        /// <summary>
        /// 回文的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 回文所屬的貼文Id
        /// </summary>
        public int postId { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 回文內容
        /// </summary>
        [AllowHtml]   // 允許輸入 < > 等字元 (前端以 ng-bind 顯示，不會執行)
        public string replyContent { get; set; }
        /// <summary>
        /// 回文日期
        /// </summary>
        public string replyDate { get; set; }
        public bool anonymous { get; set; }
        /// <summary>
        /// 回文附加的圖片
        /// </summary>
        public List<PostImage> images { get; set; }
    }
}