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
        bool Update(Schedule schedule);
        bool Delete(Schedule schedule);
        void Dispose();
    }
}
