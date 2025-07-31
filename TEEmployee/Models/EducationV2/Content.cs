using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.EducationV2
{
    public class Content
    {
        public int id { get; set; }
        public string title { get; set; }
        public string duration { get; set; }
        public string createTime { get; set; }
        public string deptName { get; set; }

        // extra

        public string content_type { get; set; }
        public string content_code { get; set; }
        public string content_scope { get; set; }
        public bool digitalized { get; set; }
        public string course_title { get; set; }
        public string course_group { get; set; }
        public string course_group_one { get; set; }
    }
}