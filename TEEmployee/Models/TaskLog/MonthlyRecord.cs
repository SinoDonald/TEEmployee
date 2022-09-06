using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    public class MonthlyRecord
    {
        public Guid guid { get; set; }
        public string empno { get; set; }        
        public string yymm { get; set; }        
    }
}