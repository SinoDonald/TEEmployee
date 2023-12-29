using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Education
{
    public class EducationRepository : IEducationRepository
    {
        private IDbConnection _conn;
        public EducationRepository()
        {
            string educationConnection = ConfigurationManager.ConnectionStrings["EducationConnection"].ConnectionString;
            _conn = new SQLiteConnection(educationConnection);
        }

        public List<Course> GetAllCourses()
        {
            var lookup = new Dictionary<int, Course>();
            _conn.Query<Course, Chapter, Course>(@"
                SELECT co.*, ch.*
                FROM Course AS co
                LEFT JOIN Chapter AS ch ON co.id = ch.course_id                   
                ", (co, ch) =>
            {
                Course course;
                if (!lookup.TryGetValue(co.id, out course))
                    lookup.Add(co.id, course = co);
                if (course.chapters == null)
                    course.chapters = new List<Chapter>();
                if (ch != null)
                    course.chapters.Add(ch);
                return course;
            }).AsQueryable();

            var resultList = lookup.Values;

            return resultList.ToList();
        }

        public bool InsertCourses(List<Course> courses)
        {
            _conn.Open();

            string insertCourseSQL = @"INSERT INTO Course (course_title, course_group, course_group_one, course_subject) 
                        VALUES(@course_title, @course_group, @course_group_one, @course_subject) 
                        ON CONFLICT(course_title) 
                        DO NOTHING
                        RETURNING *";

            string insertChapterSQL = @"INSERT INTO Chapter (id, course_id, chapter_type, chapter_title, duration, createdTime) 
                        VALUES(@id, @course_id, @chapter_type, @chapter_title, @duration, @createdTime) 
                        ON CONFLICT(id) 
                        DO NOTHING";

            List<Course> ret = new List<Course>();

            using (var tran = _conn.BeginTransaction())
            {               
                
                foreach (var course in courses)
                {
                    try
                    {
                        var c = _conn.QuerySingle<Course>(insertCourseSQL, course, tran);
                        
                        foreach(var chapter in course.chapters)
                        {
                            chapter.course_id = c.id;
                        }                         

                        _conn.Execute(insertChapterSQL, course.chapters, tran);

                        ret.Add(c);

                    }
                    catch
                    {

                    }
                    
                }

                tran.Commit();

                return ret.Count > 0;
            }
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        
    }
}