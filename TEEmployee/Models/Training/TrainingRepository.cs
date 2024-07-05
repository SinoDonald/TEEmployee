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

            ret.ForEach(x =>
            {
                string[] sparts = x.start_date.Split(' ');
                string[] eparts = x.end_date.Split(' ');
                x.start_date = sparts[0];
                x.end_date = eparts[0];
            });

            return ret;
        }

        public List<Record> GetAllRecordsByUser(string empno)
        {
            string sql = @"SELECT * FROM Record WHERE empno=@empno";
            var ret = _conn.Query<Record>(sql, new { empno }).ToList();

            // time format
            ret.ForEach(x =>
            {
                string[] sparts = x.start_date.Split(' ');
                string[] eparts = x.end_date.Split(' ');
                x.start_date = sparts[0];
                x.end_date = eparts[0];
            });

            // get and join recordExtra
            string sqlExtra = @"SELECT * FROM RecordExtra WHERE empno=@empno";
            var retExtra = _conn.Query<Record>(sqlExtra, new { empno }).ToList().ToDictionary(
                d => (d.empno, d.roc_year, d.training_id),
                d => (d.customType, d.emailSent)
            );



            foreach (var item in ret)
            {
                if (retExtra.ContainsKey((item.empno, item.roc_year, item.training_id)))
                {
                    item.customType = retExtra[(item.empno, item.roc_year, item.training_id)].customType;
                    item.emailSent = retExtra[(item.empno, item.roc_year, item.training_id)].emailSent;
                }

            }


            return ret;
        }

        public bool InsertRecords(List<Record> records)
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                // Just scan table
                string sql = @"INSERT INTO Record (empno, roc_year, training_type, training_id, title, organization, start_date, end_date, duration) 
                        VALUES(@empno, @roc_year, @training_type, @training_id, @title, @organization, @start_date, @end_date, @duration)";

                //string sql = @"INSERT INTO Record (empno, roc_year, training_type, training_id, title, organization, start_date, end_date, duration) 
                //        VALUES(@empno, @roc_year, @training_type, @training_id, @title, @organization, @start_date, @end_date, @duration) 
                //        ON CONFLICT(empno, training_id) 
                //        DO NOTHING";

                var ret = _conn.Execute(sql, records);

                tran.Commit();

                return ret > 0;
            }

        }

        public bool UpdateRecords(List<Record> records)
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"UPDATE RecordExtra SET customType=@customType WHERE 
                        empno=@empno AND roc_year=@roc_year AND training_id=@training_id";
                try
                {
                    var ret = _conn.Execute(sql, records);
                    tran.Commit();
                    return ret > 0;
                }
                catch
                {
                    return false;
                }

            }

        }

        public bool UpsertRecords(List<Record> records)
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {                
                string sql = @"INSERT INTO RecordExtra (empno, roc_year, training_id, customType, emailSent) 
                        VALUES(@empno, @roc_year, @training_id, @customType, @emailSent) 
                        ON CONFLICT(empno, roc_year, training_id) 
                        DO UPDATE SET customType=@customType, emailSent=@emailSent";
                try
                {
                    var ret = _conn.Execute(sql, records);
                    tran.Commit();
                    return ret > 0;
                }
                catch(Exception ex)
                {
                    return false;
                }

            }

        }

        public List<Record> GetRecordExtraByRecords(List<Record> records)
        {            
            // get and join recordExtra
            string sqlExtra = @"SELECT * FROM RecordExtra";
            var retExtra = _conn.Query<Record>(sqlExtra).ToList().ToDictionary(
                d => (d.empno, d.roc_year, d.training_id),
                d => (d.customType, d.emailSent)
            );

            foreach (var item in records)
            {
                if (retExtra.ContainsKey((item.empno, item.roc_year, item.training_id)))
                {
                    item.customType = retExtra[(item.empno, item.roc_year, item.training_id)].customType;
                    item.emailSent = retExtra[(item.empno, item.roc_year, item.training_id)].emailSent;
                }

            }

            return records;
        }


        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }


    }
}