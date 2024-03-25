using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
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

        /// <summary>
        /// 取得所有員工升等項目。
        /// </summary>
        /// <returns>包含所有員工升等項目的列舉。</returns>        
        public List<Promotion> GetAll(string empno)
        {
            return _promotionRepository.GetAll();
        }

        /// <summary>
        /// 取得特定員工升等項目。
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <param name="isAdmin">是否為管理員</param>
        /// <returns>包含特定員工升等項目的列舉</returns>
        /// <remarks>若使用者為管理員而非主管，隱藏機敏資料</remarks>
        public List<Promotion> GetByUser(string empno, bool isAdmin)
        {
            var promotions = _promotionRepository.GetByUser(empno);

            if (promotions.Count == 0)
                promotions = CreatePromotion(empno);

            this.TransformContent(promotions, empno);

            // Hide sensitive infomation for admin
            if (isAdmin)
            {
                Promotion performance = promotions.Where(x => x.condition == 2).FirstOrDefault();
                if (performance != null)
                    performance.content = "機敏資料";
            }        

            return promotions;
        }

        /// <summary>
        /// 建立特定員工升等項目。
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>包含特定員工升等項目的列舉</returns>
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

        /// <summary>
        /// 更新特定員工單筆升等項目。
        /// </summary>
        /// <param name="promotion">升等項目</param>
        /// <returns>是否更新成功</returns>
        public bool Update(Promotion promotion)
        {
            var ret = _promotionRepository.Update(promotion);

            return ret;
        }

        /// <summary>
        /// 上傳檔案至特定員工單筆升等項目。
        /// </summary>
        /// <param name="file">附加檔案</param>
        /// <param name="promotion">升等項目</param>
        /// <returns>是否更新成功</returns>
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

        /// <summary>
        /// 下載特定員工單筆升等項目之附加檔案。
        /// </summary>
        /// <param name="promotion">升等項目</param>
        /// <returns>檔案位元資料</returns>
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

        /// <summary>
        /// 根據員工職位轉換升等項目內容
        /// </summary>
        /// <param name="promotions">升等項目列舉</param>
        /// <param name="empno">員工編號</param>
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
                    promotions.RemoveRange(0, 7);
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

                    Regex rx = new Regex(@"\d*年", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    Match match = rx.Match(s);

                    string year = match.Value;
                    year = year.Substring(0, year.Length - 1);

                    if (int.Parse(year) >= int.Parse(strs[1]))
                    {
                        promotions[0].achieved = true;
                    }

                    break;
                }
            }

            promotions[0].content += seniorityStr;

            // add performance

            string[] spilt_performances = cv.performance.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            string[] performances = spilt_performances.Where(x => x != " ").ToArray();



            int count = Math.Min(int.Parse(strs[2]), performances.Length);
            string performanceStr = $"\n近{strs[2]}年的考績為:";

            for (int i = 0; i != count; i++)
            {
                performanceStr = performanceStr + $" {performances[i]},";
            }

            try
            {
                if (count >= int.Parse(strs[2]))
                {
                    int sum = 0;

                    for (int i = 0; i != count; i++)
                    {
                        sum += int.Parse(performances[i]);
                    }

                    if ((double)sum / int.Parse(strs[2]) >= int.Parse(strs[3]))
                    {
                        promotions[1].achieved = true;
                    }

                }

            }
            catch
            {

            }

            if (count > 0)
                performanceStr = performanceStr.Substring(0, performanceStr.Length - 1);

            promotions[1].content += performanceStr;

            return;
        }

        /// <summary>
        /// 根據員工權限取得所有升等項目之動態物件。
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>員工升等項目動態物件</returns>
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

                    List<Promotion> promotions = _promotionRepository.GetByUser(item.empno);

                    userObj.upgrade = this.CanUpgrade(item, promotions, user.department_manager);

                    userObj.isRecommended = promotions.Where(x => x.condition == 7).FirstOrDefault()?.achieved;
                    authorization.Users.Add(userObj);

                }

            }



            authorization.User = JObject.FromObject(user);
            //authorization.Users = JArray.FromObject((users).ToList());
            authorization.User.nextProfTitle = this.NextProfTitle(user.profTitle);

            return JsonConvert.SerializeObject(authorization);
        }

        // private method
        /// <summary>
        /// 取得升等職位名稱。
        /// </summary>
        /// <param name="profTitle">目前職位</param>
        /// <returns>升等職位</returns>
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
        /// <summary>
        /// 取得升等條件。
        /// </summary>
        /// <param name="profTitle">目前職位</param>
        /// <returns>升等條件相關資料陣列</returns>
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

        /// <summary>
        /// 計算是否可升等，一般或特別
        /// </summary>
        /// <param name="user">員工</param>
        /// <param name="promotions">升等項目</param>
        /// <param name="department_manager">協理</param>
        /// <returns>是否可升等，一般或特別</returns>
        private string CanUpgrade(User user, List<Promotion> promotions, bool department_manager)
        {
            // CAN'T UPGRADE TITLE
            List<string> cantUpgradeTitles = new List<string> { "主任工程師", "主任建築師", "主任規劃師", "製圖師", "資深專員二", "資深專員一" };
            if (cantUpgradeTitles.Contains(user.profTitle))
                return "";


            try
            {
                CV cv = (_talentRepository as TalentRepository).Get(user.empno).First();
                //List<Promotion> promotions = _promotionRepository.GetByUser(user.empno);

                if (department_manager)
                {
                    //if (promotions.Where(x => x.condition == 7).First().achieved == false && user.group_one != "行政組")
                    //    return "";

                    //if (promotions.Where(x => x.condition == 7).First().achieved == false && user.group_one != "行政組")
                    //    return "";

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
                //string[] performances = cv.performance.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                string[] spilt_performances = cv.performance.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                string[] performances = spilt_performances.Where(x => x != " ").ToArray();

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

        /// <summary>
        /// 刪除所有升等項目
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>是否刪除成功</returns>
        public bool DeleteAll(string empno)
        {
            var ret = _promotionRepository.DeleteAll();

            if (!ret)
                return false;

            string dir = HttpContext.Current.Server.MapPath("~/App_Data/Promotion");

            DirectoryInfo di = new DirectoryInfo(dir);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            return true;
        }

        /// <summary>
        /// 下載升等名單
        /// </summary>
        /// <param name="authStr">升等名單JSON字串</param>
        /// <returns>升等名單位元</returns>
        public byte[] DownloadAuthExcel(string authStr)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //var auth = GetAuthorization("");

            dynamic auth = JsonConvert.DeserializeObject<dynamic>(authStr);


            // version 1: color

            //using (var package = new ExcelPackage())
            //{
            //    var sheet = package.Workbook.Worksheets.Add("一般升等");

            //    sheet.Cells["A:B"].Style.Font.Size = 12f;

            //    int count = 0;

            //    foreach (var user in auth.Users)
            //    {
            //        if (user.upgrade == "normal")
            //        {
            //            count++;

            //            sheet.Cells[count, 1].Value = (string)user.name;
            //            sheet.Cells[count, 2].Value = (string)user.profTitle;

            //            if (user.isRecommended == true)
            //            {
            //                sheet.Cells[count, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            //                sheet.Cells[count, 1].Style.Fill.BackgroundColor.SetColor(1, 247, 239, 217);
            //            }

            //        }
            //    }

            //    sheet = package.Workbook.Worksheets.Add("特別升等");

            //    sheet.Cells["A:B"].Style.Font.Size = 12f;

            //    count = 0;

            //    foreach (var user in auth.Users)
            //    {
            //        if (user.upgrade == "bonus")
            //        {
            //            count++;

            //            sheet.Cells[count, 1].Value = (string)user.name;
            //            sheet.Cells[count, 2].Value = (string)user.profTitle;

            //            if (user.isRecommended == true)
            //            {
            //                sheet.Cells[count, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            //                sheet.Cells[count, 1].Style.Fill.BackgroundColor.SetColor(1, 247, 239, 217);
            //            }
            //        }
            //    }

            //    var excelData = package.GetAsByteArray();  // byte or stream


            //    return excelData;
            //}

            // version 2 - formal

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("一般升等");
                int row = 1;

                sheet.Cells["A:D"].Style.Font.Size = 12f;
                sheet.Cells["A:D"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;                

                sheet.Cells["A1:D1"].Merge = true;
                sheet.Cells[row, 1].Value = "已提報";
                row++;

                sheet.Cells[row, 1].Value = "員工編號";
                sheet.Cells[row, 2].Value = "員工名稱";
                sheet.Cells[row, 3].Value = "目前職等";
                sheet.Cells[row, 4].Value = "目標職等";
                row++;

                foreach (var user in auth.Users)
                {
                    if (user.upgrade == "normal" && user.isRecommended == true)
                    {
                        sheet.Cells[row, 1].Value = (string)user.empno;
                        sheet.Cells[row, 2].Value = (string)user.name;
                        sheet.Cells[row, 3].Value = (string)user.profTitle;
                        sheet.Cells[row, 4].Value = (string)user.nextProfTitle;
                        row++;
                    }
                }
                row++;

                sheet.Cells[row, 1, row, 4].Merge = true;
                sheet.Cells[row, 1].Value = "未提報";
                row++;

                sheet.Cells[row, 1].Value = "員工編號";
                sheet.Cells[row, 2].Value = "員工名稱";
                sheet.Cells[row, 3].Value = "目前職等";
                sheet.Cells[row, 4].Value = "目標職等";
                row++;

                foreach (var user in auth.Users)
                {
                    if (user.upgrade == "normal" && user.isRecommended != true)
                    {
                        sheet.Cells[row, 1].Value = (string)user.empno;
                        sheet.Cells[row, 2].Value = (string)user.name;
                        sheet.Cells[row, 3].Value = (string)user.profTitle;
                        sheet.Cells[row, 4].Value = (string)user.nextProfTitle;
                        row++;
                    }
                }

                sheet.Cells["A:D"].AutoFitColumns();

                // page 2

                sheet = package.Workbook.Worksheets.Add("特別升等");
                row = 1;

                sheet.Cells["A:D"].Style.Font.Size = 12f;
                sheet.Cells["A1:D1"].Merge = true;
                sheet.Cells["A:D"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells[row, 1].Value = "已提報";
                row++;

                sheet.Cells[row, 1].Value = "員工編號";
                sheet.Cells[row, 2].Value = "員工名稱";
                sheet.Cells[row, 3].Value = "目前職等";
                sheet.Cells[row, 4].Value = "目標職等";
                row++;

                foreach (var user in auth.Users)
                {
                    if (user.upgrade == "bonus" && user.isRecommended == true)
                    {
                        sheet.Cells[row, 1].Value = (string)user.empno;
                        sheet.Cells[row, 2].Value = (string)user.name;
                        sheet.Cells[row, 3].Value = (string)user.profTitle;
                        sheet.Cells[row, 4].Value = (string)user.nextProfTitle;
                        row++;
                    }
                }
                row++;

                sheet.Cells[row, 1, row, 4].Merge = true;
                sheet.Cells[row, 1].Value = "未提報";
                row++;

                sheet.Cells[row, 1].Value = "員工編號";
                sheet.Cells[row, 2].Value = "員工名稱";
                sheet.Cells[row, 3].Value = "目前職等";
                sheet.Cells[row, 4].Value = "目標職等";
                row++;

                foreach (var user in auth.Users)
                {
                    if (user.upgrade == "bonus" && user.isRecommended != true)
                    {
                        sheet.Cells[row, 1].Value = (string)user.empno;
                        sheet.Cells[row, 2].Value = (string)user.name;
                        sheet.Cells[row, 3].Value = (string)user.profTitle;
                        sheet.Cells[row, 4].Value = (string)user.nextProfTitle;
                        row++;
                    }
                }

                sheet.Cells["A:D"].AutoFitColumns();

                var excelData = package.GetAsByteArray();  // byte or stream


                return excelData;
            }

        }

        public void Dispose()
        {
            _promotionRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}