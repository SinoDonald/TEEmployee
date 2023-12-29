using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Training
{
    public class Record
    {        
        public string empno { get; set; }
        public int roc_year { get; set; }
        public string training_type { get; set; }
        public string training_id { get; set; }
        public string title { get; set; }
        public string organization { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public float duration { get; set; }        
    }
}