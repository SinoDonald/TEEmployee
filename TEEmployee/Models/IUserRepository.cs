using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models
{
    public interface IUserRepository
    {
        User Get(string UserId);
        List<User> GetAll();
        //bool Insert(User user);
        //bool Update(User user);
        //bool Delete(int UserId);
        List<User> GetManagers();
        List<string> GetGroupList(string view, string empno); // 取得群組
        List<string> GetGroupUsers(string selectedGroup, string empno); // 取得群組同仁
        bool InsertCustomUser(User user);
        void Dispose();
    }
}
