using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class AssessmentService
    {
        private IAssessmentRepository _assessmentRepository;

        public AssessmentService()
        {
            _assessmentRepository = new SelfAssessmentTxtRepository();
        }
        public List<Assessment> GetAllSelfAssessments()
        {
            var selfAssessments = _assessmentRepository.GetAll();
            return selfAssessments;
        }
        public List<Assessment> GetManageAssessments()
        {
            _assessmentRepository = new ManageAssessmentTxtRepository();
            var manageAssessments = _assessmentRepository.GetAll();
            return manageAssessments;
        }
        //public SelfAssessment GetSelfAssessment(int id)
        //{
        //    return _selfAssessmentRepository.Get(id);
        //}
        public void Dispose()
        {
            _assessmentRepository.Dispose();
        }
    }
}