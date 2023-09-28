using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Promotion
{
    public class PromotionRepository : IPromotionRepository
    {
        private IDbConnection _conn;
        public PromotionRepository()
        {
            string promotionConnection = ConfigurationManager.ConnectionStrings["PromotionConnection"].ConnectionString;
            _conn = new SQLiteConnection(promotionConnection);
        }


        public List<Promotion> GetAll()
        {
            throw new NotImplementedException();
        }

        public List<Promotion> GetByUser(string empno)
        {            
            string sql = @"SELECT p.*, c.*
                FROM Promotion AS p
                LEFT JOIN Condition AS c ON p.condition = c.id
                WHERE empno=@empno";

            var ret = _conn.Query<Promotion>(sql, new { empno }).ToList();

            return ret;
        }

        // Create new data
        public bool Insert(List<Promotion> promotions)
        {
            string sql = @"INSERT INTO Promotion (empno, condition) VALUES(@empno, @condition)";

            var ret = _conn.Execute(sql, promotions);

            return ret > 0;
        }

        public bool Update(Promotion promotion)
        {
            string sql = @"UPDATE Promotion SET achieved=@achieved, comment=@comment, filepath=@filepath WHERE empno=@empno AND condition=@condition";

            var ret = _conn.Execute(sql, promotion);

            return ret > 0;
        }

        public bool DeleteAll()
        {
            string sql = @"DELETE FROM Promotion";
            int ret = _conn.Execute(sql);
            
            return ret > 0;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        
    }
}