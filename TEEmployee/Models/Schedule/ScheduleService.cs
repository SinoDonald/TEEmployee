using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Schedule
{
    public class ScheduleService : IDisposable
    {
        private IScheduleRepository _scheduleRepository;
        private IUserRepository _userRepository;

        public ScheduleService()
        {
            _scheduleRepository = new ScheduleRepository();
            _userRepository = new UserRepository();
        }

        public List<Schedule> GetAllSchedules()
        {
            var schedules = _scheduleRepository.GetAll();
            return schedules;
        }

        public ScheduleResult GetAllOwnedSchedules(string empno)
        {
            User user = _userRepository.Get(empno);
            bool isManager = user.department_manager || user.group_manager || user.group_one_manager || user.group_two_manager || user.group_three_manager;
            
            ScheduleResult scheduleResult = new ScheduleResult();

            var schedules = _scheduleRepository.GetAllOwnedSchedules(empno);

            if (isManager)
            {
                scheduleResult.Group = schedules.Where(x => x.type == 2).ToList();                
                scheduleResult.Future = schedules.Where(x => x.type == 1).ToList();
            }

            scheduleResult.Individual = schedules.Where(x => x.type == 3).ToList();


            return scheduleResult;
        }

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

        List<User> FilterEmployeeByRole(User user)
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

        public void Dispose()
        {
            _scheduleRepository.Dispose();
        }
    }

    public class ScheduleResult
    {
        public List<Schedule> Group { get; set; }
        public List<Schedule> Individual { get; set; }
        public List<Schedule> Future { get; set; }        
    }

    
}