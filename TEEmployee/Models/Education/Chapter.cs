using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Education
{
    public class Chapter
    {
        public int id { get; set; }
        public int course_id { get; set; }
        public string chapter_type { get; set; }
        public string chapter_title { get; set; }
        public string duration { get; set; }
        public string createdTime { get; set; }
    }
}