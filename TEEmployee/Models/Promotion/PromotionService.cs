using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TEEmployee.Models.Talent;

namespace TEEmployee.Models.Promotion
{
    public class PromotionService : IDisposable
    {
        private IPromotionRepository _promotionRepository;
        private IUserRepository _userRepository;
        private ITalentRepository _talentRepository;

        public PromotionService()
        {
            _promotionRepository = new PromotionRepository();
            _userRepository = new UserRepository();
            _talentRepository = new TalentRepository();
        }

        public List<Promotion> GetAll(string empno)
        {
            return _promotionRepository.GetAll();
        }

        public List<Promotion> GetByUser(string empno)
        {
            var promotions = _promotionRepository.GetByUser(empno);

            if (promotions.Count == 0)
                promotions = CreatePromotion(empno);

            this.TransformContent(promotions, empno);

            return promotions;
        }

        private List<Promotion> CreatePromotion(string empno)
        {
            List<Promotion> promotions = new List<Promotion>();

            for (int i = 0; i != 6; i++)
            {
                promotions.Add(new Promotion { empno = empno, condition = i + 1 });
            }

            var ret = _promotionRepository.Insert(promotions);

            if (ret)
                return _promotionRepository.GetByUser(empno);
            else
                return null;
        }

        public bool Update(Promotion promotion)
        {
            var ret = _promotionRepository.Update(promotion);

            return ret;
        }

        public bool UploadFile(HttpPostedFileBase file, Promotion promotion)
        {
            try
            {
                string _appData = HttpContext.Current.Server.MapPath("~/App_Data/Promotion");
                string extension = Path.GetExtension(file.FileName);
                string fn = Path.Combine(_appData, $"{promotion.empno}_{promotion.condition}{extension}");
                file.SaveAs(fn);

                promotion.filepath = file.FileName;
                return _promotionRepository.Update(promotion);

            }
            catch
            {
                return false;
            }

        }

        public byte[] DownloadFile(Promotion promotion)
        {
            string _appData = HttpContext.Current.Server.MapPath("~/App_Data/Promotion");
            string extension = Path.GetExtension(promotion.filepath);
            string fn = Path.Combine(_appData, $"{promotion.empno}_{promotion.condition}{extension}");

            try
            {
                var fileBytes = File.ReadAllBytes(fn);
                return fileBytes;
            }
            catch
            {
                return null;
            }

        }

