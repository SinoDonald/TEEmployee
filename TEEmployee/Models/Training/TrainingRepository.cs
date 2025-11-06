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
                catch(Exception)
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

        public List<ExternalTraining> GetExternalTrainingsByGroup(string group_name)
        {
            string sql = @"SELECT * FROM ExternalTraining WHERE group_name=@group_name";
            var ret = _conn.Query<ExternalTraining>(sql, new { group_name }).ToList();

            return ret;
        }

        public List<Record> GetExternalRecordsByUser(string empno)
        {
            List<Record> records = new List<Record>();

            string sql = @"SELECT * FROM ExternalTraining WHERE members LIKE @Pattern";
            var ret = _conn.Query<ExternalTraining>(sql, new { Pattern = $"%{empno}%" }).ToList();

            // get and join externalRecordExtra with c#
            string sqlExtra = @"SELECT * FROM ExternalRecordExtra WHERE empno=@empno";
            var retExtra = _conn.Query<Record>(sqlExtra, new { empno }).ToList().ToDictionary(
                d => d.training_id,
                d => d.customType
            );

            foreach (var item in ret)
            {
                Record record = Record.FromExternalTraining(item);
                record.empno = empno;

                if (retExtra.ContainsKey(item.id.ToString()))
                {
                    record.customType = retExtra[item.id.ToString()];                    
                }

                records.Add(record);
            }

            return records;

        }

        public bool UpsertExternalRecords(List<Record> records)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"
                    INSERT INTO ExternalRecordExtra (empno, training_id, customType) 
                    VALUES(@empno, @training_id, @customType) 
                    ON CONFLICT(empno, training_id) 
                    DO UPDATE SET customType=@customType";
                try
                {
                    var ret = _conn.Execute(sql, records);
                    tran.Commit();
                    return ret > 0;
                }
                catch (Exception)
                {
                    return false;
                }

            }

        }

        public bool InsertExternalTraining(ExternalTraining training)
        {
            int ret = 0;

            string sql = @"
                INSERT INTO ExternalTraining (group_name, roc_year, training_type, title, organization, start_date, end_date, duration, members, filepath) 
                VALUES(@group_name, @roc_year, @training_type, @title, @organization, @start_date, @end_date, @duration, @members, @filepath)"
            ;

            ret = _conn.Execute(sql, training);
            return ret > 0;
        }

        public bool UpdateExternalTraining(ExternalTraining training)
        {
            int ret = 0;

            string sql = @"
                UPDATE ExternalTraining
                SET roc_year=@roc_year, training_type=@training_type, title=@title, organization=@organization, start_date=@start_date, end_date=@end_date, duration=@duration, members=@members, filepath=@filepath
                WHERE id=@id;
            ";

            ret = _conn.Execute(sql, training);
            return ret > 0;
        }


        public bool DeleteExternalTraining(ExternalTraining training)
        {
            _conn.Open();

            bool ret = true;
            string deleteTrainingSql = @"DELETE FROM ExternalTraining WHERE id=@id;";
            string deleteRecordSql = @"DELETE FROM ExternalRecordExtra WHERE training_id=@id;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(deleteTrainingSql, training);
                    _conn.Execute(deleteRecordSql, training);

                    tran.Commit();
                }
                catch (Exception)
                {
                    ret = false;
                }

            }

            return ret;
        }


        public bool EnsureTablesExist()
        {
            try
            {
                
                var createExternalRecordExtraTableSql = @"
                CREATE TABLE IF NOT EXISTS ExternalRecordExtra (
                    empno TEXT,
                    training_id TEXT,
                    customType INTEGER,
                    UNIQUE(empno, training_id)
                );
                ";

                var createExternalTrainingTableSql = @"
                CREATE TABLE IF NOT EXISTS ExternalTraining (
                    id INTEGER PRIMARY KEY,
                    group_name VARCHAR(10),
                    roc_year INTEGER,
                    training_type VARCHAR(10),
                    title VARCHAR(100),
                    organization VARCHAR(100),
                    start_date VARCHAR(50),
                    end_date VARCHAR(50),
                    duration INTEGER,
                    members VARCHAR(500),
                    filepath TEXT
                );
                ";

                _conn.Execute(createExternalRecordExtraTableSql);
                _conn.Execute(createExternalTrainingTableSql);

                return true;
                
            }
            catch (Exception)
            {
                return false;
            }
        }


        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        
    }
}