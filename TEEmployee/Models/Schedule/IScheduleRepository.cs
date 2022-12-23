using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Schedule
{
    interface IScheduleRepository
    {
        Schedule Get(int id);
        List<Schedule> GetAll();
        List<Schedule> GetAllOwnedSchedules(string empno);
        List<Schedule> GetAllReferredSchedules(string name);
        bool Insert(Schedule schedule);
        bool Insert(List<Schedule> schedules);        
        bool Update(Schedule schedule);
        bool Update(List<Schedule> schedules);
        bool Upsert(List<Schedule> schedules);
        bool Delete(Schedule schedule);
        bool Delete(List<int> schedules, List<int> milestones);
        void Dispose();
    }
}
