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
        List<Schedule> GetAllIndividualSchedules(string empno);
        List<Schedule> GetAllGroupSchedules(string role);
        List<Schedule> GetAllReferredSchedules(string name);
        Schedule Insert(Schedule schedule);
        bool Insert(List<Schedule> schedules);
        //int Insert(Milestone milestone);
        Schedule Update(Schedule schedule, List<int> deletedMilestones);
        bool Update(List<Schedule> schedules);
        //bool Update(Milestone milestone);
        bool Upsert(List<Schedule> schedules);
        bool Delete(Schedule schedule);
        bool Delete(List<Schedule> schedules);
        bool Delete(List<int> schedules, List<int> milestones);
        void Dispose();
    }
}
