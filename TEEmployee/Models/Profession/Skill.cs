using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{

    /// <summary>
    /// 代表一項專業，包含基本資訊及分數列舉。
    /// </summary>
    public class Skill
    {
        /// <summary>
        /// 專業的唯一識別碼
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 專業的排列順序
        /// </summary>
        public int custom_order { get; set; }
        /// <summary>
        /// 專業內容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 專業所屬，分為小組或是共同
        /// </summary>
        public string role { get; set; } // group or shared
        /// <summary>
        /// 專業類別，領域、核心、管理
        /// </summary>
        public string skill_type { get; set; } // domain, core, manage
        /// <summary>
        /// 專業分數列舉
        /// </summary>
        public List<Score> scores { get; set; }
    }

    // hard / soft version

    //public class Skill
    //{
    //    public int id { get; set; }
    //    public int custom_order { get; set; }
    //    public string content { get; set; }
    //    // role: employee group 
    //    public string role { get; set; }
    //    // skill_type: soft or hard
    //    public string skill_type { get; set; }
    //    // skill_time: now or future
    //    public string skill_time { get; set; }        
    //    public List<Score> scores { get; set; }
    //}
}