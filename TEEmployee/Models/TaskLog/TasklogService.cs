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


        public TasklogService() : this(true)
        {
            _projectTaskRepository = new ProjectTaskRepository();
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

        public bool UpdateProjectItem(ProjectItem projectItem)
        {
            return _projectItemRepository.Update(projectItem);
        }

        public bool UpsertProjectItem(ProjectItem projectItem)
        {
            return _projectItemRepository.Upsert(projectItem);
        }

        public List<ProjectItem> GetAllProjectItem()
        {
            var ret = _projectItemRepository.GetAll();
            return ret;
        }

        //---------------------------------------------------------

        public bool UpsertMonthlyRecord(MonthlyRecord monthlyRecord)
        {
            return _monthlyRecordRepository.Upsert(monthlyRecord);
        }
       
        public List<MonthlyRecord> GetAllMonthlyRecord()
        {
            var ret = _monthlyRecordRepository.GetAll();
            return ret;
        }
        
        // Get record base on the role
        public List<MonthlyRecord> GetAllMonthlyRecord(string empno, string yymm)
        {
            List<MonthlyRecord> monthlyRecords;

            var ret = _monthlyRecordRepository.GetAll();
            monthlyRecords = ret.Where(x => x.yymm == yymm).ToList();

            return monthlyRecords;
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



        //--------------------------------------------------------------------------

        public void Dispose()
        {
            _projectItemRepository.Dispose();
            _monthlyRecordRepository.Dispose();
            if (_projectTaskRepository != null)
                _projectTaskRepository.Dispose();
        }

        public class TasklogData
        {
            public List<ProjectItem> ProjectItems { get; set; }
            public List<ProjectTask> ProjectTasks { get; set; }
        }

    }
}