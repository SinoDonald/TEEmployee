using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Talent
{
    public interface ITalentRepository
    {
        List<CV> ReadWord(); // 讀取Word人員履歷表
        void Dispose();
    }
}