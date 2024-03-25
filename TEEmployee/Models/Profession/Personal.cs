using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{
    /// <summary>
    /// 代表一項員工給予個人的專業評分紀錄。
    /// </summary>
    public class Personal
    {
        /// <summary>
        /// 評分紀錄所屬的專業項目Id
        /// </summary>
        public int skill_id { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 分數
        /// </summary>
        public int score { get; set; }
        /// <summary>
        /// 項目排列順序
        /// </summary>
        public int custom_order { get; set; }
        /// <summary>
        /// 評分項目說明
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// 專業內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 專業所屬，分為小組或是共同
        /// </summary>
        public string role { get; set; } // group or shared
        /// <summary>
        /// 專業類別，領域、核心、管理
        /// </summary>
        public string skill_type { get; set; } // domain, core, manage
    }
}