using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using TEEmployee.Models.Promotion;

namespace TEEmployee.Models.AgentModel
{
    internal class AgentRepository : IAgentRepository
    {
        private IDbConnection _conn;
        public AgentRepository()
        {
            string agentConnection = ConfigurationManager.ConnectionStrings["AgentConnection"].ConnectionString;
            _conn = new SQLiteConnection(agentConnection);
        }

        public List<Agent> GetAllAgents(string manno)
        {
            string sql = @"SELECT * FROM GSchedule WHERE manno = @manno";
            var ret = _conn.Query<Agent>(sql, new { manno }).ToList();

            return ret;
        }

        public List<Agent> GetPageAgentsByUser(string empno, string page_id)
        {
            string sql = @"SELECT * FROM GSchedule WHERE empno=@empno AND page_id LIKE @SearchText";
            var ret = _conn.Query<Agent>(sql, new { empno, SearchText = "%" + page_id + "%" }).ToList();

            return ret;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        public bool InsertAgent(Agent agent)
        {
            string sql = @"
                INSERT INTO GSchedule (empno, manno, page_id)
                VALUES (@empno, @manno, @page_id)";

            var ret = _conn.Execute(sql, agent);

            return ret > 0;
        }

        public bool UpdateAgent(Agent agent)
        {
            string sql = @"
                UPDATE GSchedule 
                SET 
                    page_id = @page_id 
                WHERE 
                    empno = @empno AND 
                    manno = @manno";

            var ret = _conn.Execute(sql, agent);

            return ret > 0;
        }

        public bool DeleteAgent(Agent agent)
        {
            string sql = @"
                DELETE FROM GSchedule 
                WHERE 
                    empno = @empno AND 
                    manno = @manno";

            var ret = _conn.Execute(sql, agent);

            return ret > 0;
        }
    }
}