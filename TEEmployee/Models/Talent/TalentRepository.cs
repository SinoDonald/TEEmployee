using Dapper;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using IFilterTextReader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TEEmployee.Models.Profession;
using Image = System.Drawing.Image;
using Personal = TEEmployee.Models.Profession.Personal;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace TEEmployee.Models.Talent
{
    public class TalentRepository : ITalentRepository
    {
        private IDbConnection _conn;
        private string _appData = "";
        public TalentRepository()
        {
            string talentConnection = ConfigurationManager.ConnectionStrings["TalentConnection"].ConnectionString;
            _conn = new SQLiteConnection(talentConnection);
            _appData = HttpContext.Current.Server.MapPath("~/App_Data");
        }
        // 檔案版本比對
        public class FileInfo
        {
            public string empno { get; set; }
            public string lastModifiedDate { get; set; }
        }
        // 取得所有員工履歷
        public List<CV> GetAll(string empno)
        {
            UpdateUsersGroup(new UserRepository().GetAll().ToList()); // 更新SQL員工當前所屬群組

            List<CV> ret;

            string sql = @"SELECT * FROM userCV WHERE empno=@empno";
            ret = _conn.Query<CV>(sql, new { empno }).ToList();

            if (empno == "4125")
            {
                sql = @"SELECT * FROM userCV ORDER BY empno";
                ret = _conn.Query<CV>(sql, new { empno }).ToList();

                List<string> usersEmpno = new UserRepository().GetAll().Select(x => x.empno).ToList(); // 在職員工
                List<string> exEmployees = new List<string>(); // 不顯示的名單(協理+離職員工)
                exEmployees.Add("4125");
                foreach (CV user in ret)
                {
                    string exEmployee = usersEmpno.Where(x => x.Equals(user.empno)).FirstOrDefault();
                    if (exEmployee == null)
                    {
                        exEmployees.Add(user.empno);
                    }
                }
                // 移除不顯示的名單
                foreach (string exEmployee in exEmployees)
                {
                    CV removeEmployee = ret.Where(x => x.empno.Equals(exEmployee)).FirstOrDefault();
                    ret.Remove(removeEmployee);
                }
            }

            return ret;
        }
        // 取得所有員工職等職級
        public List<string> GetSenioritys()
        { 
            // 公司所有職等
            string sql = @"SELECT * FROM jobTitle";
            List<string> jobTitles = _conn.Query<JobTitle>(sql).ToList().Select(x => x.name).ToList();
            
            // 部門同仁所有職等
            sql = @"SELECT * FROM userCV ORDER BY empno";
            List<string> senioritys = _conn.Query<CV>(sql, new { }).Where(x => x.seniority != null).Select(x => x.seniority).Distinct().OrderBy(x => x).ToList();
            List<string> seniorityList = new List<string>();
            foreach (string seniority in senioritys)
            {
                string[] analyzeSeniority = seniority.Split('\n');
                if (analyzeSeniority.Length > 2)
                {
                    for (int i = 2; i < analyzeSeniority.Length; i++)
                    {
                        try
                        {
                            string saveSeniority = analyzeSeniority[i].Split('：')[0];
                            string jobTitle = jobTitles.Where(s => saveSeniority.Equals(s)).Select(s => s).FirstOrDefault();
                            if (jobTitle != null)
                            {
                                if(seniorityList.Where(x => x.Equals(jobTitle)).FirstOrDefault() == null)
                                {
                                    seniorityList.Add(jobTitle);
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            seniorityList = seniorityList.Distinct().OrderBy(x => x).ToList();
            seniorityList.Insert(0, "");

            return seniorityList;
        }
        // 條件篩選
        public List<CV> ConditionFilter(ConditionFilter filter, List<CV> userCVs)
        {
            List<CV> filterUserCVs = new List<CV>();
            try{ filterUserCVs = userCVs.Where(x => filter.age1 <= Age(x.birthday) && Age(x.birthday) <= filter.age2).ToList(); } // 年齡 
            catch (Exception) { }
            try { filterUserCVs = filterUserCVs.Where(x => filter.companyYear1 <= CompanyYears(x.seniority) && CompanyYears(x.seniority) <= filter.companyYear2).ToList(); } // 公司年資 
            catch (Exception) { }
            try { if (filter.educational != null) { filterUserCVs = filterUserCVs.Where(x => EducationalName(x.educational).Equals(filter.educational)).ToList(); } } // 教育程度 
            catch (Exception) { }
            try { if (filter.seniority != null) { filterUserCVs = filterUserCVs.Where(x => x.seniority.Contains(filter.seniority)).ToList(); } } // 曾任職等
            catch (Exception) { }
            try { if (filter.nowPosition != null) { filterUserCVs = filterUserCVs.Where(x => NowPosition(x.seniority).Equals(filter.nowPosition)).ToList(); } } // 當前職等
            catch (Exception) { }

            return filterUserCVs;
        }
        // 年齡
        private int Age(string userBirthday)
        {
            // 計算年齡, 民國轉西元後計算年齡
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            DateTime birthday = DateTime.Parse(userBirthday, culture);
            DateTime now = DateTime.Now;
            int age = now.Year - birthday.Year;

            return age;
        }
        // 公司年資
        private int CompanyYears(string seniority)
        {
            int companyYears = 0;
            try
            {
                // 計算年齡, 民國轉西元後計算年齡
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                string companyYearStr = seniority.Split('\n')[1].Split('：')[1].Split('~')[0];
                DateTime start = DateTime.Parse(companyYearStr, culture);
                (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(start, DateTime.Now);
                companyYears = calcYMD.y;
            }
            catch { }

            return companyYears;
        }
        // 教育程度
        private string EducationalName(string educational)
        {
            List<string> removeStr = new List<string>() { "博士", "碩士", "學士" };
            string word = removeStr.Where(s => educational.Contains(s)).Select(s => s).FirstOrDefault();
            if (word != null)
            {
                educational = word;
            }

            return educational;
        }
        // 當前職等
        private string NowPosition(string seniority)
        {
            string nowPosition = seniority.Split('\n')[2].Split('：')[0];

            return nowPosition;
        }
        // 更新SQL員工當前所屬群組
        public bool UpdateUsersGroup(List<User> users)
        {
            bool ret = false;

            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"UPDATE userCV SET 'group'=@group, group_one=@group_one, group_two=@group_two, group_three=@group_three WHERE empno=@empno";
                    _conn.Execute(sql, users, tran);
                    tran.Commit();
                    ret = true;
                }
                _conn.Close();
            }
            catch (Exception) { }

            return ret;
        }
        // 儲存選項
        public bool SaveChoice(List<Ability> users)
        {
            bool ret = false;
            string sql = @"SELECT * FROM userCV ORDER BY empno";
            List<CV> userCVs = _conn.Query<CV>(sql).ToList();
            foreach(Ability user in users)
            {
                CV userCV = userCVs.Where(x => x.empno.Equals(user.empno)).FirstOrDefault();
                userCV.position = user.position;
                userCV.choice1 = user.choice1;
                userCV.choice2 = user.choice2;
                userCV.choice3 = user.choice3;
                userCV.choice4 = user.choice4;
                userCV.choice5 = user.choice5;
            }
            try
            {
                _conn.Open();
                // 先確認資料庫中是否已更新當季的資料
                using (var tran = _conn.BeginTransaction())
                {
                    //string sql = @"DELETE FROM userCV";
                    //_conn.Execute(sql);
                    sql = @"UPDATE userCV SET position=@position, choice1=@choice1, choice2=@choice2, choice3=@choice3, choice4=@choice4, choice5=@choice5 WHERE empno=@empno";
                    _conn.Execute(sql, userCVs);

                    tran.Commit();
                }
                _conn.Close();
            }
            catch (Exception) { }

            return ret;
        }
        // 取得員工履歷
        public List<CV> Get(string empno)
        {
            List<CV> ret;

            string sql = @"SELECT * FROM userCV WHERE empno=@empno";
            ret = _conn.Query<CV>(sql, new { empno }).ToList();

            foreach (CV userCV in ret)
            {
                if (userCV.planning == null)
                {
                    userCV.planning = "";
                }

                // 計算年齡, 民國轉西元後計算年齡
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                DateTime birthday = DateTime.Parse(userCV.birthday, culture);
                DateTime now = DateTime.Now;
                int age = now.Year - birthday.Year;
                if (now.Month < birthday.Month || (now.Month == birthday.Month && now.Day < birthday.Day))
                {
                    age--;
                }
                userCV.age = age.ToString();

                // 解析SQL seniority文字, 儲存工作、公司與職位年資
                Tuple<string, string, string> analyzeSeniority = AnalyzeSeniority(userCV.seniority);
                userCV.workYears = analyzeSeniority.Item1; // 工作年資
                userCV.companyYears = analyzeSeniority.Item2; // 公司年資
                userCV.seniority = analyzeSeniority.Item3; // 職務經歷

                // 取得核心專業盤點的專業與管理能力分數
                List<Personal> getPersonal = new ProfessionRepository().GetPersonal(userCV.empno);
                // 專業能力_領域技能
                List<Personal> domainScores = getPersonal.Where(x => x.skill_type.Equals("domain") && x.score >= 4).OrderBy(x => x.custom_order).ToList();
                foreach (Personal domainScore in domainScores)
                {
                    userCV.domainSkill += domainScore.content + "：" + domainScore.score + "\n";
                }
                if (userCV.domainSkill != null)
                {
                    userCV.domainSkill = userCV.domainSkill.Substring(0, userCV.domainSkill.Length - 1); // 移除最後的"/n"
                }
                else
                {
                    userCV.domainSkill = "\n";
                }
                // 專業能力_核心技能
                List<Personal> coreScores = getPersonal.Where(x => x.skill_type.Equals("core") && x.score >= 4).OrderBy(x => x.custom_order).ToList();
                foreach (Personal coreScore in coreScores)
                {
                    userCV.coreSkill += coreScore.content + "：" + coreScore.score + "\n";
                }
                if (userCV.coreSkill != null)
                {
                    userCV.coreSkill = userCV.coreSkill.Substring(0, userCV.coreSkill.Length - 1); // 移除最後的"/n"
                }
                else
                {
                    userCV.coreSkill = "\n";
                }
                // 管理能力
                List<Personal> manageScores = getPersonal.Where(x => x.skill_type.Equals("manage") && x.score >= 4).OrderBy(x => x.custom_order).ToList();
                foreach (Personal manageScore in manageScores)
                {
                    userCV.manageSkill += manageScore.content + "：" + manageScore.score + "\n";
                }
                if (userCV.manageSkill != null)
                {
                    userCV.manageSkill = userCV.manageSkill.Substring(0, userCV.manageSkill.Length - 1); // 移除最後的"/n"
                }
                else
                {
                    userCV.manageSkill = "\n";
                }
            }

            return ret;
        }
        // 解析SQL seniority文字, 儲存工作、公司與職位年資
        public Tuple<string, string, string> AnalyzeSeniority(string analyzeSeniority)
        {
            string workYears = string.Empty;
            string companyYears = string.Empty;
            string positionSeniority = string.Empty;

            // 計算年齡, 民國轉西元後計算年齡
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            DateTime now = DateTime.Now;
            Regex regex = new Regex(@"(.*)\：(.*\..*)\~(.*)", RegexOptions.IgnoreCase);
            string[] senioritys = analyzeSeniority.Split('\n');
            // 工作年資
            try
            {
                MatchCollection matches = regex.Matches(senioritys[0]);
                Match match = matches[0];
                DateTime start = DateTime.Parse(match.Groups[2].Value, culture);
                (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(start, now);
                workYears = calcYMD.y + "年" + calcYMD.m + "月";
            }
            catch { }
            // 公司年資
            try
            {
                MatchCollection matches = regex.Matches(senioritys[1]);
                Match match = matches[0];
                DateTime start = DateTime.Parse(match.Groups[2].Value, culture);
                (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(start, now);
                companyYears = calcYMD.y + "年" + calcYMD.m + "月";
            }
            catch { }
            // 職位年資
            try
            {
                for (int i = 2; i < senioritys.Length; i++)
                {
                    if (senioritys[i].Contains("迄今"))
                    {
                        MatchCollection matches = regex.Matches(senioritys[i]);
                        Match match = matches[0];
                        DateTime start = DateTime.Parse(match.Groups[2].Value, culture);
                        (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(start, now);
                        positionSeniority += match.Groups[1].Value + "：" + calcYMD.y + "年" + calcYMD.m + "月\n"; ;
                    }
                    else
                    {
                        positionSeniority += senioritys[i] + "\n";
                    }
                }
                if (positionSeniority.Length > 1)
                {
                    positionSeniority = positionSeniority.Substring(0, positionSeniority.Length - 1);
                }
            }
            catch { }

            return new Tuple<string, string, string>(workYears, companyYears, positionSeniority);
        }
        // 比對上傳的檔案更新時間
        public List<string> CompareLastestUpdate(List<string> filesInfo)
        {
            List<string> updateUsers = new List<string>();
            List<FileInfo> fileInfoList = FileLastestUpdate(filesInfo); // 解析更新上傳時間
            List <CV> usersCV = GetLastestUpdate();
            foreach(FileInfo fileInfo in fileInfoList)
            {
                string userLastestUpdate = usersCV.Where(x => x.empno.Equals(fileInfo.empno)).Select(x => x.lastest_update).FirstOrDefault();
                if(userLastestUpdate != null)
                {
                    CultureInfo culture = new CultureInfo("zh-TW");
                    DateTime sqlDate = DateTime.Parse(userLastestUpdate, culture); // 資料庫的檔案更新時間
                    DateTime fileDate = DateTime.Parse(fileInfo.lastModifiedDate, culture); // 要上傳檔案的更新時間
                    if ((fileDate.Ticks - sqlDate.Ticks) > 0)
                    {
                        CV userCV = usersCV.Where(x => x.empno.Equals(fileInfo.empno)).FirstOrDefault();
                        string addFileName = userCV.empno + userCV.name + ".docx";
                        updateUsers.Add(addFileName);
                    }
                }
            }
            return updateUsers;
        }
        // 解析更新上傳時間
        public List<FileInfo> FileLastestUpdate(List<string> filesInfo)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            foreach(string info in filesInfo)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo();
                    fileInfo.empno = Regex.Replace(Path.GetFileName(info.Split('：')[0]), "[^0-9]", ""); // 僅保留數字
                    // 解析時間
                    string lastModifiedDate = info.Split('：')[1];
                    int GMTindex = lastModifiedDate.IndexOf("GMT");
                    lastModifiedDate = lastModifiedDate.Substring(4, GMTindex - 4);
                    CultureInfo culture = new CultureInfo("zh-TW");
                    fileInfo.lastModifiedDate = DateTime.Parse(lastModifiedDate, culture).ToString();
                    fileInfos.Add(fileInfo);
                }
                catch(Exception) { }
            }
            return fileInfos;
        }
        // 取得現在SQL存檔的更新時間
        public List<CV> GetLastestUpdate()
        {
            List<CV> ret = new List<CV>();
            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"SELECT * FROM userCV";
                    ret = _conn.Query<CV>(sql).ToList();

                    tran.Commit();
                }
                _conn.Close();
            }
            catch (Exception) { }

            return ret;
        }
        // 讀取Word人員履歷表
        public List<CV> SaveUserCV(List<User> userGroups)
        {
            List<CV> userCVs = new List<CV>();
            string folderPath = Path.Combine(_appData, "Talent\\CV"); // 人員履歷表Word檔路徑
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                string empno = Regex.Replace(Path.GetFileName(file), "[^0-9]", ""); // 僅保留數字
                string lastUpdate = File.GetLastWriteTime(file).ToString();
                if (File.Exists(file))
                {
                    if (Path.GetExtension(file).Contains(".doc"))
                    {
                        try
                        {
                            using (WordprocessingDocument doc = WordprocessingDocument.Open(file, false))
                            {
                                SavePicture(doc, empno); // 儲存圖片
                                // 解析文字
                                try
                                {
                                    CV userCV = ReadWord(doc, empno, lastUpdate);
                                    // 加入使用者群組
                                    User user = userGroups.Where(x => x.empno.ToString().Equals(empno)).FirstOrDefault();
                                    if (user != null)
                                    {
                                        try
                                        {
                                            userCV.group = user.group;
                                            userCV.group_one = user.group_one;
                                            userCV.group_two = user.group_two;
                                            userCV.group_three = user.group_three;
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                    // 加入三年績效考核與High Performance資訊
                                    CV cv = GetAll(empno).FirstOrDefault();
                                    if(cv != null)
                                    {
                                        userCV.advantage = cv.advantage; // 優勢
                                        userCV.disadvantage = cv.disadvantage; // 劣勢
                                        userCV.test = cv.test; // 工作成果
                                        userCV.developed = cv.developed; // 待發展能力
                                        userCV.future = cv.future; // 未來發展規劃
                                        userCV.performance = cv.performance; // 近三年考績
                                        userCV.position = cv.position; // 職位
                                        userCV.choice1 = cv.choice1; // 專業性 – 專家的潛質
                                        userCV.choice2 = cv.choice2; // 格局、視野大 - 舉一反三
                                        userCV.choice3 = cv.choice3; // 責任心驅動的主動性 - 捨我其誰
                                        userCV.choice4 = cv.choice4; // 建立系統性、計畫性的學習能力 - 學習力具體
                                        userCV.choice5 = cv.choice5; // 適應變化的韌性 - 懂得取捨、不放棄
                                    }
                                    else
                                    {
                                        userCV.performance = "";
                                    }
                                    if (userCV.empno != null && userCV.name != null)
                                    { 
                                        userCVs.Add(userCV);
                                    }
                                }
                                catch(Exception)
                                { 
                                
                                }
                            }
                        }
                        catch(Exception)
                        { 

                        }
                    }
                }
            }
            try
            {
                UpdateUserCV(userCVs); // 更新資料庫
            }
            catch (Exception) { }

            // 移除資料夾內檔案
            try
            {
                DirectoryInfo directory = new DirectoryInfo(folderPath);
                directory.EnumerateFiles().ToList().ForEach(f => f.Delete());
                directory.EnumerateDirectories().ToList().ForEach(d => d.Delete(true));
            }
            catch(Exception) { }

            return userCVs;
        }
        // 儲存圖片
        public void SavePicture(WordprocessingDocument doc, string empno)
        {
            string savePath = HttpContext.Current.Server.MapPath("~/Content/CV");
            int imgCount = doc.MainDocumentPart.GetPartsOfType<ImagePart>().Count();
            if (imgCount > 0)
            {
                List<ImagePart> imgParts = new List<ImagePart>(doc.MainDocumentPart.ImageParts);
                foreach (ImagePart imgPart in imgParts)
                {
                    Image img = Image.FromStream(imgPart.GetStream());
                    //string imgfileName = imgPart.Uri.OriginalString.Substring(imgPart.Uri.OriginalString.LastIndexOf("/") + 1);
                    img.Save(savePath + "\\" + empno + ".jpg");
                }
            }
        }
        // 解析文字
        private CV ReadWord(WordprocessingDocument doc, string empno, string lastUpdate)
        {
            CV userCV = new CV();
            userCV.empno = empno;
            userCV.lastest_update = lastUpdate;

            List<Table> tables = doc.MainDocumentPart.Document.Body.Elements<Table>().ToList();
            foreach (Table table in tables)
            {
                //取得TableRow陣列
                var rows = table.Elements<TableRow>().ToArray();
                for (int i = 0; i < rows.Length; i++)
                {
                    //取得TableRow的TableCell陣列
                    var cells = rows[i].Elements<TableCell>().ToArray();
                    // 儲存Word檔中所有的資訊
                    string title = cells[0].InnerText;
                    switch (title)
                    {
                        case "姓　　名：":
                            userCV.name = cells[1].InnerText;
                            break;
                        case "出生日期：":
                            userCV.birthday = cells[1].InnerText;
                            break;
                        case "出 生 地：":
                            userCV.address = cells[1].InnerText;
                            break;
                        case "學　　歷：":
                            //userCV.educational = cells[1].Elements<Paragraph>().Select(o => o.InnerText).FirstOrDefault();
                            userCV.educational = ReturnEducationalParagraph(cells);
                            break;
                        case "專　　長：":
                            userCV.expertise = ReturnParagraph(cells);
                            break;
                        case "論　　著：":
                            userCV.treatise = ReturnParagraph(cells);
                            break;
                        case "語文能力：":
                            userCV.language = ReturnParagraph(cells);
                            break;
                        case "參加學術組織：":
                            userCV.academic = ReturnParagraph(cells);
                            break;
                        case "專業證照：":
                            userCV.license = ReturnParagraph(cells);
                            break;
                        case "技術訓練：":
                            userCV.training = ReturnParagraph(cells);
                            break;
                        case "榮　　譽：":
                            userCV.honor = ReturnParagraph(cells);
                            break;
                        case "經歷概要：":
                            userCV.experience = ReturnParagraph(cells);
                            break;
                        case "經　　歷：":
                            Tuple<string, string> projectAndSeiority = ReturnProjectParagraph(cells);
                            userCV.project = projectAndSeiority.Item1;
                            userCV.seniority = projectAndSeiority.Item2;
                            break;
                        default:
                            break;
                    }
                }
            }

            return userCV;
        }
        // 回傳Word解析後的文字並分段
        private string ReturnParagraph(TableCell[] cells)
        {
            string returnParagraph = string.Empty;
            foreach (string paragraph in cells[1].Elements<Paragraph>().Select(o => o.InnerText).ToList())
            {
                returnParagraph += paragraph + "\n";
            }
            if(returnParagraph.Length > 2 || returnParagraph.Equals("\n"))
            {
                returnParagraph = returnParagraph.Substring(0, returnParagraph.Length - 1);
            }
            return returnParagraph;
        }
        // 大學學歷以上才加入
        private string ReturnEducationalParagraph(TableCell[] cells)
        {
            string returnParagraph = string.Empty;
            foreach (string paragraph in cells[1].Elements<Paragraph>().Select(o => o.InnerText).ToList())
            {
                if (paragraph.Contains("大學") || paragraph.Contains("學士") || paragraph.Contains("碩士") || paragraph.Contains("博士"))
                {
                    string[] splitString = paragraph.Split('　');
                    try
                    {
                        returnParagraph += splitString[2] + splitString[3] + "　" + splitString[0] + "　" + splitString[1] + "\n";
                    }
                    catch (Exception) { }
                }
            }
            if (returnParagraph.Length > 2)
            {
                returnParagraph = returnParagraph.Substring(0, returnParagraph.Length - 1);
            }
            return returnParagraph;
        }
        // 經歷文字解析
        private Tuple<string, string> ReturnProjectParagraph(TableCell[] cells)
        {
            // 讀取jobTitle資料庫內公司所有職等
            List<string> jobTitles = new List<string>();
            string sql = @"SELECT * FROM jobTitle";
            jobTitles = _conn.Query<JobTitle>(sql).ToList().Select(x => x.name/*.Replace("(", "").Replace(")", "")*/).ToList();
            foreach(string jobTitle in _conn.Query<JobTitle>(sql).ToList().Select(x => x.name.Replace("(", "").Replace(")", "")).ToList())
            {
                if(jobTitles.Where(x => x.Equals(jobTitle)).FirstOrDefault() == null)
                {
                    jobTitles.Add(jobTitle);
                }
            }
            jobTitles.Add("製圖工程師");
            jobTitles.Add("正工程師");
            jobTitles.Add("工程師");
            jobTitles.Add("繪圖員");
            jobTitles.Add("實習生");

            string returnParagraph = string.Empty;
            foreach (string paragraph in cells[1].Elements<Paragraph>().Select(o => o.InnerText).ToList())
            {
                string changeParagraph = paragraph.Replace(" ", "").Replace("（", "(").Replace("）", ")");
                if (changeParagraph.Contains(")"))
                {
                    int index = changeParagraph.IndexOf(")");
                    changeParagraph = changeParagraph.Insert(index + 1, " ");
                    // 任職區間, 如果"~"前後都是數字則結尾補上空白
                    if (changeParagraph.Contains("迄今"))
                    {
                        index = changeParagraph.IndexOf("迄今");
                        changeParagraph = changeParagraph.Insert(index + 2, " ");
                    }
                    else
                    {
                        try
                        {
                            Regex regex = new Regex(@"(\([\u4e00-\u9fa5_a-zA-Z0-9]\))\s*(\d*\.\d*)~(\d*\.\d*)", RegexOptions.IgnoreCase);
                            //將比對後集合傳給 MatchCollection
                            MatchCollection matches = null;
                            matches = regex.Matches(changeParagraph);
                            Match match = matches[0];
                            if (match.Groups[3].Value != null)
                            {
                                index = changeParagraph.IndexOf(match.Groups[3].Value);
                                changeParagraph = changeParagraph.Insert(index + match.Groups[3].Value.Length, " ");
                            }
                        }
                        catch (Exception) { }
                    }
                    // 檢查職等是否包含(), 並在職稱前面加空白
                    if (jobTitles.Any(s => changeParagraph.Contains(s)))
                    {
                        Regex rg = new Regex(@"(\([_a-zA-Z0-9]\))");
                        if (rg.IsMatch(changeParagraph))
                        {
                            string jobTitle = jobTitles.Where(s => changeParagraph.Contains(s)).FirstOrDefault();
                            index = changeParagraph.IndexOf(jobTitle);
                            if(changeParagraph.Substring(index - 1, 1) == "(")
                            {
                                changeParagraph = changeParagraph.Insert(index - 1, " ");
                                string position = changeParagraph.Split(' ')[changeParagraph.Split(' ').Length - 1].Replace("(", "").Replace(")", "");
                                string changeName = ChangeName(position); // 職稱文字判斷
                                changeParagraph = changeParagraph.Replace(changeParagraph.Split(' ')[changeParagraph.Split(' ').Length - 1], changeName);
                            }
                            else
                            {
                                changeParagraph = changeParagraph.Insert(index, " ");
                                string changeName = ChangeName(changeParagraph.Split(' ')[changeParagraph.Split(' ').Length - 1]); // 職稱文字判斷
                                changeParagraph = changeParagraph.Replace(changeParagraph.Split(' ')[changeParagraph.Split(' ').Length - 1], changeName);
                            }
                        }
                    }
                }

                returnParagraph += changeParagraph + "\n";
            }
            if (returnParagraph.Length > 2)
            {
                returnParagraph = returnParagraph.Substring(0, returnParagraph.Length - 1);
            }
            // 中興工程職務經歷
            List<Seniority> senioritys = ProjectRegex(returnParagraph);
            string seniority = Seniority(senioritys);

            return new Tuple<string, string>(returnParagraph, seniority);
        }
        // Regex解析文字後, 儲存工作、公司與職位年資
        private List<Seniority> ProjectRegex(string project)
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
                            else if(match.Groups[5].Value.Contains(")") && match.Groups[5].Value.IndexOf(')') != match.Groups[5].Value.Length - 1)
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
        // 中興工程資料
        private string Seniority(List<Seniority> senioritys)
        {
            // 公司年資&職位年資
            string positionSeniority = string.Empty;

            // 計算年齡, 民國轉西元後計算年齡
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();

            // 開始工作的年資
            string startWorkDate = senioritys.Select(x => x.start).OrderBy(x => x).FirstOrDefault();
            DateTime start = DateTime.Parse(startWorkDate, culture);
            DateTime end = DateTime.Now;
            (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(start, end);
            positionSeniority += "工作年資：" + startWorkDate + "~" + "迄今\n";

            // 在中興工程累積的年資
            Seniority SECtoNow = senioritys.Where(x => x.company.Equals("中興工程")).Where(x => x.position == null && x.now == true).OrderBy(x => DateTime.Parse(x.start, culture)).FirstOrDefault();
            start = DateTime.Parse(SECtoNow.start, culture);
            end = DateTime.Parse(SECtoNow.end, culture); // 迄今
            List<Seniority> SEC = senioritys.Where(x => x.company.Equals("中興工程")).Where(x => x.position == null && x != SECtoNow).ToList();
            foreach(Seniority item in SEC)
            {
                // 結束日+1月如果在迄今的時間內, 則比對開始工作日期
                DateTime endDate = DateTime.Parse(item.end, culture).AddMonths(1);
                if(IsInDate(endDate, start, end))
                {
                    // 開始工作日期取較小值
                    DateTime startDate = DateTime.Parse(item.start, culture);
                    if((startDate - start).Days < 0)
                    {
                        start = startDate;
                    }
                }
            }
            string year = (start.Year - 1911).ToString("000");
            string month = start.Month.ToString("00");
            positionSeniority += "公司年資：" + year + "." + month + "~" + "迄今\n";

            // 比對在中興的職位日期, 如果有重疊則合併
            List<Seniority> SECSenioritys = senioritys.Where(x => x.company.Contains("中興")).Where(x => x.position != null).Where(x => IsInDate(DateTime.Parse(x.start, culture), start, end)).ToList();
            Seniority nowPosition = SECSenioritys.Where(x => x.now == true).FirstOrDefault(); // 找到目前職位
            List<StartEndDate> startEndDates = new List<StartEndDate>();
            if(nowPosition == null)
            {
                nowPosition = SECSenioritys.Where(x => x.position != null).FirstOrDefault(); // 找到目前職位
            }
            string changeName = ChangeName(nowPosition.position); // 職稱文字判斷

            // 公司年資內當前的職位
            try
            {
                List<Seniority> SECbyPosition = SECSenioritys.Where(x => x.position.Equals(nowPosition.position)).OrderByDescending(x => DateTime.Parse(x.start, culture)).ToList();
                if (SECbyPosition.Count > 0)
                {
                    MergyDate(culture, SECbyPosition, startEndDates, false, true);
                }
            }
            catch (Exception) { }
            // 只計算現在公司年資內的職位
            foreach (string position in SECSenioritys.Where(x => x.position != nowPosition.position).Select(x => x.position).Distinct().OrderBy(x => x).ToList())
            {
                try
                {
                    List<Seniority> SECbyPosition = SECSenioritys.Where(x => x.position.Equals(position)).OrderByDescending(x => DateTime.Parse(x.start, culture)).ToList();
                    if (SECbyPosition.Count > 0)
                    {
                        MergyDate(culture, SECbyPosition, startEndDates, false, false);
                    }
                }
                catch (Exception) { }
            }
            // 只計算現在公司年資內管理的職位
            List<string> managers = SECSenioritys.Where(x => x.manager != null).Select(x => x.manager).Distinct().OrderBy(x => x).ToList();
            foreach (string manager in managers)
            {
                try
                {
                    List<Seniority> SECbyManager = SECSenioritys.Where(x => x.manager != null && x.manager.Equals(manager)).OrderByDescending(x => DateTime.Parse(x.start, culture)).ToList();
                    if (SECbyManager.Count > 0)
                    {
                        MergyDate(culture, SECbyManager, startEndDates, true, false);
                    }
                }
                catch (Exception) { }
            }

            foreach (StartEndDate startEndDate in startEndDates.Where(x => x.position != null).OrderByDescending(x => x.startDate).ToList()) // 工程師
            {
                positionSeniority += startEndDate.interval;
            }
            foreach (StartEndDate startEndDate in startEndDates.Where(x => x.manager != null).OrderByDescending(x => x.startDate).ToList()) // 管理職
            {
                positionSeniority += startEndDate.interval;
            }

            return positionSeniority;
        }
        // 驗證與當天相差幾年幾月
        (DateTime st, DateTime ed, int y, int m, int d) CalcYMD(DateTime start, DateTime end)
        {
            if (end.CompareTo(start) < 0) (start, end) = (end, start);
            int years = (end.Year - start.Year);
            int months = (end.Month - start.Month);
            int days = (end.Day - start.Day);
            if (days < 0)
            {
                months--;
                var lastMon = end.AddMonths(-1);
                days += DateTime.DaysInMonth(lastMon.Year, lastMon.Month);
            }
            if (months < 0)
            {
                years--;
                months += 12;
            }
            return (start, end, years, months, days);
        }
        // 比對職位日期, 如果有重疊則合併
        private StartEndDate MergyDate(CultureInfo culture, List<Seniority> SECbyPosition, List<StartEndDate> startEndDates, bool isManager, bool isNowPosition)
        {
            List<Seniority> senioritys = new List<Seniority>(); // 儲存未重疊區間的日期

            DateTime startDate = DateTime.Parse(SECbyPosition[0].start, culture);
            DateTime endDate = DateTime.Parse(SECbyPosition[0].end, culture);
            StartEndDate startEndDate = new StartEndDate();
            for (int i = 0; i < SECbyPosition.Count(); i++)
            {
                startEndDate = new StartEndDate();
                DateTime startisInDay = DateTime.Parse(SECbyPosition[i].start, culture).AddMonths(-1); // 落差一個月內的做合併
                DateTime endisInDay = DateTime.Parse(SECbyPosition[i].end, culture).AddMonths(1); // 落差一個月內的做合併
                bool isInDate1 = IsInDate(startisInDay, startDate, endDate);
                bool isInDate2 = IsInDate(endisInDay, startDate, endDate);
                startisInDay = startisInDay.AddMonths(1);
                endisInDay = endisInDay.AddMonths(-1);
                if (isInDate1 && isInDate2 == false)
                {
                    endDate = endisInDay;
                }
                else if (isInDate1 == false && isInDate2)
                {
                    startDate = startisInDay;
                }
                else if (isInDate1 == false && isInDate2 == false)
                {
                    (DateTime st, DateTime ed, int y, int m, int d) startDateCompare = CalcYMD(startDate, endisInDay);
                    (DateTime st, DateTime ed, int y, int m, int d) endDateCompare = CalcYMD(endDate, startisInDay);
                    // 比包含區間大
                    if (startisInDay.CompareTo(startDate) <= 0 && endisInDay.CompareTo(endDate) >= 0)
                    {
                        startDate = startisInDay;
                        endDate = endisInDay;
                    }
                    else
                    {
                        Seniority seniority = new Seniority();
                        if (isManager) { seniority.manager = SECbyPosition[i].manager; }
                        else { seniority.position = SECbyPosition[i].position; }
                        seniority.start = SECbyPosition[i].start;
                        seniority.end = SECbyPosition[i].end;
                        senioritys.Add(seniority);
                    }
                }
            }
            startEndDate.startDate = startDate;
            startEndDate.endDate = endDate;
            string position = string.Empty;
            if (isManager)
            {
                position = ChangeName(SECbyPosition.Select(x => x.manager).FirstOrDefault()); // 職稱文字判斷
                startEndDate.manager = position;
            }
            else
            {
                position = ChangeName(SECbyPosition.Select(x => x.position).FirstOrDefault()); // 職稱文字判斷
                startEndDate.position = position;
            }
            (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(startDate, endDate);
            if (isNowPosition)
            {
                string year = (startDate.Year - 1911).ToString("000");
                string month = startDate.Month.ToString("00");
                startEndDate.interval = position + "：" + year + "." + month + "~" + "迄今\n";
            }
            else
            {
                startEndDate.interval = position + "：" + calcYMD.y + "年" + calcYMD.m + "月" + "\n";
            }
            startEndDates.Add(startEndDate);

            if (senioritys.Count > 0)
            {
                MergyDate(culture, senioritys, startEndDates, isManager, isNowPosition);
            }

            return startEndDate;
        }
        // 判斷某日期是否在日期區間內
        private bool IsInDate(DateTime isInDay, DateTime startDate, DateTime endDate)
        {
            return isInDay.CompareTo(startDate) >= 0 && isInDay.CompareTo(endDate) <= 0;
        }
        // 職稱文字判斷
        private string ChangeName(string changeName)
        {
            List<string> removeStr = new List<string>() { "一", "二", "三", "四" }; // 職稱內要判斷有無()
            string word = removeStr.Where(s => changeName.Contains(s)).Select(s => s).FirstOrDefault();
            if (word != null)
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
        // 更新資料庫
        public bool UpdateUserCV(List<CV> userCVs)
        {
            bool ret = false;

            try
            {
                _conn.Open();
                // 先確認資料庫中是否已更新當季的資料
                using (var tran = _conn.BeginTransaction())
                {
                    //string sql = @"DELETE FROM userCV";
                    //_conn.Execute(sql);
                    string sql = @"INSERT INTO userCV (empno, name, 'group', group_one, group_two, group_three, birthday, address, educational, performance, expertise, treatise, language, academic, license, training, honor, experience, project, seniority, lastest_update, planning, test, advantage, disadvantage, developed, future)
                            VALUES(@empno, @name, @group, @group_one, @group_two, @group_three, @birthday, @address, @educational, @performance, @expertise, @treatise, @language, @academic, @license, @training, @honor, @experience, @project, @seniority, @lastest_update, @planning, @test, @advantage, @disadvantage, @developed, @future)
                            ON CONFLICT(empno)
                            DO UPDATE SET name=@name, 'group'=@group, group_one=@group_one, group_two=@group_two, group_three=@group_three, birthday=@birthday, address=@address, educational=@educational, performance=@performance, expertise=@expertise, treatise=@treatise, language=@language, academic=@academic, license=@license, training=@training, honor=@honor, experience=@experience, project=@project, seniority=@seniority, lastest_update=@lastest_update";
                    _conn.Execute(sql, userCVs);

                    tran.Commit();
                }
                _conn.Close();
            }
            catch (Exception) { }

            return ret;
        }
        // High Performer
        public List<Ability> HighPerformer(List<Skill> getAllScores)
        {
            List<Ability> users = new List<Ability>();
            List<User> allUser = new UserRepository().GetAll().ToList();
            foreach (User user in allUser)
            {
                List<Personal> getPersonal = new ProfessionRepository().GetPersonal(user.empno); // 取得核心專業盤點的專業與管理能力分數
                Ability userAbility = new Ability();
                // 專業能力_領域技能
                List<Personal> domainScores = getPersonal.Where(x => x.skill_type.Equals("domain")).OrderBy(x => x.custom_order).ToList();
                int domainSkills = new ProfessionRepository().GetAll().Where(x => x.skill_type.Equals("domain") && x.role.Equals(user.group_one)).Count();
                double domainScore = 0.0;
                foreach (Personal userDomainScore in domainScores)
                {
                    domainScore += userDomainScore.score;
                    if(userDomainScore.score >= 4)
                    {
                        userAbility.domainSkill += userDomainScore.content + "：" + userDomainScore.score + "\n";
                    }
                }
                // 專業能力_核心技能
                List<Personal> coreScores = getPersonal.Where(x => x.skill_type.Equals("core")).OrderBy(x => x.custom_order).ToList();
                int coreSkills = new ProfessionRepository().GetAll().Where(x => x.skill_type.Equals("core") && x.role.Equals("shared")).Count();
                double coreScore = 0.0;
                foreach (Personal userCoreScore in coreScores)
                {
                    coreScore += userCoreScore.score;
                    if (userCoreScore.score >= 4)
                    {
                        userAbility.coreSkill += userCoreScore.content + "：" + userCoreScore.score + "\n";
                    }
                }
                double professionScore = (domainScore + coreScore) / (domainSkills + coreSkills);
                // 管理能力
                List<Personal> manageScores = getPersonal.Where(x => x.skill_type.Equals("manage")).OrderBy(x => x.custom_order).ToList();
                int managekills = new ProfessionRepository().GetAll().Where(x => x.skill_type.Equals("manage") && x.role.Equals("shared")).Count();
                double manageScore = 0.0;
                foreach (Personal userManageScore in manageScores)
                {
                    manageScore += userManageScore.score;
                    if (userManageScore.score >= 4)
                    {
                        userAbility.manageSkill += userManageScore.content + "：" + userManageScore.score + "\n";
                    }
                }
                manageScore = manageScore / managekills;
                if(professionScore > 3 && manageScore > 3)
                {
                    userAbility.empno = user.empno;
                    userAbility.name = user.name;
                    string sql = @"SELECT * FROM userCV ORDER BY empno";
                    CV userCV = _conn.Query<CV>(sql).ToList().Where(x => x.empno.Equals(user.empno)).FirstOrDefault();
                    if (userCV != null)
                    {
                        userAbility.position = userCV.position;
                        userAbility.choice1 = userCV.choice1;
                        userAbility.choice2 = userCV.choice2;
                        userAbility.choice3 = userCV.choice3;
                        userAbility.choice4 = userCV.choice4;
                        userAbility.choice5 = userCV.choice5;

                        // 身份是否可進行選擇
                        int count = 0;
                        if (userAbility.choice1 == true) { count++; }
                        if (userAbility.choice2 == true) { count++; }
                        if (userAbility.choice3 == true) { count++; }
                        if (userAbility.choice4 == true) { count++; }
                        if (userAbility.choice5 == true) { count++; }
                        if (count >= 3) { userAbility.selectPosition = false; }
                        else { userAbility.selectPosition = true; }
                    }
                    users.Add(userAbility);
                }
            }

            return users;
        }
        // 上傳年度績效檔案
        public bool ImportFile(HttpPostedFileBase file)
        {
            bool ret = false;
            List<CV> userCVs = new List<CV>(); 
            try
            {
                if (Path.GetExtension(file.FileName) != ".xlsx") throw new ApplicationException("請使用Excel 2007(.xlsx)格式");
                var stream = file.InputStream;
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false))
                {
                    WorksheetPart worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(doc.WorkbookPart.Workbook.Descendants<Sheet>().First().Id);
                    Worksheet sheet = worksheetPart.Worksheet;
                    //取得共用字串表
                    SharedStringTable strTable = doc.WorkbookPart.SharedStringTablePart.SharedStringTable;
                    int i = 0;
                    foreach (Row row in sheet.Descendants<Row>())
                    {
                        if(i > 0)
                        {
                            int j = 0;
                            CV userCV = new CV();
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                if(j.Equals(0))
                                {
                                    userCV.empno = GetCellText(cell, strTable);
                                }
                                else if (j.Equals(1))
                                {
                                    userCV.name = GetCellText(cell, strTable);
                                }
                                else
                                {
                                    userCV.performance += GetCellText(cell, strTable) + "\n";
                                }
                                j++;
                            }
                            if (userCV.performance.Length > 2 || userCV.performance.Equals("\n"))
                            {
                                userCV.performance = userCV.performance.Substring(0, userCV.performance.Length - 1);
                            }
                            userCVs.Add(userCV);
                        }
                        i++;
                    }
                }

                ret = SavePerformance(userCVs); // 儲存年度績效
            }
            catch (Exception)
            {

            }

            return ret;
        }
        // 解析年度績效文字
        private string GetCellText(Cell cell, SharedStringTable strTable)
        {
            if (cell.ChildElements.Count == 0)
            {
                return null;
            }
            string val = cell.CellValue.InnerText;
            //若為共享字串時的處理邏輯
            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                val = strTable.ChildElements[int.Parse(val)].InnerText;
            }
            return val;
        }
        // 儲存年度績效
        public bool SavePerformance(List<CV> userCVs)
        {
            bool ret = false;

            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"UPDATE userCV SET performance=@performance WHERE empno=@empno";
                    _conn.Execute(sql, userCVs, tran);
                    tran.Commit();
                    ret = true;
                }
                _conn.Close();
            }
            catch (Exception) { }

            return ret;
        }
        // 上傳測評資料檔案
        public List<CV> ImportPDFFile(HttpPostedFileBase file, string empno)
        {
            string ret = "";
            List<CV> userCVs = new List<CV>();
            try
            {
                if (Path.GetExtension(file.FileName) != ".pdf") throw new ApplicationException("請使用PDF(.pdf)格式");

                //string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Files"));
                string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "Talent");
                // 檢查資料夾是否存在
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string extension = Path.GetExtension(file.FileName);
                var path = Path.Combine(folderPath, file.FileName);
                file.SaveAs(path); // 將檔案存到Server

                FilterReader rilterReader = new FilterReader(path);
                string line = rilterReader.ReadToEnd().Replace(" ", "");
                // 解析PDF內的優、缺點
                int advantages = line.LastIndexOf("1.3典型優勢");
                int disadvantage = line.LastIndexOf("1.4典型劣勢");
                int life = line.LastIndexOf("1.5關鍵的⼈⽣問題");
                int work = line.IndexOf("4.2價值觀:⼯作成果");
                int values = line.IndexOf("4.3價值觀:⼈⽣價值觀");

                CV userCV = new CV();
                userCV.empno = empno;
                userCV.advantage = line.Substring(advantages, disadvantage - advantages).Trim().Replace("1.3典型優勢", "").Replace(".", "。\n").Replace("\uff00", ";"); // 優勢
                userCV.disadvantage = line.Substring(disadvantage, life - disadvantage).Trim().Replace("1.4典型劣勢", "").Replace(".", "。\n").Replace("\uff00", ";"); // 劣勢
                string test = line.Substring(work, values - work).Trim().Replace("4.2價值觀:⼯作成果", "").Replace(".", "。\n").Replace("\uff00", ";"); // 工作成果
                int index = test.LastIndexOf("12345678");
                if(index != 0)
                {
                    userCV.test = test.Substring(0, index);
                }
                else
                {
                    userCV.test = test;
                }
                userCVs.Add(userCV);
                if(SaveTest(userCVs) == true)
                {
                    ret = line; // 儲存測評資料
                }

                rilterReader.Close();

                // 移除資料夾內檔案
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(folderPath);
                    directory.EnumerateFiles().ToList().ForEach(f => f.Delete());
                    directory.EnumerateDirectories().ToList().ForEach(d => d.Delete(true));
                }
                catch (Exception ex)
                {
                    userCV = new CV();
                    userCV.academic = ex.Message;
                    userCVs.Add(userCV);
                }
            }
            catch (Exception ex)
            {
                CV userCV = new CV();
                userCV.academic = ex.Message;
                userCVs.Add(userCV);
            }

            return userCVs;
        }
        // 儲存測評資料
        public bool SaveTest(List<CV> userCVs)
        {
            bool ret = false;

            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"UPDATE userCV SET test=@test, advantage=@advantage, disadvantage=@disadvantage WHERE empno=@empno";
                    _conn.Execute(sql, userCVs, tran);
                    tran.Commit();
                    ret = true;
                }
                _conn.Close();
            }
            catch (Exception) { }

            return ret;
        }
        // 儲存回覆
        public CV SaveResponse(CV userCV, string planning)
        {
            planning = planning.Substring(0, planning.Length - 1);
            userCV.planning = planning;
            try
            {
                _conn.Open();
                using (var tran = _conn.BeginTransaction())
                {
                    string sql = @"UPDATE userCV SET planning=@planning, test=@test, advantage=@advantage, disadvantage=@disadvantage, developed=@developed, future=@future WHERE empno=@empno";
                    _conn.Execute(sql, userCV, tran);
                    tran.Commit();
                }
                _conn.Close();
            }
            catch (Exception) { }

            return userCV;
        }

        public void Dispose()
        {
            return;
        }
    }
}