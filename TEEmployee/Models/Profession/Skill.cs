using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{
    public class Skill
    {
        public int id { get; set; }
        public int custom_order { get; set; }
        public string content { get; set; }
        // role: employee group 
        public string role { get; set; }
        // skill_type: soft or hard
        public string skill_type { get; set; }
        // skill_time: now or future
        public string skill_time { get; set; }        
        public List<Score> scores { get; set; }
    }
}