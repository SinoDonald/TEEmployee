using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TEEmployee.Models.Profession;

namespace TEEmployee.Models.Talent
{
    public interface ITalentRepository
    {
        /// <summary>
        /// 儲存選項
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        bool SaveChoice(List<Ability> users);
        /// <summary>
        /// 上傳測評資料檔案
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<CV> ImportPDFFile(HttpPostedFileBase file, string empno);
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        CV SaveResponse(CV userCV, string planning);
        /// <summary>
        /// High Performer
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        Tuple<List<Ability>, string> HighPerformer();
        /// <summary>
        /// 取得所有員工職等職級
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<string> GetSenioritys();
        /// <summary>
        /// 條件篩選
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<CV> ConditionFilter(ConditionFilter filter, List<CV> userCVs);
        /// <summary>
        /// 刪除人才資料庫
        /// </summary>
        /// <returns></returns>
        bool DeleteTalent();
        void Dispose();

        // ************** 上傳Word檔或文字檔解析 ************** //

        ///// <summary>
        ///// 上傳年度績效檔案
        ///// </summary>
        ///// <param name="users"></param>
        ///// <returns></returns>
        //bool ImportFile(HttpPostedFileBase file);
        ///// <summary>
        ///// 讀取Word人員履歷表
        ///// </summary>
        ///// <param name="users"></param>
        ///// <returns></returns>
        //List<CV> SaveUserCV(List<User> userGroups);
        ///// <summary>
        ///// 上傳員工經歷文字檔
        ///// </summary>
        ///// <param name="users"></param>
        ///// <returns></returns>
        //bool UploadExperience(HttpPostedFileBase file);
    }
}