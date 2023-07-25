using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Kpi
{
    interface IKpiRepository : IDisposable
    {
        List<KpiModel> GetAllKpiModels(int year);
        bool InsertKpiModels(List<KpiModel> kpiModels);
        List<KpiItem> UpsertKpiItems(List<KpiItem> items);
        bool DeleteKpiItems(List<KpiItem> items);

    }
}
