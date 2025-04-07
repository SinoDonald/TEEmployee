using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Web;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using TEEmployee.Models.TaskLog;
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel;
using TEEmployee.Models.GSchedule;
using TEEmployee.Models.Profession;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TEEmployee.Models.Issue
{
    public class IssueRepository : IIssueRepository
    {
        private IDbConnection _conn;
        public IssueRepository()
        {
            string issueConnection = ConfigurationManager.ConnectionStrings["IssueConnection"].ConnectionString;
            _conn = new SQLiteConnection(issueConnection);
        }

        public List<Project> GetAllProjects()
        {
            var sql = @"
                SELECT p.*, i.*, c.*                   
                FROM Project AS p
                LEFT JOIN Issue AS i ON p.id = i.project_id
                LEFT JOIN ControlledItem as c ON i.id = c.issue_id;
            "
            ;

            var projectDict = new Dictionary<int, Project>();

            var projects = _conn.Query<Project, Issue, ControlledItem, Project>(sql, (project, issue, item) =>
            {
                if (!projectDict.TryGetValue(project.id, out var currentProject))
                {
                    currentProject = project;
                    //currentProject.issues = new List<Issue>();
                    projectDict.Add(currentProject.id, currentProject);
                }
                
                if(issue != null)
                {
                    var currentIssue = currentProject.issues.FirstOrDefault(x => x.id == issue.id);

                    if (currentIssue == null)
                    {
                        currentIssue = issue;
                        currentProject.issues.Add(currentIssue);
                    }

                    if (item != null)
                    {
                        currentIssue.controlledItems.Add(item);
                    }

                }

                return currentProject;
            }/*, splitOn: "DogId,AchievementId"*/).Distinct().ToList();

            return projects;

        }

        public List<Project> GetProjectsByGroupOne(string group_one)
        {
            var sql = @"
                SELECT p.*, i.*, c.*                   
                FROM Project AS p
                LEFT JOIN Issue AS i ON p.id = i.project_id
                LEFT JOIN ControlledItem as c ON i.id = c.issue_id
                WHERE p.group_one=@group_one;
            "
            ;

            var projectDict = new Dictionary<int, Project>();

            var projects = _conn.Query<Project, Issue, ControlledItem, Project>(sql, (project, issue, item) =>
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

                    if (item != null)
                    {
                        currentIssue.controlledItems.Add(item);
                    }

                }

                return currentProject;
            }, new { group_one }).Distinct().ToList();

            return projects;
        }

        public List<Issue> GetAllIssues()
        {
            string sql = @"SELECT * FROM Issue";
            var ret = _conn.Query<Issue>(sql).ToList();

            return ret;
        }

        public List<ControlledItem> GetControlledItemsByIssueId(int issueId)
        {
            string sql = @"SELECT * FROM ControlledItem WHERE issue_id=@issueId ";
            var ret = _conn.Query<ControlledItem>(sql, new { issueId }).ToList();

            return ret;
        }

        public List<Issue> GetIssuesByGroupOne(string group_one)
        {
            string sql = @"SELECT * FROM Issue WHERE group_one=@group_one ";
            var ret = _conn.Query<Issue>(sql, new { group_one }).ToList();

            return ret;
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
                INSERT INTO Issue (project_id, category, custom_order, content, members, progress, status, registered_date) 
                VALUES(@project_id, @category, @custom_order, @content, @members, @progress, @status, @registered_date)"
            ;

            ret = _conn.Execute(sql, issue);
            return ret > 0;
        }

        public bool UpdateIssue(Issue issue)
        {
            int ret = 0;

            string sql = @"
                UPDATE Issue
                SET category=@category, custom_order=@custom_order, content=@content, members=@members, progress=@progress, status=@status, registered_date=@registered_date
                WHERE id=@id;
            "
            ;

            ret = _conn.Execute(sql, issue);
            return ret > 0;
        }        

        public bool InsertControlledItem(ControlledItem item)
        {
            int ret = 0;

            string sql = @"
                INSERT INTO ControlledItem (issue_id, members, todo, deadline) 
                VALUES(@issue_id, @members, @todo, @deadline)"
            ;

            ret = _conn.Execute(sql, item);
            return ret > 0;
        }

        public bool UpdateControlledItem(ControlledItem item)
        {
            int ret = 0;

            string sql = @"
                UPDATE ControlledItem
                SET members=@members, todo=@todo, deadline=@deadline
                WHERE id=@id;
            "
            ;

            ret = _conn.Execute(sql, item);
            return ret > 0;
        }

        public bool DeleteProject(Project project)
        {
            _conn.Open();

            bool ret = true;
            string deleteProjectSql = @"DELETE FROM Project WHERE id=@id;";
            string deleteIssueSql = @"DELETE FROM Issue WHERE id IN @issueIds;";
            string deleteItemSql = @"DELETE FROM ControlledItem WHERE issue_id IN @issueIds;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    var issueIds = project.issues.Select(x => x.id).ToList();

                    _conn.Execute(deleteItemSql, new { issueIds });
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
            string deleteItemSql = @"DELETE FROM ControlledItem WHERE id=@id;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(deleteItemSql, issue.controlledItems);
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

        //public bool DeleteIssues(List<Issue> issues)
        //{
        //    int ret = 0;
        //    string sql = @"DELETE FROM Issue WHERE id=@id";

        //    using (var tran = _conn.BeginTransaction())
        //    {
        //        try
        //        {
        //            foreach (var issue in issues)
        //            {
        //                DeleteControlledItems(issue.controlledItems);
        //            }

        //             _conn.Execute(sql, issues);

        //            tran.Commit();
        //        }
        //        catch (Exception)
        //        {
        //            return false;
        //        }

        //    }


        //}


        //public bool DeleteControlledItems(List<ControlledItem> items)
        //{
        //    int ret = 0;
        //    string sql = @"DELETE FROM ControlledItem WHERE id=@id";

        //    ret = _conn.Execute(sql, items);

        //    return ret > 0;
        //}

        public bool DeleteControlledItem(ControlledItem item)
        {
            int ret = 0;
            string sql = @"DELETE FROM ControlledItem WHERE id=@id";

            ret = _conn.Execute(sql, item);

            return ret > 0;
        }


        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}