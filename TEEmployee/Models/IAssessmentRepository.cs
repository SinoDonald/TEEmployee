using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models
{
    public interface IAssessmentRepository
    {

        //SelfAssessment Get(int Id);
        List<Assessment> GetAll();
        bool Update(List<Assessment> assessments, string user);
        List<Assessment> GetResponse(string user);
        List<Assessment> GetAllResponses();
        void Dispose();
    }
}
