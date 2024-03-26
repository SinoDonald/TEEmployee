using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    /// <summary>
    /// 代表一項單元自評實體，包含題目資訊及作答選項
    /// </summary>
    public class Assessment
    {
        /// <summary>
        /// 題目的唯一識別碼
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 題目的分類識別碼
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 題目的內容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 作答者員工編號
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 作答選項
        /// </summary>
        public string Choice { get; set; }
        /// <summary>
        /// 主管選項
        /// </summary>
        public string ManagerChoice { get; set; }
    }
}