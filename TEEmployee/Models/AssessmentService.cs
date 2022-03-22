using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class AssessmentService
    {
        private ISelfAssessmentRepository _selfAssessmentRepository;

        public AssessmentService()
        {
            _selfAssessmentRepository = new SelfAssessmentTxtRepository();
        }        
        public List<SelfAssessment> GetAllSelfAssessments()
        {
            var selfAssessments = _selfAssessmentRepository.GetAll();
            return selfAssessments;
        }
        //public SelfAssessment GetSelfAssessment(int id)
        //{
        //    return _selfAssessmentRepository.Get(id);
        //}
        public void Dispose()
        {
            _selfAssessmentRepository.Dispose();
        }
    }
}