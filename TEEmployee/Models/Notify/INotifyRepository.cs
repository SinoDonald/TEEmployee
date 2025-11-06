using System.Collections.Generic;

namespace TEEmployee.Models.Notify
{
    public interface INotifyRepository
    {
        List<User> GetAll();
        User Get(string empno);
        List<bool> GetNotify(string season, List<User> users, string date, string empno);
        string DeleteNotifyLog();
        bool UpdateDatabase(string empno, int count, string notification);
        List<User> UserManagers(string empno, string state);
        bool ManagerSuggest(string path, List<string> managers, string empno);
        void Dispose();
    }
}
