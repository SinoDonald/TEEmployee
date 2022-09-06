using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.TaskLog
{
    interface IMonthlyRecordRepository
    {
        MonthlyRecord Get(Guid guid);
        List<MonthlyRecord> GetAll();
        bool Insert(MonthlyRecord monthlyRecord);
        bool Upsert(MonthlyRecord monthlyRecord);
        bool Update(MonthlyRecord monthlyRecord);
        bool Delete(MonthlyRecord monthlyRecord);
        void Dispose();
    }
}
