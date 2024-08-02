using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TEEmployee.Models.Talent
{
    public class CvRawExtractor
    {
        private static readonly string[] ChineseDigits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

        /// <summary>
        /// 基本資料
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractAge(List<CvRaw> inputs)
        {
            string age = "";

            try
            {
                age = inputs.FirstOrDefault(x => x.datatype == "基本資料")?.dataitem;
            }
            catch
            {

            }
            
            return age;
        }

        /// <summary>
        /// 學歷
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractEducation(List<CvRaw> inputs)
        {
            string education = "";
            var texts = inputs.Where(x => x.datatype == "學歷").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((words) =>
                {
                    var p = words.Split('|');

                    return $"{p[0]} ({p[1]}) {p[2]} {p[3]}";

                }).ToList();

                //var parsedList = texts.Select(words =>
                //{
                //    var p = words.Split('|');
                //    var yearPart = p[1].Replace("年畢業", "");
                //    int graduationYear = Int32.Parse(yearPart);

                //    return new
                //    {
                //        FormatedEduString = $"{p[0]} ({p[1]}) {p[2]} {p[3]}",
                //        GraduationYear = graduationYear
                //    };
                //}).ToList();

                //var sortedList = parsedList.OrderByDescending(x => x.GraduationYear).Select(x => x.FormatedEduString).ToList();
                //var combinedString = string.Join("\n", sortedList);
                var combinedString = string.Join("\n", texts);
                List<string> educations = combinedString.Split(new char[] { '\n' }).ToList();
                education = ReturnEducationalParagraph(educations); // 大學學歷以上才加入
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }

            return education;
        }

        /// <summary>
        /// 大學學歷以上才加入
        /// </summary>
        /// <param name="empno"></param>
        /// <returns></returns>
        private static string ReturnEducationalParagraph(List<string> educations)
        {
            string returnParagraph = string.Empty;

            try
            {
                foreach (string paragraph in educations)
                {
                    if (paragraph.Contains("大學") || paragraph.Contains("學士") || paragraph.Contains("碩士") || paragraph.Contains("博士"))
                    {
                        string[] splitString = paragraph.Split('|');
                        try
                        {
                            returnParagraph += splitString[0] + "(" + splitString[1] + ")　" + splitString[2] + "　" + splitString[3] + "\n";
                        }
                        catch (Exception) { }
                    }
                }
                if (returnParagraph.Length > 2)
                {
                    returnParagraph = returnParagraph.Substring(0, returnParagraph.Length - 1);
                }
            }
            catch
            {

            }
            
            return returnParagraph;
        }

        /// <summary>
        /// 專長
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractExpertise(List<CvRaw> inputs)
        {
            string expertise = "";
            var texts = inputs.Where(x => x.datatype == "專長").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

                var combinedString = string.Join("\n", parsedList);

                expertise = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }

            return expertise;
        }

        /// <summary>
        /// 論著
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractTreatise(List<CvRaw> inputs)
        {
            string treatise = "";
            var texts = inputs.Where(x => x.datatype == "論著").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((words, index) =>
                {
                    var p = words.Split('|');

                    return $"{index + 1}.{p[0]}\n({p[1]})";

                }).ToList();

                var combinedString = string.Join("\n", parsedList);

                treatise = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }            

            return treatise;
        }

        /// <summary>
        /// 語文能力
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractLanguage(List<CvRaw> inputs)
        {
            string language = "";
            var texts = inputs.Where(x => x.datatype == "語文能力").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

                var combinedString = string.Join("\n", parsedList);

                language = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }

            return language;
        }

        /// <summary>
        /// 學術組織
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractAcademic(List<CvRaw> inputs)
        {
            string academic = "";
            var texts = inputs.Where(x => x.datatype == "學術組織").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((words, index) =>
                {
                    var p = words.Split('|');

                    return $"{index + 1}.{p[0]} {p[1]}";

                }).ToList();

                var combinedString = string.Join("\n", parsedList);

                academic = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }
            

            return academic;
        }

        /// <summary>
        /// 專業證照
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractLicense(List<CvRaw> inputs)
        {
            string license = "";
            var texts = inputs.Where(x => x.datatype == "專業證照").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((words, index) =>
                {
                    var p = words.Split('|');

                    if (string.IsNullOrEmpty(p[1]))
                        return $"{index + 1}.{p[0]} ({p[2]})";
                    else
                        return $"{index + 1}.{p[0]} ({p[1]}、{p[2]})";

                }).ToList();

                var combinedString = string.Join("\n", parsedList);

                license = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }           

            return license;

        }

        /// <summary>
        /// 技術訓練
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractTraining(List<CvRaw> inputs)
        {
            string training = "";
            var texts = inputs.Where(x => x.datatype == "技術訓練").Select(x => x.dataitem).ToList();

            try
            {
                var parsedList = texts.Select((words, index) =>
                {
                    var p = words.Split('|');

                    return $"{index + 1}.{p[0]} ({p[1]}) {p[2]}\n({p[3]})";

                }).ToList();

                var combinedString = string.Join("\n", parsedList);

                training = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }
            

            return training;
        }

        /// <summary>
        /// 榮譽
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractHonor(List<CvRaw> inputs)
        {
            string honor = "";
            var texts = inputs.Where(x => x.datatype == "榮譽").Select(x => x.dataitem).ToList();

            try
            {
                
                var parsedList = texts.Select((words, index) =>
                {
                    var p = words.Split('|');

                    return $"{index + 1}.{p[0]} {p[1]} ({p[2]})";

                }).ToList();

                var combinedString = string.Join("\n", parsedList);

                honor = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }            

            return honor;
        }

        /// <summary>
        /// 經歷概要
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractExperience(List<CvRaw> inputs)
        {
            string experience = "";
            var texts = inputs.Where(x => x.datatype == "經歷概要").Select(x => x.dataitem).ToList();

            try
            {              
                var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

                var combinedString = string.Join("\n", parsedList);

                experience = combinedString;
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }            

            return experience;
        }

        /// <summary>
        /// 職等歷程
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractSeniority(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "職等歷程").Select(x => x.dataitem)/*.Reverse()*/.ToList();

            var parsedList = texts.Select((x, index) => $"{x}"/*$"{index + 1}.{x}"*/).ToList();

            var combinedString = string.Join("\n", parsedList);
            List<string> jobTitles = combinedString.Split(new char[] { '\n' }).ToList();
            string seniority = AnalysisSeniority(jobTitles, ExtractProject(inputs));

            return seniority;
        }
        /// <summary>
        /// 解析公司、工作、職務年資
        /// </summary>
        /// <param name="jobTitles"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        private static string AnalysisSeniority(List<string> jobTitles, string project)
        {
            CV userCV = new CV();
            List<Seniority> senioritys = new List<Seniority>();
            for (int i = 0; i < jobTitles.Count(); i++)
            {
                if (i == 0) { userCV.companyYears = jobTitles[i].Split('|')[0]; } // 基本資料：公司到職日 --> 計算員工公司年資
                Seniority userSeniority = new Seniority();
                userSeniority.start = jobTitles[i].Split('|')[0];
                userSeniority.position = jobTitles[i].Split('|')[jobTitles[i].Split('|').Length - 1];
                senioritys.Add(userSeniority);
            }
            // 工作年資
            string workYear = string.Empty;
            try { if (!String.IsNullOrEmpty(project)) { workYear = ProjectRegex(project).LastOrDefault().start; } }
            catch (Exception ex) { string error = ex.Message + "\n" + ex.ToString(); }
            // 職務年資
            string seniority = string.Empty;
            senioritys = senioritys.OrderByDescending(x => x.start).ToList();
            for (int i = 0; i < senioritys.Count; i++)
            {
                if (i.Equals(0))
                {
                    // 西元轉民國
                    DateTime companyDT = DateTime.Parse(userCV.companyYears);
                    DateTime seniorityDT = DateTime.Parse(senioritys[i].start);
                    CultureInfo culture = new CultureInfo("zh-TW");
                    culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                    string companyDate = companyDT.ToString("yyy.MM.dd", culture);
                    if (!String.IsNullOrEmpty(workYear)) { seniority += "工作年資：" + workYear + "~迄今\n公司年資：" + companyDate + "~迄今\n"; }
                    else { seniority += "工作年資：\n公司年資：" + companyDate + "~迄今\n"; }
                    string seniorityDate = seniorityDT.ToString("yyy.MM.dd", culture);
                    seniority += senioritys[i].position + "：" + seniorityDate + "~迄今\n";
                }
                else
                {
                    DateTime start = DateTime.Parse(senioritys[i].start);
                    DateTime end = DateTime.Parse(senioritys[i - 1].start);
                    (DateTime st, DateTime ed, int y, int m, int d) calcYMD = TalentRepository.CalcYMD(start, end);
                    seniority += senioritys[i].position + "：" + calcYMD.y + "年" + calcYMD.m + "月\n"/* + calcYMD.d + "日\n"*/;
                }
            }
            if (seniority.Length > 2)
            {
                seniority = seniority.Substring(0, seniority.Length - 1);
            }

            return seniority;
        }
        /// <summary>
        /// 解析工作年資
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static List<Seniority> ProjectRegex(string project)
        {
            List<Seniority> senioritys = new List<Seniority>();
            string company = string.Empty;
            foreach (string readLine in project.Split('\n'))
            {
                try
                {
                    Regex rg = new Regex(@"(\([\u4e00-\u9fa5_a-zA-Z0-9]\))");
                    if (rg.IsMatch(readLine))
                    {
                        Seniority seniority = new Seniority();
                        // 先查詢有幾個space
                        int spaceCount = readLine.Split(' ').Length;
                        if (spaceCount <= 3)
                        {
                            rg = new Regex(@"(\([\u4e00-\u9fa5_a-zA-Z0-9]\))\ (\d*\.\d*)~(.*)\ (.*)", RegexOptions.IgnoreCase);
                            MatchCollection m = rg.Matches(readLine); //將比對後集合傳給 MatchCollection
                            Match match = m[0];
                            string[] startStr = match.Groups[2].Value.Split('.');
                            string start = startStr[0].PadLeft(3, '0') + "." + startStr[1].PadLeft(2, '0');
                            seniority.start = start;
                            if (match.Groups[3].Value.Equals("迄今"))
                            {
                                string year = (DateTime.Now.Year - 1911).ToString("000");
                                string month = DateTime.Now.Month.ToString("00");
                                seniority.end = year + "." + month;
                                seniority.now = true;
                            }
                            else
                            {
                                string[] endStr = match.Groups[3].Value.Split('.');
                                string end = endStr[0].PadLeft(3, '0') + "." + endStr[1].PadLeft(2, '0');
                                seniority.end = end;
                            }
                            if (match.Groups[4].Value.Contains("中興"))
                            {
                                company = "中興工程";
                                seniority.company = company;
                            }
                            else
                            {
                                rg = new Regex(@"(\([\u4e00-\u9fa5]\))\ (\d*\.\d*)~(.*)\ (.*)", RegexOptions.IgnoreCase);
                                if (rg.IsMatch(readLine))
                                {
                                    company = match.Groups[4].Value;
                                    seniority.company = company;
                                }
                                else
                                {
                                    seniority.company = company;
                                    seniority.department = match.Groups[4].Value;
                                }
                            }
                            senioritys.Add(seniority);
                        }
                        else
                        {
                            rg = new Regex(@"(\([\u4e00-\u9fa5_a-zA-Z0-9]\))\ (\d*\.\d*)~(.*)\ (.*)\ (.*)", RegexOptions.IgnoreCase);
                            MatchCollection m = rg.Matches(readLine); //將比對後集合傳給 MatchCollection
                            Match match = m[0];
                            string[] startStr = match.Groups[2].Value.Split('.');
                            string start = startStr[0].PadLeft(3, '0') + "." + startStr[1].PadLeft(2, '0');
                            seniority.start = start;
                            if (match.Groups[3].Value.Equals("迄今"))
                            {
                                string year = (DateTime.Now.Year - 1911).ToString("000");
                                string month = DateTime.Now.Month.ToString("00");
                                seniority.end = year + "." + month;
                                seniority.now = true;
                            }
                            else
                            {
                                string[] endStr = match.Groups[3].Value.Split('.');
                                string end = endStr[0].PadLeft(3, '0') + "." + endStr[1].PadLeft(2, '0');
                                seniority.end = end;
                            }
                            seniority.company = company;
                            seniority.department = match.Groups[4].Value;
                            if (match.Groups[5].Value.Contains("兼"))
                            {
                                string changeName = match.Groups[5].Value.Replace("重大", "").Replace("（", "(").Replace("）", ")");
                                int index = changeName.IndexOf('兼');
                                seniority.position = changeName.Substring(0, index);
                                seniority.manager = changeName.Substring(index + 1, changeName.Length - index - 1);
                            }
                            else if (match.Groups[5].Value.Contains("/"))
                            {
                                string changeName = match.Groups[5].Value.Replace("重大", "").Replace("（", "(").Replace("）", ")");
                                int index = changeName.IndexOf('/');
                                seniority.position = changeName.Substring(0, index);
                                seniority.manager = changeName.Substring(index + 1, changeName.Length - index - 1);
                            }
                            else if (match.Groups[5].Value.Contains(")") && match.Groups[5].Value.IndexOf(')') != match.Groups[5].Value.Length - 1)
                            {
                                string changeName = match.Groups[5].Value.Replace("重大", "").Replace("（", "(").Replace("）", ")");
                                int index = changeName.IndexOf(')');
                                seniority.position = changeName.Substring(0, index + 1);
                                seniority.manager = changeName.Substring(index + 1, changeName.Length - index - 1);
                            }
                            else
                            {
                                string changeName = match.Groups[5].Value.Replace("（", "(").Replace("）", ")");
                                changeName = ChangeName(changeName); // 職稱文字判斷
                                seniority.position = changeName;
                            }
                            senioritys.Add(seniority);
                        }
                    }
                }
                catch (Exception) { }
            }

            return senioritys;
        }
        /// <summary>
        /// 職稱文字判斷
        /// </summary>
        /// <param name="empno"></param>
        /// <returns></returns>
        private static string ChangeName(string changeName)
        {
            List<string> removeStr = new List<string>() { "一", "二", "三", "四" }; // 職稱內要判斷有無()
            string word = removeStr.Where(s => changeName.Contains(s)).Select(s => s).FirstOrDefault();
            if (!String.IsNullOrEmpty(word))
            {
                changeName = changeName.Replace("(", "").Replace(")", "");
                int index = changeName.LastIndexOf(word);
                changeName = changeName.Insert(index, "(").Insert(index + 2, ")");
            }
            else
            {
                if (changeName.Contains("(") || changeName.Contains(")"))
                {
                    changeName = changeName.Replace("(", "").Replace(")", "");
                }
            }

            return changeName;
        }

        /// <summary>
        /// 經歷
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string ExtractProject(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "經歷").Select(x => x.dataitem).ToList();

            string combinedString = "";
            int outerCount = 1;
            int innerCount = 1;

            try
            {
                foreach (var words in texts)
                {
                    var p = words.Split('|');

                    // Outer (W|W|E|E)
                    if (string.IsNullOrEmpty(p[2]) && string.IsNullOrEmpty(p[3]) && !string.IsNullOrEmpty(p[1]))
                    {
                        combinedString += $"({ChineseDigits[outerCount]}) {p[0]} {p[1]}\n";
                        outerCount++;
                        innerCount = 1;
                        continue;
                    }

                    // Inner Case 1(W|W|W|E)
                    if (!string.IsNullOrEmpty(p[2]) && string.IsNullOrEmpty(p[3]))
                    {
                        combinedString += $"({innerCount}) {p[0]} {p[1]} {p[2]}\n";
                        innerCount++;
                        continue;
                    }

                    // Inner Case 2(W|E|E|E)
                    if (string.IsNullOrEmpty(p[2]) && string.IsNullOrEmpty(p[3]))
                    {
                        combinedString += $"({innerCount}) {p[0]} {p[1]} {p[2]}\n";
                        innerCount++;
                        continue;
                    }

                    // Bullet Case 1 (W|E|E|W)
                    if (!string.IsNullOrEmpty(p[0]) && !string.IsNullOrEmpty(p[3]))
                    {
                        combinedString += $"． {p[3]}({p[0]})\n";
                        continue;
                    }

                    // Bullet Case 2 (E|E|E|W)
                    if (string.IsNullOrEmpty(p[0]) && !string.IsNullOrEmpty(p[3]))
                    {
                        combinedString += $"． {p[3]}\n";
                        continue;
                    }
                }
            }
            catch
            {
                var concatString = "";

                foreach (var item in texts)
                {
                    concatString += item;
                }

                return concatString;
            }            

            return combinedString;
        }
    }
}