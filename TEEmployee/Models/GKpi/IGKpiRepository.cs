using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.GKpi
{
    interface IGKpiRepository : IDisposable
    {
        List<KpiModel> GetAllKpiModels(int year);
        bool InsertKpiModels(List<KpiModel> kpiModels);
        List<KpiModel> InsertKpiModelsNew(List<KpiModel> kpiModels);
        List<KpiItem> UpsertKpiItems(List<KpiItem> items);
        bool DeleteKpiItems(List<KpiItem> items);
        bool DeleteKpiModelsByYear(int year);
        bool DeleteSolitaryKpiModels();
    }
}
