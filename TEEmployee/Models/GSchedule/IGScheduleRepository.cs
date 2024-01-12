using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
        List<ProjectSchedule> GetAllProjectSchedules();
        bool Insert(ProjectSchedule projectSchedule);
        bool Update(ProjectSchedule projectSchedule);
        bool Delete(ProjectSchedule projectSchedule);
        List<string> GetYears(string view); // 取得年份
        bool UploadPDFFile(HttpPostedFileBase file, string view, string empno); // 上傳PDF
        string GetPDF(string view, string year, string group, string userName); // 取得PDF
        bool SaveResponse(string empno, string userName, string comment); // 儲存回覆
        void Dispose();
    }
}
