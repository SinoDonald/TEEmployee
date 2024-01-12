using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Education
{
    interface IEducationRepository : IDisposable
    {
        List<Course> GetAllCourses();
        bool InsertCourses(List<Course> courses);
        List<Record> GetAllRecords();
        List<Record> GetAllRecordsByUser(string empno);
        bool UpsertRecords(List<Record> records);
    }
}
