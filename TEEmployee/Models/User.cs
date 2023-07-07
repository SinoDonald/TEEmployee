using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

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
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group { get; set; }
        public bool group_manager { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group_one { get; set; }
        public bool group_one_manager { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group_two { get; set; }
        public bool group_two_manager { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group_three { get; set; }
        public bool group_three_manager { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string projects { get; set; }
        public bool project_manager { get; set; }
        public bool assistant_project_manager { get; set; }

        [ScriptIgnore]
        public string gid { get; set; }
        [ScriptIgnore]
        public string profTitle { get; set; }
        [ScriptIgnore]
        public string duty { get; set; }
        [ScriptIgnore]
        public string tel { get; set; }
        [ScriptIgnore]
        public string email { get; set; }
        
    }
    // 首頁通知
    public class UserNotify
    {
        public string empno { get; set; }
        public string date { get; set; }
        public int self { get; set; }
        public int manager_suggest { get; set; }
        public int freeback { get; set; }
        public int future { get; set; }
    }
}