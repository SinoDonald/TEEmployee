using System.Collections.Generic;

namespace TEEmployee.Models.Facility
{
    public interface IFacilityRepository
    {
        List<Facility> GetDevices(); // 取得所有公用裝置
        bool Delete(int id); // 刪除
        string Send(string state, Facility reserve); // 修改與新增
        string CreateDevice(Facility facility); // 新增裝置
        string RemoveDevice(string deviceID); // 移除裝置
        string Change(string deviceID, string password); // 修改Teams密碼
    }
}