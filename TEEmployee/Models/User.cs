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
    // 首頁通知 <-- 培文
    public class UserNotify
    {
        public string empno { get; set; }
        public string date { get; set; }
        public int self { get; set; }
        public int manager_suggest { get; set; }
        public int freeback { get; set; }
        public int future { get; set; }
    }
    // 人才資料庫 <-- 培文
    public class CV
    {
        public string empno { get; set; }
        public string name { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group_one { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group_two { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string group_three { get; set; }
        public string birthday { get; set; }
        public string age { get; set; }
        public string address { get; set; }
        public string educational { get; set; }
        public string performance { get; set; }
        public string expertise { get; set; }
        public string treatise { get; set; }
        public string language { get; set; }
        public string academic { get; set; }
        public string license { get; set; }
        public string training { get; set; }
        public string honor { get; set; }
        public string experience { get; set; }
        public string project { get; set; }
        public string lastest_update { get; set; }
        public string domainSkill { get; set; } // 專業能力_領域技能
        public string coreSkill { get; set; } // 專業能力_核心技能
        public string manageSkill { get; set; } // 管理能力
        public string planning { get; set; } // 規劃進程
        public string test { get; set; } // 測評資料
        public string advantage { get; set; } // 優勢
        public string disadvantage { get; set; } // 劣勢
        public string developed { get; set; } // 待發展能力
        public string future { get; set; } // 未來發展規劃
    }
}