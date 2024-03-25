using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    /// <summary>
    /// 代表一項員工的工作內容實體。
    /// </summary>
    public class ProjectTask
    {
        /// <summary>
        /// 工作內容的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }       
        /// <summary>
        /// 年月份
        /// </summary>
        public string yymm { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string projno { get; set; }
        /// <summary>
        /// 工作內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 預計完成日期
        /// </summary>
        public string endDate { get; set; }
        /// <summary>
        /// 說明
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 實際時數
        /// </summary>
        public int realHour { get; set; }
    }
}