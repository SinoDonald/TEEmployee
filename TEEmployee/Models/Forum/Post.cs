using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public string title { get; set; }
        /// <summary>
        /// 貼文內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 貼文日期
        /// </summary>
        public string postDate { get; set; }
        /// <summary>
        /// 回文數量
        /// </summary>
        public int count { get; set; }
    }
}