using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models
{
    public interface ISelfAssessmentRepository
    {

        SelfAssessment Get(int Id);
        List<SelfAssessment> GetAll();      
        void Dispose();
    }
}
