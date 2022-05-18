using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class SelfAssessmentChart
    {       
        public string Content { get; set; }
        public List<int> Votes { get; set; }
    }
}