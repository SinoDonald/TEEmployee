using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    public class Record
    {
        public Course course { get; set; }
        public string empno { get; set; }
        public bool completed { get; set; }
        public bool assigned { get; set; }
    }
}