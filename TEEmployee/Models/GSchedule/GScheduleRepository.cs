using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TEEmployee.Models.GSchedule
{
    public class GScheduleRepository : IGScheduleRepository, IDisposable
    {
        private IDbConnection _conn;

        public GScheduleRepository()
        {
            string scheduleConnection = ConfigurationManager.ConnectionStrings["GScheduleConnection"].ConnectionString;
            _conn = new SQLiteConnection(scheduleConnection);
        }

        public List<Schedule> GetAll()
        {
            var lookup = new Dictionary<int, Schedule>();
            _conn.Query<Schedule, Milestone, Schedule>(@"
                SELECT s.*, m.*
                FROM Schedule AS s
                LEFT JOIN Milestone AS m ON s.id = m.schedule_id                   
                ", (s, m) =>
            {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                    schedule.milestones.Add(m);
                return schedule;
            }).AsQueryable();
            var resultList = lookup.Values;

            return resultList.ToList();
        }

        public List<Schedule> GetAllGroupSchedules(string role)
        {
            var lookup = new Dictionary<int, Schedule>();
            _conn.Query<Schedule, Milestone, Schedule>(@"
                SELECT s.*, m.*
                FROM Schedule AS s
                LEFT JOIN Milestone AS m ON s.id = m.schedule_id
                WHERE s.role=@role
                ", (s, m) =>
            {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                    schedule.milestones.Add(m);
                return schedule;
            }, new { role }).AsQueryable();
            var resultList = lookup.Values;

            return resultList.ToList();
        }


        public Schedule Update(Schedule schedule)
        {
            if (_conn.State == 0)
                _conn.Open();

            int ret;

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"UPDATE Schedule SET member=@member, content=@content, start_date=@start_date, end_date=@end_date, percent_complete=@percent_complete, last_percent_complete=@last_percent_complete, history=@history, projno=@projno WHERE id=@id";

                try
                {
                    ret = _conn.Execute(sql, schedule, tran);

                    var MilestoneIDsInDb = GetMilestones(schedule.id).Select(x => x.id).ToList();


                    // update or insert milestones
                    if (schedule.milestones != null)
                    {
                        foreach (Milestone milestone in schedule.milestones)
                        {
                            if (MilestoneIDsInDb.Contains(milestone.id))
                            {
                                Update(milestone, tran);
                                MilestoneIDsInDb.Remove(milestone.id);
                            }
                            else
                            {
                                milestone.id = Insert(milestone, tran);
                            }
                        }
                    }

                    // delete milestones
                    string deletedMilestoneSql = @"DELETE FROM Milestone WHERE id=@id;";

                    MilestoneIDsInDb?.ForEach(id =>
                    {
                        ret = _conn.Execute(deletedMilestoneSql, new { id }, tran);
                    });


                    tran.Commit();
                    return schedule;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public Schedule Insert(Schedule schedule)
        {
            if (_conn.State == 0)
                _conn.Open();

            //int ret;

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"INSERT INTO Schedule (empno, member, type, content, start_date, end_date, percent_complete, last_percent_complete, parent_id, history, projno, role) 
                        VALUES(@empno, @member, @type, @content, @start_date, @end_date, @percent_complete, @last_percent_complete, @parent_id, @history, @projno, @role) 
                        RETURNING id";
                try
                {
                    int schedule_id = _conn.QuerySingle<int>(sql, schedule, tran);
                    schedule.id = schedule_id;

                    if (schedule.milestones != null)
                    {
                        foreach (Milestone milestone in schedule.milestones)
                        {
                            milestone.schedule_id = schedule_id;
                            milestone.id = Insert(milestone, tran);
                        }
                    }

                    tran.Commit();
                    return schedule;
                }
                catch (Exception)
                {
                    return null;
                }

            }

        }

        // update percent complete
        public bool Update(List<Schedule> schedules)
        {
            _conn.Open();

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"UPDATE Schedule SET percent_complete=@percent_complete, last_percent_complete=@last_percent_complete WHERE id=@id";
                ret = _conn.Execute(sql, schedules, tran);
                tran.Commit();

                return ret > 0;

            }
        }

        private List<Milestone> GetMilestones(int schedule_id)
        {
            List<Milestone> ret;

            string sql = @"SELECT * FROM Milestone WHERE schedule_id = @schedule_id";
            ret = _conn.Query<Milestone>(sql, new { schedule_id }).ToList();

            return ret;
        }

        private int Insert(Milestone milestone, IDbTransaction tran)
        {
            string insertMilestoneSql = @"INSERT INTO Milestone (content, date, schedule_id) 
                        VALUES(@content, @date, @schedule_id) 
                        RETURNING id";

            int ret = _conn.QuerySingle<int>(insertMilestoneSql, milestone, tran);

            return ret;
        }

        private void Update(Milestone milestone, IDbTransaction tran)
        {
            string updateMilestoneSql = @"UPDATE Milestone SET content=@content, date=@date WHERE id=@id";
            _conn.Execute(updateMilestoneSql, milestone, tran);
        }

        public bool Delete(List<Schedule> schedules)
        {
            if (_conn.State == 0)
                _conn.Open();

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {
                string deleteScheduleSql = @"DELETE FROM Schedule WHERE id=@id;";
                string deleteMilestoneSql = @"DELETE FROM Milestone WHERE id=@id;";

                try
                {
                    schedules?.ForEach(schedule =>
                    {
                        ret = _conn.Execute(deleteScheduleSql, schedule, tran);

                        schedule.milestones?.ForEach(milestone =>
                        {
                            ret = _conn.Execute(deleteMilestoneSql, milestone, tran);
                        });

                    });

                    tran.Commit();
                }
                catch
                {
                    ret = 0;
                }

            }

            return ret > 0;
        }

        public List<ProjectSchedule> GetAllProjectSchedules()
        {
            List<ProjectSchedule> ret;

            string sql = @"SELECT * FROM Project ORDER BY projno";
            ret = _conn.Query<ProjectSchedule>(sql).ToList();

            return ret;
        }

        public bool Insert(ProjectSchedule projectSchedule)
        {
            string sql = @"INSERT INTO Project (projno) 
                        VALUES(@projno)";

            int ret = _conn.Execute(sql, projectSchedule);

            return ret > 0;
        }

        public bool Update(ProjectSchedule projectSchedule)
        {
            string sql = @"UPDATE Project SET filepath=@filepath WHERE projno=@projno;";

            int ret = _conn.Execute(sql, projectSchedule);

            return ret > 0;
        }

        public bool Delete(ProjectSchedule projectSchedule)
        {
            string sql = @"DELETE FROM Project WHERE projno=@projno;";

            int ret = _conn.Execute(sql, projectSchedule);

            return ret > 0;
        }
        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User Get(string id)
        {
            User ret;

            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno WHERE u.empno=@id";
            ret = _conn.Query<User>(sql, new { id }).SingleOrDefault();

            return ret;
        }
        /// <summary>
        /// 取得年份
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<string> GetYears(string view)
        {
            // 當前民國年
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            string thisYear = DateTime.Now.ToString("yyy", culture);
            int month = DateTime.Now.Month;

            List<string> ret = new List<string>();

            //string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content"), "GSchedule", view);
            string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "GSchedule", view);
            string yearFolderPath = Path.Combine(folderPath, thisYear);
            if (!Directory.Exists(yearFolderPath)) { Directory.CreateDirectory(yearFolderPath); }
            // 11月即增加明年度的資料夾
            if (month >= 11)
            {
                string nextYear = (Convert.ToInt32(thisYear) + 1).ToString();
                string nextYearFolderPath = Path.Combine(folderPath, nextYear);
                if (!Directory.Exists(nextYearFolderPath)) { Directory.CreateDirectory(nextYearFolderPath); }
            }
            // 查詢所有子資料夾
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            ret = dirInfo.GetDirectories().Select(x => x.Name).OrderByDescending(x => x).ToList();

            return ret;
        }
        /// <summary>
        /// 上傳PDF
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string UploadPDFFile(HttpPostedFileBase file, string view, string year, string empno, string folder)
        {
            string path = "";

            try
            {
                //// 當前民國年
                //CultureInfo culture = new CultureInfo("zh-TW");
                //culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                //string year = DateTime.Now.ToString("yyy", culture);

                User user = new UserRepository().GetAll().Where(x => x.empno.Equals(empno)).FirstOrDefault();

                if (Path.GetExtension(file.FileName).Equals(".pdf"))
                {
                    try
                    {
                        //string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content"), "GSchedule", view, year);
                        string folderPath = Path.Combine(HttpContext.Current.Server.MapPath(folder), "GSchedule", view, year);
                        // 檢查資料夾是否存在
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        try
                        {
                            string group = user.group;
                            if (group.Equals(""))
                            {
                                string group_one = new UserRepository().GetAll().Where(x => String.IsNullOrEmpty(x.group)).Where(x => !String.IsNullOrEmpty(x.group_one)).Select(x => x.group_one).Distinct().FirstOrDefault();
                                group = group_one;
                            }
                            path = Path.Combine(folderPath, group + Path.GetExtension(file.FileName));
                            if (view.Equals("PersonalPlan"))
                            {
                                path = Path.Combine(folderPath, user.empno + Path.GetExtension(file.FileName));
                            }
                            try
                            {
                                file.SaveAs(path); // 將檔案存到Server
                            }
                            catch (Exception)
                            {
                                folderPath = Path.Combine("Content", "GSchedule", view, year);
                                path = Path.Combine(folderPath, group + Path.GetExtension(file.FileName));
                                if (view.Equals("PersonalPlan"))
                                {
                                    path = Path.Combine(folderPath, user.empno + Path.GetExtension(file.FileName));
                                }
                                file.SaveAs(path); // 將檔案存到Server
                            }
                        }
                        catch (Exception)
                        {
                            path = "Error";
                        }
                    }
                    catch (Exception ex)
                    {
                        path = "Error：Exist無法檢查 or 資料夾無法建立\n" + ex.Message + "\n" + ex.ToString();
                    }
                }
            }
            catch(Exception ex)
            {
                path = "Error：年份 or User.db有誤\n" + ex.Message + "\n" + ex.ToString();
            }

            return path;
        }
        /// <summary>
        /// 上傳PDF
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string ImportPDFFile(HttpPostedFileBase file, string empno)
        {
            string ret = "";
            try
            {
                if (Path.GetExtension(file.FileName) != ".pdf") throw new ApplicationException("請使用PDF(.pdf)格式");

                //string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Files"));
                string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "GSchedule", "PersonalPlan", "113");
                // 檢查資料夾是否存在
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string extension = Path.GetExtension(file.FileName);
                var path = Path.Combine(folderPath, empno + Path.GetExtension(file.FileName));
                file.SaveAs(path); // 將檔案存到Server
            }
            catch (Exception ex)
            {
                ret = "Error：" + ex.Message + "\n" + ex.ToString();
            }

            return ret;
        }
        /// <summary>
        /// 取得PDF
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetPDF(string view, string year, string group, string userName)
        {
            string folder = "App_Data";
            //string folder = "Content";

            if (String.IsNullOrEmpty(year))
            {
                // 當前民國年
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                year = DateTime.Now.ToString("yyy", culture);
            }

            User user = new UserRepository().GetAll().Where(x => x.name.Equals(userName)).FirstOrDefault();
            string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/" + folder), "GSchedule", view, year);
            // 檢查資料夾是否存在
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string relativePath = Path.Combine(folder, "GSchedule", view, year);
            relativePath = folderPath;
            var ret = Path.Combine(relativePath, user.group + ".pdf");
            if (view.Equals("GroupPlan"))
            {
                if (user.group.Equals(""))
                {
                    if (user.department_manager)
                    {
                        ret = Path.Combine(relativePath, group + ".pdf");
                    }
                    else
                    {
                        string group_one = new UserRepository().GetAll().Where(x => String.IsNullOrEmpty(x.group)).Where(x => !String.IsNullOrEmpty(x.group_one)).Select(x => x.group_one).Distinct().FirstOrDefault();
                        ret = Path.Combine(relativePath, group_one + ".pdf");
                    }
                }
            }
            else
            {
                ret = Path.Combine(relativePath, user.empno + ".pdf");
            }
            
            return ret;
        }
        /// <summary>
        /// 取得主管回饋
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Planning> GetResponse(string view, string year, string group, string empno, string name)
        {
            List<Planning> ret = new List<Planning>();

            if (String.IsNullOrEmpty(year))
            {
                // 當前民國年
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                year = DateTime.Now.ToString("yyy", culture);
            }

            // 確認是否有Planning資料表, 沒有則CREATE
            if (view.Equals("PersonalPlan"))
            {
                User user = new UserRepository().GetAll().Where(x => x.name.Equals(name)).FirstOrDefault();
                string userEmpno = user.empno;
                List<Planning> plannings = new List<Planning>();

                _conn.Open();
                SQLiteConnection conn = (SQLiteConnection)_conn;
                DataTable dataTable = conn.GetSchema("Tables");
                bool tableExist = false;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (dataRow[2].ToString().Equals("Planning")) { tableExist = true; break; }
                }
                if (tableExist == false)
                {
                    using (var tran = _conn.BeginTransaction())
                    {
                        SQLiteCommand sqliteCmd = (SQLiteCommand)_conn.CreateCommand();
                        sqliteCmd.CommandText = "CREATE TABLE IF NOT EXISTS Planning (view TEXT, year TEXT, 'group' TEXT, empno INTEGER, user_name TEXT, manager_id INTEGER, manager_name TEXT, response TEXT)";
                        sqliteCmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"SELECT * FROM Planning WHERE empno=@userEmpno";
                    plannings = _conn.Query<Planning>(sql, new { userEmpno }).Where(x => x.year.Equals(year)).ToList();
                    if (plannings.Where(x => x.manager_id.ToString().Equals(empno)).Count().Equals(0) && !userEmpno.Equals(empno))
                    {
                        Planning planning = new Planning();
                        planning.view = view;
                        planning.year = year;
                        planning.group = group;
                        planning.empno = Convert.ToInt32(userEmpno);
                        planning.user_name = name;
                        planning.manager_id = Convert.ToInt32(empno);
                        planning.manager_name = new UserRepository().GetAll().Where(x => x.empno.Equals(empno)).Select(x => x.name).FirstOrDefault();
                        planning.response = "";
                        plannings.Add(planning);
                    }
                    tran.Commit();
                }

                ret = plannings;
            }

            return ret;
        }
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SaveResponse(string view, string year, string group, string manager_id, string name, List<Planning> response)
        {
            if(year == null)
            {
                // 當前民國年
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                year = DateTime.Now.ToString("yyy", culture);
            }

            List<User> users = new UserRepository().GetAll();
            User manager = users.Where(x => x.empno.Equals(manager_id)).FirstOrDefault();
            User user = users.Where(x => x.name.Equals(name)).FirstOrDefault();
            string empno = user.empno;
            Planning planning = response.Where(x => x.empno.ToString().Equals(user.empno)).Where(x => x.manager_id.ToString().Equals(manager_id)).FirstOrDefault();
            if(planning != null)
            {
                planning.view = view;
                if (!String.IsNullOrEmpty(user.group)) { planning.group = user.group; }
                else if (!String.IsNullOrEmpty(user.group_one)) { planning.group = user.group_one; }
                else if (!String.IsNullOrEmpty(user.group_two)) { planning.group = user.group_two; }
                else if (!String.IsNullOrEmpty(user.group_three)) { planning.group = user.group_three; }
            }

            var ret = false;
            try
            {
                // 先判斷SQL內有無此筆資料, 有的話UPDATE、沒有的話INSERT
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"SELECT * FROM Planning WHERE empno=@empno";
                    List<Planning> plannings = _conn.Query<Planning>(sql, new { empno }).Where(x => x.year.Equals(year)).ToList();
                    if (plannings.Where(x => x.manager_id.ToString().Equals(manager_id)).Count().Equals(0))
                    {
                        sql = @"INSERT INTO Planning (view, year, 'group', empno, user_name, manager_id, manager_name, response) 
                        VALUES(@view, @year, @group, @empno, @user_name, @manager_id, @manager_name, @response) 
                        RETURNING empno";
                        _conn.Execute(sql, planning, tran);
                        tran.Commit();
                        ret = true;
                    }
                    else
                    {
                        //string sql = @"UPDATE Planning SET view=@view, year=@year, 'group'=@group, empno=@empno, user_name=@user_name, manager_id=@manager_id, manager_name=@manager_name, response=@response WHERE empno=empno AND manager_id=@manager_id";
                        sql = @"UPDATE Planning SET response=@response WHERE year=@year AND empno=@empno AND manager_id=@manager_id";
                        _conn.Execute(sql, planning, tran);
                        tran.Commit();
                        ret = true;
                    }
                }
                _conn.Close();
            }
            catch (Exception ex) { string error = ex.Message + "\n" + ex.ToString(); }

            return ret;
        }
        /// <summary>
        /// 刪除群組規劃
        /// </summary>
        /// <returns></returns>
        public bool DeleteGroupPlan()
        {
            bool ret = false;
            string folder = "App_Data";
            string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/" + folder), "GSchedule");
            // 檢查資料夾是否存在
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string relativePath = Path.Combine(folder, "GSchedule", "GroupPlan");
            relativePath = Path.Combine(folderPath, "GroupPlan");
            try { Directory.Delete(relativePath, true); ret = true; }
            catch(Exception ex) { string error = ex.Message + "\n" + ex.ToString(); }

            return ret;
        }
        /// <summary>
        /// 刪除個人規劃
        /// </summary>
        /// <returns></returns>
        public bool DeletePersonalPlan()
        {
            bool ret = false;
            string folder = "App_Data";
            string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/" + folder), "GSchedule");
            // 檢查資料夾是否存在
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string relativePath = Path.Combine(folder, "GSchedule", "PersonalPlan");
            relativePath = Path.Combine(folderPath, "PersonalPlan");
            try { Directory.Delete(relativePath, true); ret = true; }
            catch { }

            return ret;
        }


        public bool DeleteAll()
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(@"DELETE FROM Milestone", tran);
                    _conn.Execute(@"DELETE FROM Schedule", tran);

                    tran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }

        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }


    }
}