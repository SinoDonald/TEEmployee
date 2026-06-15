using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Wish
{    
    public class Wish
    {
        public int id { get; set; }                          // 編號
        public string empno { get; set; }                // 申請人
        public string category { get; set; }                 // 需求類別
        public WishStatus status { get; set; }               // 處理狀態
        public DateTime applicationDate { get; set; }        // 申請日期
        public string purpose { get; set; }              // 需求目的
        public string detail { get; set; }              // 需求描述
        public string filepath { get; set; }
    }

    public enum WishStatus
    {
        Pending,        // 未處理
        Processing,     // 處理中
        Closed          // 已結案
    }
}