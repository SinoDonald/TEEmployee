using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Dapper;

namespace TEEmployee.Models
{
    public class UserRepository : IUserRepository, IDisposable
    {        
        private IDbConnection _conn;
        public UserRepository()
        {
            string userConnection = ConfigurationManager.ConnectionStrings["UserConnection"].ConnectionString;
            _conn = new SQLiteConnection(userConnection);
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }               
       
        public List<User> GetAll()
        {
            List<User> ret;

            //string sql = @"select * from user order by empno";
            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno";
            ret = _conn.Query<User>(sql).ToList();

            return ret;
        }
        public List<User> GetManagers()
        {
            List<User> ret;
            string sql = @"select * from user where duty != 'NULL' order by dutyName";
            ret = _conn.Query<User>(sql).ToList();

            return ret;
        }
        public User Get(string id)
        {
            User ret;

            //string sql = @"select * from user where empno=@id";
            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno WHERE u.empno=@id";
            ret = _conn.Query<User>(sql, new { id }).SingleOrDefault();

            return ret;
        }

        // insert userExtra
        public bool Insert(List<User> users)
        {

            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {

                int ret;

                string sql = @"DELETE FROM userExtra";
                ret = _conn.Execute(sql);


                sql = @"INSERT INTO userExtra (empno, department_manager, 'group', group_manager,
                                group_one, group_one_manager, group_two, group_two_manager, group_three, group_three_manager, project_manager, projects, assistant_project_manager) 
                        VALUES(@empno, @department_manager, @group, @group_manager, @group_one, @group_one_manager,
                                @group_two, @group_two_manager, @group_three, @group_three_manager, @project_manager, @projects, @assistant_project_manager)";

                ret = _conn.Execute(sql, users);

                tran.Commit();

                return ret > 0;

            }

            //    int ret;

            //string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
            //            VALUES(@empno, @depno, @yymm, @projno, @itemno, @workhour, @overtime)";

            //ret = _conn.Execute(sql, projectItem);

            //return ret > 0 ? true : false;

            
        }

        // 1206: insert "User" (Delete all first)
        public bool InsertUser(List<User> users)
        {

            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {

                int ret;

                string sql = @"DELETE FROM user";
                ret = _conn.Execute(sql);


                sql = @"INSERT INTO user (empno, name, gid, profTitle, duty, dutyName, tel, email) 
                        VALUES(@empno, @name, @gid, @profTitle, @duty, @dutyName, @tel, @email)";

                ret = _conn.Execute(sql, users);

                tran.Commit();

                return ret > 0;

            }
            
        }


        // GET the group one name list owned by the manager
        public List<string> GetSubGroups(string manno)
        {
            List<string> groups = new List<string>();
            User user = this.Get(manno);

            // department manager or group manager

            if ((user.group_manager && user.group == "設計") || user.department_manager)
            {
                groups.AddRange(new List<string> { "地工組", "界面整合管理組", "BIM暨程式開發組", "智慧軌道創新小組" });
            }

            if ((user.group_manager && user.group == "規劃") || user.department_manager)
            {
                groups.AddRange(new List<string> { "土木組", "規劃組" });
            }

            if ((user.group_manager && user.group == "專管") || user.department_manager)
            {
                groups.AddRange(new List<string> { "工程管理組", "成本/契約組", "工務組" });
            }

            if (user.department_manager)
            {
                groups.AddRange(new List<string> { /*"計畫管理組",*/ "行政組" });
            }

            // sub group manager
            if (user.group_one_manager) groups.Add(user.group_one);
            if (user.group_two_manager) groups.Add(user.group_two);
            if (user.group_three_manager) groups.Add(user.group_three);

            // remove duplicates from special case
            groups = groups.Distinct().ToList();

            return groups;
        }

