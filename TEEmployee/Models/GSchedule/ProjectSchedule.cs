using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GSchedule
{
    public class ProjectSchedule
    {
        public string projno  { get; set; }
        public string filepath { get; set; }
    }
    public class Planning
    {
        public string view { get; set; }
        public string group { get; set; }
        public string year { get; set; }
        public int empno { get; set; }
        public string user_name { get; set; }
        public int manager_id { get; set; }
        public string manager_name { get; set; }
        public string response { get; set; }
    }
}