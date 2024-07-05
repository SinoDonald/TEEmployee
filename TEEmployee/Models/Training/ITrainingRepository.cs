using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Training
{
    interface ITrainingRepository : IDisposable
    {
        List<Record> GetAllRecords();
        List<Record> GetAllRecordsByUser(string empno);
        bool InsertRecords(List<Record> records);
        bool UpdateRecords(List<Record> records);
        bool UpsertRecords(List<Record> records);
        List<Record> GetRecordExtraByRecords(List<Record> records);
        //List<Promotion> GetByUser(string empno);
        //bool Insert(List<Promotion> promotions);
        //bool Update(Promotion promotion);
        //bool DeleteAll();
    }
}
