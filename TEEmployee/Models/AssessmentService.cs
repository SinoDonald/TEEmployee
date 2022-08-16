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
            _userRepository = new UserRepository();
            //_userRepository = new UserTxtRepository();
        }

        public List<Assessment> GetAllSelfAssessments()
        {
            var selfAssessments = _assessmentRepository.GetAll();
            return selfAssessments;
        }

        public SelfAssessResponse GetAllManageAssessments(User manager, string user)
        {
            string state = (_assessmentRepository as ManageAssessmentTxtRepository).GetStateOfResponse(manager.empno, user);
            var manageAssessments = _assessmentRepository.GetResponse(manager.empno, user);
            //return manageAssessments;

            return new SelfAssessResponse() { Responses = manageAssessments, State = state };
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
            //return (_assessmentRepository as SelfAssessmentTxtRepository).Update(assessments, user, state, year);
            return (_assessmentRepository as SelfAssessmentTxtRepository).Update(assessments, user, state, year, DateTime.Now);
        }

        public bool UpdateManageResponse(List<Assessment> assessments, string state, User manager, string user)
        {
            return _assessmentRepository.Update(assessments, state, manager.empno, user);
        }
        public bool UpdateMResponse(List<Assessment> assessments, string empId, string user)
        { 
            return (_assessmentRepository as SelfAssessmentTxtRepository).UpdateMResponse(assessments, empId, user);
        }

        // 0729: Feedback for all categories
        public Feedback GetFeedback(string empno, string manno)
        {
            var name = _userRepository.Get(manno).name;
            var res = (_assessmentRepository as SelfAssessmentTxtRepository).GetFeedback(empno, manno, name);

            return new Feedback() { State = res.Item1, Text = res.Item2 };
        }

        // 0729: Feedback for all categories
        public List<Feedback> GetAllFeedbacks(string user, string year)
        {
            
            if (String.IsNullOrEmpty(year))
                year = Utilities.DayStr();

            List<Feedback> feedbacks = new List<Feedback>();

            var res = (_assessmentRepository as SelfAssessmentTxtRepository).GetAllFeedbacks(user, year);

            foreach (var item in res)
                feedbacks.Add(new Feedback() { Name = item.Item1, Text = item.Item2 });

            return feedbacks;
        }


        // 0729:  Feedback for all category
        public bool UpdateFeedback(List<string> feedbacks, string state, string empno, string manno)
        {
            var name = _userRepository.Get(manno).name;

            for (int i = 0; i < feedbacks.Count; i++)
            {
                if (String.IsNullOrEmpty(feedbacks[i]))
                    feedbacks[i] = "";
            }

            return (_assessmentRepository as SelfAssessmentTxtRepository).UpdateFeedback(feedbacks, state, empno, manno, name);
        }

        //public bool UpdateFeedback(string feedback, string state, string empno, string manno)
        //{
        //    var name = _userRepository.Get(manno).name;
        //    return (_assessmentRepository as SelfAssessmentTxtRepository).UpdateFeedback(feedback, state, empno, manno, name);
        //}


        //public List<Assessment> GetSelfAssessmentResponse(string user)
        //{            
        //    var selfAssessmentResponse = _assessmentRepository.GetResponse(user);
        //    return selfAssessmentResponse;
        //}

        //with state
        //public SelfAssessResponse GetSelfAssessmentResponse(string user, string year = "")
        //{
        //    if (String.IsNullOrEmpty(year))
        //        year = Utilities.DayStr();

        //    string state = "submit";
        //    var selfAssessmentResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetResponse(user, state, year);

        //    if (selfAssessmentResponse.Count == 0)
        //    {
        //        state = "save";
        //        selfAssessmentResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetResponse(user, state, year);
        //    }

        //    return new SelfAssessResponse() { Responses = selfAssessmentResponse, State = state };
        //}

        public SelfAssessResponse GetSelfAssessmentResponse(string user, string year = "")
        {
            if (String.IsNullOrEmpty(year))
                year = Utilities.DayStr();

            string state = (_assessmentRepository as SelfAssessmentTxtRepository).GetStateOfResponse(user, year);
            var selfAssessmentResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetResponse(user, year);

            return new SelfAssessResponse() { Responses = selfAssessmentResponse, State = state };
        }


        public List<Assessment> GetSelfAssessmentMResponse(string empId, string user)
        {
            var selfAssessmentMResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetMResponse(empId, user);
            return selfAssessmentMResponse;
        }
        public SelfAssessResponse GetManageAssessmentResponse(string manager, string user)
        {
            string state = (_assessmentRepository as ManageAssessmentTxtRepository).GetStateOfResponse(manager, user);
            var manageAssessmentResponse = _assessmentRepository.GetResponse(manager, user);

            return new SelfAssessResponse() { Responses = manageAssessmentResponse, State = state };
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
        public List<User> GetManagers()
        {
            var allManagers = _userRepository.GetManagers().OrderBy(x => x.empno).ToList();
            return allManagers;
        }

        // 0713 
        //public List<EmployeesWithState> GetAllEmployeesWithState(string manno, string name)
        //{
        //    //var allEmployees = _userRepository.GetAll().Where(employee => employee.Role == "3").ToList();

        //    List<EmployeesWithState> employeesWithStates = new List<EmployeesWithState>();

        //    var allEmployees = _userRepository.GetAll();

        //    foreach (var item in allEmployees)
        //    {
        //        string state = "unfinished";

        //        var empState = (_assessmentRepository as SelfAssessmentTxtRepository).GetStateOfResponse(item.empno, Utilities.DayStr());
        //        var manState = (_assessmentRepository as SelfAssessmentTxtRepository).GetFeedback(item.empno, manno, name);


        //        if (empState == "submit")
        //            state = "submit";

        //        if (manState.Item1 == "submit")
        //            state = "complete";

        //        employeesWithStates.Add(new EmployeesWithState()
        //        {
        //            Employee = item,
        //            State = state
        //        });
        //    }
            
        //    return employeesWithStates;
        //}

        // 0721
        public List<EmployeesWithState> GetAllEmployeesWithStateByRole(string manno)
        {
            //var allEmployees = _userRepository.GetAll().Where(employee => employee.Role == "3").ToList();
            User user = _userRepository.Get(manno);

            List<EmployeesWithState> employeesWithStates = new List<EmployeesWithState>();

            var allEmployees = _userRepository.GetAll();
            List<User> filtered_employees = new List<User>();

            var a = allEmployees.Where(p => p.group == user.group);


            if (user.department_manager == true)
                filtered_employees.AddRange(allEmployees.ToList());

            if (user.group_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group == user.group).ToList());

            if (user.group_one_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group_one == user.group_one).ToList());

            if (user.group_two_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group_two == user.group_two).ToList());

            filtered_employees = filtered_employees.Distinct().ToList();


            foreach (var item in filtered_employees)
            {
                string state = "unfinished";

                var empState = (_assessmentRepository as SelfAssessmentTxtRepository).GetStateOfResponse(item.empno, Utilities.DayStr());
                var manState = (_assessmentRepository as SelfAssessmentTxtRepository).GetFeedback(item.empno, manno, user.name).Item1;


                if (empState == "submit")
                    state = "submit";

                if (manState == "submit")
                    state = "complete";

                employeesWithStates.Add(new EmployeesWithState()
                {
                    Employee = item,
                    State = state
                });
            }

            return employeesWithStates;
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

    public class EmployeesWithState
    {
        public User Employee { get; set; }
        public string State { get; set; }
    }

    public class Feedback
    {
        public string Name { get; set; }
        public List<string> Text { get; set; }
        public string State { get; set; }
    }



}