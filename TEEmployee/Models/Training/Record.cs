using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Training
{
    /// <summary>
    /// 代表一項培訓紀錄實體，包含培訓基本資訊及修課員工。
    /// </summary>
    public class Record
    {        
        /// <summary>
        /// 員工編號
        /// </summary>
        public string empno { get; set; }
        /// <summary>
        /// 培訓課程民國年分
        /// </summary>
        public int roc_year { get; set; }
        /// <summary>
        /// 培訓課程種類
        /// </summary>
        public string training_type { get; set; }
        /// <summary>
        /// 培訓課程編號
        /// </summary>
        public string training_id { get; set; }
        /// <summary>
        /// 培訓課程標題
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 培訓課程提供單位
        /// </summary>
        public string organization { get; set; }
        /// <summary>
        /// 培訓課程開始日期
        /// </summary>
        public string start_date { get; set; }
        //public DateTime start_date { get; set; }
        /// <summary>
        /// 培訓課程結束日期
        /// </summary>
        public string end_date { get; set; }
        //public DateTime end_date { get; set; }
        /// <summary>
        /// 培訓課程總時數
        /// </summary>
        public float duration { get; set; }
        public int customType { get; set; }
        public bool emailSent { get; set; }
    }
}