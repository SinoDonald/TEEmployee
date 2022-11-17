using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using Dapper;

namespace TEEmployee.Models.TaskLog
{
    public class MonthlyRecordRepository : IMonthlyRecordRepository
    {
        private IDbConnection _conn;

        public MonthlyRecordRepository()
        {
            string tasklogConnection = ConfigurationManager.ConnectionStrings["TasklogConnection"].ConnectionString;
            var builder = new SQLiteConnectionStringBuilder("Data Source=./mydatabase.db") { BinaryGUID = true };
            _conn = new SQLiteConnection(tasklogConnection);

            SqlMapper.AddTypeHandler(new GuidByteHandler());
            
        }

        public bool Delete(MonthlyRecord monthlyRecord)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        public MonthlyRecord Get(Guid guid)
        {
            MonthlyRecord ret;

            string sql = @"SELECT * FROM MonthlyRecord WHERE guid=@guid";
            ret = _conn.Query<MonthlyRecord>(sql, new { guid }).SingleOrDefault();

            return ret;
        }

        public List<MonthlyRecord> GetAll()
        {
            List<MonthlyRecord> ret;

            string sql = @"SELECT * FROM MonthlyRecord ORDER BY yymm";
            ret = _conn.Query<MonthlyRecord>(sql).ToList();

            return ret;
        }

        public bool Insert(MonthlyRecord monthlyRecord)
        {
            int ret;

            string sql = @"INSERT INTO MonthlyRecord (guid, empno, yymm) 
                        VALUES(@guid, @empno, @yymm)";

            ret = _conn.Execute(sql, monthlyRecord);

            return ret > 0 ? true : false;

        }

        public bool Update(MonthlyRecord monthlyRecord)
        {
            throw new NotImplementedException();
        }

        public bool Upsert(MonthlyRecord monthlyRecord)
        {
            int ret;

            string sql = @"INSERT INTO MonthlyRecord (guid, empno, yymm) 
                        VALUES(@guid, @empno, @yymm) 
                        ON CONFLICT(empno, yymm) 
                        DO NOTHING";

            ret = _conn.Execute(sql, monthlyRecord);

            return ret > 0 ? true : false;
        }

        public bool Upsert(List<MonthlyRecord> monthlyRecord)
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                int ret;

                string sql = @"INSERT INTO MonthlyRecord (guid, empno, yymm) 
                        VALUES(@guid, @empno, @yymm) 
                        ON CONFLICT(empno, yymm) 
                        DO NOTHING";

                ret = _conn.Execute(sql, monthlyRecord);

                tran.Commit();

                return ret > 0 ? true : false;

                
            }
            //    int ret;

            //string sql = @"INSERT INTO MonthlyRecord (guid, empno, yymm) 
            //            VALUES(@guid, @empno, @yymm) 
            //            ON CONFLICT(empno, yymm) 
            //            DO NOTHING";

            //ret = _conn.Execute(sql, monthlyRecord);

            //return ret > 0 ? true : false;
        }
    }
}