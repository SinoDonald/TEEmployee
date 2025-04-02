using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace TEEmployee.Models
{
    public class NotifyService
    {
        private INotifyRepository _notifyRepository;

        public NotifyService()
        {
            _notifyRepository = new NotifyRepository();
        }
        // 首頁通知 <-- 培文
        public List<bool> GetNotify(string empno)
        {
            List<bool> ret = new List<bool>();

            // 先確認當月為5、11月
            DateTime now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;
            string season = string.Empty;
            if (month == 5 || month == 11)
            {
                if (month == 5) season = year + "H1";
                else if(month == 11) season = year + "H2";

                // 檢查資料庫中, userNotify是否為當季資料, 不是的話則新建
                string date = year.ToString() + month.ToString("00");
                List<User> users = _notifyRepository.GetAll();
                users = users.Where(x => x.group != null && x.group_one != null && x.group_two != null && x.group_three != null).ToList(); // 移除沒有群組的使用者
                ret = _notifyRepository.GetNotify(season, users, date, empno);
            }

            return ret;
        }
        // 更新資料庫 <-- 培文
        public bool UpdateDatabase(string empno, int count, string notification)
        {
            bool ret = false;

            // 先確認當月為5、11月才會進行資料庫更新
            int month = DateTime.Now.Month;
            if (month == 5 || month == 11)
            {
                ret = _notifyRepository.UpdateDatabase(empno, count, notification);
            }

            return ret;
        }
        // 找到使用者各群組的主管們
        public List<User> UserManagers(string empno, string state)
        {
            List<User> ret = _notifyRepository.UserManagers(empno, state);
            return ret;
        }
        // 給予主管建議表
        public bool ManagerSuggest(string empno)
        {
            bool ret = false;

            // 先確認當月為5、11月
            DateTime now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;
            if (month == 5 || month == 11)
            {
                string season = string.Empty;
                if (month == 5)
                    season = year + "H1";
                else
                    season = year + "H2";

                string _appData = HttpContext.Current.Server.MapPath("~/App_Data");
                string path = Path.Combine(_appData, "ManageResponse", season);
                // 是否已填寫給予主管建議評估表, 必填協理+group_manager2位
                List<User> userManagers = UserManagers(empno, "");
                List<string> managers = userManagers.Select(x => x.empno).ToList();
                ret = _notifyRepository.ManagerSuggest(path, managers, empno);
            }

            return ret;
        }
        // 年度個人規劃
        public bool PersonalPlan(string empno)
        {
            bool ret = false;

            // 先確認當月為5、11月
            DateTime now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;
            if (month == 5 || month == 11)
            {
                string season = string.Empty;
                if (month == 5)
                    season = year + "H1";
                else
                    season = year + "H2";

                string _appData = HttpContext.Current.Server.MapPath("~/App_Data");
                string path = Path.Combine(_appData, "GSchedule", season);
                // 是否已填寫給予主管建議評估表, 必填協理+group_manager2位
                List<User> userManagers = UserManagers(empno, "");
                List<string> managers = userManagers.Select(x => x.empno).ToList();
                ret = _notifyRepository.ManagerSuggest(path, managers, empno);
            }

            return ret;
        }

        public void Dispose()
        {
            _notifyRepository.Dispose();
        }
    }
}