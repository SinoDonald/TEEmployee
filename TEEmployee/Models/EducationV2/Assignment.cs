using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.EducationV2
{
    public class Assignment
    {
        public string empno {  get; set; }
        public string content_id { get; set; }
        public string assigner { get; set; } // assigner empno
        public bool assigned { get; set; }

        // Left join

        public Record record { get; set; }

        // calculated from record

        public bool completed { get; set; }
    }
}