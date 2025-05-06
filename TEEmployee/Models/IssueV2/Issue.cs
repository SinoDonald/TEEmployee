using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TEEmployee.Models.Issue;

namespace TEEmployee.Models.IssueV2
{
    public class Issue
    {
        public int id { get; set; }
        public int project_id { get; set; }
        public int importance { get; set; }
        public string category { get; set; }
        public string content { get; set; }
        public string members { get; set; }
        public string progress { get; set; }
        public string status { get; set; }
        public string registered_date { get; set; }
        public string finished_date { get; set; }

        public Issue()
        {
            importance = 1;
        }
    }
}