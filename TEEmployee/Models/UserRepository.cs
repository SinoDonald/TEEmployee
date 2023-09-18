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