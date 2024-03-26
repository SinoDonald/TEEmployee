using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Assessments
{
    /// <summary>
    /// 代表一項主管能力分群員工實體
    /// </summary>
    public class Performance
    {
        /// <summary>
        /// 能力分群分數
        /// </summary>
        public string score { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 主管員工編號
        /// </summary>
        public string manager { get; set; }
    }
}