        // 取得群組 <-- 培文
        public List<string> GetGroupList(string view, string empno)
        {
            List<string> ret = new List<string>();
            List<User> users = new UserRepository().GetAll();
            User user = users.Where(x => x.empno.Equals(empno)).FirstOrDefault();

            if (view.Equals("GroupPlan"))
            {
                if (empno.Equals("4125"))
                {
                    ret = users.Where(x => x.group != "").Select(x => x.group).Distinct().ToList();
                    ret.Insert(0, "行政");
                }
                else { ret = users.Where(x => x.empno.Equals(empno)).Select(x => x.group).ToList(); }
            }
            else if (view.Equals("PersonalPlan"))
            {
                if (empno.Equals("4125"))
                {
                    if (user.department_manager){ ret = new List<string> { "規劃", "設計", "專管" }; }
                    else { ret.Add(user.group); users = users.Where(x => x.group == user.group).ToList(); }

                    foreach (var item in users)
                    {
                        if (!String.IsNullOrEmpty(item.group))
                        {
                            //三大群組 小組1
                            if (!String.IsNullOrEmpty(item.group_one) && !ret.Contains(item.group_one))
                            {
                                ret.Insert(ret.FindIndex(x => x == item.group) + 1, item.group_one);
                            }
                            //跨三大群組 小組2 小組3 (協理 only)
                            if (user.department_manager)
                            {
                                if (!String.IsNullOrEmpty(item.group_two) && !ret.Contains(item.group_two))
                                {
                                    ret.Add(item.group_two);
                                }
                                if (!String.IsNullOrEmpty(item.group_three) && !ret.Contains(item.group_three))
                                {
                                    ret.Add(item.group_three);
                                }
                            }
                        }
                        else
                        {
                            //非三大群組
                            if (!String.IsNullOrEmpty(item.group_one) && !ret.Contains(item.group_one))
                            {
                                ret.Add(item.group_one);
                            }
                        }
                    }

                    // Special Case
                    if (ret.Remove("規劃組")) ret.Insert(ret.FindIndex(x => x == "規劃") + 1, "規劃組");
                    if (user.group_one_manager) ret.Add(user.group_one);
                    if (user.group_two_manager) ret.Add(user.group_two);
                    if (user.group_three_manager) ret.Add(user.group_three);

                    ret = ret.Distinct().ToList();
                }
                else if(user.group_manager.Equals(true) || user.group_one_manager.Equals(true) || user.group_two_manager.Equals(true) || user.group_three_manager.Equals(true))
                {
                    string groupName = users.Where(x => x.empno.Equals(empno)).Select(x => x.group).FirstOrDefault();
                    users = users.Where(x => x.group != null).Where(x => x.group.Equals(groupName)).ToList();
                    List<string> group_list = users.Where(x => x.group != "").Select(x => x.group).Distinct().ToList();
                    List<string> group_one_list = users.Where(x => x.group_one != "").Select(x => x.group_one).Distinct().ToList();
                    List<string> group_two_list = users.Where(x => x.group_two != "").Select(x => x.group_two).Distinct().ToList();
                    List<string> group_three_list = users.Where(x => x.group_three != "").Select(x => x.group_three).Distinct().ToList();
                    foreach (string group in group_list) { ret.Add(group); }
                    foreach (string group in group_one_list) { ret.Add(group); }
                    foreach (string group in group_two_list) { ret.Add(group); }
                    foreach (string group in group_three_list) { ret.Add(group); }
                    ret = ret.Where(x => x != "").Distinct().ToList();
                }
                else
                {
                    ret = users.Where(x => x.group != null).Where(x => x.group.Equals(user.group)).Select(x => x.group).Distinct().ToList();
                }
            }

            return ret;
        }

        // 取得群組同仁 <-- 培文
        public List<string> GetGroupUsers(string selectedGroup, string empno)
        {
            List<string> ret = new List<string>();
            List <User> users = new UserRepository().GetAll();
            User user = users.Where(x => x.empno.Equals(empno)).FirstOrDefault();

            if(user.empno.Equals("4125") || user.group_manager.Equals(true))
            {
                ret = users.Where(x => x.group != null).Where(x => x.group.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組
                if (ret.Count.Equals(0))
                {
                    ret = users.Where(x => x.group_one != null).Where(x => x.group_one.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組一
                    if (ret.Count.Equals(0))
                    {
                        ret = users.Where(x => x.group_two != null).Where(x => x.group_two.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組二
                        if (ret.Count.Equals(0))
                        {
                            ret = users.Where(x => x.group_three != null).Where(x => x.group_three.Equals(selectedGroup)).Select(x => x.name).OrderBy(x => x).ToList(); // 群組三
                        }
                    }
                }
            }
            else
            {
                ret.Add(user.name);
            }

            return ret;
        }

        // 取得員工群組 <-- 培文
        public List<User> UserGroups()
        {
            List<User> ret;

            string sql = @"SELECT * FROM userExtra";
            ret = _conn.Query<User>(sql).ToList();

            return ret;
        }

        //public bool DeleteUserExtra()
        //{
        //    int ret;

        //    string sql = @"DELETE FROM userExtra";
        //    ret = _conn.Execute(sql);

        //    return ret > 0;
        //}
    }
}