        private void TransformContent(List<Promotion> promotions, string empno)
        {
            User user = _userRepository.Get(empno);

            List<CV> cvs = (_talentRepository as TalentRepository).Get(empno);

            string[] strs = new string[4];
            //string nextPro = "";
            //string nextYear = "";
            //string score = "";
            //string scoreYear = "";

            // modify content
            switch (user.profTitle)
            {
                case "正工程師":
                    strs = new string[] { "正工程師", "5", "5", "88" };
                    break;

                case "工程師一":
                    strs = new string[] { "工程師(一)", "4", "4", "88" };
                    break;

                case "工程師二":
                    strs = new string[] { "工程師(二)", "3", "3", "87" };
                    break;

                case "工程師三":
                    strs = new string[] { "工程師(三)", "3", "3", "86" };
                    break;

                case "工程師四":
                    strs = new string[] { "工程師(四)", "3", "3", "85" };
                    break;

                case "專員一":
                    strs = new string[] { "專員(一)", "4", "4", "88" };
                    break;

                case "專員二":
                    strs = new string[] { "專員(二)", "3", "3", "88" };
                    break;

                case "專員三":
                    strs = new string[] { "專員(三)", "3", "3", "86" };
                    break;

                case "正建築師":
                    strs = new string[] { "正建築師", "5", "5", "88" };
                    break;

                case "建築師一":
                    strs = new string[] { "建築師(一)", "4", "4", "88" };
                    break;

                case "建築師二":
                    strs = new string[] { "建築師(二)", "3", "3", "87" };
                    break;

                case "建築師三":
                    strs = new string[] { "建築師(三)", "3", "3", "86" };
                    break;

                case "建築師四":
                    strs = new string[] { "建築師(四)", "3", "3", "85" };
                    break;

                case "正規劃師":
                    strs = new string[] { "正規劃師", "5", "5", "88" };
                    break;

                case "規劃師一":
                    strs = new string[] { "規劃師(一)", "4", "4", "88" };
                    break;

                case "規劃師二":
                    strs = new string[] { "規劃師(二)", "3", "3", "87" };
                    break;

                case "規劃師三":
                    strs = new string[] { "規劃師(三)", "3", "3", "86" };
                    break;

                case "規劃師四":
                    strs = new string[] { "規劃師(四)", "3", "3", "85" };
                    break;
            }

            promotions[0].content = promotions[0].content.Replace("xxx", strs[0]);
            promotions[0].content = promotions[0].content.Replace("yyy", strs[1]);
            promotions[1].content = promotions[1].content.Replace("c", strs[2]);
            promotions[1].content = promotions[1].content.Replace("d", strs[3]);

            // remove content
            switch (user.profTitle)
            {
                case "主任工程師":
                    promotions.RemoveRange(0, 2);
                    return;
                    //break;

                case "製圖師":
                    promotions.RemoveRange(0, 2);
                    return;
                    //break;
            }

            // add performance
            string[] stringSeparators = new string[] { "\r\n" };
            string[] performances = cvs[0].performance.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            int count = Math.Min(int.Parse(strs[2]), performances.Length);
            string performanceStr = $"\n近{strs[2]}年的考績為:";

            for (int i = 0; i != count; i++)
            {
                performanceStr = performanceStr + $" {performances[i]},";
            }

            if (count > 0)
                performanceStr = performanceStr.Substring(0, performanceStr.Length - 1);

            promotions[1].content += performanceStr;

            return;
        }

        public dynamic GetAuthorization(string empno)
        {
            User user = _userRepository.Get(empno);
            List<User> users = new List<User>();

            if (user.department_manager)
            {
                users = _userRepository.GetAll();

                List<string> titleOrder = new List<string> { "主任工程師", "正工程師", "工程師一", "工程師二", "工程師三", "工程師四",
                "正規劃師", "規劃師一", "規劃師二", "規劃師三", "規劃師四",
                "正建築師", "建築師一", "建築師二", "建築師三", "建築師四",
                "製圖師", "專員一", "專員二", "專員三" };

                users = users.OrderBy(x =>
                {
                    int engIdx = titleOrder.IndexOf(x.profTitle);

                    return engIdx;
                }).ThenBy(x => x.name).ToList();

            }


            dynamic authorization = new JObject();
            authorization.User = JObject.FromObject(user);
            authorization.Users = JArray.FromObject((users).ToList());

            return JsonConvert.SerializeObject(authorization);
        }

        // private method

        private List<string> GetManagerGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = new List<string>();

            if (user.department_manager)
                groups = (_userRepository as UserRepository).GetSubGroups(empno);

            return groups;
        }

        private List<string> GetEmployeeGroups(string empno)
        {
            User user = _userRepository.Get(empno);
            List<string> groups = new List<string>();

            // add sub group if as a member
            if (!String.IsNullOrEmpty(user.group_one))
                groups.Add(user.group_one);

            if (!String.IsNullOrEmpty(user.group_two))
                groups.Add(user.group_two);

            if (!String.IsNullOrEmpty(user.group_three))
                groups.Add(user.group_three);

            // remove duplicates
            groups = groups.Distinct().ToList();

            return groups;
        }

        private List<User> GetGroupMembers(string group)
        {
            List<User> users = _userRepository.GetAll();
            return users.Where(x => x.group_one == group || x.group_two == group || x.group_three == group).ToList();
        }


        public void Dispose()
        {
            _promotionRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}