using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.AgentModel;
using TEEmployee.Models.TaskLog;

namespace TEEmployee.Models.GSchedule
{
    public class GScheduleService : IDisposable
    {
        private IGScheduleRepository _scheduleRepository;
        private IUserRepository _userRepository;
        private IProjectItemRepository _projectItemRepository;
        private IAgentRepository _agentRepository;

        public GScheduleService()
        {
            _scheduleRepository = new GScheduleRepository();
            _userRepository = new UserRepository();
            _projectItemRepository = new ProjectItemRepository();
            _agentRepository = new AgentRepository();
        }

        /// <summary>
        /// 取得所有行事曆項目。
        /// </summary>
        /// <returns>包含所有行事曆項目的列舉。</returns>
        public List<GroupSchedule> GetAllSchedules()
        {
            var groupSchedules = new List<GroupSchedule>();
            var schedules = _scheduleRepository.GetAll();

            groupSchedules = schedules.Where(x => x.type == 1).Select(x => new GroupSchedule() { Group = x }).ToList();
            var detailSchedules = schedules.Where(x => x.type == 2);
            var personalSchedules = schedules.Where(x => x.type == 3);


            foreach (var groupSchedule in groupSchedules)
            {
                groupSchedule.Details = detailSchedules
                    .Where(x => x.parent_id == groupSchedule.Group.id)
                    .Select(x => new DetailSchedule() { Detail = x })
                    .ToList();
            }

            foreach (var groupSchedule in groupSchedules)
            {
                foreach (var detailSchedule in groupSchedule.Details)
                {
                    detailSchedule.Personals = personalSchedules
                        .Where(x => x.parent_id == detailSchedule.Detail.id)
                        .ToList();
                }
            }


            return groupSchedules;
        }

        /// <summary>
        /// 取得所有權限下群組之行事曆項目。
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>包含所有行事曆項目的列舉。</returns>
        public List<GroupSchedule> GetAllGroupSchedules(string empno)
        {
            // get user information
            User user = _userRepository.Get(empno);

            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();
            var projectItems = _projectItemRepository.GetAll();

            var allGroupSchedules = new List<GroupSchedule>();

            foreach (var group in groups)
            {
                var schedules = _scheduleRepository.GetAllGroupSchedules(group);

                var groupSchedules = new List<GroupSchedule>();

                groupSchedules = schedules.Where(x => x.type == 1).Select(x => new GroupSchedule() { Group = x }).ToList();
                var detailSchedules = schedules.Where(x => x.type == 2);
                var personalSchedules = schedules.Where(x => x.type == 3);

                var groupMembers = GetGroupMembers(group);
                var groupMemberEmpnos = groupMembers.Select(x => x.empno);

                foreach (var groupSchedule in groupSchedules)
                {
                    groupSchedule.Details = detailSchedules
                        .Where(x => x.parent_id == groupSchedule.Group.id)
                        .Select(x => new DetailSchedule() { Detail = x })
                        .ToList();


                    foreach (var detailSchedule in groupSchedule.Details)
                    {
                        // add personal empno data if just as a member

                        if (managerGroups.Contains(group))
                        {
                            detailSchedule.Personals = personalSchedules
                            .Where(x => x.parent_id == detailSchedule.Detail.id)
                            .ToList();
                        }
                        else
                        {
                            detailSchedule.Personals = personalSchedules
                            .Where(x => x.empno == empno)
                            .Where(x => x.parent_id == detailSchedule.Detail.id)
                            .ToList();
                        }
                    }

                    // project man hours

                    groupSchedule.ManHours = new List<ManHour>();

                    var filteredProjectItems = projectItems.Where(x => (x.projno == groupSchedule.Group.projno) && (groupMemberEmpnos.Contains(x.empno)));

                    foreach (var item in filteredProjectItems)
                    {
                        string name = groupMembers.Where(x => x.empno == item.empno).Select(x => x.name).FirstOrDefault();
                        groupSchedule.ManHours.Add(new ManHour() { name = name, empno = item.empno, hours = item.workHour + item.overtime, yymm = item.yymm });
                    }

                    // authority

                }


                allGroupSchedules.AddRange(groupSchedules);
            }

            //allGroupSchedules = allGroupSchedules.OrderByDescending(x => x.Group).ToList();

            List<string> engOrder = new List<string> { "D", "C", "E", "B", "Z", "N" };

            allGroupSchedules = allGroupSchedules
                .OrderByDescending(x =>
                {

                    int engIdx = 0;

                    if (x.Group.projno?.Length == 5)
                    {
                        engIdx = engOrder.IndexOf(x.Group.projno?.Substring(4));
                    }

                    return engIdx;
                })
                .ThenByDescending(x =>
                {
                    string num = "";

                    if (x.Group.projno?.Length == 5)
                    {
                        num = x.Group.projno?.Substring(0, 4);
                    }

                    return num;
                    
                    })
                .ToList();


            return allGroupSchedules;
        }

