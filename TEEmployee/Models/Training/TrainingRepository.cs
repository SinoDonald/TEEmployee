using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Training
{
    public class TrainingRepository : ITrainingRepository
    {
        private IDbConnection _conn;
        public TrainingRepository()
        {
            string trainingConnection = ConfigurationManager.ConnectionStrings["TrainingConnection"].ConnectionString;
            _conn = new SQLiteConnection(trainingConnection);
        }
        
        public List<Record> GetAllRecords()
        {
            string sql = @"SELECT * FROM Record";
            var ret = _conn.Query<Record>(sql).ToList();

            return ret;
        }

        public List<Record> GetAllRecordsByUser(string empno)
        {
            string sql = @"SELECT * FROM Record WHERE empno=@empno";
            var ret = _conn.Query<Record>(sql, new { empno }).ToList();

            return ret;
        }

        public bool InsertRecords(List<Record> records)
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"INSERT INTO Record (empno, roc_year, training_type, training_id, title, organization, start_date, end_date, duration) 
                        VALUES(@empno, @roc_year, @training_type, @training_id, @title, @organization, @start_date, @end_date, @duration) 
                        ON CONFLICT(empno, training_id) 
                        DO NOTHING";

                var ret = _conn.Execute(sql, records);

                tran.Commit();

                return ret > 0;
            }

        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        
    }
}