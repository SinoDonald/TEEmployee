using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GSchedule
{
    /// <summary>
    /// 代表一項計畫行事曆項目實體。
    /// </summary>
    public class ProjectSchedule
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string projno  { get; set; }
        /// <summary>
        /// 附加檔案位置
        /// </summary>
        public string filepath { get; set; }
    }
    /// <summary>
    /// 主管回饋
    /// </summary>
    /// <param name="selectedGroup"></param>
    /// <returns></returns>
    public class Planning
    {
        public string view { get; set; }
        public string group { get; set; }
        public string year { get; set; }
        public int empno { get; set; }
        public string user_name { get; set; }
        public int manager_id { get; set; }
        public string manager_name { get; set; }
        public string response { get; set; }
    }
}