        /// <summary>
        /// 更新行事曆項目。
        /// </summary>
        /// <param name="schedule">行事曆項目</param>
        /// <param name="empno">員工編號</param>
        /// <returns>更新的行事曆項目</returns>
        public Schedule UpdateSchedule(Schedule schedule, string empno)
        {
            //if (!CheckIsAuthorized(schedule, empno))
            //    return null;

            Schedule ret = _scheduleRepository.Update(schedule);
            return ret;
        }

        /// <summary>
        /// 新增行事曆項目。
        /// </summary>
        /// <param name="schedule">行事曆項目</param>
        /// <param name="empno">員工編號</param>
        /// <returns>新增的行事曆項目</returns>
        public Schedule InsertSchedule(Schedule schedule, string empno)
        {
            //if (!CheckIsAuthorized(schedule, empno))
            //    return null;

            Schedule ret = _scheduleRepository.Insert(schedule);
            return ret;
        }

        /// <summary>
        /// 刪除行事曆項目。
        /// </summary>
        /// <param name="schedule">行事曆項目</param>
        /// <param name="empno">員工編號</param>
        /// <returns>是否刪除成功</returns>
        public bool DeleteSchedule(Schedule schedule, string empno)
        {
            //if (!CheckIsAuthorized(schedule, empno))
            //    return false;

            List<Schedule> deletedSchedules = new List<Schedule>();

            List<Schedule> schedules = _scheduleRepository.GetAllGroupSchedules(schedule.role);
            schedules = schedules.Where(x => x.parent_id == schedule.id).ToList();

            if (schedule.type == 1 || schedule.type == 4) // group and future main schedule
            {
                deletedSchedules.AddRange(schedules.Where(x => x.parent_id == schedule.id).ToList());

                List<int> parentIds = deletedSchedules.Select(x => x.id).ToList();
                deletedSchedules.AddRange(schedules.Where(x => parentIds.Contains(x.id)).ToList());
            }
            else if (schedule.type == 2)
            {
                deletedSchedules.AddRange(schedules.Where(x => x.parent_id == schedule.id).ToList());
            }

            deletedSchedules.Add(schedule);

            bool ret = _scheduleRepository.Delete(deletedSchedules);

            return ret;
        }

        /// <summary>
        /// 更新所有行事曆項目每月進度。
        /// </summary>
        /// <returns>是否更新成功</returns>
        /// <remarks>將本月進度移至上月進度，並記錄在歷史進度紀錄</remarks>
        public bool UpdateAllPercentComplete()
        {
            var schedules = _scheduleRepository.GetAll()
                .Where(x => x.type < 4)
                .Where(x => x.percent_complete != 100).ToList();

            foreach (var schedule in schedules)
            {
                schedule.last_percent_complete = schedule.percent_complete;
            }

            return _scheduleRepository.Update(schedules);
        }

        /// <summary>
        /// 根據權限，取得權限下員工資料
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>員工資料動態物件</returns>
        /// 

