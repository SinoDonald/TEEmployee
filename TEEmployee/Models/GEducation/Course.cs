using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    public class Course
    {
        public int id { get; set; }
        public string course_title { get; set; }
        public string course_group { get; set; }
        public string course_group_one { get; set; }
        public List<Chapter> chapters { get; set; }

        public Course ShallowCopy()
        {
            return (Course)this.MemberwiseClone();
        }
    }
}