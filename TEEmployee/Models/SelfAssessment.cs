using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class SelfAssessment
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Content { get; set; }
    }
}