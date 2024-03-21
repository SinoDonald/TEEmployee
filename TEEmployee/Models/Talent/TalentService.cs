using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.Profession;

namespace TEEmployee.Models.Talent
{
    public class TalentService : IDisposable
    {
        private ITalentRepository _talentRepository;
        public TalentService()
        {
            _talentRepository = new TalentRepository();
        }
        /// <summary>
        /// 比對上傳的檔案更新時間
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public List<string> CompareLastestUpdate(List<string> filesInfo)
        {
            List<string> updateUsers = _talentRepository.CompareLastestUpdate(filesInfo);
            return updateUsers;
        }
        /// <summary>
        /// 上傳員工履歷表多檔, 並解析Word後存到SQL
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
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
                                // 檢查資料夾是否存在, 沒有則建立資料夾
                                if (!Directory.Exists(savePath))
                                {
                                    Directory.CreateDirectory(savePath);
                                }
                                uploadFile.SaveAs(savePath + uploadFile.FileName);
                            }
                        }
                        catch (NullReferenceException) { }
                    }
                    List<User> users = new UserRepository().GetAll(); // 取得員工群組
                    _talentRepository.SaveUserCV(users); // 讀取Word人員履歷表
                }
                catch (Exception) { }
            }
        }
        /// <summary>
        /// 上傳員工經歷文字檔
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public bool UploadExperience(HttpPostedFileBase file)
        {
            var ret = _talentRepository.UploadExperience(file);
            return ret;
        }
        /// <summary>
        /// 上傳年度績效檔案
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public bool ImportFile(HttpPostedFileBase file)
        {
            var ret = _talentRepository.ImportFile(file);
            return ret;
        }
        /// <summary>
        /// 上傳測評資料檔案
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public List<CV> ImportPDFFile(HttpPostedFileBase file, string empno)
        {
            List<CV> ret = _talentRepository.ImportPDFFile(file, empno);
            return ret;
        }
        /// <summary>
        /// High Performer
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public Tuple<List<Ability>, string> HighPerformer()
        {
            Tuple<List<Ability>, string> users = _talentRepository.HighPerformer();
            return users;
        }
        /// <summary>
        /// 取得群組
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public List<string> GetGroupList(string empno)
        {
            List<string> groups = new List<string>();

            // 從User資料庫取得群組
            var _userRepository = new UserRepository();
            User user = _userRepository.Get(empno);
            var allEmployees = _userRepository.GetAll();

            if (user.department_manager)
            {
                //if (empno.Equals("4125"))
                //{
                //    groups = new List<string> { "High Performer", "全部顯示", "規劃", "設計", "專管" };
                //}
                //else
                //{
                    groups = new List<string> { "全部顯示", "規劃", "設計", "專管" };
                //}
            }
            else
            {
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
        /// <summary>
        /// 條件篩選
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        internal List<CV> ConditionFilter(ConditionFilter filter, List<CV> userCVs)
        {
            List<CV> filterUserCVs = _talentRepository.ConditionFilter(filter, userCVs);
            return filterUserCVs;
        }
        /// <summary>
        /// 取得所有員工職等職級
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        internal List<string> GetSenioritys()
        {
            List<string> senioritys = _talentRepository.GetSenioritys();
            return senioritys;
        }
        /// <summary>
        /// 儲存個人紀錄選項
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public bool SaveChoice(List<Ability> users)
        {
            bool ret = _talentRepository.SaveChoice(users);
            return ret;
        }
        /// <summary>
        /// 取得所有員工履歷
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public List<CV> GetAll(string empno)
        {
            _talentRepository = new TalentRepository();
            List<CV> cv = (_talentRepository as TalentRepository).GetAll(empno);

            return cv;
        }
        /// <summary>
        /// 取得員工履歷
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public List<CV> Get(string empno)
        {
            _talentRepository = new TalentRepository();
            List<CV> cv = (_talentRepository as TalentRepository).Get(empno);

            return cv;
        }
        /// <summary>
        /// 更新人才培訓資料庫
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public void TalentUpdate()
        {
            List<User> users = new UserRepository().GetAll(); // 取得員工群組
            _talentRepository.SaveUserCV(users); // 讀取Word人員履歷表
        }
        /// <summary>
        /// 儲存回覆
        /// </summary>
        /// <param name="filesInfo"></param>
        /// <returns></returns>
        public CV SaveResponse(CV userCV, string planning)
        {
            CV ret = _talentRepository.SaveResponse(userCV, planning);
            return ret;
        }

        public void Dispose()
        {
            _talentRepository.Dispose();
        }
    }
}