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

    /// <summary>票選活動</summary>
    public class BallotEvent
    {
        public string code { get; set; }          // 存在 Ballot.event_name 的代碼，例：2026emp
        public string name { get; set; }          // 顯示名稱
        public DateTime start { get; set; }       // 開始投票時間 (含)
        public DateTime end { get; set; }         // 截止時間 (含)
        public int choiceCount { get; set; } = 6; // 每人須選幾位
        public string periodText { get; set; }    // 說明中顯示的投票時間文字 (只顯示用，不影響 start / end 的檢查)；空白時依 start / end 自動產生

        public bool IsOpen(DateTime now)
        {
            return now >= start && now <= end;
        }
    }

    /// <summary>
    /// 票選活動清單。新增年度時在這裡加一行，重新編譯後更新 DLL 即可。
    /// 頁面預設顯示「投票中」的活動；沒有投票中的活動時顯示最新一個。
    /// </summary>
    public static class BallotEvents
    {
        public static readonly List<BallotEvent> All = new List<BallotEvent>
        {
            new BallotEvent { code = "2025emp", name = "2025 績優員工票選", start = new DateTime(2025, 11, 5, 9, 0, 0), end = new DateTime(2025, 11, 7, 17, 0, 0), periodText = "投票時間於11/5日(三)上午9:00開始投票，11/7日(五)下午5:00截止" },
            new BallotEvent { code = "2026emp", name = "2026 績優員工票選", start = new DateTime(2026, 11, 19, 9, 0, 0), end = new DateTime(2026, 12, 31, 23, 59, 59), periodText = "投票時間於11/19日(四)上午9:00開始投票，11/23日(一)下午5:00截止" },
        };

        public static BallotEvent Find(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return All.FirstOrDefault(x => x.code == code.Trim());
        }

        /// <summary>預設活動：投票中的 (多個時取最晚開始的)，否則取最新的</summary>
        public static BallotEvent Default(DateTime now)
        {
            var open = All.Where(x => x.IsOpen(now)).OrderByDescending(x => x.start).FirstOrDefault();
            return open ?? All.OrderByDescending(x => x.start).FirstOrDefault();
        }
    }
}
