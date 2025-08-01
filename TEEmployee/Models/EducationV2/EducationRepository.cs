using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Web;
using TEEmployee.Models.Issue;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using TEEmployee.Models.IssueV2;

namespace TEEmployee.Models.EducationV2
{
    public class EducationRepository : IEducationRepository
    {
        private IDbConnection _conn;
        public EducationRepository()
        {
            string educationConnection = ConfigurationManager.ConnectionStrings["Education2024Connection"].ConnectionString;
            _conn = new SQLiteConnection(educationConnection);
        }

        public List<Content> GetAllContents()
        {
            List<Content> ret;

            string sql = @"SELECT * FROM Content AS c LEFT JOIN ContentExtra AS e ON c.id = e.content_id";
            ret = _conn.Query<Content>(sql).ToList();

            return ret;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        public bool InsertContentExtras(List<Content> contentExtras)
        {
            _conn.Open();

            string deleteContentExtrasSql = @"DELETE FROM ContentExtra;";
            string InsertContentExtrasSql = @"
                INSERT INTO ContentExtra (content_id, course_title, content_type, content_code, content_scope, digitalized, course_group, course_group_one) 
                VALUES(@id, @course_title, @content_type, @content_code, @content_scope, @digitalized, @course_group, @course_group_one)
            "
            ;

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(deleteContentExtrasSql);
                    _conn.Execute(InsertContentExtrasSql, contentExtras);

                    tran.Commit();
                }
                catch (Exception)
                {
                    return false;
                }

            }

            return true;

        }

        public bool UpsertAssignments(List<Assignment> assignments)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                int ret = 0;

                //string sql = @"INSERT INTO Assignment (empno, content_id, assigned, assigner) 
                //        VALUES(@empno, @content_id, @assigned, @assigner) 
                //        ON CONFLICT(empno, content_id, assigner) 
                //        DO UPDATE SET assigned=@assigned";

                string sql = @"INSERT INTO Assignment (empno, content_id, assigner) 
                        VALUES(@empno, @content_id, @assigner) 
                        ON CONFLICT(empno, content_id, assigner) 
                        DO NOTHING";

                ret = _conn.Execute(sql, assignments);

                tran.Commit();

                return ret > 0;

            }
        }

        public bool DeleteAssignments(List<Assignment> assignments)
        {
            // Had better using transaction when INSERT, UPDATE, DELTE on List of objects
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"
                    DELETE FROM Assignment
                    WHERE empno=@empno AND content_id=@content_id AND assigner=@assigner    
                "
                ;

                int ret = _conn.Execute(sql, assignments);
                
                tran.Commit();

                return ret > 0;
            }

        }

        public List<Assignment> GetAssignmentsByAssigner(string assigner)
        {
            string sql = @"SELECT * FROM Assignment WHERE assigner=@assigner ";
            var ret = _conn.Query<Assignment>(sql, new { assigner }).ToList();

            return ret;
        }

        public List<Assignment> GetAssignmentsWithRecordByUsers(List<string> empnos)
        {
            
            var sql = @"
                SELECT a.*, r.*
                FROM Assignment AS a
                LEFT JOIN Record AS r ON a.empno = r.empno AND a.content_id = r.mediaId
                WHERE a.empno IN @empnos
            "
            ;

            var assignments = _conn.Query<Assignment, Record, Assignment>(sql, (assignment, record) =>
            {
                assignment.record = record;
                return assignment;
            }, new { empnos }).ToList();

            return assignments;

            //var projects = _conn.Query<Project, Issue, ControlledItem, Project>(sql, (project, issue, item) =>
            //{
            //    if (!projectDict.TryGetValue(project.id, out var currentProject))
            //    {
            //        currentProject = project;
            //        projectDict.Add(currentProject.id, currentProject);
            //    }

            //    if (issue != null)
            //    {
            //        var currentIssue = currentProject.issues.FirstOrDefault(x => x.id == issue.id);

            //        if (currentIssue == null)
            //        {
            //            currentIssue = issue;
            //            currentProject.issues.Add(currentIssue);
            //        }

            //        if (item != null)
            //        {
            //            currentIssue.controlledItems.Add(item);
            //        }

            //    }

            //    return currentProject;
            //}, new { group_one }).Distinct().ToList();


            //var dogs = connection.Query<Dog, DogExtra, Dog>(
            //    sql,
            //    (dog, dogExtra) =>
            //    {
            //        dog.DogExtra = dogExtra;
            //        return dog;
            //    },
            //    new { Breeds = breeds },
            //    splitOn: "DogId"  // or whatever the first column in DogExtra is
            //).ToList();

            //throw new NotImplementedException();
        }
    }
}