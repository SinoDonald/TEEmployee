using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models
{
    public interface INotifyRepository
    {
        List<User> GetAll();
        User Get(string empno);
        List<bool> GetNotify(List<User> users, string date, string empno);
        bool UpdateDatabase(string empno, int count, string notification);
        void Dispose();
    }
}
