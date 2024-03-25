using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GKpi
{
    /// <summary>
    /// 代表一名員工於當年度的一項KPI類型，裡面連結KPI細項。
    /// </summary>
    public class KpiModel
    {
        /// <summary>
        /// 員工、年度與KPI類型的唯一識別碼
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
        /// KPI年度
        /// </summary>
        public int year { get; set; }
        /// <summary>
        /// KPI類型，例：專業、計畫、管理
        /// </summary>
        public string kpi_type { get; set; }
        /// <summary>
        /// 員工所屬小組
        /// </summary>
        public string group_name { get; set; }
        /// <summary>
        /// 員工部門職位
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// KPI細項列舉
        /// </summary>
        public List<KpiItem> items { get; set; }
    }
}