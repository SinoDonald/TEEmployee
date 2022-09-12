using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    public class ProjectTask
    {
        public int id { get; set; }
        public string empno { get; set; }       
        public string yymm { get; set; }
        public string projno { get; set; }
        public string content { get; set; }
        public string endDate { get; set; }
        public string note { get; set; }
        public int realHour { get; set; }
    }
}