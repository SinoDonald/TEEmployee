using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GSchedule
{
    public class Schedule : IComparable<Schedule>
    {
        public int id { get; set; }        
        public string empno { get; set; }
        public string role { get; set; }
        public string member { get; set; }
        public int type { get; set; } // 1: Group, 2: Detail, 3:Personal, 4: Future
        public string content { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public int percent_complete { get; set; }
        public int last_percent_complete { get; set; }
        public int parent_id { get; set; }
        public string history { get; set; }
        public string projno { get; set; }
        public List<Milestone> milestones { get; set; }

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

            if (engIdxA == -1) return 1;
            if (engIdxB == -1) return -1;

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

    public class Milestone
    {
        public int id { get; set; }        
        public string content { get; set; }
        public string date { get; set; }        
        public int schedule_id { get; set; }

    }
}