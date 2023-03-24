using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TEEmployee.Models.TaskLog;

namespace TEEmployee.Models.Schedule
{
    public class ScheduleService : IDisposable
    {
        private IScheduleRepository _scheduleRepository;
        private IUserRepository _userRepository;
        private IProjectItemRepository _projectItemRepository;

        public ScheduleService()
        {
            _scheduleRepository = new ScheduleRepository();
            _userRepository = new UserRepository();
            _projectItemRepository = new ProjectItemRepository();
        }

        public List<Schedule> GetAllSchedules()
        {
            var schedules = _scheduleRepository.GetAll();
            return schedules;
        }

        public ScheduleResult GetAllOwnedSchedules(string empno)
        {
            // get user information
            User user = _userRepository.Get(empno);
            bool isManager = user.department_manager || user.group_manager || user.group_one_manager || user.group_two_manager || user.group_three_manager;
            
            // setup
            ScheduleResult scheduleResult = new ScheduleResult();
            List<User> employees = GetAllEmployeesByRole(empno);
            List<string> empnos = employees.Select(x => x.empno).ToList();

            // individual
            scheduleResult.Individual = _scheduleRepository.GetAllIndividualSchedules(empno);
            
            // group

            if (isManager)
            {
                List<string> groupNames = (_userRepository as UserRepository).GetSubGroups(empno);
                scheduleResult.Group = new List<Schedule>();

                foreach(var group in groupNames)
                {
                    scheduleResult.Group.AddRange(_scheduleRepository.GetAllGroupSchedules(group));
                }

            }

            // add project man hours data 
            if (isManager)
            {
                scheduleResult.Projects = new List<ProjectManHours>();
                var projectItems = _projectItemRepository.GetAll();

                foreach (var schedule in scheduleResult.Group)
                {
                    ProjectManHours projectManHours = new ProjectManHours();
                    projectManHours.projno = schedule.projno;
                    projectManHours.manHours = new List<ManHour>();

                    var filtered = projectItems.Where(x => (x.projno == schedule.projno) && (empnos.Contains(x.empno)));

                    foreach (var item in filtered)
                    {
                        string name = employees.Where(x => x.empno == item.empno).Select(x => x.name).FirstOrDefault();
                        projectManHours.manHours.Add(new ManHour() { name = name, empno = item.empno, hours = item.workHour + item.overtime, yymm = item.yymm });
                    }

                    scheduleResult.Projects.Add(projectManHours);
                }

            }

            return scheduleResult;
        }

        //public ScheduleResult GetAllOwnedSchedules(string empno)
        //{
        //    // get user information
        //    User user = _userRepository.Get(empno);
        //    bool isManager = user.department_manager || user.group_manager || user.group_one_manager || user.group_two_manager || user.group_three_manager;

        //    // setup
        //    ScheduleResult scheduleResult = new ScheduleResult();
        //    List<User> employees = GetAllEmployeesByRole(empno);
        //    List<string> empnos = employees.Select(x => x.empno).ToList();

            
        //    // add project man hours data 
        //    if (isManager)
        //    {
        //        scheduleResult.Projects = new List<ProjectManHours>();
        //        var projectItems = _projectItemRepository.GetAll();

        //        foreach (var schedule in scheduleResult.Group)
        //        {
        //            ProjectManHours projectManHours = new ProjectManHours();
        //            projectManHours.projno = schedule.projno;
        //            projectManHours.manHours = new List<ManHour>();

        //            var filtered = projectItems.Where(x => (x.projno == schedule.projno) && (empnos.Contains(x.empno)));

        //            foreach (var item in filtered)
        //            {
        //                string name = employees.Where(x => x.empno == item.empno).Select(x => x.name).FirstOrDefault();
        //                projectManHours.manHours.Add(new ManHour() { name = name, empno = item.empno, hours = item.workHour + item.overtime, yymm = item.yymm });
        //            }

        //            scheduleResult.Projects.Add(projectManHours);
        //        }

        //    }


        //    var schedules = _scheduleRepository.GetAllOwnedSchedules(empno);

        //    if (isManager)
        //    {
        //        scheduleResult.Group = schedules.Where(x => x.type == 2).ToList();
        //        scheduleResult.Future = schedules.Where(x => x.type == 1).ToList();

        //    }

        //    scheduleResult.Individual = schedules.Where(x => x.type == 3).ToList();

        //    // add project man hours data 
        //    if (isManager)
        //    {
        //        scheduleResult.Projects = new List<ProjectManHours>();
        //        var projectItems = _projectItemRepository.GetAll();

        //        foreach (var schedule in scheduleResult.Group)
        //        {
        //            ProjectManHours projectManHours = new ProjectManHours();
        //            projectManHours.projno = schedule.projno;
        //            projectManHours.manHours = new List<ManHour>();

        //            var filtered = projectItems.Where(x => (x.projno == schedule.projno) && (empnos.Contains(x.empno)));

        //            foreach (var item in filtered)
        //            {
        //                string name = employees.Where(x => x.empno == item.empno).Select(x => x.name).FirstOrDefault();
        //                projectManHours.manHours.Add(new ManHour() { name = name, empno = item.empno, hours = item.workHour + item.overtime, yymm = item.yymm });
        //            }

