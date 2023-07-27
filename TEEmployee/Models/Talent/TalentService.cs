using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        // 比對上傳的檔案更新時間
        public List<string> CompareLastestUpdate(List<string> filesInfo)
        {
            List<string> updateUsers = _talentRepository.CompareLastestUpdate(filesInfo);
            return updateUsers;
        }
        // 上傳員工履歷表多檔, 並解析Word後存到SQL
        public void Uploaded(HttpPostedFileBase[] files)
        {
            if (files.Count() > 0)
            {
                try
                {
                    //// 取得現在SQL存檔的更新時間
                    //List<CV> userCVs = _talentRepository.GetLastestUpdate();
                    foreach (HttpPostedFileBase uploadFile in files)
                    {
                        try
                        {
                            //string empno = Regex.Replace(Path.GetFileName(uploadFile.FileName), "[^0-9]", ""); // 僅保留數字
                            //string lastest_update = userCVs.Where(x => x.empno.Equals(empno)).Select(x => x.lastest_update).FirstOrDefault();
                            if (uploadFile.ContentLength > 0)
                            {
                                string savePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "Talent\\CV\\"); // 人員履歷表Word檔路徑
                                uploadFile.SaveAs(savePath + uploadFile.FileName);
                            }
                        }
                        catch (NullReferenceException) { }
                    }
                    List<User> userGroups = new UserRepository().UserGroups(); // 取得員工群組
                    _talentRepository.SaveUserCV(userGroups); // 讀取Word人員履歷表
                }
                catch (Exception) { }
            }
        }
        // 取得群組
        public List<string> GetGroupList(string empno)
        {
            List<string> groups = new List<string>();

            // 從User資料庫取得群組
            var _userRepository = new UserRepository();
            User user = _userRepository.Get(empno);
            var allEmployees = _userRepository.GetAll();

            if (user.department_manager)
            {
                groups = new List<string> { "全部顯示", "規劃", "設計", "專管" };
            }
            else
            {
                groups.Add("全部顯示");
                groups.Add(user.group);
                allEmployees = allEmployees.Where(x => x.group == user.group).ToList();
            }

            foreach (var item in allEmployees)
            {
                if (!String.IsNullOrEmpty(item.group))
                {
                    //三大群組 小組1
                    if (!String.IsNullOrEmpty(item.group_one) && !groups.Contains(item.group_one))
                    {
                        groups.Insert(groups.FindIndex(x => x == item.group) + 1, item.group_one);
                    }

                    //跨三大群組 小組2 小組3 (協理 only)
                    if (user.department_manager)
                    {
                        if (!String.IsNullOrEmpty(item.group_two) && !groups.Contains(item.group_two))
                        {
                            groups.Add(item.group_two);
                        }
                        if (!String.IsNullOrEmpty(item.group_three) && !groups.Contains(item.group_three))
                        {
                            groups.Add(item.group_three);
                        }
                    }
                }
                else
                {
                    //非三大群組
                    if (!String.IsNullOrEmpty(item.group_one) && !groups.Contains(item.group_one))
                    {
                        groups.Add(item.group_one);
                    }
                }
            }

            //special case
            if (groups.Remove("規劃組"))
                groups.Insert(groups.FindIndex(x => x == "規劃") + 1, "規劃組");

            if (user.group_one_manager) groups.Add(user.group_one);
            if (user.group_two_manager) groups.Add(user.group_two);
            if (user.group_three_manager) groups.Add(user.group_three);

            groups = groups.Distinct().ToList();

            return groups;
        }
        // 取得所有員工履歷
        public List<CV> GetAll(string empno)
        {
            _talentRepository = new TalentRepository();
            List<CV> cv = (_talentRepository as TalentRepository).Get(empno);

            return cv;
        }
        // 人才資料庫 <-- 培文
        public void TalentUpdate()
        {
            List<User> userGroups = new UserRepository().UserGroups(); // 取得員工群組
            _talentRepository.SaveUserCV(userGroups); // 讀取Word人員履歷表
        }
        // 儲存回覆
        public CV SaveResponse(CV userCV)
        {
            CV ret = _talentRepository.SaveResponse(userCV);
            return ret;
        }

        public void Dispose()
        {
            _talentRepository.Dispose();
        }        
    }
}