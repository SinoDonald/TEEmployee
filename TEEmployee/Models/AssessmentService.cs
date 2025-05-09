﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using TEEmployee.Models.Assessments;

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
        }

        /// <summary>
        /// 取得所有自評題目。
        /// </summary>
        /// <returns>包含所有自評題目的列舉。</returns>
        public List<Assessment> GetAllSelfAssessments()
        {
            var selfAssessments = _assessmentRepository.GetAll();
            return selfAssessments;
        }
        /// <summary>
        /// 取得員工評核主管各年季度的回覆
        /// </summary>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public SelfAssessResponse GetAllManageAssessments(string year, User manager, string user)
        {
            string state = (_assessmentRepository as ManageAssessmentTxtRepository).GetStateOfResponse(year, manager.empno, user);
            var manageAssessments = _assessmentRepository.GetResponse(year, manager.empno, user);

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

        /// <summary>
        /// 建立回覆
        /// </summary>
        /// <param name="assessments"></param>
        /// <param name="user"></param>
        /// <param name="state"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public bool UpdateResponse(List<Assessment> assessments, string user, string state, string year)
        {
            //return _assessmentRepository.Update(assessments, user);
            //return (_assessmentRepository as SelfAssessmentTxtRepository).Update(assessments, user, state, year);
            return (_assessmentRepository as SelfAssessmentTxtRepository).Update(assessments, user, state, year, DateTime.Now);
        }
        /// <summary>
        /// 建立主管回覆
        /// </summary>
        /// <param name="assessments"></param>
        /// <param name="state"></param>
        /// <param name="year"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UpdateManageResponse(List<Assessment> assessments, string state, string year, User manager, string user)
        {
            return _assessmentRepository.Update(assessments, state, year, manager.empno, user);
        }
        public bool UpdateMResponse(List<Assessment> assessments, string empId, string user)
        {
            return (_assessmentRepository as SelfAssessmentTxtRepository).UpdateMResponse(assessments, empId, user);
        }

        // 0729: Feedback of all categories for employer himself

        /// <summary>
        /// 取得主管評員工回饋內容
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <param name="manno">主管員工編號</param>
        /// <returns>包含所有回饋的列舉。</returns>
        public Feedback GetFeedback(string empno, string manno)
        {
            var name = _userRepository.Get(manno).name;
            var res = (_assessmentRepository as SelfAssessmentTxtRepository).GetFeedback(empno, manno, name);

            return new Feedback() { State = res.Item1, Text = res.Item2 };
        }

        // 1208: Get all other manager feedbacks based on duty

        /// <summary>
        /// 取得其他主管評員工回饋內容
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <param name="manno">主管員工編號</param>
        /// <returns>包含所有回饋的列舉。</returns>
        public List<Feedback> GetAllOtherFeedbacks(string empno, string manno)
        {
            var feedbacks = GetAllFeedbacks(empno, Utilities.DayStr());

            var user = _userRepository.Get(manno);
            var depart_manager = _userRepository.GetAll().Where(x => x.department_manager).FirstOrDefault();

            if (user.group_manager)
                feedbacks.RemoveAll(x => x.Name == depart_manager.name);
            feedbacks.RemoveAll(x => x.Name == user.name);

            return feedbacks;
        }

        // 0428: Get all manager feedbacks based on duty for manager
        /// <summary>
        /// 取得其他主管評員工回饋內容
        /// </summary>
        /// <param name="year">年度</param>
        /// <param name="empno">員工編號</param>
        /// <param name="manno">主管員工編號</param>
        /// <returns>包含所有回饋的列舉。</returns>
        public List<Feedback> GetAllFeedbacksForManager(string year, string empno, string manno)
        {
            var feedbacks = GetAllFeedbacks(empno, year);

            var user = _userRepository.Get(manno);
            var depart_manager = _userRepository.GetAll().Where(x => x.department_manager).FirstOrDefault();

            if (user.department_manager) { }
            else if (user.group_manager)
            {
                feedbacks.RemoveAll(x => x.Name == depart_manager.name);
            }
            else
            {
                feedbacks.RemoveAll(x => x.Name != user.name);
            }

            return feedbacks;
        }


        // 0729: Feedback of all categories for employee
        /// <summary>
        /// 員工取得所有回饋內容
        /// </summary>
        /// <param name="user">員工編號</param>
        /// <param name="year">年份</param>
        /// <returns>包含所有回饋的列舉。</returns>
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
        /// <summary>
        /// 主管更新員工回饋內容
        /// </summary>
        /// <param name="feedbacks">回饋內容列舉</param>
        /// <param name="state">儲存狀態</param>
        /// <param name="empno">員工編號</param>
        /// <param name="manno">主管員工編號</param>
        /// <returns>更新成功</returns>
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

        /// <summary>
        /// 員工取得自評內容
        /// </summary>
        /// <param name="user">員工編號</param>
        /// <param name="year">年份</param>
        /// <returns>自評內容實體</returns>
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
        public SelfAssessResponse GetManageAssessmentResponse(string year, string manager, string user)
        {
            if (String.IsNullOrEmpty(year))
            {
                year = Utilities.DayStr();
            }

            string state = (_assessmentRepository as ManageAssessmentTxtRepository).GetStateOfResponse(year, manager, user);
            var manageAssessmentResponse = _assessmentRepository.GetResponse(year, manager, user);

            return new SelfAssessResponse() { Responses = manageAssessmentResponse, State = state };
        }

        public List<Assessment> GetAllSelfAssessmentResponses()
        {
            var allSelfAssessmentResponse = _assessmentRepository.GetAllResponses();
            return allSelfAssessmentResponse;
        }

        /// <summary>
        /// 主管取得員工自評圖表內容
        /// </summary>
        /// <returns>員工自評圖表列舉</returns>
        public List<CategorySelfAssessmentChart> GetAllCategorySelfAssessmentCharts()
        {
            var selfAssessments = _assessmentRepository.GetAll();
            var selfResponses = _assessmentRepository.GetAllResponses();
            string[] options = { "option1", "option2", "option3" };

            var allCategorySelfAssessmentCharts = new List<CategorySelfAssessmentChart>();

            //Create chart model by category
            foreach (var item in selfAssessments)
            {
                if (item.Id == 0)
                    allCategorySelfAssessmentCharts.Add(new CategorySelfAssessmentChart()
                    {
                        CategoryId = item.CategoryId,
                        CategoryName = item.Content,
                        Charts = new List<SelfAssessmentChart>()
                    });
            }

            //Sum up votes by Id
            foreach (var item in selfAssessments)
            {
                if (item.Id != 0 && item.Content != "自評摘要")
                {
                    List<int> votes = new List<int>();

                    foreach (var option in options)
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

        /// <summary>
        /// 更新評主管名單
        /// </summary>
        /// <returns>主管名單</returns>
        //public List<User> SetScorePeople()
        //{
        //    List<User> users = new List<User>();
        //    int index = 0;
        //    int i = 0;
        //    foreach (User user in _userRepository.GetAll())
        //    {
        //        if (user != null)
        //        {
        //            if (user.dutyName.Equals("NULL"))
        //            {
        //                user.dutyName = "";
        //            }
        //            if (user.empno.Equals("4125"))
        //            {
        //                user.dutyName = "協理";
        //                index = i;
        //            }

        //            users.Add(user);
        //        }
        //        i++;
        //    }
        //    // 依員編排序
        //    users = users.OrderByDescending(x => x.dutyName).ThenBy(x => x.empno).ToList();
        //    return users;
        //}
        public List<User> GetManagers()
        {
            var allManagers = _userRepository.GetManagers().OrderBy(x => x.empno).ToList();
            return allManagers;
        }
        //public List<User> GetScorePeople()
        //{
        //    _assessmentRepository = new ManageAssessmentTxtRepository();
        //    List<User> scorePeople = (_assessmentRepository as ManageAssessmentTxtRepository).GetScorePeople();
        //    return scorePeople;
        //}

        //0923 return score people with state
        /// <summary>
        /// 取得評主管名單
        /// </summary>
        /// <returns>主管名單</returns>
        public List<EmployeesWithState> GetScorePeople(string empno)
        {
            List<EmployeesWithState> employeesWithStates = new List<EmployeesWithState>();
            _assessmentRepository = new ManageAssessmentTxtRepository();
            List<User> scorePeople = (_assessmentRepository as ManageAssessmentTxtRepository).GetScorePeople();

            foreach (var item in scorePeople)
            {
                string state = (_assessmentRepository as ManageAssessmentTxtRepository).GetStateOfResponse(Utilities.DayStr(), item.empno, empno);
                employeesWithStates.Add(new EmployeesWithState() { Employee = item, State = state });
            }

            return employeesWithStates;
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
        /// <summary>
        /// 取得所有員工自評狀態
        /// </summary>
        /// <param name="manno">主管員工編號</param>
        /// <returns>員工自評狀態列舉</returns>
        public List<EmployeesWithState> GetAllEmployeesWithStateByRole(string manno)
        {
            //var allEmployees = _userRepository.GetAll().Where(employee => employee.Role == "3").ToList();
            User user = _userRepository.Get(manno);

            List<EmployeesWithState> employeesWithStates = new List<EmployeesWithState>();

            List<User> filtered_employees = FilterEmployeeByRole(user);

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

                employeesWithStates = employeesWithStates.OrderBy(x => x.Employee.name).ToList();
            }

            return employeesWithStates;
        }

        List<User> FilterEmployeeByRole(User user)
        {
            var allEmployees = _userRepository.GetAll();
            List<User> filtered_employees = new List<User>();

            if (user.department_manager == true)
                filtered_employees.AddRange(allEmployees);

            if (user.group_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group == user.group).ToList());

            // 2024 version
            //if (user.group_one_manager == true)
            //    filtered_employees.AddRange(allEmployees.Where(p => p.group_one == user.group_one || p.group_two == user.group_one).ToList());

            //// 智慧組長不互看
            //if (user.group_two_manager == true)
            //    filtered_employees.AddRange(allEmployees.Where(p => p.group_two == user.group_two && p.group_two_manager == false).ToList());

            //if (user.group_three_manager == true)
            //    filtered_employees.AddRange(allEmployees.Where(p => p.group_three == user.group_three).ToList());

            // 2025 version: 技術長 = 多個小組組長, 組長間不互看
            if (user.group_one_manager == true)
            {
                filtered_employees.AddRange(allEmployees.Where(x =>
                (x.group_one == user.group_one && !x.group_one_manager) 
                || (x.group_two == user.group_one && !x.group_two_manager)
                || (x.group_three == user.group_one && !x.group_three_manager)
                ));
            }

            if (user.group_two_manager == true)
            {
                filtered_employees.AddRange(allEmployees.Where(x =>
                (x.group_one == user.group_two && !x.group_one_manager)
                || (x.group_two == user.group_two && !x.group_two_manager)
                || (x.group_three == user.group_two && !x.group_three_manager)
                ));
            }

            if (user.group_three_manager == true)
            {
                filtered_employees.AddRange(allEmployees.Where(x =>
                (x.group_one == user.group_three && !x.group_one_manager)
                || (x.group_two == user.group_three && !x.group_two_manager)
                || (x.group_three == user.group_three && !x.group_three_manager)
                ));
            }

            filtered_employees = filtered_employees.Distinct().ToList();

            return filtered_employees;
        }




        public List<MixResponse> GetSelfAssessmentMixResponse(string empId, string user)
        {
            var selfAssessments = _assessmentRepository.GetAll();
            var selfAssessmentResponse = _assessmentRepository.GetResponse(empId);
            var selfAssessmentMResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetMResponse(empId, user);

            List<MixResponse> mixResponses = new List<MixResponse>();

            foreach (var selfAssessment in selfAssessments)
            {
                MixResponse mixResponse = new MixResponse() { Id = selfAssessment.Id, CategoryId = selfAssessment.CategoryId, Content = selfAssessment.Content };

                if (selfAssessment.Id != 0)
                {
                    foreach (var response in selfAssessmentResponse)
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
        public List<string> GetManageYearList(string user)
        {
            var assessYearList = (_assessmentRepository as ManageAssessmentTxtRepository).GetManageYearList(user);
            return assessYearList;
        }


        public SelfAssessResponse GetReviewByYear(string year, string empno, string manno)
        {
            var ret = this.GetSelfAssessmentResponse(empno, year);
            if (ret.State != "submit")
            {
                ret.Responses = new List<Assessment>();
            }

            return ret;
        }


        // chart

        // 0825 used for both service
        public List<string> GetChartYearList()
        {
            var assessYearList = _assessmentRepository.GetChartYearList();
            return assessYearList;
        }

        /// <summary>
        /// 取得自評圖表小組名單
        /// </summary>
        /// <param name="manno">主管員工編號</param>
        /// <returns>評圖表小組名單</returns>
        public List<string> GetChartGroupList(string manno)
        {
            List<string> groups = new List<string>();
            User user = _userRepository.Get(manno);

            if (user.department_manager || user.group_manager)
            {
                //var allEmployees = _userRepository.GetAll();
                //groups.AddRange(allEmployees.Select(x => x.group).ToList());
                //groups.AddRange(allEmployees.Select(x => x.group_one).ToList());
                //groups.AddRange(allEmployees.Select(x => x.group_two).ToList());
                //groups.AddRange(allEmployees.Select(x => x.group_three).ToList());
                //groups = groups.Where(x => !String.IsNullOrEmpty(x)).Distinct().ToList();

                //groups = ["規劃", "土木組", "規劃組", "設計", "地工組", "BIM暨程式開發組", "界面整合管理組", "界面整合管理組", "界面整合管理組"];

                var allEmployees = _userRepository.GetAll();

                if (user.department_manager)
                {
                    if (user.gid == "24")
                    {
                        //groups = new List<string> { "規劃", "設計", "專管" };
                        groups = allEmployees.Select(x => x.group).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
                    }
                    else // For other departments
                    {
                        groups = allEmployees.Select(x => x.group).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    }

                }
                else
                {
                    groups.Add(user.group);
                    allEmployees = allEmployees.Where(x => x.group == user.group).ToList();
                }


                foreach (var item in allEmployees)
                {
                    if (!String.IsNullOrEmpty(item.group))
                    {
                        //三大群組 小組1
                        if (!String.IsNullOrEmpty(item.group_one) && !groups.Contains(item.group_one))
                        {
                            groups.Insert(groups.FindIndex(x => x == item.group) + 1, item.group_one);
                        }

                        //跨三大群組 小組2 小組3 (協理 only)
                        if (user.department_manager)
                        {
                            if (!String.IsNullOrEmpty(item.group_two) && !groups.Contains(item.group_two))
                            {
                                groups.Add(item.group_two);
                            }
                            if (!String.IsNullOrEmpty(item.group_three) && !groups.Contains(item.group_three))
                            {
                                groups.Add(item.group_three);
                            }
                        }

                    }
                    else
                    {
                        //非三大群組
                        if (!String.IsNullOrEmpty(item.group_one) && !groups.Contains(item.group_one))
                        {
                            groups.Add(item.group_one);
                        }
                    }

                }

                //special case
                //if (groups.Remove("規劃組"))
                //    groups.Insert(groups.FindIndex(x => x == "規劃") + 1, "規劃組");

            }


            if (user.group_one_manager) groups.Add(user.group_one);
            if (user.group_two_manager) groups.Add(user.group_two);
            if (user.group_three_manager) groups.Add(user.group_three);

            groups = groups.Distinct().ToList();

            return groups;
        }


        /// <summary>
        /// 取得權限下所有自評圖表內容
        /// </summary>
        /// <param name="manno">主管員工編號</param>
        /// <param name="year">年份</param>
        /// <returns>自評圖表內容列舉</returns>
        public List<ChartEmployeeData> GetChartEmployeeData(string manno, string year)
        {
            User user = _userRepository.Get(manno);
            List<User> filteredEmployees = FilterEmployeeByRole(user);
            List<ChartEmployeeData> chartEmployeeData = new List<ChartEmployeeData>();

            foreach (var employee in filteredEmployees)
            {
                string state = (_assessmentRepository as SelfAssessmentTxtRepository).GetStateOfResponse(employee.empno, year);

                if (state == "submit")
                {
                    var selfAssessmentResponse = (_assessmentRepository as SelfAssessmentTxtRepository).GetResponse(employee.empno, year);
                    selfAssessmentResponse = selfAssessmentResponse.Where(x => x.Choice.Contains("option")).ToList();
                    chartEmployeeData.Add(new ChartEmployeeData() { Employee = employee, Responses = selfAssessmentResponse });
                }
            }

            return chartEmployeeData;
        }

        //public List<string> GetChartManagerList(string manno, string year)
        //{
        //    User user = _userRepository.Get(manno);
        //    List<string> names = new List<string>();

        //    if (String.IsNullOrEmpty(year))
        //        year = Utilities.DayStr();

        //    if (user.department_manager)
        //    {
        //        names = (_assessmentRepository as ManageAssessmentTxtRepository).GetChartManagers(year).ToList();
        //        names.Remove(user.name);
        //        names.Insert(0, user.name);
        //    }

        //    else
        //        names.Add(user.name);

        //    return names;
        //}

        // 20230511 - Add num of options variable 
        /// <summary>
        /// 取得評主管圖表內容
        /// </summary>
        /// <param name="manno">主管員工編號</param>
        /// <param name="year">年份</param>
        /// <returns>評主管圖表內容</returns>
        public ChartManagerData GetChartManagerData(string manno, string year)
        {
            User user = _userRepository.Get(manno);
            List<User> managers = new List<User>();
            List<ChartManagerResponse> chartManagerResponses = new List<ChartManagerResponse>();

            // Get chart manager list of the year 

            List<string> empnos;

            if (String.IsNullOrEmpty(year))
                year = Utilities.DayStr();

            empnos = (_assessmentRepository as ManageAssessmentTxtRepository).GetChartManagers(year).ToList();

            if (user.department_manager)
            {
                empnos.Remove(user.empno);
                empnos.Insert(0, user.empno);
            }
            else if (empnos.Contains(user.empno))
                empnos = new List<string>() { user.empno };
            else
                empnos = new List<string>();
            // Get Manager Assessment 

            var managerAssessments = _assessmentRepository.GetAll();
            int numOfCategory = managerAssessments.Where(x => x.Id == 0).Count();
            int numOfQuestion = managerAssessments.Count() - numOfCategory * 2;
            managerAssessments = managerAssessments.Where(x => x.Id != 0 && x.Content != "建議").ToList();
            int numOfOptions = 0;
            // Collect user data and chart data

            // additional tuning            

            //numOfOptions = (year.CompareTo("2022H2") > 0) ? 6 : 4;   // 2022H2 = 4 ; 2023H1 = 6
            //numOfQuestion = (year.CompareTo("2023H1") > 0) ? numOfQuestion : 43; // 2023H1 = 43 ; 2023H2 = 30

            if (year == "2022H2")
            {
                numOfOptions = 4;
                numOfQuestion = 43;
                numOfCategory = 8;
                managerAssessments = (_assessmentRepository as ManageAssessmentTxtRepository).GetAllByYear(year).Where(x => x.Id != 0 && x.Content != "建議").ToList();
            }
            else if (year == "2023H1")
            {
                numOfOptions = 6;
                numOfQuestion = 43;
                numOfCategory = 8;
                managerAssessments = (_assessmentRepository as ManageAssessmentTxtRepository).GetAllByYear(year).Where(x => x.Id != 0 && x.Content != "建議").ToList();

            }
            // current "2023H2"
            else
            {
                numOfOptions = 5;
                numOfQuestion = 30;
                numOfCategory = 7;
            }


            foreach (var empno in empnos)
            {
                User manager = _userRepository.Get(empno);
                managers.Add(manager);

                var allResponses = (_assessmentRepository as ManageAssessmentTxtRepository).GetAllManagerAssessmentResponses(empno, year);
                //if (allResponses.Count == 0) break;



                // Create empty list
                List<List<int>> Votes = new List<List<int>>();
                List<List<string>> Responses = new List<List<string>>();

                for (int i = 0; i != numOfQuestion; i++)
                    Votes.Add(Enumerable.Repeat(0, numOfOptions).ToList());
                //Votes.Add(Enumerable.Repeat(0, 4).ToList());

                for (int i = 0; i != numOfCategory; i++)
                    Responses.Add(new List<string>());

                // Collect chart data

                foreach (var item in allResponses)
                {
                    if (item.Id == 0)
                        continue;

                    if (item.Id > numOfQuestion)
                    {
                        if (!String.IsNullOrEmpty(item.Choice))
                            Responses[item.CategoryId - 1].Add(item.Choice);
                    }
                    else
                    {
                        int idx = Int32.Parse(item.Choice.Substring(6)) - 1;
                        Votes[item.Id - 1][idx] += 1;
                    }
                }

                chartManagerResponses.Add(new ChartManagerResponse() { Manager = manager, Responses = Responses, Votes = Votes });

            }

            return new ChartManagerData() { ChartManagerResponses = chartManagerResponses, ManagerAssessments = managerAssessments };
        }

        //=============================
        // Manage Response New service
        //=============================
        public List<EmployeesWithState> ManageResponseStateCheck(string empno)
        {
            List<EmployeesWithState> employeesWithStates = new List<EmployeesWithState>();

            if (!_userRepository.Get(empno).department_manager)
                return null;


            string year = Utilities.DayStr();
            List<User> users = _userRepository.GetAll();
            List<User> managers = (_assessmentRepository as ManageAssessmentTxtRepository).GetScorePeople();

            foreach (var user in users)
            {
                string state = "unfinished";

                foreach (var manager in managers)
                {
                    string res = (_assessmentRepository as ManageAssessmentTxtRepository).GetStateOfResponse(year, manager.empno, user.empno);
                    if (res == "sent")
                    {
                        state = res;
                        break;
                    }
                }

                employeesWithStates.Add(new EmployeesWithState() { Employee = user, State = state });
            }

            employeesWithStates = employeesWithStates.OrderBy(x => x.Employee.name).ToList();

            return employeesWithStates;
        }

        // Get all managers and project manager to be scored
        public List<User> GetAllScoreManagers()
        {
            List<User> users = _userRepository.GetAll();
            users = users.Where(x => x.department_manager || x.group_manager || x.group_one_manager
                                    || x.group_two_manager || x.group_three_manager || x.project_manager).ToList();

            return users;
        }

        // Update the score manager list this half year
        public bool UpdateScoreManagers(List<User> selectedManagers)
        {
            var res = (_assessmentRepository as ManageAssessmentTxtRepository).UpdateScoreManagers(selectedManagers);
            return res;
        }

        //=============================
        // Feedback Notification
        //=============================

        public FeedbackNotification GetFeedbackNotification(string empno)
        {
            var ret = (_assessmentRepository as SelfAssessmentTxtRepository).GetFeedbackNotification(empno, Utilities.DayStr());
            return new FeedbackNotification() { Unread = ret };
        }

        public bool UpdateFeedbackNotification(string user, string empno)
        {
            bool unread = true;
            if (user == empno) unread = false;
            if (String.IsNullOrEmpty(empno))
            {
                empno = user;
                unread = false;
            }

            var ret = (_assessmentRepository as SelfAssessmentTxtRepository).UpdateFeedbackNotification(empno, Utilities.DayStr(), unread);

            // 如果submit+read, 則更新通知資料庫, 取消通知 <-- 培文
            List<bool> userNotify = (new NotifyRepository()).UserNotifyState(empno);
            if (userNotify[0].Equals(true))
            {
                NotifyService notifyService = new NotifyService();
                if (ret == true)
                {
                    notifyService.UpdateDatabase(empno, 1, "0");
                }
            }

            return ret;
        }

        // ==========================================================
        // Performance Cluster (By Group123)
        // Authority: Department Manager > Group Manager > Group123 Manager

        /// <summary>
        /// 取得主管評員工分群圖表
        /// </summary>
        /// <param name="empno">主管員工編號</param>
        /// <param name="year">年份</param>
        /// <returns>主管評員工分群圖表動態物件</returns>
        public dynamic GetPerformanceChart(string year, string empno)
        {
            // Get managers that can be reviewed
            User user = _userRepository.Get(empno);
            List<User> users = _userRepository.GetAll();
            List<User> managers = this.GetSubordinateManagers(user);

            List<Performance> performances = (_assessmentRepository as SelfAssessmentTxtRepository).GetAllPerformances("2024H1");

            dynamic performanceChart = new JArray();

            foreach (var manager in managers)
            {
                dynamic managerObj = new JObject();
                managerObj.name = manager.name;
                managerObj.empno = manager.empno;
                managerObj.groups = new JArray();

                List<string> owned_groups = (_userRepository as UserRepository).GetSubGroups(manager.empno);

                foreach (var group in owned_groups)
                {
                    dynamic groupObj = new JObject();
                    groupObj.group_name = group;
                    groupObj.performances = new JArray();

                    var members = this.GetSubGroupMembers(group);

                    foreach (var member in members)
                    {
                        Performance p = performances.Find(x => x.empno == member.empno && x.manager == manager.empno);

                        if (p == null) continue;

                        dynamic memberObj = new JObject();
                        memberObj.empno = member.empno;
                        memberObj.name = member.name;
                        memberObj.score = p.score;

                        groupObj.performances.Add(memberObj);
                    }

                    managerObj.groups.Add(groupObj);

                }

                performanceChart.Add(managerObj);

            }



            return JsonConvert.SerializeObject(performanceChart);
        }

        public List<Performance> GetAllPerformance(string year, string empno)
        {
            return (_assessmentRepository as SelfAssessmentTxtRepository).GetAllPerformances("2024H1");
        }

        private List<User> GetSubGroupMembers(string group)
        {
            List<User> users = new List<User>();
            users = _userRepository.GetAll().Where(x => x.group_one == group || x.group_two == group || x.group_three == group).ToList();
            return users;
        }

        private List<User> GetSubordinateManagers(User user)
        {
            List<User> managers = new List<User>();

            if (user.department_manager)
            {
                managers = _userRepository.GetAll()
                    .Where(x => x.group_manager || x.group_one_manager || x.group_two_manager).ToList();
            }
            else if (user.group_manager)
            {
                managers = _userRepository.GetAll()
                   .Where(x => x.group_one_manager || x.group_two_manager || x.group_three_manager)
                   .Where(x => x.group == user.group).ToList();
            }

            // Add manager himself
            managers.Insert(0, user);

            return managers;
        }


        public bool DeleteAll()
        {
            var ret = _assessmentRepository.DeleteAll();
            return ret;
        }


        public void Dispose()
        {
            _assessmentRepository.Dispose();
            _userRepository.Dispose();
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

    public class ChartEmployeeData
    {
        public User Employee { get; set; }
        public List<Assessment> Responses { get; set; }
    }

    public class ChartManagerResponse
    {
        public User Manager { get; set; }
        public List<List<int>> Votes { get; set; }
        public List<List<string>> Responses { get; set; }
    }

    public class ChartManagerData
    {
        public List<Assessment> ManagerAssessments { get; set; }
        public List<ChartManagerResponse> ChartManagerResponses { get; set; }
    }

    public class FeedbackNotification
    {
        public bool Unread { get; set; }
    }
}