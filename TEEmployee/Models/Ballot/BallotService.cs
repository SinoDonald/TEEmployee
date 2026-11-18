using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.IssueV2;

namespace TEEmployee.Models.Ballot
{
    public class BallotService
    {
        private IBallotRepository _ballotRepository;
        private IUserRepository _userRepository;

        public BallotService()
        {
            _ballotRepository = new BallotRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>活動清單 (給前端下拉選單)，日期轉成字串避免 /Date()/ 格式</summary>
        public object GetEvents()
        {
            var now = DateTime.Now;
            var def = BallotEvents.Default(now);

            var events = BallotEvents.All
                .OrderByDescending(x => x.start)
                .Select(x => new
                {
                    x.code,
                    x.name,
                    x.choiceCount,
                    period = $"{FormatTime(x.start)} 至 {FormatTime(x.end)}",
                    periodText = string.IsNullOrWhiteSpace(x.periodText) ? $"投票時間：{FormatTime(x.start)} 至 {FormatTime(x.end)}" : x.periodText,
                    status = now < x.start ? "notStarted" : (now > x.end ? "closed" : "open"),
                })
                .ToList();

            return new { events, defaultCode = def?.code };
        }

        private static string FormatTime(DateTime t)
        {
            string[] week = { "日", "一", "二", "三", "四", "五", "六" };
            return $"{t:yyyy/MM/dd}({week[(int)t.DayOfWeek]}) {t:HH:mm}";
        }

        public List<User> GetAllEmployeeCandidates()
        {
            var users = _userRepository.GetAll();
            users.RemoveAll(x => x.department_manager || x.group_manager || x.empno == "6843");
            return users;
        }

        public Ballot GetBallotByUserAndEvent(string empno, string event_name)
        {
            var ret = _ballotRepository.GetBallotByUserAndEvent(empno, event_name);
            return ret;
        }

        /// <summary>投票：檢查活動存在、在投票期間、尚未投過、選滿指定人數且都是候選人</summary>
        public string CreateBallot(Ballot ballot)
        {
            var ev = BallotEvents.Find(ballot.event_name);
            if (ev == null) return "找不到此票選活動";

            var now = DateTime.Now;
            if (now < ev.start) return "投票尚未開始";
            if (now > ev.end) return "投票已截止";

            if (_ballotRepository.GetBallotByUserAndEvent(ballot.empno, ev.code) != null) return "你已經投過票了";

            var choices = (ballot.choices ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct()
                .ToList();
            if (choices.Count != ev.choiceCount) return $"請選擇 {ev.choiceCount} 位不同的候選人";

            var candidates = new HashSet<string>(GetAllEmployeeCandidates().Select(x => x.empno));
            if (choices.Any(x => !candidates.Contains(x))) return "選票中有不在候選名單的人員";

            ballot.event_name = ev.code;
            ballot.choices = string.Join(";", choices);

            return _ballotRepository.InsertBallot(ballot) ? null : "投票失敗，請稍後再試";
        }

        public bool DeleteAll()
        {
            var ret = _ballotRepository.DeleteAll();
            return ret;
        }

        public byte[] DownloadEmployeeVoteExcel(string event_name)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var users = _userRepository.GetAll();
            var nameDict = users.ToDictionary(x => x.empno, x => x.name);
            users.RemoveAll(x => x.empno == "6843");

            var voters_list = users.Select(x => x.empno).ToList();
            users.RemoveAll(x => x.department_manager || x.group_manager);
            var candidates = users.Select(x => x.empno).ToList();

            var ballots = _ballotRepository.GetBallotsByEvent(event_name);
            var votes = candidates.ToDictionary(x => x, x => 0);

            foreach (var ballot in ballots)
            {
                string[] choices = (ballot.choices ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < choices.Length; i++) {
                    // 被投票者若已不在候選名單 (離職、升任主管…) 仍列出，不讓統計出錯
                    if (!votes.ContainsKey(choices[i])) votes[choices[i]] = 0;
                    votes[choices[i]]++;
                }

                voters_list.Remove(ballot.empno);
            }

            using (var package = new ExcelPackage())
            {
                var ws1 = package.Workbook.Worksheets.Add("票選結果");
                var ws2 = package.Workbook.Worksheets.Add("未投名單");

                // votes
                int row = 1;
                foreach (var v in votes)
                {
                    ws1.Cells[row, 1].Value = v.Key;
                    ws1.Cells[row, 2].Value = nameDict.TryGetValue(v.Key, out var n1) ? n1 : "";
                    ws1.Cells[row, 3].Value = v.Value;
                    row++;
                }

                // Non voter list
                for (int i = 0; i < voters_list.Count; i++)
                {
                    ws2.Cells[i + 1, 1].Value = voters_list[i];
                    ws2.Cells[i + 1, 2].Value = nameDict.TryGetValue(voters_list[i], out var n2) ? n2 : "";
                }

                var excelData = package.GetAsByteArray();  // byte or stream


                return excelData;
            }

        }

        public void Dispose()
        {
            _ballotRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}
