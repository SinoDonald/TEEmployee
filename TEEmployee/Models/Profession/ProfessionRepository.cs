using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Profession
{
    public class ProfessionRepository : IProfessionRepository
    {
        private IDbConnection _conn;

        public ProfessionRepository()
        {
            string scheduleConnection = ConfigurationManager.ConnectionStrings["ProfessionConnection"].ConnectionString;
            _conn = new SQLiteConnection(scheduleConnection);
        }

        public List<Skill> GetAll()
        {
            var lookup = new Dictionary<int, Skill>();
            _conn.Query<Skill, Score, Skill>(@"
                SELECT sk.*, sc.*
                FROM Skill AS sk
                LEFT JOIN Score AS sc ON sk.id = sc.skill_id",
                (sk, sc) =>
            {
                Skill skill;
                if (!lookup.TryGetValue(sk.id, out skill))
                    lookup.Add(sk.id, skill = sk);
                if (skill.scores == null)
                    skill.scores = new List<Score>();
                if (sc != null)
                    skill.scores.Add(sc);
                return skill;
            }).AsQueryable();
            var resultList = lookup.Values;

            return resultList.ToList();
        }

        public List<Skill> GetAllSkillsByRole(List<string> roles)
        {
            List<Skill> ret;

            string sql = @"SELECT * FROM Skill WHERE role IN @roles";
            ret = _conn.Query<Skill>(sql, new { roles }).ToList();

            return ret;
        }


        public List<Skill> UpsertSkills(List<Skill> skills)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                List<Skill> ret = new List<Skill>();

                string insertSql = @"INSERT INTO Skill (content, role, skill_type, skill_time, custom_order) 
                        VALUES(@content, @role, @skill_type, @skill_time, @custom_order) 
                        RETURNING *";

                string updateSql = @"UPDATE Skill SET content=@content, custom_order=@custom_order WHERE id=@id RETURNING *";
                

                try
                {
                    foreach (var skill in skills)
                    {
                        if (skill.id != 0)
                        {
                            var ret_skill = _conn.QuerySingle<Skill>(updateSql, skill, tran);
                            ret.Add(ret_skill);
                        }
                        else
                        {
                            var ret_skill = _conn.QuerySingle<Skill>(insertSql, skill, tran);
                            ret.Add(ret_skill);
                        }                       
                    }

                    tran.Commit();
                }
                catch (Exception e)
                {

                }

                return ret;



                // ---------------------------------------------------------------

                // id is the primary key, not suited for upsert sql command

                //List<Skill> ret = new List<Skill>();

                //string sql = @"INSERT INTO Skill (content, role, skill_type, skill_time, custom_order) 
                //        VALUES(@content, @role, @skill_type, @skill_time, @custom_order) 
                //        ON CONFLICT(id=@id) 
                //        DO UPDATE SET content=@content, custom_order=@custom_order RETURNING *";

                //try
                //{
                //    foreach (var skill in skills)
                //    {
                //        var ret_skill = _conn.QuerySingle<Skill>(sql, skill, tran);
                //        ret.Add(ret_skill);
                //    }

                //    tran.Commit();
                //}
                //catch (Exception e)
                //{

                //}               

                //return ret;

                // ----------------------------------------------------------

                // RETURNING sql not support IEnumerable parameters (?)

                //List<Skill> ret;

                //string sql = @"INSERT INTO Skill (content, role, skill_type, skill_time, order) 
                //        VALUES(@content, @role, @skill_type, @skill_time, @order) 
                //        ON CONFLICT(id) 
                //        DO UPDATE SET content=@content, order=@order
                //        RETURING *";

                //ret = _conn.Query<Skill>(sql, skills).ToList();


                //tran.Commit();

                //return ret;

            }

        }

        // delete skills and related scores
        public bool DeleteSkills(List<Skill> skills)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                int ret = 0;

                string delSkillsql = @"DELETE FROM Skill WHERE id=@id";
                string delScoresql = @"DELETE FROM Score WHERE skill_id=@id";

                try
                {
                    foreach (var skill in skills)
                    {
                        ret += _conn.Execute(delSkillsql, skill, tran);
                        ret += _conn.Execute(delScoresql, skill, tran);
                    }

                    tran.Commit();
                }
                catch (Exception e)
                {
                    ret = 0;
                }

                return ret > 0;

            }
        }

        // For score page: return scores wrapped with skill
        // Current skills only
        public List<Skill> GetAllScoresByRole(List<string> roles)
        {
            var lookup = new Dictionary<int, Skill>();
            _conn.Query<Skill, Score, Skill>(@"
                SELECT sk.*, sc.*
                FROM Skill AS sk
                LEFT JOIN Score AS sc ON sk.id = sc.skill_id
                WHERE skill_time = 'now' AND role IN @roles",
                (sk, sc) =>
                {
                    Skill skill;
                    if (!lookup.TryGetValue(sk.id, out skill))
                        lookup.Add(sk.id, skill = sk);
                    if (skill.scores == null)
                        skill.scores = new List<Score>();
                    if (sc != null)
                        skill.scores.Add(sc);
                    return skill;
                }, new { roles }, splitOn: "skill_id").AsQueryable();
            var resultList = lookup.Values;

            return resultList.ToList();
        }

        public bool UpsertScores(List<Score> scores)
        {
            if (_conn.State == 0)
                _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                int ret = 0;

                string sql = @"INSERT INTO Score (skill_id, empno, score) 
                    VALUES(@skill_id, @empno, @score) 
                    ON CONFLICT(skill_id, empno) 
                    DO UPDATE SET score=@score";


                try
                {
                    foreach (var score in scores)
                    {
                        ret += _conn.Execute(sql, score, tran);
                    }

                    tran.Commit();
                }
                catch (Exception e)
                {
                    ret = 0;
                }

                return ret > 0;
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