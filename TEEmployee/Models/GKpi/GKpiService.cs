using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GKpi
{
    public class GKpiService
    {
        private IGKpiRepository _kpiRepository;
        private IUserRepository _userRepository;

        public GKpiService()
        {
            _kpiRepository = new GKpiRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// 根據員工權限取得所有員工KPI及細項
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <param name="year">年度</param>
        /// <returns>包含所有員工KPI的列舉</returns>
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
                kpi.items = kpi.items.OrderBy(x => x.id).ToList();
            }            

            return ret.Distinct().OrderBy(x => x.empno).ToList();
        }

        /// <summary>
        /// 更新多筆KPI細項
        /// </summary>
        /// <param name="items">KPI細項列舉</param>
        /// <param name="removedItems">欲刪除的KPI細項列舉</param>
        /// <returns>更新之KPI細項列舉</returns>
        /// <remarks>包含更新及刪除項目</remarks>
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
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "管理", role = "技術經理" });
                    isNormal = false;
                }
                // 大計畫 > 技術 (計畫)
                if (user.project_manager && !user.department_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "計畫", role = "重大計畫經理" });
                    isNormal = false;
                }
                // 小計畫 > 技術 (計畫)
                if (user.assistant_project_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "計畫", role = "一般計畫經理" });                    
                    //isNormal = false;
                }
                // 組長 > 技術 (管理, 專業)
                if (user.group_one_manager/* || user.group_two_manager || user.group_three_manager*/)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "管理", role = "組長" });
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業", role = "組長" });
                    isNormal = false;
                }
                // 行政
                if (user.group_one == "行政")
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業", role = "行政專員" });
                    isNormal = false;
                }
                // 一般員工
                if (isNormal)
                {
                    if (!String.IsNullOrEmpty(user.group_one))
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_one, kpi_type = "專業", role = "一般工程師" });
                    //if (!String.IsNullOrEmpty(user.group_two))
                    //    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_two, kpi_type = "專業" });
                    //if (!String.IsNullOrEmpty(user.group_three))
                    //    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_three, kpi_type = "專業" });
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

        /// <summary>
        /// 取得權限包含之群組
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>群組名稱字串列舉</returns>
        private List<string> GetManagerGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = (_userRepository as UserRepository).GetSubGroups(empno);

            if (user.department_manager)
            {
                if (user.gid == "24")
                {
                    //groups.AddRange(new List<string>() { "規劃", "設計", "專管" });
                    var allEmployees = _userRepository.GetAll();
                    List<string> grouplist = allEmployees.Select(x => x.group).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    groups.AddRange(grouplist);
                }
                else // For other departments
                {
                    var allEmployees = _userRepository.GetAll();
                    List<string> grouplist = allEmployees.Select(x => x.group).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    groups.AddRange(grouplist);
                }
            }
                
            if (user.group_manager)
                groups.Add(user.group);

            return groups;
        }

        //private List<string> GetEmployeeGroups(string empno)
        //{
        //    User user = _userRepository.Get(empno);
        //    List<string> groups = new List<string>();

        //    // add sub group if as a member
        //    if (!String.IsNullOrEmpty(user.group_one))
        //        groups.Add(user.group_one);

        //    if (!String.IsNullOrEmpty(user.group_two))
        //        groups.Add(user.group_two);

        //    if (!String.IsNullOrEmpty(user.group_three))
        //        groups.Add(user.group_three);

        //    // remove duplicates
        //    groups = groups.Distinct().ToList();

        //    return groups;
        //}

        // DLC

        /// <summary>
        /// 取得個人年度KPI項目
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <param name="int">年度</param>
        /// <returns>個人年度KPI項目列舉</returns>
        public List<KpiModel> GetEmployeeKpiModelsByRole(string empno, int year)
        {
            var ret = new List<KpiModel>();
            var kpiModels = _kpiRepository.GetAllKpiModels(year);
            User user = _userRepository.Get(empno);

            // Get personal Kpi models
            ret.AddRange(kpiModels.Where(x => x.empno == user.empno).ToList());

            // Add employee name
            var users = _userRepository.GetAll();
            foreach (var kpi in ret)
            {
                kpi.name = users.Find(x => x.empno == kpi.empno).name;
                kpi.items = kpi.items.OrderBy(x => x.id).ToList();
            }

            // Filter by group again in case employee change their group in the same year
            List<string> grouplist = new List<string> { user.group, user.group_one, user.group_two, user.group_three };
            grouplist.RemoveAll(x => string.IsNullOrEmpty(x));
            ret = ret.Where(x => grouplist.Contains(x.group_name)).ToList();

            return ret.Distinct().OrderBy(x => x.empno).ToList();
        }

        /// <summary>
        /// 取得主管權限下包含的員工年度KPI項目
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <param name="int">年度</param>
        /// <returns>個人年度KPI項目列舉</returns>
        public List<KpiModel> GetManagerKpiModelsByRole(string empno, int year)
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

            // Add employee name
            var users = _userRepository.GetAll();
            foreach (var kpi in ret)
            {
                kpi.name = users.Find(x => x.empno == kpi.empno)?.name;
                kpi.items = kpi.items.OrderBy(x => x.id).ToList();
            }

            // Remove special case (技術經理 -> 協理)
            ret.RemoveAll(x => x.name == user.name);

            return ret.Distinct().OrderBy(x => x.empno).ToList();
        }

        /// <summary>
        /// 上傳個人年度KPI項目
        /// </summary>
        /// <param name="input">年度KPI項目Stream</param>
        /// <returns>個人年度KPI項目列舉</returns>
        /// <remarks>上傳EXCEL檔，匯入資料庫</remarks>
        public List<KpiModel> UploadKpiFile(Stream input, string empno)
        {
            string gid = _userRepository.Get(empno).gid;

            // Insert KpiModels then Insert Kpiitems
            List<KpiModel> processedInput = ProcessKpiXlsx(input);
            List<KpiModel> insertedModels = InsertKpiModelsNew();

            List<KpiItem> createdKpiitems = new List<KpiItem>();



            // special case
            // 專管組長有細分
            // 規劃組員無細分

            if (gid == "24")
            {

                foreach (var model in insertedModels)
                {
                    KpiModel kpm = new KpiModel();

                    if (model.role == "技術經理")
                    {
                        kpm = processedInput.Where(x => x.role == "技術經理").FirstOrDefault();

                        // 技術經理也分群
                        //User user = _userRepository.Get(model.empno);
                        //kpm = processedInput.Where(x =>
                        //        x.role.Contains(user.group_one.Substring(0, 2)) &&
                        //        x.role.Contains("組員")).FirstOrDefault();
                    }
                    else if (model.role == "重大計畫經理")
                    {
                        kpm = processedInput.Where(x => x.role == "重大計畫經理").FirstOrDefault();
                    }
                    else if (model.role == "一般計畫經理")
                    {
                        kpm = processedInput.Where(x => x.role == "一般計畫經理").FirstOrDefault();
                    }
                    else if (model.role == "組長")
                    {
                        // 管理 kpi = Excel 組長 kpi

                        if (model.kpi_type == "管理")
                        {
                            User user = _userRepository.Get(model.empno);
                            kpm = processedInput.Where(x =>
                                x.role.Contains(user.group_one.Substring(0, 2)) &&
                                x.role.Contains("組長")).FirstOrDefault();

                            //if (model.group_name == "專管")
                            //{
                            //    User user = _userRepository.Get(model.empno);
                            //    kpm = processedInput.Where(x =>
                            //        x.role.Contains(user.group_one.Substring(0, 2)) &&
                            //        x.role.Contains("組長")).FirstOrDefault();
                            //}
                            //else
                            //{
                            //    kpm = processedInput.Where(x =>
                            //        x.role.Contains(model.group_name.Substring(0, 2)) &&
                            //        x.role.Contains("組長")).FirstOrDefault();
                            //}
                        }
                        // 專業 kpi = Excel 組員 kpi
                        else
                        {
                            User user = _userRepository.Get(model.empno);
                            kpm = processedInput.Where(x =>
                                x.role.Contains(user.group_one.Substring(0, 2)) &&
                                x.role.Contains("組員")).FirstOrDefault();

                            //if (model.group_name == "專管" || model.group_name == "設計")
                            //{
                            //    User user = _userRepository.Get(model.empno);
                            //    kpm = processedInput.Where(x =>
                            //        x.role.Contains(user.group_one.Substring(0, 2)) &&
                            //        x.role.Contains("組員")).FirstOrDefault();
                            //}
                            //else
                            //{
                            //    kpm = processedInput.Where(x =>
                            //        x.role.Contains(model.group_name.Substring(0, 2)) &&
                            //        x.role.Contains("組員")).FirstOrDefault();
                            //}
                        }


                    }
                    else if (model.role == "一般工程師")
                    {
                        kpm = processedInput.Where(x =>
                        x.role.Contains(model.group_name.Substring(0, 2)) &&
                        x.role.Contains("組員")).FirstOrDefault();

                        //if (model.group_name == "排水管線組" || model.group_name == "營運規劃組" || model.group_name == "交通工程組" || model.group_name == "用地開發組")
                        //{
                        //    kpm = processedInput.Where(x => x.role == "規劃群組組員").FirstOrDefault();
                        //}
                        //else
                        //{
                        //    kpm = processedInput.Where(x =>
                        //    x.role.Contains(model.group_name.Substring(0, 2)) &&
                        //    x.role.Contains("組員")).FirstOrDefault();
                        //}


                    }
                    else if (model.role == "行政專員")
                    {

                    }

                    if (kpm == null || kpm.items == null)
                    {

                    }
                    else
                    {
                        //List<KpiItem> kpiItems = kpm.items.GetRange(0, kpm.items.Count);
                        List<KpiItem> kpiItems = kpm.items;
                        foreach (var kpiitem in kpiItems)
                        {
                            KpiItem kp = kpiitem.ShallowCopy();
                            kp.kpi_id = model.id;
                            createdKpiitems.Add(kp);
                        }


                        //createdKpiitems.AddRange(kpiItems);
                    }



                }



            }
            else // For other departments
            {
                foreach (var model in insertedModels)
                {
                    KpiModel kpm = new KpiModel();

                    if (model.role == "技術經理")
                    {
                        kpm = processedInput.Where(x => x.role == "技術經理").FirstOrDefault();
                    }
                    else if (model.role == "重大計畫經理")
                    {
                        kpm = processedInput.Where(x => x.role == "重大計畫經理").FirstOrDefault();
                    }
                    else if (model.role == "一般計畫經理")
                    {
                        kpm = processedInput.Where(x => x.role == "一般計畫經理").FirstOrDefault();
                    }
                    else if (model.role == "組長")
                    {
                        // 管理 kpi = Excel 組長 kpi

                        if (model.kpi_type == "管理")
                        {
                            
                            User user = _userRepository.Get(model.empno);
                            kpm = processedInput.Where(x =>
                                x.role.Contains(user.group_one.Substring(0, user.group_one.Length - 1)) &&
                                x.role.Contains("組長")).FirstOrDefault();
                           
                        }
                        // 專業 kpi = Excel 組員 kpi
                        else
                        {
                           
                            User user = _userRepository.Get(model.empno);
                            kpm = processedInput.Where(x =>
                                x.role.Contains(user.group_one.Substring(0, user.group_one.Length - 1)) &&
                                x.role.Contains("組員")).FirstOrDefault();
                          
                        }


                    }
                    else if (model.role == "一般工程師")
                    {
                       
                        kpm = processedInput.Where(x =>
                        x.role.Contains(model.group_name.Substring(0, model.group_name.Length - 1)) &&
                        x.role.Contains("組員")).FirstOrDefault();


                    }
                    else if (model.role == "行政專員")
                    {

                    }

                    if (kpm == null || kpm.items == null)
                    {

                    }
                    else
                    {
                        //List<KpiItem> kpiItems = kpm.items.GetRange(0, kpm.items.Count);
                        List<KpiItem> kpiItems = kpm.items;
                        foreach (var kpiitem in kpiItems)
                        {
                            KpiItem kp = kpiitem.ShallowCopy();
                            kp.kpi_id = model.id;
                            createdKpiitems.Add(kp);
                        }


                        //createdKpiitems.AddRange(kpiItems);
                    }



                }

            }


            this.UpdateKpiItems(createdKpiitems, new List<KpiItem>(), "");


            return null;
        }

        /// <summary>
        /// 年度KPI項目Excel資料轉換
        /// </summary>
        /// <param name="input">年度KPI項目Stream</param>
        /// <returns>個人年度KPI項目列舉</returns>
        private List<KpiModel> ProcessKpiXlsx(Stream input)
        {
            var ret = new List<KpiModel>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(input))
            {
                var sheets = package.Workbook.Worksheets;

                foreach (var sheet in sheets)
                {
                    KpiModel kpiModel = new KpiModel();

                    for (int i = 1; i <= sheet.Dimension.End.Row; i++)
                    {
                        if (sheet.Cells[i, 2].Value as string == "編號")
                        {
                            if (kpiModel.role != null)
                            {
                                ret.Add(kpiModel);
                            }

                            kpiModel = new KpiModel() { items = new List<KpiItem>() };
                            kpiModel.role = sheet.Cells[i - 1, 2].GetValue<string>();
                        }

                        else
                        {
                            try
                            {
                                int? n = sheet.Cells[i, 2].GetValue<int?>();

                                if (n != null)
                                {
                                    kpiModel.items.Add(new KpiItem
                                    {
                                        content = sheet.Cells[i, 3].GetValue<string>(),
                                        target = sheet.Cells[i, 4].GetValue<string>(),
                                        weight = sheet.Cells[i, 6].GetValue<double>(),
                                    });
                                }

                            }
                            catch
                            {

                            }

                        }

                        if (i == sheet.Dimension.End.Row)
                            ret.Add(kpiModel);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 根據年度KPI項目建立個人KPI項目
        /// </summary>
        /// <returns>個人年度KPI項目列舉</returns>
        // Insert, Update and Delete KpiModels based on current Users in database and kpi relationships this year
        public List<KpiModel> InsertKpiModelsNew()
        {
           
            List<User> users = _userRepository.GetAll();
            List<KpiModel> kpimodels = new List<KpiModel>();
            
            
            int year = DateTime.Now.Year;

            // Create next year data when it is Nov
            if (DateTime.Now.Month >= 11)
                year += 1;

            // create data from user role
            foreach (var user in users)
            {
                bool isNormal = true;

                // 技術 > 協理 (管理)
                if (user.group_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "管理", role = "技術經理" });
                    isNormal = false;
                }
                // 大計畫 > 技術 (計畫)
                if (user.project_manager && !user.department_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "計畫", role = "重大計畫經理" });
                    isNormal = false;
                }
                // 小計畫 > 技術 (計畫, 專業)
                if (user.assistant_project_manager)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "計畫", role = "一般計畫經理" });
                    //kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業", role = "小型計畫經理" });
                    //isNormal = false;
                }
                // 組長 > 技術 (管理, 專業)
                if (user.group_one_manager/* || user.group_two_manager || user.group_three_manager*/)
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "管理", role = "組長" });

                    // Special case: 技術長 先不要有組長專業KPI
                    if (!(user.group_two_manager && user.group_three_manager))
                    {
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業", role = "組長" });
                    }

                    //kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業", role = "組長" });
                    //isNormal = false;
                }
                // 行政
                if (user.group_one == "行政")
                {
                    kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group, kpi_type = "專業", role = "行政專員" });
                    isNormal = false;
                }
                // 一般員工
                if (isNormal)
                {
                    if (!String.IsNullOrEmpty(user.group_one) && !user.group_one_manager)
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_one, kpi_type = "專業", role = "一般工程師" });
                    if (!String.IsNullOrEmpty(user.group_two) && !user.group_two_manager)
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_two, kpi_type = "專業", role = "一般工程師" });
                    if (!String.IsNullOrEmpty(user.group_three) && !user.group_three_manager)
                        kpimodels.Add(new KpiModel { empno = user.empno, group_name = user.group_three, kpi_type = "專業", role = "一般工程師" });
                }
            }

            // set year
            foreach (var kpi in kpimodels)
                kpi.year = year;

            var ret = _kpiRepository.InsertKpiModelsNew(kpimodels);

            // TODO: Delete kpiModels of employee not in User database
            // Also delete its related KpiItems 

            return ret;
        }

        public bool DeleteKpiModels(string year)
        {
            var ret = _kpiRepository.DeleteKpiModelsByYear(int.Parse(year));
            return ret;
        }

        public bool DeleteSolitaryKpiModels()
        {
            var ret = _kpiRepository.DeleteSolitaryKpiModels();
            return ret;
        }

        public bool DeleteAll()
        {
            var ret = _kpiRepository.DeleteAll();
            return ret;
        }

        public void Dispose()
        {
            _kpiRepository.Dispose();
        }


    }
}