        public Authorization GetAuthorization(string page_id, string empno)
        {
            Authorization authorization = new Authorization() { User = _userRepository.Get(empno), GroupAuthorities = new List<GroupAuthority>() };

            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();

            if (!string.IsNullOrEmpty(page_id))
            {
                var agents = _agentRepository.GetPageAgentsByUser(empno, page_id);

                if (page_id == "0" || page_id == "1")
                {
                    foreach (var agent in agents)
                    {
                        managerGroups = managerGroups.Concat(GetManagerGroups(agent.manno)).Distinct().ToList();
                        groups = groups.Concat(GetManagerGroups(agent.manno)).Distinct();
                    }
                }

                if (page_id == "2")
                {
                    foreach (var agent in agents)
                    {
                        var manager = _userRepository.Get(agent.manno);

                        if (manager.group_manager)
                            authorization.User.group_manager = true;

                    }
                }
            }
            

            foreach (var group in groups)
            {
                GroupAuthority groupAuthority = new GroupAuthority() { GroupName = group, Members = new List<string>() };

                if (managerGroups.Contains(group))
                {
                    groupAuthority.Members = GetGroupMembers(group).Select(x => x.name).ToList();
                    groupAuthority.Editable = true;
                }
                else
                    groupAuthority.Members.Add(authorization.User.name);

                authorization.GroupAuthorities.Add(groupAuthority);
            }

            //AddCustomAgent(authorization);

            return authorization;
        }



        //public Authorization GetAuthorization(string empno)
        //{
        //    Authorization authorization = new Authorization() { User = _userRepository.Get(empno), GroupAuthorities = new List<GroupAuthority>() };

        //    var managerGroups = GetManagerGroups(empno);
        //    var employeeGroups = GetEmployeeGroups(empno);
        //    var groups = managerGroups.Concat(employeeGroups).Distinct();

        //    foreach (var group in groups)
        //    {
        //        GroupAuthority groupAuthority = new GroupAuthority() { GroupName = group, Members = new List<string>() };

        //        if (managerGroups.Contains(group))
        //        {
        //            groupAuthority.Members = GetGroupMembers(group).Select(x => x.name).ToList();
        //            groupAuthority.Editable = true;
        //        }
        //        else
        //            groupAuthority.Members.Add(authorization.User.name);

        //        authorization.GroupAuthorities.Add(groupAuthority);
        //    }

        //    AddCustomAgent(authorization);

        //    return authorization;
        //}

        /// <summary>
        /// 取得群組之未來數位轉型行事曆
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>未來數位轉型行事曆項目列舉</returns>
        public List<GroupSchedule> GetAllFutures(string empno)
        {
            // get user information
            User user = _userRepository.Get(empno);

            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();

            var allGroupFutureSchedules = new List<GroupSchedule>();

            foreach (var group in groups)
            {
                var schedules = _scheduleRepository.GetAllGroupSchedules(group);

                var groupFutureSchedules = new List<GroupSchedule>();

                groupFutureSchedules = schedules.Where(x => x.type == 4).Select(x => new GroupSchedule() { Group = x }).ToList();
                var detailFutureSchedules = schedules.Where(x => x.type == 5);


                var groupMembers = GetGroupMembers(group);
                var groupMemberEmpnos = groupMembers.Select(x => x.empno);

                foreach (var groupFutureSchedule in groupFutureSchedules)
                {
                    groupFutureSchedule.Details = detailFutureSchedules
                        .Where(x => x.parent_id == groupFutureSchedule.Group.id)
                        .Select(x => new DetailSchedule() { Detail = x })
                        .ToList();
                }


                allGroupFutureSchedules.AddRange(groupFutureSchedules);
            }

            return allGroupFutureSchedules;
        }

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

        private bool CheckIsAuthorized(Schedule schedule, string empno)
        {
            // personal
            if (schedule.type == 3)
            {
                return schedule.empno == empno;
            }
            // group, detail, future
            else
            {
                return GetManagerGroups(empno).Contains(schedule.role);
            }

        }

        public List<User> GetGroupMembers(string group)
        {
            List<User> users = _userRepository.GetAll();
            return users.Where(x => x.group_one == group || x.group_two == group || x.group_three == group).ToList();
        }

