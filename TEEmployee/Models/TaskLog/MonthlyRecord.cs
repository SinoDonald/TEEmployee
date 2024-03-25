using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    /// <summary>
    /// 代表一項員工與月份的工作紀錄實體。
    /// </summary>
    public class MonthlyRecord
    {
        /// <summary>
        /// 工作紀錄唯一識別GUID
        /// </summary>
        public Guid guid { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 年月份
        /// </summary>
        public string yymm { get; set; }        
    }
}