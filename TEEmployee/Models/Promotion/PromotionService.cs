using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

            for (int i = 0; i != 7; i++)
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


            CV cv = (_talentRepository as TalentRepository).Get(empno).FirstOrDefault();

            if (cv == null)
                return;

            string[] strs = this.NextConditions(user.profTitle);

            // modify content            

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


            string[] stringSeparators = new string[] { "\n" };

            // add seniority
            string seniorityStr = "\n";
            string[] seniorities = cv.seniority.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in seniorities)
            {
                if (s.Contains(strs[0]))
                {
                    seniorityStr += "已任職";
                    seniorityStr += s;
                    break;
                }
            }

            promotions[0].content += seniorityStr;

            // add performance

            string[] performances = cv.performance.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

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
            dynamic authorization = new JObject();
            authorization.Users = new JArray();


            if (user.department_manager || user.group_manager)
            {
                users = _userRepository.GetAll();

                if (user.group_manager)
                    users = users.Where(x => x.group == user.group).ToList();

                // order in selection component
                List<string> titleOrder = new List<string> {
                    "主任工程師", "正工程師一", "正工程師二", "工程師一", "工程師二", "工程師三", "工程師四",
                    "主任規劃師", "正規劃師一", "正規劃師二", "規劃師一", "規劃師二", "規劃師三", "規劃師四",
                    "主任建築師", "正建築師一", "正建築師二", "建築師一", "建築師二", "建築師三", "建築師四",
                    "製圖師", "資深專員一", "資深專員二", "資深專員三", "專員一", "專員二", "專員三" };

                users = users.OrderBy(x =>
                {
                    int engIdx = titleOrder.IndexOf(x.profTitle);

                    return engIdx;
                }).ThenBy(x => x.name).ToList();


                foreach (var item in users)
                {
                    dynamic userObj = JObject.FromObject(item);
                    userObj.nextProfTitle = this.NextProfTitle(item.profTitle);
                    userObj.upgrade = this.CanUpgrade(item, user.department_manager);
                    authorization.Users.Add(userObj);

                }

            }



            authorization.User = JObject.FromObject(user);
            //authorization.Users = JArray.FromObject((users).ToList());
            authorization.User.nextProfTitle = this.NextProfTitle(user.profTitle);

            return JsonConvert.SerializeObject(authorization);
        }

        // private method
        private string NextProfTitle(string profTitle)
        {
            string next = "";

            switch (profTitle)
            {
                case "正工程師一":
                    next = "主任工程師";
                    break;

                case "正工程師二":
                    next = "正工程師(一)";
                    break;

                case "工程師一":
                    next = "正工程師(二)";
                    break;

                case "工程師二":
                    next = "工程師(一)";
                    break;

                case "工程師三":
                    next = "工程師(二)";
                    break;

                case "工程師四":
                    next = "工程師(三)";
                    break;

                case "資深專員三":
                    next = "資深專員(二)";
                    break;

                case "專員一":
                    next = "資深專員(三)";
                    break;

                case "專員二":
                    next = "專員(一)";
                    break;

                case "專員三":
                    next = "專員(二)";
                    break;

                case "正建築師一":
                    next = "主任建築師";
                    break;

                case "正建築師二":
                    next = "正建築師(一)";
                    break;

                case "建築師一":
                    next = "正建築師(二)";
                    break;

                case "建築師二":
                    next = "建築師(一)";
                    break;

                case "建築師三":
                    next = "建築師(二)";
                    break;

                case "建築師四":
                    next = "建築師(三)";
                    break;

                case "正規劃師一":
                    next = "主任規劃師";
                    break;

                case "正規劃師二":
                    next = "正規劃師(一)";
                    break;

                case "規劃師一":
                    next = "正規劃師(二)";
                    break;

                case "規劃師二":
                    next = "規劃師(一)";
                    break;

                case "規劃師三":
                    next = "規劃師(二)";
                    break;

                case "規劃師四":
                    next = "規劃師(三)";
                    break;
            }

            return next;
        }

        // private method
        private string[] NextConditions(string profTitle)
        {
            // nextPro nextYear scoreYear score 

            string[] strs;

            switch (profTitle)
            {
                case "正工程師一":
                    strs = new string[] { "正工程師(一)", "5", "5", "88" };
                    break;

                case "正工程師二":
                    strs = new string[] { "正工程師(二)", "4", "4", "88" };
                    break;

                case "工程師一":
                    strs = new string[] { "工程師(一)", "3", "3", "88" };
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

                case "資深專員三":
                    strs = new string[] { "資深專員(三)", "5", "5", "88" };
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

                case "正建築師一":
                    strs = new string[] { "正建築師(一)", "5", "5", "88" };
                    break;

                case "正建築師二":
                    strs = new string[] { "正建築師(二)", "4", "4", "88" };
                    break;

                case "建築師一":
                    strs = new string[] { "建築師(一)", "3", "3", "88" };
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

                case "正規劃師一":
                    strs = new string[] { "正規劃師(一)", "5", "5", "88" };
                    break;

                case "正規劃師二":
                    strs = new string[] { "正規劃師(二)", "4", "4", "88" };
                    break;

                case "規劃師一":
                    strs = new string[] { "規劃師(一)", "3", "3", "88" };
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
                default:
                    strs = new string[4];
                    break;
            }

            return strs;
        }

        private string CanUpgrade(User user, bool department_manager)
        {
            // CAN'T UPGRADE TITLE
            List<string> cantUpgradeTitles = new List<string> { "主任工程師", "主任建築師", "主任規劃師", "製圖師", "資深專員二", "資深專員一" };
            if (cantUpgradeTitles.Contains(user.profTitle))
                return "";


            try
            {
                CV cv = (_talentRepository as TalentRepository).Get(user.empno).First();
                List<Promotion> promotions = _promotionRepository.GetByUser(user.empno);

                if (department_manager)
                {
                    if (promotions.Where(x => x.condition == 7).First().achieved == false && user.group_one != "行政組")
                        return "";
                }

                bool hasBonus = promotions.Where(x => x.condition > 2 && x.condition < 7 && x.achieved).Count() > 0;
                string[] strs = this.NextConditions(user.profTitle); // nextPro nextYear score scoreYear
                string[] stringSeparators = new string[] { "\n" };

                // If user can upgrade in normay way, use normal
                // Else try special way.


                // Year Condition                
                bool passYear = false, passYearByBonus = false;
                int seniorityYear = int.Parse(strs[1]);
                
                string[] seniorities = cv.seniority.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (var s in seniorities)
                {
                    if (s.Contains(strs[0]))
                    {
                        Regex rx = new Regex(@"\d*年", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                        Match match = rx.Match(s);

                        string year = match.Value;
                        year = year.Substring(0, year.Length - 1);

                        if (int.Parse(year) >= seniorityYear)
                        {
                            passYear = true;
                        }
                        else if (hasBonus && (int.Parse(year) >= seniorityYear - 1))
                        {
                            passYearByBonus = true;
                        }
                        break;
                    }
                }                

                // Score Condition 
                bool passScore = false, passScoreByBonus = false;
                int scoreYear = int.Parse(strs[2]);
                int score = int.Parse(strs[3]);
                string[] performances = cv.performance.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                if (performances.Length >= scoreYear)
                {
                    int sum = 0;

                    for (int i = 0; i != scoreYear; i++)
                        sum += int.Parse(performances[i]);

                    if ((double)sum / scoreYear >= score)
                    {
                        passScore = true;
                    }
                    else if (hasBonus && (double)sum / scoreYear >= score - 1)
                    {
                        passScoreByBonus = true;
                    }
                }

                // decide upgrade method

                if (passYear && passScore)
                    return "normal";

                if ((passYear || passYearByBonus) && (passScore || passScoreByBonus))
                    return "bonus";
            }
            catch
            {

            }


            return "";

        }

        public void Dispose()
        {
            _promotionRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}