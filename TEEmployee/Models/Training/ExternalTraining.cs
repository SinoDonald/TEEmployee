using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Training
{
    public class ExternalTraining
    {        
        public int id { get; set; }
        public string group_name { get; set; }        
        public int roc_year { get; set; }
        public string training_type { get; set; }
        public string title { get; set; }
        public string organization { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public float duration { get; set; }
        public string members { get; set; }
        public string filepath { get; set; }
    }
}