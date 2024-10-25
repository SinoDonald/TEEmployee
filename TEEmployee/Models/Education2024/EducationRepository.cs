using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace TEEmployee.Models.Education2024
{
    public class EducationRepository : IEducationRepository
    {
        private IDbConnection _conn;
        public EducationRepository()
        {
            string educationConnection = ConfigurationManager.ConnectionStrings["GEducationConnection"].ConnectionString;
            _conn = new SQLiteConnection(educationConnection);
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }

        public List<Content> GetAllContents()
        {
            List<Content> ret;

            string sql = @"SELECT * FROM Content AS c LEFT JOIN ContentExtra AS e ON c.id = e.content_id";
            ret = _conn.Query<Content>(sql).ToList();

            return ret;
        }

    }
}