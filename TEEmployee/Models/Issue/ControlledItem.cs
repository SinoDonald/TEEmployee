using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Issue
{
    public class ControlledItem
    {
        public int id { get; set; }
        public int issue_id { get; set; }
        public string members { get; set; }
        public string todo { get; set; }
        public string deadline { get; set; }
    }
}