using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    public class GEducationRepository : IGEducationRepository
    {
        private IDbConnection _conn;
        public GEducationRepository()
        {
            string educationConnection = ConfigurationManager.ConnectionStrings["GEducationConnection"].ConnectionString;
            _conn = new SQLiteConnection(educationConnection);
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        //public List<Course> GetAllCourses()
        //{
        //    var lookup = new Dictionary<int, Course>();
        //    _conn.Query<Course, Chapter, Course>(@"
        //        SELECT co.*, ch.*
        //        FROM Course AS co
        //        LEFT JOIN Chapter AS ch ON co.id = ch.course_id                   
        //        ", (co, ch) =>
        //    {
        //        Course course;
        //        if (!lookup.TryGetValue(co.id, out course))
        //            lookup.Add(co.id, course = co);
        //        if (course.chapters == null)
        //            course.chapters = new List<Chapter>();
        //        if (ch != null)
        //            course.chapters.Add(ch);
        //        return course;
        //    }).AsQueryable();

        //    var resultList = lookup.Values;

        //    return resultList.ToList();
        //}

    }
}