using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Promotion
{
    public class Promotion
    {
        public string empno { get; set; }
        public int condition { get; set; }
        public string content { get; set; }
        public bool achieved { get; set; }
        public string comment { get; set; }
        public string filepath { get; set; }
    }
}