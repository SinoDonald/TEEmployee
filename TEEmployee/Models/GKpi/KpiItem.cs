using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GKpi
{
    /// <summary>
    /// 代表一項KPI細項，包含基本資訊及所屬KPI。
    /// </summary>
    public class KpiItem
    {
        /// <summary>
        /// KPI細項的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 所屬的KpiModel ID
        /// </summary>
        public int kpi_id { get; set; }
        /// <summary>
        /// KPI項目內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// KPI項目目標
        /// </summary>
        public string target { get; set; }
        /// <summary>
        /// KPI項目分數
        /// </summary>
        public double weight { get; set; }
        /// <summary>
        /// 上半年KPI項目員工自我檢核
        /// </summary>
        public bool h1_employee_check { get; set; }
        /// <summary>
        /// 上半年KPI項目員工說明
        /// </summary>
        public string h1_reason { get; set; }
        /// <summary>
        /// 上半年KPI項目主管回饋
        /// </summary>
        public string h1_feedback { get; set; }
        /// <summary>
        /// 下半年KPI項目員工自我檢核
        /// </summary>
        public bool h2_employee_check { get; set; }
        /// <summary>
        /// 下半年KPI項目員工說明
        /// </summary>
        public string h2_reason { get; set; }
        /// <summary>
        /// 下半年KPI項目主管回饋
        /// </summary>
        public string h2_feedback { get; set; }
        /// <summary>
        /// 整年KPI項目雙方溝通達成
        /// </summary>
        public bool consensual { get; set; }

        public KpiItem ShallowCopy()
        {
            return (KpiItem)this.MemberwiseClone();
        }

    }
}