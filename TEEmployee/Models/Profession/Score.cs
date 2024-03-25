using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{
    /// <summary>
    /// 代表一項主管給予員工的專業評分紀錄。
    /// </summary>
    public class Score
    {
        //public int id { get; set; }
        /// <summary>
        /// 評分紀錄所屬的專業項目Id
        /// </summary>
        public int skill_id { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 分數
        /// </summary>
        public int score { get; set; }
    }
}