using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Ballot
{
    public class Ballot
    {
        public int id { get; set; }
        public string empno { get; set; }
        public string event_name { get; set; }
        public string choices { get; set; }
    }
}