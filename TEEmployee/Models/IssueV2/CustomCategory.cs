using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.IssueV2
{
    public class CustomCategory
    {
        public int id { get; set; }
        public string group_one { get; set; }
        public string name { get; set; }
        public bool isFromProfession { get; set; }
    }
}