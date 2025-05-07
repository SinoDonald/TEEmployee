using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GSchedule
{
    /// <summary>
    /// 代表一項行事曆項目實體，包含基本資訊及里程碑列舉。
    /// </summary>
    public class Schedule : IComparable<Schedule>
    {
        /// <summary>
        /// 行事曆項目的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 行事曆項目所屬小組
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// 行事曆項目包含的成員
        /// </summary>
        public string member { get; set; }
        /// <summary>
        /// 行事曆項目類別：1: Group, 2: Detail, 3:Personal, 4: Future
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 行事曆項目內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 行事曆項目開始日期
        /// </summary>
        public string start_date { get; set; }
        /// <summary>
        /// 行事曆項目結束日期
        /// </summary>
        public string end_date { get; set; }
        /// <summary>
        /// 行事曆項目完成進度
        /// </summary>
        public int percent_complete { get; set; }
        /// <summary>
        /// 行事曆項目上次完成進度
        /// </summary>
        public int last_percent_complete { get; set; }
        /// <summary>
        /// 行事曆項目母項行事曆Id
        /// </summary>
        public int parent_id { get; set; }
        /// <summary>
        /// 行事曆項目每月完成進度
        /// </summary>
        public string history { get; set; }
        /// <summary>
        /// 行事曆項目計畫編號
        /// </summary>
        public string projno { get; set; }
        /// <summary>
        /// 行事曆項目里程碑列舉
        /// </summary>
        public List<Milestone> milestones { get; set; }
        public int custom_order { get; set; } = 0;

        public int CompareTo(Schedule other)
        {
            // A null value means that this object is greater.
            //if (other == null)
            //    return 1;
            
            if (this.projno == null)
            {
                if (other.projno == null)
                    return 0;
                else
                    return 1;                
            }

            if (other.projno == null)
            {
                return -1;
            }


            List<string> engOrder = new List<string> { "N", "Z", "B", "E", "C", "D" };

            int engIdxA = engOrder.IndexOf(this.projno.Substring(4));
            int engIdxB = engOrder.IndexOf(other.projno.Substring(4));

            if (engIdxA < engIdxB)
            {
                return 1;
            }
            else if (engIdxA > engIdxB)
            {
                return -1;
            }
            else
            {
                return this.projno.Substring(0, 4).CompareTo(other.projno.Substring(0, 4));
            }
           
        }
    }

    /// <summary>
    /// 代表一項里程碑實體。
    /// </summary>
    public class Milestone
    {
        /// <summary>
        /// 里程碑的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 里程碑的內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 里程碑的日期
        /// </summary>
        public string date { get; set; }
        /// <summary>
        /// 里程碑所屬的行事曆項目
        /// </summary>
        public int schedule_id { get; set; }

    }
}