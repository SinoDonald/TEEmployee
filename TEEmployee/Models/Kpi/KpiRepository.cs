using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Kpi
{
    public class KpiRepository : IKpiRepository
    {
        private IDbConnection _conn;
        public KpiRepository()
        {
            string kpiConnection = ConfigurationManager.ConnectionStrings["KpiConnection"].ConnectionString;
            _conn = new SQLiteConnection(kpiConnection);
        }

        public List<KpiModel> GetAllKpiModels(int year)
        {
            var lookup = new Dictionary<int, KpiModel>();
            _conn.Query<KpiModel, KpiItem, KpiModel>(@"
                SELECT km.*, ki.*
                FROM KpiModel AS km
                LEFT JOIN KpiItem AS ki ON km.id = ki.kpi_id
                WHERE year = @year",
                (km, ki) =>
                {
                    KpiModel kpiModel;
                    if (!lookup.TryGetValue(km.id, out kpiModel))
                        lookup.Add(km.id, kpiModel = km);
                    if (kpiModel.items == null)
                        kpiModel.items = new List<KpiItem>();
                    if (ki != null)
                        kpiModel.items.Add(ki);
                    return kpiModel;
                }, new { year }).AsQueryable();
            var resultList = lookup.Values;

            return resultList.ToList();
        }

        public bool InsertKpiModels(List<KpiModel> kpiModels)
        {
            int ret;

            string sql = @"INSERT INTO KpiModel (empno, year, kpi_type, group_name) 
                        VALUES(@empno, @year, @kpi_type, @group_name) 
                        ON CONFLICT(empno, year, kpi_type, group_name) 
                        DO NOTHING";

            ret = _conn.Execute(sql, kpiModels);

            return ret > 0;
        }        

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

        
    }
}