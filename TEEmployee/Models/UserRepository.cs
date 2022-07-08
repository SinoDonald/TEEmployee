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
        
        private IDbConnection conn;

        public UserRepository()
        {
            string userConnection = ConfigurationManager.ConnectionStrings["UserConnection"].ConnectionString;
            conn = new SQLiteConnection(userConnection);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
               
       
        public List<User> GetAll()
        {
            List<User> ret;

            string sql = @"select * from user order by empno";
            ret = conn.Query<User>(sql).ToList();

            return ret;
        }
        public User Get(string id)
        {
            User ret;

            string sql = @"select * from user where empno=@id";
            ret = conn.Query<User>(sql, new { id }).SingleOrDefault();

            return ret;
        }
    }
}