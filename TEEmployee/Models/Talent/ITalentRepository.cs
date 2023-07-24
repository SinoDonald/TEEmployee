using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Talent
{
    public interface ITalentRepository
    {
        List<CV> SaveUserCV(List<User> userGroups); // 讀取Word人員履歷表
        CV SaveResponse(CV userCV); // 儲存回覆
        void Dispose();
    }
}