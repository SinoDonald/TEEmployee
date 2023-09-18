using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{
    public class Personal
    {       
        public int skill_id { get; set; }
        public string empno { get; set; }
        public int score { get; set; }
        public int custom_order { get; set; }
        public string comment { get; set; }
        public string content { get; set; }
        public string role { get; set; } // group or shared
        public string skill_type { get; set; } // domain, core, manage
    }
}