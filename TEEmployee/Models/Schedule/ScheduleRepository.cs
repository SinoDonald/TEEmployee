using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Schedule
{
    public class ScheduleRepository : IScheduleRepository, IDisposable
    {
        private IDbConnection _conn;

        public ScheduleRepository()
        {
            string scheduleConnection = ConfigurationManager.ConnectionStrings["MiscConnection"].ConnectionString;
            _conn = new SQLiteConnection(scheduleConnection);
        }


        public bool Delete(Schedule schedule)
        {
            throw new NotImplementedException();
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

        public bool Delete(List<int> schedules, List<int> milestones)
        {
            if (_conn.State == 0)
                _conn.Open();

            int ret = 0;

            
            //ret = _conn.Execute(sql, new { id = id, empno = empno });


            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    string sql = @"DELETE FROM Schedule WHERE id=@id;";

                    schedules?.ForEach(id =>
                    {
                        ret += _conn.Execute(sql, new { id }, tran);
                    });

                    string sql2 = @"DELETE FROM Milestone WHERE id=@id;";

                    milestones?.ForEach(id =>
                    {
                        ret += _conn.Execute(sql2, new { id }, tran);
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

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        /* *****************************************************************************
         *  https://stackoverflow.com/questions/7508322/how-do-i-map-lists-of-nested-objects-with-dapper
         *  Method 1: Dapper Multimapping (not suited for this case)
         *  Method 2: Step (i). Multiple result using query multiple / custom Multimapping function
         *            Step (ii). Mapping manually iterating for loop / dictionary (lots of data) 
         **************************************************************************** */

        public Schedule Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<Schedule> GetAll()
        {
            //List<Schedule> ret;

            var lookup = new Dictionary<int, Schedule>();
            _conn.Query<Schedule, Milestone, Schedule>(@"
                SELECT s.*, m.*
                FROM Schedule AS s
                LEFT JOIN Milestone AS m ON s.id = m.schedule_id                   
                ", (s, m) => {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                schedule.milestones.Add(m); /* Add locations to course */
                return schedule;
            }).AsQueryable();
            var resultList = lookup.Values;



            //string sql = @"SELECT * FROM Schedule;
            //               SELECT m.*, s.id FROM Milestone AS m INNER JOIN Schedule AS s ON m.schedule_id = s.id";

            //var results = _conn.QueryMultiple(sql);

            //var schedules = results.Read<Schedule>();
            //var milestones = results.Read<Milestone>(); //(Location will have that extra CourseId on it for the next part)


            //string sql = @"SELECT * FROM Schedule AS s LEFT JOIN Milestone AS m ON s.id = m.scheduleId";


            //ret = _conn.Query<Schedule, Milestone, Schedule>(sql, (schedule, milestone) => { 

            //}).ToList();

            //foreach (var course in courses)
            //{
            //    course.Locations = locations.Where(a => a.CourseId == course.CourseId).ToList();
            //}


            //return schedules.ToList();
            return resultList.ToList();
        }


        public List<Schedule> GetAllOwnedSchedules(string empno)
        {            
            var lookup = new Dictionary<int, Schedule>();
            _conn.Query<Schedule, Milestone, Schedule>(@"
                SELECT s.*, m.*
                FROM Schedule AS s
                LEFT JOIN Milestone AS m ON s.id = m.schedule_id
                WHERE s.empno=@empno  
                ", (s, m) => {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                    schedule.milestones.Add(m); /* Add locations to course */
                return schedule;
            }, new { empno }).AsQueryable();
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
                WHERE s.role=@role AND s.type = 2
                ", (s, m) => {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                    schedule.milestones.Add(m); /* Add locations to course */
                return schedule;
            }, new { role }).AsQueryable();
            var resultList = lookup.Values;


            return resultList.ToList();
        }

        public List<Schedule> GetAllIndividualSchedules(string empno)
        {
            var lookup = new Dictionary<int, Schedule>();
            _conn.Query<Schedule, Milestone, Schedule>(@"
                SELECT s.*, m.*
                FROM Schedule AS s
                LEFT JOIN Milestone AS m ON s.id = m.schedule_id
                WHERE s.empno=@empno  
                ", (s, m) => {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                    schedule.milestones.Add(m); /* Add locations to course */
                return schedule;
            }, new { empno }).AsQueryable();
            var resultList = lookup.Values;


            return resultList.ToList();
        }

        public List<Schedule> GetAllReferredSchedules(string name)
        {            
            var lookup = new Dictionary<int, Schedule>();
            _conn.Query<Schedule, Milestone, Schedule>(@"
                SELECT s.*, m.*
                FROM Schedule AS s
                LEFT JOIN Milestone AS m ON s.id = m.schedule_id
                WHERE s.member LIKE @n
                ", (s, m) => {
                Schedule schedule;
                if (!lookup.TryGetValue(s.id, out schedule))
                    lookup.Add(s.id, schedule = s);
                if (schedule.milestones == null)
                    schedule.milestones = new List<Milestone>();
                if (m != null)
                    schedule.milestones.Add(m); /* Add locations to course */
                return schedule;
            },  new { n = "%" + name + "%" }).AsQueryable();
            var resultList = lookup.Values;


            return resultList.ToList();
        }


        // return schedule in service layer; return boolean or new ids in repository
        public Schedule Insert(Schedule schedule)
        {
            if (_conn.State == 0)
                _conn.Open();

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
                        string insertMilestoneSql = @"INSERT INTO Milestone (content, date, schedule_id) 
                        VALUES(@content, @date, @schedule_id) 
                        RETURNING id";

                        foreach (Milestone milestone in schedule.milestones)
                        {
                            milestone.schedule_id = schedule_id;
                            milestone.id = _conn.Execute(insertMilestoneSql, milestone, tran);
                        }
                    }

                    tran.Commit();
                    return schedule;
                }
                catch(Exception e)
                {
                    return null;
                }

            }

        }

        public Schedule Update(Schedule schedule, List<int> deletedMilestones)
        {
            if (_conn.State == 0)
                _conn.Open();

            int ret;

            using (var tran = _conn.BeginTransaction())
            {                
                string sql = @"UPDATE Schedule SET member=@member, content=@content, start_date=@start_date, end_date=@end_date, percent_complete=@percent_complete, last_percent_complete=@last_percent_complete, projno=@projno WHERE id=@id";

                try
                {
                    ret = _conn.Execute(sql, schedule, tran);

                    // update or insert milestones
                    if (schedule.milestones != null)
                    {
                        string insertMilestoneSql = @"INSERT INTO Milestone (content, date, schedule_id) 
                        VALUES(@content, @date, @schedule_id) 
                        RETURNING id";

                        string updateMilestoneSql = @"UPDATE Milestone SET content=@content, date=@date WHERE id=@id";

                        foreach (Milestone milestone in schedule.milestones)
                        {
                            if(milestone.id != 0)
                            {
                                ret = _conn.Execute(updateMilestoneSql, milestone, tran);
                            }
                            else
                            {
                                milestone.schedule_id = schedule.id;
                                milestone.id = _conn.Execute(insertMilestoneSql, milestone, tran);
                            }                            
                        }
                    }

                    // delete milestones
                    string deletedMilestoneSql = @"DELETE FROM Milestone WHERE id=@id;";

                    deletedMilestones?.ForEach(id =>
                    {
                        ret = _conn.Execute(deletedMilestoneSql, new { id }, tran);
                    });


                    tran.Commit();
                    return schedule;
                }
                catch
                {
                    return null;
                }
            }            
        }
               
        public bool Insert(List<Schedule> schedules)
        {

            //List<Milestone> milestones = new List<Milestone>();
            //foreach (var sc in schedules)
            //    milestones.AddRange(sc.milestones);

            if(_conn.State == 0)
                _conn.Open();

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {

                string sql = @"INSERT INTO Schedule (empno, member, type, content, start_date, end_date, percent_complete, last_percent_complete, parent_id, history, projno) 
                        VALUES(@empno, @member, @type, @content, @start_date, @end_date, @percent_complete, @last_percent_complete, @parent_id, @history, @projno) 
                        RETURNING id";

                //var ids = _conn.Query<int>(sql, schedules, tran).ToList();
                //ret += ids.Count;

                List<Milestone> milestones = new List<Milestone>();

                for (int i = 0; i < schedules.Count; i++)
                {
                    var id = _conn.QuerySingle<int>(sql, schedules[i], tran);
                    ret++;

                    if (schedules[i].milestones != null)
                    {
                        schedules[i].milestones.ForEach(x => x.schedule_id = id);
                        milestones.AddRange(schedules[i].milestones);
                    }
                }

                string sql2 = @"INSERT INTO Milestone (content, date, schedule_id) 
                        VALUES(@content, @date, @schedule_id) 
                        ";

                ret += _conn.Execute(sql2, milestones, tran);


                tran.Commit();

                return ret > 0;

            }
        }
        public bool Update(List<Schedule> schedules)
        {
            
            List<Milestone> new_milestones = new List<Milestone>();
            List<Milestone> update_milestones = new List<Milestone>();

            foreach (var sc in schedules)
            {
                if(sc.milestones is object)
                {
                    foreach (var m in sc.milestones)
                    {
                        if (m.id != 0)
                            update_milestones.Add(m);
                        else
                            new_milestones.Add(m);
                    }
                }
                    
            }
                
               
            _conn.Open();

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {

                string sql = @"UPDATE Schedule SET member=@member, content=@content, start_date=@start_date, end_date=@end_date, percent_complete=@percent_complete, last_percent_complete=@last_percent_complete, projno=@projno WHERE id=@id";

                ret = _conn.Execute(sql, schedules, tran);

                string sql2 = @"UPDATE Milestone SET content=@content, date=@date WHERE id=@id";

                ret += _conn.Execute(sql2, update_milestones, tran);

                string sql3 = @"INSERT INTO Milestone (content, date, schedule_id) 
                        VALUES(@content, @date, @schedule_id) 
                        ";

                ret += _conn.Execute(sql3, new_milestones, tran);

                tran.Commit();

                return ret > 0;

            }
        }

        public bool Upsert(List<Schedule> schedules)
        {

            List<Milestone> milestones = new List<Milestone>();
            foreach (var sc in schedules)
                milestones.AddRange(sc.milestones);

            _conn.Open();

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {              

                string sql = @"INSERT INTO Schedule (id, empno, member, type, content, start_date, end_date, percent_complete, last_percent_complete, parent_id, history, projno) 
                        VALUES(@id, @empno, @member, @type, @content, @start_date, @end_date, @percent_complete, @last_percent_complete, @parent_id, @history, @projno) 
                        ON CONFLICT(id) 
                        DO UPDATE SET member=@member, content=@content, start_date=@start_date, end_date=@end_date, percent_complete=@percent_complete, last_percent_complete=@last_percent_complete, projno=@projno";


                ret = _conn.Execute(sql, schedules, tran);

                string sql2 = @"INSERT INTO Milestone (id, content, date, schedule_id) 
                        VALUES(@id, @content, @date, @schedule_id) 
                        ON CONFLICT(id) 
                        DO UPDATE SET content=@content, date=@date";


                ret += _conn.Execute(sql2, milestones, tran);


                tran.Commit();

                return ret > 0;

            }
        }
    }
}