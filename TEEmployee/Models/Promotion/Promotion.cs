using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Promotion
{
    /// <summary>
    /// 代表一項員工升等規劃實體，包含基本資訊及附檔位置。
    /// </summary>
    public class Promotion
    {
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 升等條件ID
        /// </summary>
        public int condition { get; set; }
        /// <summary>
        /// 升等條件內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 是否達成升等條件
        /// </summary>
        public bool achieved { get; set; }
        /// <summary>
        /// 升等條件個人說明
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// 升等條件附加檔案位置
        /// </summary>
        public string filepath { get; set; }
    }
}