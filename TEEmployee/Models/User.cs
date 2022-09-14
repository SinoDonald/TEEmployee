using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class User
    {
        //public string UserId { get; set; }
        //public string UserName { get; set; }
        //public string Role { get; set; }

        public string empno { get; set; }
        public string name { get; set; }
        public string dutyName { get; set; }
        public bool department_manager { get; set; }
        public string group { get; set; }
        public bool group_manager { get; set; }
        public string group_one { get; set; }
        public bool group_one_manager { get; set; }
        public string group_two { get; set; }
        public bool group_two_manager { get; set; }
        public string group_three { get; set; }
        public bool group_three_manager { get; set; }
        public string projects { get; set; }
        public bool project_manager { get; set; }
    }
}