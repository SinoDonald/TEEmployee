using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Issue
{
    public class Issue
    {
        public int id { get; set; }
        public int project_id { get; set; }        
        public int custom_order { get; set; }
        public string category { get; set; }
        public string content { get; set; }
        public string members { get; set; }
        public string progress { get; set; }
        public string status { get; set; }
        public string registered_date { get; set; }

        public List<ControlledItem> controlledItems { get; set; }

        public Issue()
        {
            controlledItems = new List<ControlledItem>();
        }
    }
}