using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.EducationV2
{
    internal interface IEducationRepository : IDisposable
    {
        List<Content> GetAllContents();
        bool InsertContentExtras(List<Content> contentExtras);
        bool UpsertAssignments(List<Assignment> assignments);
        bool DeleteAssignments(List<Assignment> assignments);
        List<Assignment> GetAssignmentsByAssigner(string assigner);
        List<Assignment> GetAssignmentsWithRecordByUsers(List<string> empnos);
    }
}
