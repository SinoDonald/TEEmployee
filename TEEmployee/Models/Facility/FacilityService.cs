using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace TEEmployee.Models.Facility
{
    public class FacilityService
    {
        private IFacilityRepository _facilityRepository;

        public FacilityService()
        {
            _facilityRepository = new FacilityRepository();
        }

        public class TimeInterval
        {
            public string Start { get; set; }  // "HH:mm"
            public string End { get; set; }    // "HH:mm"
        }

        /// <summary>
        /// 取得所有公用裝置
        /// </summary>
        /// <returns></returns>
        public List<Facility> GetDevices()
        {
            List<Facility> ret = _facilityRepository.GetDevices();
            return ret;
        }
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            bool ret = _facilityRepository.Delete(id);
            return ret;
        }
        /// <summary>
        /// 修改與新增
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reserve"></param>
        /// <returns></returns>
        public string Send(string state, Facility reserve)
        {
            string ret = string.Empty;
            (bool, string) validateInterval = ValidateInterval(reserve.startTime, reserve.endTime, false);
            // 驗證開始與結束時間格式
            if(validateInterval.Item1 == false)
            {
                ret = validateInterval.Item2;
            }
            // 借用名稱不得為空值
            else if (String.IsNullOrEmpty(reserve.title))
            {
                ret = "未輸入名稱";
            }
            else
            {
                List<Facility> sameDayEvents = GetDevices().Where(x => x.deviceID.Equals(reserve.deviceID)).Where(x => !x.id.Equals(reserve.id)).Where(x => x.meetingDate.Equals(reserve.meetingDate)).ToList();
                if (state.Equals("create")) // 新增時不排除
                {
                    sameDayEvents = GetDevices().Where(x => x.deviceID.Equals(reserve.deviceID)).Where(x => x.meetingDate.Equals(reserve.meetingDate)).ToList();
                    // 找到裝置中最大id+1
                    long maxId = _facilityRepository.GetDevices().Select(x => x.id).OrderByDescending(x => x).FirstOrDefault();
                    reserve.id = maxId + 1;
                }
                List<TimeInterval> timeIntervals = new List<TimeInterval>();
                foreach (Facility f in sameDayEvents)
                {
                    timeIntervals.Add(new TimeInterval() { Start = f.startTime, End = f.endTime });
                }
                timeIntervals.Add(new TimeInterval() { Start = reserve.startTime, End = reserve.endTime });
                (bool ok, string reason) checkNoOverlap = CheckNoOverlap(timeIntervals, false);
                if (checkNoOverlap.ok == false)
                {
                    ret = "此時段已有人預約！";
                }
                else
                {
                    ret = _facilityRepository.Send(state, reserve);
                }
            }
            return ret;
        }
        /// <summary>
        /// 驗證日期格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsValidTime(string date)
        {
            Regex rx = new Regex(@"^[0-2]\d:[0-5]\d$");
            if (!rx.IsMatch(date)) return false;
            int hh = int.Parse(date.Substring(0, 2));
            return hh < 24;
        }

        public int ToMinutes(string t)
        {
            var parts = t.Split(':').Select(int.Parse).ToArray();
            return parts[0] * 60 + parts[1];
        }

        public (bool ok, string reason) ValidateInterval(string start, string end, bool allowMidnightCross)
        {
            if (!IsValidTime(start)) return (false, "開始時間格式錯誤");
            if (!IsValidTime(end)) return (false, "結束時間格式錯誤");
            int s = ToMinutes(start), e = ToMinutes(end);
            if (!allowMidnightCross) // 不允許跨日
            {
                if (e < s) return (false, "結束時間必須大於開始時間");
                else if (s == e) return (false, "開始與結束時間相同");
            }
            else
            {
                if (s == e) return (false, "開始與結束時間相同");
            }
            return (true, null);
        }
        public (bool ok, string reason) CheckNoOverlap(List<TimeInterval> intervals, bool allowMidnightCross)
        {
            // 轉成 ranges
            var ranges = new List<(int s, int e, int idx)>();
            for (int i = 0; i < intervals.Count; i++)
            {
                var it = intervals[i];
                var v = ValidateInterval(it.Start, it.End, allowMidnightCross);
                if (!v.ok) return (false, $"區間 {i} 不合法: {v.reason}");
                int s = ToMinutes(it.Start), e = ToMinutes(it.End);
                if (!allowMidnightCross || e > s)
                {
                    ranges.Add((s, e, i));
                }
                else
                {
                    ranges.Add((s, 1440, i));
                    ranges.Add((0, e, i));
                }
            }
            var sorted = ranges.OrderBy(r => r.s).ThenBy(r => r.e).ToList();
            for (int i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var cur = sorted[i];
                if (prev.e > cur.s)
                {
                    return (false, $"區間 {prev.idx} 與 區間 {cur.idx} 重疊");
                }
            }
            return (true, null);
        }
        /// <summary>
        /// 新增裝置
        /// </summary>
        /// <param name="facility"></param>
        /// <returns></returns>
        public string CreateDevice(Facility facility)
        {
            string ret = _facilityRepository.CreateDevice(facility);
            return ret;
        }
        /// <summary>
        /// 移除裝置
        /// </summary>
        /// <param name="facility"></param>
        /// <returns></returns>
        public string RemoveDevice(string deviceID)
        {
            string ret = _facilityRepository.RemoveDevice(deviceID);
            return ret;
        }
    }
}