using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.EducationV2
{
    public class Record
    {
        public int id { get; set; }
        public int year { get; set; }
        public string empno { get; set; }
        public string deptno { get; set; }
        public string mediaId { get; set; }
        public string mediaTitle { get; set; }
        public string trainingDate { get; set; }
        public double score { get; set; }
        public string mark { get; set; }
        public string mediaLength { get; set; }
    }
}
