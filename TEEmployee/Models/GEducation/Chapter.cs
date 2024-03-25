using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    /// <summary>
    /// 代表一項單元課程實體，包含基本資訊及所屬群組。
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// 課程的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 系列課程名稱
        /// </summary>
        public string course_title { get; set; }
        /// <summary>
        /// 課程所屬群組
        /// </summary>
        public string course_group { get; set; }
        /// <summary>
        /// 課程所屬小組
        /// </summary>
        public string course_group_one { get; set; }
        /// <summary>
        /// 課程性質，例：基礎、進階
        /// </summary>
        public string chapter_type { get; set; }
        /// <summary>
        /// 課程名稱
        /// </summary>
        public string chapter_title { get; set; }
        /// <summary>
        /// 課程部門編碼
        /// </summary>
        public string chapter_code { get; set; }
        /// <summary>
        /// 課程專業科目，例：經驗交流、預算編制
        /// </summary>
        public string chapter_scope { get; set; }
        /// <summary>
        /// 課程總時數
        /// </summary>
        public string duration { get; set; }
        /// <summary>
        /// 課程開始時間
        /// </summary>
        public string createdTime { get; set; }
        /// <summary>
        /// 是否登記為數位規劃課程
        /// </summary>
        public bool digitalized { get; set; }
    }
}