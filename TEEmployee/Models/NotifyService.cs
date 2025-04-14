using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.GSchedule;

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

            // 先確認當月為1、5、11月
            DateTime now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;
            string season = string.Empty;
            if (month == 4 || month == 5 || month == 11)
            {
                if (month == 4) season = year + "H0"; // 個人規劃是否上傳與回饋
                else if (month == 5) season = year + "H1";
                else if(month == 11) season = year + "H2";

                // 檢查資料庫中, userNotify是否為當季資料, 不是的話則新建
                string date = year.ToString() + month.ToString("00");
                List<User> users = _notifyRepository.GetAll();
                users = users.Where(x => x.group != null && x.group_one != null && x.group_two != null && x.group_three != null).ToList(); // 移除沒有群組的使用者
                ret = _notifyRepository.GetNotify(season, users, date, empno);
            }

            return ret;
        }
        /// <summary>
        /// 更新年度個人規劃回饋
        /// </summary>
        /// <param name="empno"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UpdatePersonPlanFeedback(string empno, string name)
        {
            bool ret = false;

            // 先確認當月為1月
            DateTime now = DateTime.Now;
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            string year = DateTime.Now.ToString("yyy", culture);
            int month = now.Month;
            if (month == 4)
            {
                NotifyRepository notifyRepository = new NotifyRepository();
                List<User> users = notifyRepository.GetAll();
                users = users.Where(x => x.group != null && x.group_one != null && x.group_two != null && x.group_three != null).ToList(); // 移除沒有群組的使用者
                User user = users.Where(x => x.empno.Equals(empno)).FirstOrDefault();
                if (user != null)
                {
                    NotifyService notifyService = new NotifyService();
                    // 主管、組長身份需先檢查是否已回覆所有同仁
                    if (user.department_manager.Equals(true) || user.group_manager.Equals(true) || user.group_one_manager.Equals(true) ||
                        user.group_two_manager.Equals(true) || user.group_three_manager.Equals(true))
                    {
                        List<string> uploadUsers = notifyRepository.GetUploadUsers(users); // 儲存所有已上傳年度個人規劃簡報的名單
                        // 找到同group的user
                        List<User> sameGroupUsers = notifyRepository.SameGroupUsers(user, users);
                        // 檢驗有哪些group為manager
                        List<string> groupManagers = new List<string>();
                        if (user.group_manager.Equals(true)) { groupManagers.Add(user.group); }
                        if (user.group_one_manager.Equals(true)) { groupManagers.Add(user.group_one); }
                        if (user.group_two_manager.Equals(true)) { groupManagers.Add(user.group_two); }
                        if (user.group_three_manager.Equals(true)) { groupManagers.Add(user.group_three); }
                        foreach (string groupManager in groupManagers)
                        {
                            // 該組長相同群組的同仁
                            List<User> list = sameGroupUsers.Where(x => x.group.Equals(groupManager) || x.group_one.Equals(groupManager) ||
                                                                   x.group_two.Equals(groupManager) || x.group_three.Equals(groupManager)).ToList();
                            // 該年度有上傳簡報
                            list = list.Where(x => uploadUsers.Where(y => y.Equals(x.empno)).Count() > 0).ToList();
                            // 查詢主管是否已經回饋
                            foreach (User sameGroupUser in list)
                            {
                                List<Planning> responses = new GScheduleRepository().GetUserPlanning("PersonalPlan", year, sameGroupUser.name).ToList();
                                if (responses.Count.Equals(0)) { ret = true; break; }
                                else
                                {
                                    if (responses.Where(x => x.manager_id.ToString().Equals(user.empno)).Count().Equals(0)) { ret = true; break; }
                                }
                            }
                        }
                        if (ret == true)
                        {
                            User freebackUser = users.Where(x => x.name.Equals(name)).FirstOrDefault();
                            if (freebackUser != null)
                            {
                                notifyService.UpdateDatabase(user.empno, 6, "1"); // 主管回饋後, 尚有未通知的同仁
                                notifyService.UpdateDatabase(freebackUser.empno, 6, "1");// 通知使用者主管已回饋
                            }
                        }
                        else { notifyService.UpdateDatabase(user.empno, 6, "0"); } // 主管回饋全部已上傳簡報的同仁後, 取消個人規劃回饋通知
                    }
                    else
                    {
                        ret = notifyService.UpdateDatabase(user.empno, 6, "0"); // 取消個人規劃回饋通知
                    }
                }
            }

            return ret;
        }
        // 更新資料庫 <-- 培文
        public bool UpdateDatabase(string empno, int count, string notification)
        {
            bool ret = false;

            // 先確認當月為1、5、11月才會進行資料庫更新
            int month = DateTime.Now.Month;
            if (month == 4 || month == 5 || month == 11)
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

        public void Dispose()
        {
            _notifyRepository.Dispose();
        }
    }
}