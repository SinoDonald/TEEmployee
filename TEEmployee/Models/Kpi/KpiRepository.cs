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

        public List<KpiItem> UpsertKpiItems(List<KpiItem> items)
        {
            if (_conn.State == 0)
                _conn.Open();

            // collection parameter not support returning
            // id is the primary key for kpiitems. Not suited for upsert sql command because new item has to got a non-repeated id first
            string insertSql = @"INSERT INTO KpiItem (kpi_id, content, target, weight, h1_employee_check, h1_manager_check,
                    h1_reason, h2_employee_check, h2_manager_check, h2_reason, consensual) 
                    VALUES(@kpi_id, @content, @target, @weight, @h1_employee_check, @h1_manager_check,
                    @h1_reason, @h2_employee_check, @h2_manager_check, @h2_reason, @consensual)
                    RETURNING *";

            string updateSql = @"UPDATE KpiItem 
                    SET content=@content, target=@target, weight=@weight, h1_employee_check=@h1_employee_check, 
                    h1_manager_check=@h1_manager_check, h1_reason=@h1_reason, h2_employee_check=@h2_employee_check, 
                    h2_manager_check=@h2_manager_check, h2_reason=@h2_reason, consensual=@consensual
                    WHERE id=@id
                    RETURNING *";

            var ret = new List<KpiItem>();

            using (var tran = _conn.BeginTransaction())
            {                
                try
                {
                    foreach (var item in items)
                    {
                        if (item.id == 0)
                        {
                            ret.Add(_conn.QuerySingle<KpiItem>(insertSql, item, tran));
                        }
                        else
                        {
                            ret.Add(_conn.QuerySingle<KpiItem>(updateSql, item, tran));
                        }

                    }

                    tran.Commit();
                }
                catch (Exception e)
                {
                    int a = 0;
                }

                return ret;
            }
        }


        public bool DeleteKpiItems(List<KpiItem> items)
        {
            int ret = 0;
            string sql = @"DELETE FROM KpiItem WHERE id=@id";

            try
            {
                ret = _conn.Execute(sql, items);
            }
            catch (Exception e)
            {
                ret = -1;
            }

            return ret >= 0;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }


    }
}