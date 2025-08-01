﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using Dapper;

namespace TEEmployee.Models.TaskLog
{
    public class ProjectTaskRepository : IProjectTaskRepository
    {

        private IDbConnection _conn;

        public ProjectTaskRepository()
        {
            string tasklogConnection = ConfigurationManager.ConnectionStrings["TasklogConnection"].ConnectionString;
            _conn = new SQLiteConnection(tasklogConnection);
        }

        public bool Delete(int id, string empno)
        {
            int ret;
                        
            string sql = @"DELETE FROM ProjectTask WHERE id=@id AND empno=@empno;";

            //ret = _conn.Execute(sql, new { id = id, empno = empno });
            ret = _conn.Execute(sql, new { id, empno });

            return ret > 0 ? true : false;
            
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        public ProjectTask Get(int id)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 取得專案編號工作項目內容
        /// </summary>
        /// <returns></returns>
        public List<ProjectTask> GetAll()
        {
            List<ProjectTask> ret;

            string sql = @"SELECT * FROM ProjectTask";
            ret = _conn.Query<ProjectTask>(sql).ToList();

            return ret;
        }

        public bool Insert(ProjectTask projectTask)
        {
            int ret;

            string sql = @"INSERT INTO ProjectTask (empno, yymm, projno, content, endDate, note, realHour, projectType, custom_order, generate_schedule) 
                        VALUES(@empno, @yymm, @projno, @content, @endDate, @note, @realHour, @projectType, @custom_order, @generate_schedule)";

            ret = _conn.Execute(sql, projectTask);

            return ret > 0;
        }

        public bool Update(ProjectTask projectTask)
        {
            int ret;

            string sql = @"UPDATE ProjectTask SET projno=@projno, content=@content, endDate=@endDate, note=@note, realHour=@realHour, projectType=@projectType, custom_order=@custom_order, generate_schedule=@generate_schedule WHERE id=@id";

            ret = _conn.Execute(sql, projectTask);

            return ret > 0;
        }

        public bool Upsert(ProjectTask projectTask)
        {
            throw new NotImplementedException();
        }

        public void AddProjectTypeColumn()
        {           
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"ALTER TABLE ProjectTask ADD COLUMN projectType INTEGER";
                _conn.Execute(sql);
                tran.Commit();
            }

        }

        public bool AddCustomOrderColumn()
        {
            string sql = "ALTER TABLE ProjectTask ADD COLUMN custom_order INTEGER;";

            try
            {                
                _conn.Execute(sql);
                return true;
            }
            catch (Exception e)
            {                
                return false;
            }
        }

        public bool AddGenerateScheduleColumn()
        {
            string sql = "ALTER TABLE ProjectTask ADD COLUMN generate_schedule INTEGER;";

            try
            {
                _conn.Execute(sql);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteAll()
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(@"DELETE FROM ProjectTask", tran);
                    tran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }

        }

        public List<ProjectTask> GetProjectTasksByEmpnoAndYYMM(string empno, string yymm)
        {
            string sql = @"SELECT * FROM ProjectTask WHERE empno=@empno AND yymm=@yymm";
            var ret = _conn.Query<ProjectTask>(sql, new { empno, yymm }).ToList();

            return ret;
        }
    }
}