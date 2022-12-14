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