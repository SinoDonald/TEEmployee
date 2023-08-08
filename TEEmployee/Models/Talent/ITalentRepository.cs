using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Talent
{
    public interface ITalentRepository
    {
        List<string> CompareLastestUpdate(List<string> filesInfo); // 比對上傳的檔案更新時間
        List<CV> GetLastestUpdate(); // 取得現在SQL存檔的更新時間
        List<CV> SaveUserCV(List<User> userGroups); // 讀取Word人員履歷表
        List<string> ImportFile(HttpPostedFileBase file); // 上傳年度績效檔案
        CV SaveResponse(CV userCV); // 儲存回覆
        void Dispose();
    }
}