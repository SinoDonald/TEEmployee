using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

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

        /// <summary>
        /// 取得所有公用裝置
        /// </summary>
        /// <returns></returns>
        public List<Facility> GetDevices()
        {
            List<Facility> ret;
            string sql = @"SELECT * FROM facility";
            ret = _conn.Query<Facility>(sql).Where(x => x.available.Equals(true)).OrderBy(x => x.deviceID).ToList();

            return ret;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}