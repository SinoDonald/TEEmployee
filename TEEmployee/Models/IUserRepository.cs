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
        void Dispose();
    }
}
