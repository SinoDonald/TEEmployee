using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;

namespace TEEmployee.Models.Facility
{
    public class FacilityRepository : IFacilityRepository
    {
        private IDbConnection _conn;
        public FacilityRepository()
        {
            string educationConnection = ConfigurationManager.ConnectionStrings["FacilityConnection"].ConnectionString;
            _conn = new SQLiteConnection(educationConnection);
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}