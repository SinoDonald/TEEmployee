using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.TaskLog;

namespace TEEmployee.Models.GSchedule
{
    public class GScheduleService : IDisposable
    {
        private IGScheduleRepository _scheduleRepository;
        private IUserRepository _userRepository;
        private IProjectItemRepository _projectItemRepository;

        public GScheduleService()
        {
            _scheduleRepository = new GScheduleRepository();
            _userRepository = new UserRepository();
            _projectItemRepository = new ProjectItemRepository();
        }

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

        public Schedule UpdateSchedule(Schedule schedule, string empno)
        {
            if (!CheckIsAuthorized(schedule, empno))
                return null;

            Schedule ret = _scheduleRepository.Update(schedule);
            return ret;
        }

        public Schedule InsertSchedule(Schedule schedule, string empno)
        {
            if (!CheckIsAuthorized(schedule, empno))
                return null;

            Schedule ret = _scheduleRepository.Insert(schedule);
            return ret;
        }

        public bool DeleteSchedule(Schedule schedule, string empno)
        {
            if (!CheckIsAuthorized(schedule, empno))
                return false;

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

        public Authorization GetAuthorization(string empno)
        {
            Authorization authorization = new Authorization() { User = _userRepository.Get(empno), GroupAuthorities = new List<GroupAuthority>() };

            var managerGroups = GetManagerGroups(empno);
            var employeeGroups = GetEmployeeGroups(empno);
            var groups = managerGroups.Concat(employeeGroups).Distinct();

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

            return authorization;
        }

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

        public List<ProjectSchedule> GetAllProjectSchedules(string empno)
        {
            var projectSchedules = _scheduleRepository.GetAllProjectSchedules();
            return projectSchedules;
        }

        public bool InsertProjectSchedule(ProjectSchedule projectSchedule, string empno)
        {
            var ret = _scheduleRepository.Insert(projectSchedule);
            return ret;
        }

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

        // 取得群組
        public List<string> GetGroupList(string view, string empno)
        {
            var ret = _userRepository.GetGroupList(view, empno);
            return ret;
        }

        // 取得群組同仁
        public List<string> GetGroupUsers(string selectedGroup, string empno)
        {
            var ret = _userRepository.GetGroupUsers(selectedGroup, empno);
            return ret;
        }

        // 取得使用者資訊
        public User Get(string empno)
        {
            var user = _userRepository.Get(empno);
            return user;
        }

        // 取得年份
        public List<string> GetYears(string view)
        {
            List<string> ret = _scheduleRepository.GetYears(view);
            return ret;
        }

        // 上傳群組規劃PDF
        public bool UploadPDFFile(HttpPostedFileBase file, string view, string empno)
        {
            bool ret = _scheduleRepository.UploadPDFFile(file, view, empno);
            return ret;
        }

        // 取得PDF
        public string GetPDF(string view, string year, string group, string userName)
        {
            string ret = _scheduleRepository.GetPDF(view, year, group, userName);
            return ret;
        }

        // 儲存回覆
        public bool SaveResponse(string empno, string userName, string comment)
        {
            bool ret = _scheduleRepository.SaveResponse(empno, userName, comment);
            return ret;
        }

        public void Dispose()
        {
            _scheduleRepository.Dispose();
            _userRepository.Dispose();
            _projectItemRepository.Dispose();
        }
    }

}