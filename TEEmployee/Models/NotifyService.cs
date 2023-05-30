using System;
using System.Collections.Generic;
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
            if (month == 5 || month == 11)
            {
                // 檢查資料庫中, userNotify是否為當季資料, 不是的話則新建
                string date = year.ToString() + month.ToString("00");
                List<User> users = _notifyRepository.GetAll();
                ret = _notifyRepository.GetNotify(users, date, empno);
            }

            return ret;
        }
        // 更新資料庫 <-- 培文
        public bool UpdateDatabase(string empno, int count, string notification)
        {
            bool ret = _notifyRepository.UpdateDatabase(empno, count, notification);
            return ret;
        }
        public void Dispose()
        {
            _notifyRepository.Dispose();
        }
    }
}