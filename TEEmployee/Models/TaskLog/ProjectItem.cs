using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace TEEmployee.Models.TaskLog
{
    public class ProjectItem
    {        
        public string empno { get; set; }
        [ScriptIgnore]
        public string depno { get; set; }
        public string yymm { get; set; }
        public string projno { get; set; }
        public string itemno { get; set; }
        public int workHour { get; set; }
        public int overtime { get; set; }
    }
}