        private void AddCustomAgent(Authorization auth)
        {
            if (auth.User.empno == "8485")
            {
                var group = auth.GroupAuthorities.Find(x => x.GroupName == "營運規劃組");

                if (group != null)
                {
                    group.Editable = true;
                    group.Members = GetGroupMembers(group.GroupName).Select(x => x.name).ToList();
                }
                    
            }
        }


        public class GroupSchedule
        {
            public Schedule Group { get; set; }
            public List<DetailSchedule> Details { get; set; }
            public List<ManHour> ManHours { get; set; }
        }

        public class DetailSchedule
        {
            public Schedule Detail { get; set; }
            public List<Schedule> Personals { get; set; }
        }

        public class Authorization
        {
            public User User { get; set; }
            public List<GroupAuthority> GroupAuthorities { get; set; }
        }

        public class GroupAuthority
        {
            public string GroupName { get; set; }
            public bool Editable { get; set; }
            public List<string> Members { get; set; }
        }

        public class ManHour
        {
            public string name { get; set; }
            public string empno { get; set; }
            public int hours { get; set; }
            public string yymm { get; set; }
        }

        /// <summary>
        /// 取得所有計畫行事曆項目
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>包含所有計畫行事曆項目的列舉</returns>
        public List<ProjectSchedule> GetAllProjectSchedules(string empno)
        {
            var projectSchedules = _scheduleRepository.GetAllProjectSchedules();
            return projectSchedules;
        }

        /// <summary>
        /// 新增計畫行事曆項目
        /// </summary>
        /// <param name="projectSchedule">計畫行事曆</param>
        /// <param name="empno">員工編號</param>
        /// <returns>是否新增成功</returns>
        public bool InsertProjectSchedule(ProjectSchedule projectSchedule, string empno)
        {
            var ret = _scheduleRepository.Insert(projectSchedule);
            return ret;
        }

        /// <summary>
        /// 刪除計畫行事曆項目
        /// </summary>
        /// <param name="projectSchedule">計畫行事曆</param>
        /// <param name="empno">員工編號</param>
        /// <returns>是否刪除成功</returns>
        public bool DeleteProjectSchedule(ProjectSchedule projectSchedule, string empno)
        {
            var ret = _scheduleRepository.Delete(projectSchedule);

            try
            {
                string _appData = HttpContext.Current.Server.MapPath("~/Content/assets/img/project");
                string fn = Path.Combine(_appData, projectSchedule.filepath);
                File.Delete(fn);
            }
            catch
            {
                
            }

            return ret;
        }

