using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.GSchedule
{
    interface IGScheduleRepository
    {
        List<Schedule> GetAll();
        List<Schedule> GetAllGroupSchedules(string role);
        Schedule Update(Schedule schedule);
        bool Update(List<Schedule> schedules);
        Schedule Insert(Schedule schedule);
        bool Delete(List<Schedule> schedules);
        void Dispose();
    }
}
