using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Kpi
{
    public class KpiService
    {
        private IKpiRepository _kpiRepository;
        private IUserRepository _userRepository;

        public KpiService()
        {
            _kpiRepository = new KpiRepository();
            _userRepository = new UserRepository();
        }

        public List<KpiModel> GetAllKpiModelsByRole(string empno, int year)
        {
            var ret = new List<KpiModel>();
            var kpiModels = _kpiRepository.GetAllKpiModels(year);
            User user = _userRepository.Get(empno);
            var managerGroups = GetManagerGroups(empno);

            // If the User is the manager of a group or subgroups, filter with group.
            if (managerGroups.Count > 0)
            {
                ret.AddRange(kpiModels.Where(x => managerGroups.Contains(x.group_name)).ToList());
            }

            // Get personal Kpi models
            ret.AddRange(kpiModels.Where(x => x.empno == user.empno).ToList());

            // Add employee name
            var users = _userRepository.GetAll();
            foreach (var kpi in ret)
            {
                kpi.name = users.Find(x => x.empno == kpi.empno).name;
            }            
            
            return ret.Distinct().OrderBy(x => x.empno).ToList();
        }

        // 
        public List<KpiItem> UpdateKpiItems(List<KpiItem> items, List<KpiItem> removedItems, string empno)
        {
            // TODO: Check authorization
            
            bool retDelete = true;
            List<KpiItem> ret = null;

            if (removedItems != null)
                retDelete = _kpiRepository.DeleteKpiItems(removedItems);

            if (!retDelete) return ret;

            if (items != null)
                ret = _kpiRepository.UpsertKpiItems(items);

            return ret;
        }

        // Insert, Update and Delete KpiModels based on current Users in database and kpi relationships this year
        public bool InsertKpiModels()
        {
            List<User> users = _userRepository.GetAll();
            List<KpiModel> kpimodels = new List<KpiModel>();
            int year = DateTime.Now.Year;

            // create data from user role
            foreach (var user in users)
            {
                bool isNormal = true;

                // 技術 > 協理 (管理)
                if (user.group_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "管理" });
                    isNormal = false;
                }
                // 大計畫 > 技術 (計畫)
                if (user.project_manager && !user.department_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "計畫" });
                    isNormal = false;
                }
                // 小計畫 > 技術 (計畫, 專業)
                if (user.assistant_project_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "計畫" });
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業" });
                    isNormal = false;
                }
                // 組長 > 技術 (管理, 專業)
                if (user.group_one_manager || user.group_two_manager || user.group_three_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "管理" });
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業" });
                    isNormal = false;
                }
                // 行政
                if (user.group_one == "行政")
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業" });
                    isNormal = false;
                }
                // 一般員工
                if (isNormal)
                {
                    if (!String.IsNullOrEmpty(user.group_one))
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_one, kpi_type = "專業" });
                    if (!String.IsNullOrEmpty(user.group_two))
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_two, kpi_type = "專業" });
                    if (!String.IsNullOrEmpty(user.group_three))
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_three, kpi_type = "專業" });
                }
            }

            // set year
            foreach (var kpi in kpimodels)
                kpi.year = year;

            var ret = _kpiRepository.InsertKpiModels(kpimodels);

            // TODO: Delete kpiModels of employee not in User database
            // Also delete its related KpiItems 

            return ret;
        }

        
        private List<string> GetManagerGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = (_userRepository as UserRepository).GetSubGroups(empno);

            if (user.department_manager)
                groups.AddRange(new List<string>() { "規劃", "設計", "專管" });
            if (user.group_manager)
                groups.Add(user.group);

            return groups;
        }

        private List<string> GetEmployeeGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = new List<string>();

            // add sub group if as a member
            if (!String.IsNullOrEmpty(user.group_one))
                groups.Add(user.group_one);

            if (!String.IsNullOrEmpty(user.group_two))
                groups.Add(user.group_two);

            if (!String.IsNullOrEmpty(user.group_three))
                groups.Add(user.group_three);

            // remove duplicates
            groups = groups.Distinct().ToList();

            return groups;
        }


        public void Dispose()
        {
            _kpiRepository.Dispose();
        }
    }
}