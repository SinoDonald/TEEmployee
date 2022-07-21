using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class UserTxtRepository : IUserRepository
    {
        private readonly string _appData;
        
        public UserTxtRepository()
        {
            _appData = HttpContext.Current.Server.MapPath("~/App_Data"); 
        }

        public User Get(string userId)
        {
            User ret = new User();
            
            string fn = Path.Combine(_appData, "User/User.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);            
            
            foreach (var item in lines)
            {
                string[] subs = item.Split('/');

                if (subs[0] == userId)
                {
                    ret.empno = subs[0];
                    ret.name = subs[1];
                    //ret.Role = subs[2];
                    return ret;
                }                
            }
            return null;
        }

        public List<User> GetAll()
        {
            List<User> users = new List<User>();

            string fn = Path.Combine(_appData, "User/User.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);

            foreach (var item in lines)
            {
                string[] subs = item.Split('/');
                //users.Add(new User() { UserId = subs[0], UserName = subs[1], Role = subs[2] });
                users.Add(new User() { empno = subs[0], name = subs[1] });
            }

            users = users.OrderBy(a => a.empno).ToList();

            return users;
        }
        public List<User> GetManagers()
        {
            List<User> users = new List<User>();
            return users;
        }
        public void Dispose()
        {
            return;
        }

        
    }
}