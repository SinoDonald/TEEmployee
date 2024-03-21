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

            string sql = @"INSERT INTO ProjectTask (empno, yymm, projno, content, endDate, note, realHour) 
                        VALUES(@empno, @yymm, @projno, @content, @endDate, @note, @realHour)";

            ret = _conn.Execute(sql, projectTask);

            return ret > 0 ? true : false;
        }

        public bool Update(ProjectTask projectTask)
        {
            int ret;

            string sql = @"UPDATE ProjectTask SET projno=@projno, content=@content, endDate=@endDate, note=@note, realHour=@realHour WHERE id=@id";

            ret = _conn.Execute(sql, projectTask);

            return ret > 0 ? true : false;
        }

        public bool Upsert(ProjectTask projectTask)
        {
            throw new NotImplementedException();
        }
    }
}