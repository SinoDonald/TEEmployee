using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;

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

        // 取得使用者資訊
        public User Get(string id)
        {
            User ret;

            string sql = @"SELECT * FROM user AS u LEFT JOIN userExtra AS e ON u.empno = e.empno WHERE u.empno=@id";
            ret = _conn.Query<User>(sql, new { id }).SingleOrDefault();

            return ret;
        }

        // 上傳PDF
        public bool UploadPDFFile(HttpPostedFileBase file, string view, string empno)
        {
            User user = new UserRepository().GetAll().Where(x => x.empno.Equals(empno)).FirstOrDefault();

            bool ret = false;
            if (Path.GetExtension(file.FileName).Equals(".pdf"))
            {
                string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content"), "GSchedule", view);
                // 檢查資料夾是否存在
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string group = user.group;
                if (group.Equals("")) { group = "行政"; }
                var path = Path.Combine(folderPath, group + Path.GetExtension(file.FileName));
                if (view.Equals("PersonalPlan"))
                {
                    path = Path.Combine(folderPath, user.empno + Path.GetExtension(file.FileName));
                }
                file.SaveAs(path); // 將檔案存到Server
                ret = true;
            }
            return ret;
        }

        // 取得PDF
        public string GetPDF(string view, string group, string userName)
        {
            User user = new UserRepository().GetAll().Where(x => x.name.Equals(userName)).FirstOrDefault();

            string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content"), "GSchedule", view);
            // 檢查資料夾是否存在
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var ret = Path.Combine("Content", "GSchedule", view, user.group + ".pdf");
            if (view.Equals("GroupPlan"))
            {
                if (user.group.Equals(""))
                {
                    if (user.department_manager)
                    {
                        ret = Path.Combine("Content", "GSchedule", view, group + ".pdf");
                    }
                    else
                    {
                        ret = Path.Combine("Content", "GSchedule", view, "行政.pdf");
                    }
                }
            }
            else
            {
                ret = Path.Combine("Content", "GSchedule", view, user.empno + ".pdf");
            }
            
            return ret;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }


    }
}