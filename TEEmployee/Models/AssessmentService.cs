using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class AssessmentService
    {
        private IAssessmentRepository _assessmentRepository;
        //private IResponseRepository _responseRepository;

        public AssessmentService()
        {
            _assessmentRepository = new SelfAssessmentTxtRepository();
            //_responseRepository = new ResponseTxtRepository();
        }
        public AssessmentService(string manage)
        {
            _assessmentRepository = new ManageAssessmentTxtRepository();
        }

        public List<Assessment> GetAllSelfAssessments()
        {
            var selfAssessments = _assessmentRepository.GetAll();
            return selfAssessments;
        }

        public List<Assessment> GetAllManageAssessments()
        {
            //_assessmentRepository = new ManageAssessmentTxtRepository();
            var manageAssessments = _assessmentRepository.GetAll();
            return manageAssessments;
        }

        //public bool UpdateResponse(Response response)
        //{            
        //    return _responseRepository.Update(response);
        //}

        //public SelfAssessment GetSelfAssessment(int id)
        //{
        //    return _selfAssessmentRepository.Get(id);
        //}

        public bool UpdateResponse(List<Assessment> assessments, string user)
        {
            return _assessmentRepository.Update(assessments, user);
        }
        public bool UpdateManageResponse(List<Assessment> assessments, string user)
        {
            //_assessmentRepository = new ManageAssessmentTxtRepository();
            return _assessmentRepository.Update(assessments, user);
        }

        public List<Assessment> GetSelfAssessmentResponse(string user)
        {
            var selfAssessmentResponse = _assessmentRepository.GetResponse(user);
            return selfAssessmentResponse;
        }

        public List<Assessment> GetManageAssessmentResponse(string user)
        {
            //_assessmentRepository = new ManageAssessmentTxtRepository();
            var manageAssessmentResponse = _assessmentRepository.GetResponse(user);
            return manageAssessmentResponse;
        }

        public List<Assessment> GetAllSelfAssessmentResponses()
        {
            var allSelfAssessmentResponse = _assessmentRepository.GetAllResponses();
            return allSelfAssessmentResponse;
        }

        public void Dispose()
        {
            _assessmentRepository.Dispose();
            //_responseRepository.Dispose();
        }
    }
}