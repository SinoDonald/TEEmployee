using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace TEEmployee.Models
{
    public class Ability
    {
        public string empno { get; set; }
        public string name { get; set; }
        public string domainSkill { get; set; }
        public string coreSkill { get; set; }
        public string manageSkill { get; set; }
        public string position { get; set; }
        public bool choice1 { get; set; } // 專業性 – 專家的潛質
        public bool choice2 { get; set; } // 格局、視野大 - 舉一反三
        public bool choice3 { get; set; } // 責任心驅動的主動性 - 捨我其誰
        public bool choice4 { get; set; } // 建立系統性、計畫性的學習能力 - 學習力具體
        public bool choice5 { get; set; } // 適應變化的韌性 - 懂得取捨、不放棄
        public bool selectPosition { get; set; } // 開放身份選擇
    }
    public class Seniority
    {
        public string empno { get; set; }
        public string name { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public bool now { get; set; }        
        public string company { get; set; }
        public string department { get; set; }        
        public string position { get; set; }
        public string manager { get; set; }
    }
    public class StartEndDate
    {
        public string position { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string interval { get; set; }
    }
}