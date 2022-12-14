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



        public bool Insert(Schedule schedule)
        {
            throw new NotImplementedException();
        }

        public bool Update(Schedule schedule)
        {
            throw new NotImplementedException();
        }
    }
}