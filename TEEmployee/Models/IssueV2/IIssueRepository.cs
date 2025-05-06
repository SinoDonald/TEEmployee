using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEEmployee.Models.Issue;

namespace TEEmployee.Models.IssueV2
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
        List<CustomCategory> GetCustomCategoriesByGroupOne(string group_one);
        bool InsertCustomCategory(CustomCategory category);
        bool DeleteCustomCategory(CustomCategory category);
    }
}
