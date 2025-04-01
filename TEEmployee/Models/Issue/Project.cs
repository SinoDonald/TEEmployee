using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Issue
{
    public class Project
    {
        public int id { get; set; }
        public string group_one { get; set; }
        public string name { get; set; }
        public int project_type { get; set; }
        public List<Issue> issues { get; set; }

        public Project()
        {
            issues = new List<Issue>();
        }
    }
}