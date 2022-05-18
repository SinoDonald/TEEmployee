using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class CategorySelfAssessmentChart
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<SelfAssessmentChart> Charts { get; set; }
    }
}