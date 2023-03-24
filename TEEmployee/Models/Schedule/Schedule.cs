using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Schedule
{
    public class Schedule
    {
        public int id { get; set; }
        // group schedule owned by manager role (group one name), individual schedule owned by empno
        public string empno { get; set; }
        public string role { get; set; }
        public string member { get; set; }
        public int type { get; set; }
        public string content { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public int percent_complete { get; set; }
        public int last_percent_complete { get; set; }
        public int parent_id { get; set; }
        public string history { get; set; }
        public string projno { get; set; }
        public List<Milestone> milestones { get; set; }
        
    }

    public class Milestone
    {
        public int id { get; set; }        
        public string content { get; set; }
        public string date { get; set; }        
        public int schedule_id { get; set; }

    }
}