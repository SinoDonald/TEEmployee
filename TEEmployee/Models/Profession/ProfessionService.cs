using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{
    public class ProfessionService : IDisposable
    {
        private IProfessionRepository _professionRepository;
        private IUserRepository _userRepository;

        public ProfessionService()
        {
            _professionRepository = new ProfessionRepository();
            _userRepository = new UserRepository();
        }       

        // skill page service

        public List<Skill> GetAllSkillsByRole(string role, string empno)
        {
            var ret = new List<Skill>();

            // get user information
            User user = _userRepository.Get(empno);
            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();

            // group skill
            // check role
            if (groups.Contains(role))
                ret = _professionRepository.GetAllSkillsByRole(new List<string> { role });

            // shared skills
            var sharedSkills = _professionRepository.GetAllSkillsByRole(new List<string> { "shared" });

            ret.AddRange(sharedSkills);

            return ret;
        }

        //public List<Skill> GetAllSkillsByRole(string role, string empno)
        //{
        //    var ret = new List<Skill>();

        //    // get user information
        //    User user = _userRepository.Get(empno);
        //    var managerGroups = GetManagerGroups(empno);
        //    var employeeGroups = GetEmployeeGroups(empno);
        //    var groups = managerGroups.Concat(employeeGroups).Distinct();

        //    // check role
        //    if (groups.Contains(role))
        //        ret = _professionRepository.GetAllSkillsByRole(new List<string> { role });

        //    return ret;
        //}


        public List<Skill> UpsertSkills(List<Skill> skills, string empno)
        {
            var ret = _professionRepository.UpsertSkills(skills);
            //if (!CheckIsAuthorized(schedule, empno))

            return ret;
        }

        public bool DeleteSkills(List<Skill> skills, string empno)
        {
            var ret = _professionRepository.DeleteSkills(skills);
            //if (!CheckIsAuthorized(schedule, empno))
           
            return ret;
        }

        // score page service

        public List<Skill> GetAllScoresByRole(string role, string empno)
        {
            var ret = new List<Skill>();

            // get user information
            User user = _userRepository.Get(empno);
            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();

            // group skills
            // check role
            if (groups.Contains(role))
                ret = _professionRepository.GetAllScoresByRole(new List<string> { role });

            // shared skills
            var sharedSkills = _professionRepository.GetAllScoresByRole(new List<string> { "shared" });
            var groupmembers = this.GetGroupMembers(role).Select(x => x.empno).ToList();
            foreach (var item in sharedSkills)
            {
                item.scores = item.scores?.Where(x => groupmembers.Contains(x.empno)).ToList();
            }

            ret.AddRange(sharedSkills);
            ret = ret.OrderBy(x => x.custom_order).ToList();

            return ret;
        }

        public bool DownloadProfessionDB()
        {
            bool ret = _professionRepository.DownloadProfessionDB();
            return ret;
        }

        //public List<Skill> GetAllScoresByRole(string role, string empno)
        //{
        //    var ret = new List<Skill>();

        //    // get user information
        //    User user = _userRepository.Get(empno);
        //    var managerGroups = GetManagerGroups(empno);
        //    var employeeGroups = GetEmployeeGroups(empno);
        //    var groups = managerGroups.Concat(employeeGroups).Distinct();

        //    // check role
        //    if (groups.Contains(role))
        //        ret = _professionRepository.GetAllScoresByRole(new List<string> { role });

        //    return ret;
        //}


        public bool UpsertScores(List<Score> scores, string empno)
        {
            var ret = _professionRepository.UpsertScores(scores);
            //if (!CheckIsAuthorized(schedule, empno))

            return ret;
        }


        // shared service

        public dynamic GetAuthorization(string empno)
        {
            User user = _userRepository.Get(empno);

            dynamic authorization = new JObject(); 
            authorization.User = JObject.FromObject(user);
            authorization.GroupAuthorities = new JArray();

            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();

            foreach (var group in groups)
            {
                dynamic groupAuthority = new JObject();
                groupAuthority.GroupName = group;
                //groupAuthority.Members = new List<string>();
                groupAuthority.Members = new JArray();
                groupAuthority.Editable = false;

                if (managerGroups.Contains(group))
                {
                    //groupAuthority.Members = JArray.FromObject(GetGroupMembers(group).Select(x => x.name).ToList());
                    groupAuthority.Members = JArray.FromObject(GetGroupMembers(group).ToList());
                    groupAuthority.Editable = true;
                }
                else
                {
                    //groupAuthority.Members.Add(authorization.User.name);
                    groupAuthority.Members.Add(JObject.FromObject(user));
                }

                authorization.GroupAuthorities.Add(groupAuthority);
            }

            return JsonConvert.SerializeObject(authorization);
        }


        public List<Skill> GetAll(string empno)
        {            
            return _professionRepository.GetAll();
        }


        public List<Personal> GetPersonal(string empno)
        {
            return _professionRepository.GetPersonal(empno);
        }

        public bool UpsertPersonal(List<Personal> personals, string empno)
        {
            var ret = _professionRepository.UpsertPersonal(personals);
            //if (!CheckIsAuthorized(schedule, empno))

            return ret;
        }

        public bool DeletePersonal(List<Personal> personals, string empno)
        {
            var ret = _professionRepository.DeletePersonal(personals);
            //if (!CheckIsAuthorized(schedule, empno))

            return ret;
        }

        // private method

        private List<string> GetManagerGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = (_userRepository as UserRepository).GetSubGroups(empno);

            return groups;
        }

        private List<string> GetEmployeeGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = new List<string>();

            // add sub group if as a member
            if (!String.IsNullOrEmpty(user.group_one))
                groups.Add(user.group_one);

            if (!String.IsNullOrEmpty(user.group_two))
                groups.Add(user.group_two);

            if (!String.IsNullOrEmpty(user.group_three))
                groups.Add(user.group_three);

            // remove duplicates
            groups = groups.Distinct().ToList();

            return groups;
        }
        
        public List<User> GetGroupMembers(string group)
        {
            List<User> users = _userRepository.GetAll();
            return users.Where(x => x.group_one == group || x.group_two == group || x.group_three == group).ToList();
        }

        public void Dispose()
        {
            _professionRepository.Dispose();
            _userRepository.Dispose();
        }

        //public dynamic GetAllSkills()
        //{
        //    dynamic ret = new JObject();

        //    var skills = _professionRepository.GetAll();

        //    //ret.skills = skills;
        //    ret.group = "3DBBQ";


        //    //dynamic dyna = new JObject();
        //    //dyna.d = new DateTime(2015, 1, 1);
        //    //dyna.n = 32767;
        //    //dyna.s = "darkthread";
        //    //dyna.a = new JArray(1, 2, 3, 4, 5);
        //    //dyna.k = JArray.FromObject(skills);

        //    var dyna = new
        //    {
        //        d = new DateTime(2015, 1, 1),
        //        n = 32767,
        //        s = "darkthread",
        //        a = new List<int> { 1,2,3,4,5},
        //        k = skills
        //    };

        //    JObject o = JObject.FromObject(dyna);

        //    //dynamic dyna = new JObject();
        //    //dyna.d = 
        //    //dyna.n = 32767;
        //    //dyna.s = "darkthread";
        //    //dyna.a = new JArray(1, 2, 3, 4, 5);
        //    //dyna.k = JArray.FromObject(skills);

        //    return JsonConvert.SerializeObject(dyna);
        //    //return dyna;
        //    //return skills;
        //}
    }
}