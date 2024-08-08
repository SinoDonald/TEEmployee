using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.GEducation
{
    interface IGEducationRepository : IDisposable
    {
        List<Chapter> GetAllChapters();
        //List<Course> GetAllCourses();
        bool InsertChapters(List<Chapter> chapters);
        //List<Record> GetAllRecords();
        List<Record> GetAllRecordsByUser(string empno);
        bool UpsertRecords(List<Record> records);
        Record UpdateRecordCompleted(Record record);
        Chapter UpdateChapterDigitalized(Chapter chapter);
        bool DeleteAll();

    }
}
