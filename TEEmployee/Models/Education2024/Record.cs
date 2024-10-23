using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Education2024
{
    public class Record
    {
        public int Id { get; set; }
        public Content content { get; set; }
        public string Empno { get; set; }
        public string FinishedTime { get; set; }
    }
}