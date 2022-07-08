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
        private IUserRepository _userRepository;

        public AssessmentService()
        {
            _assessmentRepository = new SelfAssessmentTxtRepository();
            //_userRepository = new UserTxtRepository();
            _userRepository = new UserRepository();
            //_responseRepository = new ResponseTxtRepository();
        }
        public AssessmentService(string manage)
        {
            _assessmentRepository = new ManageAssessmentTxtRepository();
            _userRepository = new UserTxtRepository();
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
        
        
        public bool UpdateResponse(List<Assessment> assessments, string user, string state, string year)
        {
            //return _assessmentRepository.Update(assessments, user);
            return (_assessmentRepository as SelfAssessmentTxtRepository).Update(assessments, user, state, year);
        }

        public bool UpdateManageResponse(List<Assessment> assessments, string user)
        {
            //_assessmentRepository = new ManageAssessmentTxtRepository();
            return _assessmentRepository.Update(assessments, user);
        }
        public bool UpdateMResponse(List<Assessment> assessments, string empId, string user)
        { 
            return (_assessmentRepository as SelfAssessmentTxtRepository).UpdateMResponse(assessments, empId, user);
        }

        //public List<Assessment> GetSelfAssessmentResponse(string user)
        //{            
        //    var selfAssessmentResponse = _assessmentRepository.GetResponse(user);
        //    return selfAssessmentResponse;
        //}

        //with state
        public SelfAssessResponse GetSelfAssessmentResponse(string user, string year = "")
        {
            if (String.IsNullOrEmpty(year))
                year = Utilities.DayStr();

            string state = "submit";
            var selfAssessmentResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetResponse(user, state, year);

            if (selfAssessmentResponse.Count == 0)
            {
                state = "save";
                selfAssessmentResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetResponse(user, state, year);
            }

            return new SelfAssessResponse() { Responses = selfAssessmentResponse, State = state };
        }

        public List<Assessment> GetSelfAssessmentMResponse(string empId, string user)
        {
            var selfAssessmentMResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetMResponse(empId, user);
            return selfAssessmentMResponse;
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

        public List<CategorySelfAssessmentChart> GetAllCategorySelfAssessmentCharts()
        {
            var selfAssessments = _assessmentRepository.GetAll();
            var selfResponses = _assessmentRepository.GetAllResponses();
            string[] options = { "option1", "option2", "option3" };

            var allCategorySelfAssessmentCharts = new List<CategorySelfAssessmentChart>();

            //Create chart model by category
            foreach(var item in selfAssessments)
            {
                if (item.Id == 0)
                    allCategorySelfAssessmentCharts.Add(new CategorySelfAssessmentChart() { 
                        CategoryId = item.CategoryId, CategoryName = item.Content, Charts = new List<SelfAssessmentChart>() });
            }

            //Sum up votes by Id
            foreach (var item in selfAssessments)
            {
                if (item.Id != 0 && item.Content != "自評摘要")
                {
                    List<int> votes = new List<int>();

                    foreach(var option in options)
                    {
                        votes.Add((from response in selfResponses
                                   where response.Choice == option && response.Id == item.Id
                                   select response).Count());
                    }

                    allCategorySelfAssessmentCharts[item.CategoryId - 1].Charts.Add(new SelfAssessmentChart() { Content = item.Content, Votes = votes });
                }                    
            }
            
            return allCategorySelfAssessmentCharts;
        }

        public List<User> GetAllEmployees()
        {
            //var allEmployees = _userRepository.GetAll().Where(employee => employee.Role == "3").ToList();
            var allEmployees = _userRepository.GetAll();
            return allEmployees;
        }

        public List<MixResponse> GetSelfAssessmentMixResponse(string empId, string user)
        {
            var selfAssessments = _assessmentRepository.GetAll();
            var selfAssessmentResponse = _assessmentRepository.GetResponse(empId);
            var selfAssessmentMResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetMResponse(empId, user);

            List<MixResponse> mixResponses = new List<MixResponse>();

            foreach(var selfAssessment in selfAssessments)
            {
                MixResponse mixResponse = new MixResponse() { Id = selfAssessment.Id, CategoryId = selfAssessment.CategoryId, Content = selfAssessment.Content };
                
                if(selfAssessment.Id != 0)
                {
                    foreach(var response in selfAssessmentResponse)
                    {
                        if (selfAssessment.Id == response.Id)
                        {
                            mixResponse.Choice = response.Choice;
                            break;
                        }                            
                    }

                    foreach (var mresponse in selfAssessmentMResponse)
                    {
                        if (selfAssessment.Id == mresponse.Id)
                        {
                            mixResponse.ManagerChoice = mresponse.Choice;
                            break;
                        }
                    }

                }

                mixResponses.Add(mixResponse);
            }

            return mixResponses;
        }

        public bool UpdateSelfAssessmentMixResponse(List<MixResponse> mixResponses, string empId, string user)
        {
            List<Assessment> assessments = new List<Assessment>();
            foreach (var m in mixResponses)
                assessments.Add(new Assessment() { Id = m.Id, CategoryId = m.CategoryId, Content = m.Content, Choice = m.ManagerChoice });
            return (_assessmentRepository as SelfAssessmentTxtRepository).UpdateMResponse(assessments, empId, user);
        }

        public List<string> GetYearList(string user)
        {            
            var assessYearList = (_assessmentRepository as SelfAssessmentTxtRepository).GetYearList(user);                      
            return assessYearList;
        }



        public void Dispose()
        {
            _assessmentRepository.Dispose();
            //_responseRepository.Dispose();
        }
    }

    public class SelfAssessResponse
    {
        public string State { get; set; }
        public List<Assessment> Responses { get; set; }
    }


}