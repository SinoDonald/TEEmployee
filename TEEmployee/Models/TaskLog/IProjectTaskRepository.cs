using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.TaskLog
{
    interface IProjectTaskRepository
    {
        ProjectTask Get(int id);
        List<ProjectTask> GetAll();
        bool Insert(ProjectTask projectTask);
        bool Upsert(ProjectTask projectTask);
        bool Update(ProjectTask projectTask);
        bool Delete(int id, string empno);
        bool DeleteAll();
        List<ProjectTask> GetProjectTasksByEmpnoAndYYMM(string empno, string yymm);
        void Dispose();
    }
}
