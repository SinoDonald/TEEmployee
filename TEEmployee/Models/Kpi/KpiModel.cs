using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Kpi
{
    public class KpiModel
    {
        public int id { get; set; }
        public string empno { get; set; }
        public string name { get; set; }
        //public string manager { get; set; }
        public int year { get; set; }
        public string kpi_type { get; set; }
        public string group_name { get; set; }
        public List<KpiItem> items { get; set; }
    }
}