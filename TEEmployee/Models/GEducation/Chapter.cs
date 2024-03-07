using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    public class Chapter
    {
        public int id { get; set; }
        public int course_id { get; set; }
        public string chapter_type { get; set; }
        public string chapter_title { get; set; }
        public string chapter_code { get; set; }
        public string chapter_scope { get; set; }
        public string duration { get; set; }
        public string createdTime { get; set; }
        public bool digitalized { get; set; }
    }
}