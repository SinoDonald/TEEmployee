using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GKpi
{
    public class KpiItem
    {
        public int id { get; set; }
        public int kpi_id { get; set; }
        public string content { get; set; }
        public string target { get; set; }
        public double weight { get; set; }
        public bool h1_employee_check { get; set; }
        //public bool h1_manager_check { get; set; }
        public string h1_reason { get; set; }
        public string h1_feedback { get; set; }
        public bool h2_employee_check { get; set; }
        //public bool h2_manager_check { get; set; }
        public string h2_reason { get; set; }
        public string h2_feedback { get; set; }
        public bool consensual { get; set; }

        public KpiItem ShallowCopy()
        {
            return (KpiItem)this.MemberwiseClone();
        }

    }
}