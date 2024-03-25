using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    /// <summary>
    /// 代表一筆修課紀錄
    /// </summary>
    public class Record
    {
        /// <summary>
        /// 修課的課程單元
        /// </summary>
        public Chapter chapter { get; set; }
        /// <summary>
        /// 修課員工
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 是否已完成修課
        /// </summary>
        public bool completed { get; set; }
        /// <summary>
        /// 是否為被指派的課程
        /// </summary>
        public bool assigned { get; set; }
    }
}