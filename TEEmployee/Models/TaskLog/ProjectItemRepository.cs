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
    public class ProjectItemRepository : IProjectItemRepository, IDisposable
    {
        private IDbConnection _conn;

        public ProjectItemRepository()
        {
            string tasklogConnection = ConfigurationManager.ConnectionStrings["TasklogConnection"].ConnectionString;
            _conn = new SQLiteConnection(tasklogConnection);
        }

        public bool Delete(ProjectItem projectItem)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        public ProjectItem Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<ProjectItem> GetAll()
        {
            List<ProjectItem> ret;

            string sql = @"SELECT empno, yymm, projno, itemno, workhour, overtime FROM ProjectItem";
            ret = _conn.Query<ProjectItem>(sql).ToList();

            return ret;
        }

        public bool Insert(ProjectItem projectItem)
        {
            int ret;

            string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
                        VALUES(@empno, @depno, @yymm, @projno, @itemno, @workHour, @overtime)";

            ret = _conn.Execute(sql, projectItem);

            return ret > 0 ? true : false;
        }

        public bool Insert(List<ProjectItem> projectItem)
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {               

                int ret;

                string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
                        VALUES(@empno, @depno, @yymm, @projno, @itemno, @workHour, @overtime)";

                ret = _conn.Execute(sql, projectItem);


                tran.Commit();

                return ret > 0 ? true : false;
                
            }

            //    int ret;

            //string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
            //            VALUES(@empno, @depno, @yymm, @projno, @itemno, @workhour, @overtime)";

            //ret = _conn.Execute(sql, projectItem);

            //return ret > 0 ? true : false;
        }

        public bool Update(ProjectItem projectItem)
        {
            int ret;

            string sql = @"UPDATE ProjectItem SET workhour=@workHour WHERE 
                        empno=@empno AND yymm=@yymm AND projno=@projno AND itemno=@itemno";

            ret = _conn.Execute(sql, projectItem);

            return ret > 0 ? true : false;
        }

        public bool Upsert(ProjectItem projectItem)
        {            
            int ret;

            //string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour) 
            //            VALUES(@empno, @depno, @yymm, @projno, @itemno, @workhour) 
            //            ON CONFLICT(empno, yymm, projno, itemno) 
            //            DO UPDATE SET workhour=@workhour, overtime=@overtime";

            // Add overtime
            string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
                        VALUES(@empno, @depno, @yymm, @projno, @itemno, @workHour, @overtime) 
                        ON CONFLICT(empno, yymm, projno, itemno) 
                        DO UPDATE SET workhour=@workHour, overtime=@overtime";


            ret = _conn.Execute(sql, projectItem);

            return ret > 0 ? true : false;
        }


        public bool Upsert(List<ProjectItem> projectItem)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {

                int ret;

                string sql = @"INSERT INTO ProjectItem (empno, depno, yymm, projno, itemno, workhour, overtime) 
                        VALUES(@empno, @depno, @yymm, @projno, @itemno, @workHour, @overtime) 
                        ON CONFLICT(empno, yymm, projno, itemno) 
                        DO UPDATE SET workhour=@workHour, overtime=@overtime";

                ret = _conn.Execute(sql, projectItem);


                tran.Commit();

                return ret > 0 ? true : false;

            }

        }

        public bool Delete(List<ProjectItem> projectItems)
        {
            if (_conn.State == 0)
                _conn.Open();

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {
                string deleteProjectItemSql = @"DELETE FROM ProjectItem WHERE empno=@empno AND yymm=@yymm AND projno=@projno;";
                
                try
                {
                    ret = _conn.Execute(deleteProjectItemSql, projectItems, tran);

                    tran.Commit();
                }
                catch
                {
                    ret = 0;
                }

            }

            return ret > 0;
        }

    }
}