        /// <summary>
        /// 上傳計畫行事曆項目附件
        /// </summary>
        /// <param name="file">附件</param>
        /// <param name="projectSchedule">計畫行事曆</param>
        /// <returns>是否上傳成功</returns>
        public bool UploadProjectSchedule(HttpPostedFileBase file, ProjectSchedule projectSchedule)
        {
            string _appData = HttpContext.Current.Server.MapPath("~/Content/assets/img/project");

            // delete origin image
            try
            {
                var originSchedule = _scheduleRepository.GetAllProjectSchedules().Where(x => x.projno == projectSchedule.projno).First();
                string fn = Path.Combine(_appData, originSchedule.filepath);
                File.Delete(fn);
            }
            catch
            {

            }

            // upload new image (filepath = projno + datetime)
            try
            {
                string filepath = "";

                if (file != null)
                {
                    string extension = Path.GetExtension(file.FileName);
                    string datetime = DateTime.Now.ToString("yyyyMMddhhmmss");
                    filepath = $"{projectSchedule.projno}_{datetime}{extension}";
                    string fn = Path.Combine(_appData, filepath);
                    file.SaveAs(fn);
                }                
                
                projectSchedule.filepath = filepath;
                return _scheduleRepository.Update(projectSchedule);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 取得群組
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public List<string> GetGroupList(string view, string empno)
        {
            var ret = _userRepository.GetGroupList(view, empno);
            return ret;
        }
        /// <summary>
        /// 取得群組同仁
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public List<string> GetGroupUsers(string selectedGroup, string empno)
        {
            var ret = _userRepository.GetGroupUsers(selectedGroup, empno);
            return ret;
        }
        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public User Get(string page_id, string empno)
        {
            var user = _userRepository.Get(empno);

            if (page_id == "3")
            {
                var agents = _agentRepository.GetPageAgentsByUser(empno, page_id);

                foreach (var agent in agents)
                {
                    var manager = _userRepository.Get(agent.manno);

                    if (manager.group_manager)
                        user.group_manager = true;

                }
            }
            
            return user;
        }
        /// <summary>
        /// 取得年份
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public List<string> GetYears(string view)
        {
            List<string> ret = _scheduleRepository.GetYears(view);
            return ret;
        }
        /// <summary>
        /// 上傳群組規劃PDF
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public string UploadPDFFile(HttpPostedFileBase file, string view, string year, string empno)
        {
            string ret = _scheduleRepository.UploadPDFFile(file, view, year, empno);
            return ret;
        }
        /// <summary>
        /// 上傳個人規劃PDF
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public string ImportPDFFile(HttpPostedFileBase file, string empno)
        {
            string ret = _scheduleRepository.ImportPDFFile(file, empno);
            return ret;
        }
        /// <summary>
        /// 取得PDF
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public string GetPDF(string view, string year, string group, string userName)
        {
            string ret = _scheduleRepository.GetPDF(view, year, group, userName);
            return ret;
        }
        /// <summary>
        /// 下載PDF
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public byte[] DownloadFile(string pdfPath)
        {
            try
            {
                var fileBytes = File.ReadAllBytes(pdfPath);
                return fileBytes;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 取得主管回饋
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public List<Planning> GetResponse(string view, string year, string group, string empno, string name)
        {
            List<Planning> ret = _scheduleRepository.GetResponse(view, year, group, empno, name);
            return ret;
        }
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="view"></param>
        /// <param name="empno"></param>
        /// <returns></returns>
        public bool SaveResponse(string view, string year, string group, string manager_id, string name, List<Planning> response)
        {
            bool ret = _scheduleRepository.SaveResponse(view, year, group, manager_id, name, response);
            return ret;
        }
        /// <summary>
        /// 刪除群組規劃
        /// </summary>
        /// <returns></returns>
        public bool DeleteGroupPlan()
        {
            var ret = _scheduleRepository.DeleteGroupPlan();
            return ret;
        }
        /// <summary>
        /// 刪除個人規劃
        /// </summary>
        /// <returns></returns>
        public bool DeletePersonalPlan()
        {
            var ret = _scheduleRepository.DeletePersonalPlan();
            return ret;
        }

        public bool DeleteAll()
        {
            var ret = _scheduleRepository.DeleteAll();
            return ret;
        }

        public List<Agent> GetAllAgents(string manno)
        {
            var agents = _agentRepository.GetAllAgents(manno);
            agents.ForEach(x => x.name = _userRepository.Get(x.empno).name);
            return agents;
        }

        public bool InsertAgent(Agent agent, string manno)
        {
            agent.manno = manno;
            agent.empno = _userRepository.GetAll().Find(x => x.name == agent.name).empno;
            var ret = _agentRepository.InsertAgent(agent);
            return ret;
        }

        public bool UpdateAgent(Agent agent, string manno)
        {
            agent.manno = manno;
            agent.empno = _userRepository.GetAll().Find(x => x.name == agent.name).empno;
            var ret = _agentRepository.UpdateAgent(agent);
            return ret;
        }

        public bool DeleteAgent(Agent agent, string manno)
        {
            agent.manno = manno;
            agent.empno = _userRepository.GetAll().Find(x => x.name == agent.name).empno;
            var ret = _agentRepository.DeleteAgent(agent);
            return ret;
        }

        public void Dispose()
        {
            _scheduleRepository.Dispose();
            _userRepository.Dispose();
            _projectItemRepository.Dispose();
            _agentRepository.Dispose();
        }
    }

}