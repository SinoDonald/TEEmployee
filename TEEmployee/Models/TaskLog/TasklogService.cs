using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    public class TasklogService : IDisposable
    {
        private IProjectItemRepository _projectItemRepository;
        private IMonthlyRecordRepository _monthlyRecordRepository;
        private IProjectTaskRepository _projectTaskRepository;
        private IUserRepository _userRepository;

        public TasklogService() : this(true)
        {
            _projectTaskRepository = new ProjectTaskRepository();
            _userRepository = new UserRepository();
        }
        public TasklogService(bool isDB)
        {
            if (isDB) _projectItemRepository = new ProjectItemRepository();
            else _projectItemRepository = new ProjectItemTxtRepository();

            _monthlyRecordRepository = new MonthlyRecordRepository();
        }

        public bool InsertProjectItem(ProjectItem projectItem)
        {
            return _projectItemRepository.Insert(projectItem);
        }

        public bool InsertProjectItem(List<ProjectItem> projectItem)
        {
            return _projectItemRepository.Insert(projectItem);
        }

        public bool UpdateProjectItem(ProjectItem projectItem)
        {
            return _projectItemRepository.Update(projectItem);
        }

        public bool UpsertProjectItem(ProjectItem projectItem)
        {
            return _projectItemRepository.Upsert(projectItem);
        }
        public bool UpsertProjectItem(List<ProjectItem> projectItem)
        {
            return _projectItemRepository.Upsert(projectItem);
        }

        public List<ProjectItem> GetAllProjectItem()
        {
            var ret = _projectItemRepository.GetAll();
            return ret;
        }

        //---------------------------------------------------------
        public bool InsertMonthlyRecord(MonthlyRecord monthlyRecord)
        {
            return _monthlyRecordRepository.Insert(monthlyRecord);
        }

        // 1109: Create monthlyRecord by All users
        public bool CreateMonthlyRecord()
        {
            var users = _userRepository.GetAll();
            List<MonthlyRecord> monthlyRecords = new List<MonthlyRecord>();
            string yymm = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");

            foreach (var item in users)
            {
                MonthlyRecord monthlyRecord = new MonthlyRecord()
                {
                    empno = item.empno,
                    yymm = yymm,
                    guid = Guid.NewGuid()
                };

                monthlyRecords.Add(monthlyRecord);
            }

            return _monthlyRecordRepository.Upsert(monthlyRecords);
        }


        public bool UpsertMonthlyRecord(MonthlyRecord monthlyRecord)
        {
            return _monthlyRecordRepository.Upsert(monthlyRecord);
        }

        public bool UpsertMonthlyRecord(List<MonthlyRecord> monthlyRecord)
        {
            return _monthlyRecordRepository.Upsert(monthlyRecord);
        }

        public List<MonthlyRecord> GetAllMonthlyRecord()
        {
            var ret = _monthlyRecordRepository.GetAll();
            return ret;
        }
        
      
        public List<MonthlyRecord> GetAllMonthlyRecord(string empno, string yymm)
        {            
            List<MonthlyRecord> monthlyRecords;

            var ret = _monthlyRecordRepository.GetAll();
            monthlyRecords = ret.Where(x => x.yymm == yymm).ToList();

            return monthlyRecords;
        }

        // Get record base on the role
        public List<MonthlyRecordData> GetAllMonthlyRecordData(string empno, string yymm)
        {
            List<MonthlyRecordData> monthlyRecordData = new List<MonthlyRecordData>();

            User user = _userRepository.Get(empno);
            List<User> users = FilterEmployeeByRole(user);                       

            var ret = _monthlyRecordRepository.GetAll();
            ret = ret.Where(x => x.yymm == yymm).ToList();

            foreach (var employee in users)
            {
                var record = ret.Where(x => x.empno == employee.empno).FirstOrDefault();

                if(record != null)
                {
                    monthlyRecordData.Add(new MonthlyRecordData() { MonthlyRecord = record, User = employee });
                }
            }

            monthlyRecordData = monthlyRecordData.OrderBy(x => x.User.empno).ToList();

            return monthlyRecordData;
        }


        //----------------------------------------------------------

        //public List<ProjectItem> GetTasklogData(string empno, string yymm)
        //{
        //    if (String.IsNullOrEmpty(yymm))
        //        yymm = Utilities.yymmStr();

        //    yymm = "11107";
        //    List<ProjectItem> projectItems = new List<ProjectItem>();

        //    var ret = _projectItemRepository.GetAll();
        //    projectItems = ret.Where(x => x.empno == empno && x.yymm == yymm)
        //                    .OrderBy(x => x.projno).ThenBy(x => x.itemno).ToList();

        //    return projectItems;
        //}

        //-----------------------------------------------------------

        //public List<ProjectTask> GetProjectTask(string empno, string yymm)
        //{
        //    //if (String.IsNullOrEmpty(yymm))
        //    //    yymm = Utilities.yymmStr();

        //    //yymm = "11107";
        //    List<ProjectTask> projectTasks = new List<ProjectTask>();

        //    var ret = _projectTaskRepository.GetAll();
        //    projectTasks = ret.Where(x => x.empno == empno && x.yymm == yymm)
        //                    .OrderBy(x => x.projno).ThenBy(x => x.id).ToList();

        //    return projectTasks;
        //}



        public bool UpdateProjectTask(List<ProjectTask> projectTasks, string empno)
        {
            projectTasks.ForEach(x => x.empno = empno);

            bool ret = true;

            try
            {
                foreach (var item in projectTasks)
                {
                    if (item.id != 0)
                        _projectTaskRepository.Update(item);
                    else
                        _projectTaskRepository.Insert(item);
                }
            }
            catch
            {
                ret = false;
            }

            return ret;

        }

        public bool DeleteProjectTask(List<int> ids, string empno)
        {         
            bool ret = true;

            try
            {
                ids.ForEach(id => _projectTaskRepository.Delete(id, empno));

                //foreach (var id in ids)
                //{
                //    _projectTaskRepository.Delete(id, empno);
                //}
               
            }
            catch
            {
                ret = false;
            }

            return ret;

        }


        //----------------------------------------------------------------------------

        public TasklogData GetTasklogData(string empno, string yymm)
        {
            List<ProjectItem> projectItems = new List<ProjectItem>();

            var ret = _projectItemRepository.GetAll();
            projectItems = ret.Where(x => x.empno == empno && x.yymm == yymm)
                            .OrderBy(x => x.projno).ThenBy(x => x.itemno).ToList();

            List<ProjectTask> projectTasks = new List<ProjectTask>();

            var ret2 = _projectTaskRepository.GetAll();
            projectTasks = ret2.Where(x => x.empno == empno && x.yymm == yymm)
                            .OrderBy(x => x.projno).ThenBy(x => x.id).ToList();

            return new TasklogData() { ProjectItems = projectItems, ProjectTasks = projectTasks };
        }

        public TasklogData GetTasklogDataByGuid(string guid)
        {
            var record = _monthlyRecordRepository.Get(new Guid(guid));
            var ret = this.GetTasklogData(record.empno, record.yymm);

            return ret;
        }

        public User GetUserByGuid(string guid)
        {
            var record = _monthlyRecordRepository.Get(new Guid(guid));
            var ret = _userRepository.Get(record.empno);

            return ret;
        }


        List<User> FilterEmployeeByRole(User user)
        {
            var allEmployees = _userRepository.GetAll();
            List<User> filtered_employees = new List<User>();

            if (user.department_manager == true)
                filtered_employees.AddRange(allEmployees);

            if (user.group_manager == true)
                filtered_employees.AddRange(allEmployees.Where(p => p.group == user.group).ToList());

            // group one and group two can watch each other

            if (!String.IsNullOrEmpty(user.group_one))
                filtered_employees.AddRange(allEmployees.Where(p => p.group_one == user.group_one).ToList());

            if (!String.IsNullOrEmpty(user.group_two))
                filtered_employees.AddRange(allEmployees.Where(p => p.group_two == user.group_two).ToList());

            if (!String.IsNullOrEmpty(user.group_three))
                filtered_employees.AddRange(allEmployees.Where(p => p.group_three == user.group_three).ToList());

            filtered_employees = filtered_employees.Distinct().ToList();

            return filtered_employees;
        }


        //=============================
        // Insert User From txt file
        //=============================

        public bool InsertUser()
        {
            var userTxtRepository = new UserTxtRepository();            
            var users = userTxtRepository.GetAll();

            var ret = (_userRepository as UserRepository).InsertUser(users);

            return ret;
        }



        //=============================
        // Insert UserExtra 
        //=============================
        public bool InsertUserExtra(List<User> users)
        {           
            var ret = (_userRepository as UserRepository).Insert(users);

            return ret;
        }

               


    //--------------------------------------------------------------------------

    public void Dispose()
        {
            _projectItemRepository.Dispose();
            _monthlyRecordRepository.Dispose();
            if (_projectTaskRepository != null)
            {
                _userRepository.Dispose();
                _projectTaskRepository.Dispose();
            }
                
        }

        public class TasklogData
        {
            public List<ProjectItem> ProjectItems { get; set; }
            public List<ProjectTask> ProjectTasks { get; set; }
        }

        public class MonthlyRecordData
        {
            public MonthlyRecord MonthlyRecord { get; set; }
            public User User { get; set; }
        }


    }
}