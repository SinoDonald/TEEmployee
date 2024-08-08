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

        public List<Chapter> GetAllChapters()
        {
            List<Chapter> ret;

            string sql = @"SELECT * FROM Chapter";
            ret = _conn.Query<Chapter>(sql).ToList();

            return ret;
        }

        private string GetMaxChapterSerial(string code_prefix)
        {
            code_prefix += "%";
            string query = @"SELECT COUNT(*) FROM Chapter WHERE chapter_code LIKE @code_prefix";
            var count = _conn.QuerySingle<int>(query, new { code_prefix });

            count++;
            string formattedNumber = count.ToString("D3");

            return formattedNumber;
        }

        public bool InsertChapters(List<Chapter> chapters)
        {
            _conn.Open();

            //string insertCourseSQL = @"INSERT INTO Course (course_title, course_group, course_group_one) 
            //            VALUES(@course_title, @course_group, @course_group_one) 
            //            ON CONFLICT(course_title) 
            //            DO NOTHING
            //            RETURNING *";

            string insertChapterSQL = @"INSERT INTO Chapter (id, course_group, course_group_one, course_title, chapter_type, chapter_title, duration, createdTime, chapter_code, chapter_scope) 
                        VALUES(@id, @course_group, @course_group_one, @course_title, @chapter_type, @chapter_title, @duration, @createdTime, @chapter_code, @chapter_scope) 
                        ON CONFLICT(id) 
                        DO NOTHING";

            int ret = 0;

            using (var tran = _conn.BeginTransaction())
            {


                foreach (var chapter in chapters)
                {
                    string serial_number = GetMaxChapterSerial(chapter.chapter_code);
                    chapter.chapter_code += serial_number;
                    ret += _conn.Execute(insertChapterSQL, chapter, tran);
                }

                tran.Commit();

                return ret > 0;
            }
        }

        public bool UpsertRecords(List<Record> records)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                int ret = 0;

                string sql = @"INSERT INTO Record (empno, chapter_id, assigned) 
                        VALUES(@empno, @chapter_id, @assigned) 
                        ON CONFLICT(empno, chapter_id) 
                        DO UPDATE SET assigned=@assigned";

                var db_records = records.Select(x => new
                {
                    empno = x.empno,
                    chapter_id = x.chapter.id,
                    assigned = x.assigned,
                }).ToList();

                ret = _conn.Execute(sql, db_records);

                tran.Commit();

                return ret > 0;

            }
        }

        public List<Record> GetAllRecordsByUser(string empno)
        {
            var sql = @"SELECT r.*, c.* 
                FROM Record as r
                INNER JOIN Chapter as c ON r.chapter_id = c.id
                WHERE r.empno=@empno";

            var ret = _conn.Query<Record, Chapter, Record>(sql, (record, chapter) =>
            {
                record.chapter = chapter;
                return record;
            }, new { empno }).ToList();

            return ret;
        }

        public Record UpdateRecordCompleted(Record record)
        {
            Record ret;

            string sql = @"UPDATE Record SET completed=@completed 
                            WHERE chapter_id=@chapter_id AND empno=@empno
                            RETURNING *";

            var recordCollection = new[] { record };

            var new_record = recordCollection.Select(x => new
            {
                empno = x.empno,
                chapter_id = x.chapter.id,
                completed = x.completed,
            }).First();

            ret = _conn.Query<Record>(sql, new_record).First();

            return ret;
        }

        public Chapter UpdateChapterDigitalized(Chapter chapter)
        {
            Chapter ret;

            string sql = @"UPDATE Chapter SET digitalized=@digitalized 
                            WHERE id=@id
                            RETURNING *";


            ret = _conn.Query<Chapter>(sql, chapter).First();

            return ret;
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

        public bool DeleteAll()
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(@"DELETE FROM Record", tran);
                    _conn.Execute(@"DELETE FROM Chapter", tran);

                    tran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }

        }

    }
}