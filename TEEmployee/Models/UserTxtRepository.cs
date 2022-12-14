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

        //public List<User> GetAll()
        //{
        //    List<User> users = new List<User>();

        //    string fn = Path.Combine(_appData, "User/User.txt");
        //    string[] lines = System.IO.File.ReadAllLines(fn);

        //    foreach (var item in lines)
        //    {
        //        string[] subs = item.Split('/');
        //        //users.Add(new User() { UserId = subs[0], UserName = subs[1], Role = subs[2] });
        //        users.Add(new User() { empno = subs[0], name = subs[1] });
        //    }

        //    users = users.OrderBy(a => a.empno).ToList();

        //    return users;


        //    //try
        //    //{
        //    //    string fn = Path.Combine(_appData, "employee.txt");
        //    //    List<string> lines = System.IO.File.ReadAllLines(fn).ToList();

        //    //    foreach (var item in lines)
        //    //    {
        //    //        string[] subs = item.Split('\t');
        //    //        User user = new User();

        //    //        user.empno = subs[0];
        //    //        user.name = subs[1];
        //    //        user. = subs[2];
        //    //        user.yymm = subs[3];

        //    //        // work type
        //    //        if (Convert.ToInt32(subs[5]) == 0)
        //    //            projectItem.overtime = Convert.ToInt32(subs[4]);
        //    //        else
        //    //            projectItem.workHour = Convert.ToInt32(subs[4]);

        //    //        var ret = projectItems.Find(x =>
        //    //                                    x.empno == projectItem.empno &&
        //    //                                    x.projno == projectItem.projno &&
        //    //                                    x.yymm == projectItem.yymm &&
        //    //                                    x.itemno == projectItem.itemno);

        //    //        if (ret is object)
        //    //        {
        //    //            ret.workHour += projectItem.workHour;
        //    //            ret.overtime += projectItem.overtime;
        //    //        }
        //    //        else
        //    //            projectItems.Add(projectItem);

        //    //    }


        //        //-------------------------------------------------------------

        //    //    projectItems = projectItems.OrderBy(x => x.empno).ToList();

        //    //    // Delete the resource after reading it

        //    //    File.Delete(fn);



        //    //}
        //    //catch
        //    //{

        //    //}


        //    //return projectItems;

        //}


        // 1206: get all user from txt 
        public List<User> GetAll()
        {
            List<User> users = new List<User>();

            try
            {
                string fn = Path.Combine(_appData, "User.txt");
                List<string> lines = System.IO.File.ReadAllLines(fn).ToList();

                foreach (var item in lines)
                {
                    string[] subs = item.Split('\t');
                    User user = new User();

                    user.empno = subs[0];
                    user.name = subs[1];
                    user.gid = subs[2];
                    user.profTitle = subs[3];
                    user.duty = subs[4];                    
                    user.dutyName = subs[5];
                    user.tel = subs[6];
                    user.email = subs[7];

                    users.Add(user);
                }

                users = users.OrderBy(x => x.empno).ToList();

                // Delete the file after reading it

                File.Delete(fn);
            }
            catch
            {

            }


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