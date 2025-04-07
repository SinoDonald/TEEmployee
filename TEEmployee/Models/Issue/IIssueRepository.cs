using System;
using System.Collections.Generic;

namespace TEEmployee.Models.Issue
{
    interface IIssueRepository : IDisposable
    {
        List<Project> GetAllProjects();
        List<Project> GetProjectsByGroupOne(string group_one);
        bool InsertProject(Project project);
        bool DeleteProject(Project project);
        bool InsertIssue(Issue issue);
        bool UpdateIssue(Issue issue);
        bool DeleteIssue(Issue issue);
        bool InsertControlledItem(ControlledItem item);
        bool UpdateControlledItem(ControlledItem item);
        //bool DeleteControlledItems(List<ControlledItem> items);
        bool DeleteControlledItem(ControlledItem item);
        //List<Issue> GetAllIssues();
        //List<Issue> GetIssuesByGroupOne(string group_one);
        //List<ControlledItem> GetControlledItemsByIssueId(int issueId);
    }
}
