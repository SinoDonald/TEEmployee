using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace TEEmployee.Models.TaskLog
{
    /// <summary>
    /// 代表一項員工的旬卡紀錄實體。
    /// </summary>
    public class ProjectItem
    {        
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        [ScriptIgnore]
        public string depno { get; set; }
        /// <summary>
        /// 年月份
        /// </summary>
        public string yymm { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string projno { get; set; }
        /// <summary>
        /// 項目編號
        /// </summary>
        public string itemno { get; set; }
        /// <summary>
        /// 項目工作時數
        /// </summary>
        public int workHour { get; set; }
        /// <summary>
        /// 項目加班時數
        /// </summary>
        public int overtime { get; set; }
    }
}