using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Web;
using Dapper;
using TEEmployee.Models.Issue;

namespace TEEmployee.Models.IssueV2
{
    public class IssueRepository : IIssueRepository
    {
        private IDbConnection _conn;
        public IssueRepository()
        {
            string issueConnection = ConfigurationManager.ConnectionStrings["IssueV2Connection"].ConnectionString;
            _conn = new SQLiteConnection(issueConnection);
        }

        public List<Project> GetAllProjects()
        {
            var sql = @"
                SELECT p.*, i.*                
                FROM Project AS p
                LEFT JOIN Issue AS i ON p.id = i.project_id;
            "
            ;

            var projectDict = new Dictionary<int, Project>();

            var projects = _conn.Query<Project, Issue, Project>(sql, (project, issue) =>
            {
                if (!projectDict.TryGetValue(project.id, out var currentProject))
                {
                    currentProject = project;
                    projectDict.Add(currentProject.id, currentProject);
                }

                if (issue != null)
                {
                    var currentIssue = currentProject.issues.FirstOrDefault(x => x.id == issue.id);

                    if (currentIssue == null)
                    {
                        currentIssue = issue;
                        currentProject.issues.Add(currentIssue);
                    }

                }

                return currentProject;

            }).Distinct().ToList();

            return projects;

        }

        public List<Project> GetProjectsByGroupOne(string group_one)
        {
            var sql = @"
                SELECT p.*, i.*                  
                FROM Project AS p
                LEFT JOIN Issue AS i ON p.id = i.project_id
                WHERE p.group_one=@group_one;
            "
            ;

            var projectDict = new Dictionary<int, Project>();

            var projects = _conn.Query<Project, Issue, Project>(sql, (project, issue) =>
            {
                if (!projectDict.TryGetValue(project.id, out var currentProject))
                {
                    currentProject = project;
                    projectDict.Add(currentProject.id, currentProject);
                }

                if (issue != null)
                {
                    var currentIssue = currentProject.issues.FirstOrDefault(x => x.id == issue.id);

                    if (currentIssue == null)
                    {
                        currentIssue = issue;
                        currentProject.issues.Add(currentIssue);
                    }

                }

                return currentProject;
            }, new { group_one }).Distinct().ToList();

            return projects;
        }

        public bool InsertProject(Project project)
        {
            int ret = 0;

            string sql = @"
                INSERT INTO Project (name, group_one, project_type) 
                VALUES(@name, @group_one, @project_type)"
            ;

            ret = _conn.Execute(sql, project);
            return ret > 0;
        }

        public bool InsertIssue(Issue issue)
        {
            int ret = 0;

            string sql = @"
                INSERT INTO Issue (project_id, category, importance, content, members, progress, status, registered_date, finished_date) 
                VALUES(@project_id, @category, @importance, @content, @members, @progress, @status, @registered_date, @finished_date)"
            ;

            ret = _conn.Execute(sql, issue);
            return ret > 0;
        }

        public bool UpdateIssue(Issue issue)
        {
            int ret = 0;

            string sql = @"
                UPDATE Issue
                SET category=@category, importance=@importance, content=@content, members=@members, progress=@progress, status=@status, registered_date=@registered_date, finished_date=@finished_date
                WHERE id=@id;
            "
            ;

            ret = _conn.Execute(sql, issue);
            return ret > 0;
        }

        public bool DeleteProject(Project project)
        {
            _conn.Open();

            bool ret = true;
            string deleteProjectSql = @"DELETE FROM Project WHERE id=@id;";
            string deleteIssueSql = @"DELETE FROM Issue WHERE id IN @issueIds;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    var issueIds = project.issues.Select(x => x.id).ToList();
                                        
                    _conn.Execute(deleteIssueSql, new { issueIds });
                    _conn.Execute(deleteProjectSql, project);

                    tran.Commit();
                }
                catch (Exception)
                {
                    ret = false;
                }

            }

            return ret;
        }

        public bool DeleteIssue(Issue issue)
        {
            _conn.Open();

            bool ret = true;
            string deleteIssueSql = @"DELETE FROM Issue WHERE id=@id;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {                    
                    _conn.Execute(deleteIssueSql, issue);
                    tran.Commit();
                }
                catch (Exception)
                {
                    ret = false;
                }

            }

            return ret;
        }

        public List<CustomCategory> GetCustomCategoriesByGroupOne(string group_one)
        {
            string sql = @"SELECT * FROM CustomCategory WHERE group_one=@group_one ";
            var ret = _conn.Query<CustomCategory>(sql, new { group_one }).ToList();

            return ret;
        }

        public bool InsertCustomCategory(CustomCategory category)
        {
            int ret = 0;

            string sql = @"
                INSERT INTO CustomCategory (group_one, name) 
                VALUES(@group_one, @name)"
            ;

            ret = _conn.Execute(sql, category);
            return ret > 0;
        }

        public bool DeleteCustomCategory(CustomCategory category)
        {
            int ret = 0;
            string sql = @"DELETE FROM CustomCategory WHERE id=@id";

            ret = _conn.Execute(sql, category);

            return ret > 0;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}