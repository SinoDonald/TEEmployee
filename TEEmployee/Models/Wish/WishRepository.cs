using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using TEEmployee.Models.Forum;

namespace TEEmployee.Models.Wish
{
    public class WishRepository : IWishRepository
    {
        private IDbConnection _conn;
        public WishRepository()
        {
            string wishConnection = ConfigurationManager.ConnectionStrings["WishConnection"].ConnectionString;
            _conn = new SQLiteConnection(wishConnection);
        }


        public List<Wish> GetAll()
        {
            string sql = @"SELECT * FROM Wish";
            var ret = _conn.Query<Wish>(sql).ToList();

            //ret.ForEach(x =>
            //{
            //    string[] sparts = x.start_date.Split(' ');
            //    string[] eparts = x.end_date.Split(' ');
            //    x.start_date = sparts[0];
            //    x.end_date = eparts[0];
            //});

            return ret;
        }

        public bool InsertWish(Wish wish)
        {
            string sql = @"INSERT INTO Wish (empno, category, status, applicationDate, purpose, detail, filepath) 
                    VALUES(@empno, @category, @status, @applicationDate, @purpose, @detail, @filepath)";

            var ret = _conn.Execute(sql, wish);

            return ret > 0;
        }


        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        public bool UpdateWishStatus(Wish wish)
        {
            int ret = 0;

            string sql = @"
                UPDATE Wish
                SET status=@status
                WHERE id=@id;
            "
            ;

            ret = _conn.Execute(sql, wish);
            return ret > 0;
        }
    }
}