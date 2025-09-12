using System;
using System.Collections.Generic;

namespace TEEmployee.Models.Facility
{
    public interface IFacilityRepository
    {
        List<Facility> GetDevices(); // 取得所有公用裝置
        bool Delete(int id); // 刪除
        bool Send(string state, Facility reserve); // 修改與新增
    }
}