        //            scheduleResult.Projects.Add(projectManHours);
        //        }

        //    }

        //    return scheduleResult;
        //}

        public ScheduleResult GetAllReferredSchedules(string empno)
        {
            User user = _userRepository.Get(empno);
            bool isManager = user.department_manager || user.group_manager || user.group_one_manager || user.group_two_manager || user.group_three_manager;

            ScheduleResult scheduleResult = new ScheduleResult();

            var schedules = _scheduleRepository.GetAllReferredSchedules(user.name);

            scheduleResult.Group = schedules.Where(x => x.type == 2).ToList();            
            scheduleResult.Future = schedules.Where(x => x.type == 1).ToList();

            return scheduleResult;
        }

        public bool UpdateOwnedSchedules(List<Schedule> schedules, string empno)
        {
            var update_schedules = schedules.Where(x => x.id != 0).ToList();
            var new_schedules = schedules.Where(x => x.id == 0).ToList();
            new_schedules.ForEach(x => x.empno = empno);

            bool ret = _scheduleRepository.Update(update_schedules);
            ret = _scheduleRepository.Insert(new_schedules);

            return ret;
        }

        // return updated/new schedule with updated data and new ids of schedule and milestones
        public Schedule UpdateSingleSchedule(Schedule schedule, List<int> deletedMilestones, string empno)
        {
            Schedule ret = _scheduleRepository.Update(schedule, deletedMilestones);
            
            return ret;
        }

        public Schedule InsertSingleSchedule(Schedule schedule, string empno)
        {
            Schedule ret = _scheduleRepository.Insert(schedule);

            return ret;
        }

        public bool DeleteSingleSchedule(Schedule schedule, string empno)
        {
            List<Schedule> deletedSchedules = new List<Schedule>() { schedule };

            // Delete group schedule will delete all individual schedules corresponding to it
            if (schedule.type == 2)
                deletedSchedules.AddRange(GetCorrespondingSchedules(schedule));

            bool ret = _scheduleRepository.Delete(deletedSchedules);

            return ret;
        }

        private List<Schedule> GetCorrespondingSchedules(Schedule schedule)
        {
            List<Schedule> schedules = _scheduleRepository.GetAll();
            schedules = schedules.Where(x => x.parent_id == schedule.id).ToList();

            return schedules;
        }

        public bool DeleteOwnedSchedules(List<int> schedules, List<int> milestones)
        {
            bool ret = _scheduleRepository.Delete(schedules, milestones);
            return ret;
        }

        public List<User> GetAllEmployeesByRole(string empno)
        {
            User user = _userRepository.Get(empno);
            List<User> filtered_employees = FilterEmployeeByRole(user);            
            return filtered_employees;
        }

        private List<User> FilterEmployeeByRole(User user)
        {
            var allEmployees = _userRepository.GetAll();
            List<User> filtered_employees = new List<User>();

            if (user.department_manager == true)
                filtered_employees.AddRange(allEmployees);

            if (user.group_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group == user.group).ToList());

            if (user.group_one_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group_one == user.group_one).ToList());

            // 智慧組長不互看
            if (user.group_two_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group_two == user.group_two && p.group_two_manager == false).ToList());

            if (user.group_three_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group_three == user.group_three).ToList());

            filtered_employees = filtered_employees.Distinct().ToList();

            return filtered_employees;
        }

        // get sub groups owned by managers
        public List<string> GetAllOwnedSubGroups(string empno)
        {
            User user = _userRepository.Get(empno);
                        
            List<string> groups = (_userRepository as UserRepository).GetSubGroups(empno);

            // add sub group if as a member
            //if (!String.IsNullOrEmpty(user.group_one))
            //    groups.Add(user.group_one);

            //if (!String.IsNullOrEmpty(user.group_two))
            //    groups.Add(user.group_two);

            //if (!String.IsNullOrEmpty(user.group_three))
            //    groups.Add(user.group_three);

            //// remove duplicates
            //groups = groups.Distinct().ToList();

            return groups;
        }

        


        public void Dispose()
        {
            _scheduleRepository.Dispose();
            _userRepository.Dispose();
            _projectItemRepository.Dispose();
        }
    }

    public class ScheduleResult
    {
        public List<Schedule> Group { get; set; }
        public List<Schedule> Individual { get; set; }
        public List<Schedule> Future { get; set; }
        public List<ProjectManHours> Projects { get; set; }
    }

    public class ProjectManHours
    {
        public string projno { get; set; }
        public List<ManHour> manHours { get; set; }        
    }

    public class ManHour
    {
        public string name { get; set; }
        public string empno { get; set; }
        public int hours { get; set; }
        public string yymm { get; set; }
    }

    // test: only for showing schedule 
    public class ScheduleShow
    {
        public Schedule Group { get; set; }
        public List<Schedule> Individual { get; set; }  // all sub schedules belong to the group schedules
        //public List<Schedule> Future { get; set; }
        //public List<ProjectManHours> Projects { get; set; }
    }

}