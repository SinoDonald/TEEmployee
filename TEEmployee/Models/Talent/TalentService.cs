﻿using System;
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
                groups = new List<string> { "規劃", "設計", "專管" };
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
            _talentRepository.ReadWord(); // 讀取Word人員履歷表
        }

        public void Dispose()
        {
            _talentRepository.Dispose();
        }        
    }
}