using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Talent
{
    public class TalentService : IDisposable
    {
        private ITalentRepository _talentRepository;

        public TalentService()
        {
            _talentRepository = new TalentRepository();
        }
        // 人才資料庫 <-- 培文
        public void TalentUpdate()
        {
            _talentRepository.ReadWord(); // 讀取Word人員履歷表
        }

        public void Dispose()
        {
            _talentRepository.Dispose();
        }        
    }
}