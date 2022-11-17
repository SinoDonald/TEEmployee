using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;
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

        public bool Insert(List<User> users)
        {

            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {

                int ret;

                string sql = @"DELETE FROM userExtra";
                ret = _conn.Execute(sql);


                sql = @"INSERT INTO userExtra (empno, department_manager, 'group', group_manager,
                                group_one, group_one_manager, group_two, group_two_manager, group_three, group_three_manager, project_manager, projects) 
                        VALUES(@empno, @department_manager, @group, @group_manager, @group_one, @group_one_manager,
                                @group_two, @group_two_manager, @group_three, @group_three_manager, @project_manager, @projects)";

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




        //public bool DeleteUserExtra()
        //{
        //    int ret;

        //    string sql = @"DELETE FROM userExtra";
        //    ret = _conn.Execute(sql);

        //    return ret > 0;
        //}

    }
}