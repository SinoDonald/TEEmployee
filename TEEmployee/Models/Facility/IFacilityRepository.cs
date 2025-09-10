using System;
using System.Collections.Generic;

namespace TEEmployee.Models.Facility
{
    public interface IFacilityRepository
    {
        List<Facility> GetDevices(); // 取得所有公用裝置
    }
}