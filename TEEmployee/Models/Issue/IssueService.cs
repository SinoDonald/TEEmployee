using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TEEmployee.Models.TaskLog;
using TEEmployee.Models.Training;

namespace TEEmployee.Models.Issue
{
    public class IssueService : IDisposable
    {
        private IIssueRepository _issueRepository;
        private IUserRepository _userRepository;

        public IssueService()
        {
            _issueRepository = new IssueRepository();
            _userRepository = new UserRepository();
        }

        public List<Project> GetAllProjects()
        {
            var ret = _issueRepository.GetAllProjects();
            return ret;
        }

        public List<Project> GetProjectsByGroupOne(string group_one)
        {
            var ret = _issueRepository.GetProjectsByGroupOne(group_one);
            return ret;
        }

        public bool CreateProject(Project project)
        {
            var ret = _issueRepository.InsertProject(project);
            return ret;
        }

        public bool DeleteProject(Project project)
        {
            var ret = _issueRepository.DeleteProject(project);
            return ret;
        }

        public bool CreateIssue(Issue issue)
        {
            var ret = _issueRepository.InsertIssue(issue);
            return ret;
        }

        public bool UpdateIssue(Issue issue)
        {
            var ret = _issueRepository.UpdateIssue(issue);
            return ret;
        }

        public bool DeleteIssue(Issue issue)
        {
            var ret = _issueRepository.DeleteIssue(issue);
            return ret;
        }

        public bool CreateControlledItem(ControlledItem item)
        {
            var ret = _issueRepository.InsertControlledItem(item);
            return ret;
        }

        public bool UpdateControlledItem(ControlledItem item)
        {
            var ret = _issueRepository.UpdateControlledItem(item);
            return ret;
        }

        public bool DeleteControlledItem(ControlledItem item)
        {
            var ret = _issueRepository.DeleteControlledItem(item);
            return ret;
        }

        public dynamic GetAuthorization(string empno)
        {
            User user = _userRepository.Get(empno);
            List<User> users = _userRepository.GetAll();
            dynamic authorization = new JObject();
            authorization.Users = new JArray();

            if (user.department_manager)
            {

            }
            else if (user.group_manager)
            {
                users = users.Where(x => x.group == user.group).ToList();
            }            
            else
            {
                users = users.Where(x => x.group_one == user.group_one).ToList();
            }

            users = users.Where(x => !string.IsNullOrEmpty(x.group_one)).ToList();

            foreach (var item in users)
            {
                dynamic userObj = JObject.FromObject(item);
                authorization.Users.Add(userObj);
            }

            authorization.User = JObject.FromObject(user);

            return JsonConvert.SerializeObject(authorization);
        }

        public void Dispose()
        {
            _issueRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}