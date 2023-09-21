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

                // Regex解析文字後, 儲存年月份
                List<DateTime> matchYears = new List<DateTime>();
                Regex regex = new Regex(@"\) (.*)\~", RegexOptions.IgnoreCase);
                //將比對後集合傳給 MatchCollection 
                MatchCollection matches = regex.Matches(userCV.project);
                foreach (Match match in matches)
                {
                    try
                    {
                        string matchYear = match.Groups[1].Value;
                        // 如果年份小於3位數則開頭補0
                        string[] yearMonth = matchYear.Split('.');
                        if(yearMonth[0].Count() < 3)
                        {
                            matchYear = matchYear.Insert(0, "0");
                        }
                        DateTime dt = DateTime.Parse(matchYear, culture);
                        matchYears.Add(dt);
                    }
                    catch (Exception ex) { string error = ex.Message + "\n" + ex.ToString(); }
                }
                // 最早與最晚的日期
                try
                {
                    DateTime minMatchYear = matchYears[0];
                    DateTime maxMatchYear = matchYears[matchYears.Count - 1];
                    (DateTime st, DateTime ed, int y, int m, int d) calcYMD = CalcYMD(minMatchYear, now); // 公司年資
                    userCV.companyYears = calcYMD.y + "年" + calcYMD.m + "月";
                    calcYMD = CalcYMD(maxMatchYear, now); // 工作年資
                    userCV.workYears = calcYMD.y + "年" + calcYMD.m + "月";
                }
                catch(Exception ex) { string error = ex.Message + "\n" + ex.ToString(); }

                // 取得核心專業盤點的專業與管理能力分數
                List<Skill> getAllScores = new ProfessionRepository().GetAll();
                // 專業能力_領域技能
                List<Skill> domainScores = getAllScores.Where(x => x.skill_type.Equals("domain")).ToList();
                foreach (Skill skill in domainScores)
                {
                    if (skill.scores != null)
                    {
                        try
                        {
                            Score score = skill.scores.Where(x => x.empno.Equals(empno)).FirstOrDefault();
                            if (score != null)
                            {
                                if (score.score >= 4)
                                {
                                    userCV.domainSkill += skill.content + "：" + score.score + "\n";
                                }
                            }
                        }
                        catch (Exception) { }
                    }
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
                List<Skill> coreScores = getAllScores.Where(x => x.skill_type.Equals("core")).ToList();
                foreach (Skill skill in coreScores)
                {
                    if (skill.scores != null)
                    {
                        try
                        {
                            Score score = skill.scores.Where(x => x.empno.Equals(empno)).FirstOrDefault();
                            if (score != null)
                            {
                                if (score.score >= 4)
                                {
                                    userCV.coreSkill += skill.content + "：" + score.score + "\n";
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                if(userCV.coreSkill != null)
                {
                    userCV.coreSkill = userCV.coreSkill.Substring(0, userCV.coreSkill.Length - 1); // 移除最後的"/n"
                }
                else
                {
                    userCV.coreSkill = "\n";
                }
                // 管理能力
                List<Skill> manageScores = getAllScores.Where(x => x.skill_type.Equals("manage")).ToList();
                foreach (Skill skill in manageScores)
                {
                    if (skill.scores != null)
                    {
                        try
                        {
                            Score score = skill.scores.Where(x => x.empno.Equals(empno)).FirstOrDefault();
                            if (score != null)
                            {
                                if (score.score >= 4)
                                {
                                    userCV.manageSkill += skill.content + "：" + score.score + "\n";
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                if(userCV.manageSkill != null)
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
                            userCV.project = ReturnParagraph(cells);
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
                    string sql = @"INSERT INTO userCV (empno, name, 'group', group_one, group_two, group_three, birthday, address, educational, performance, expertise, treatise, language, academic, license, training, honor, experience, project, lastest_update, planning, test, advantage, disadvantage, developed, future)
                            VALUES(@empno, @name, @group, @group_one, @group_two, @group_three, @birthday, @address, @educational, @performance, @expertise, @treatise, @language, @academic, @license, @training, @honor, @experience, @project, @lastest_update, @planning, @test, @advantage, @disadvantage, @developed, @future)
                            ON CONFLICT(empno)
                            DO UPDATE SET name=@name, 'group'=@group, group_one=@group_one, group_two=@group_two, group_three=@group_three, birthday=@birthday, address=@address, educational=@educational, performance=@performance, expertise=@expertise, treatise=@treatise, language=@language, academic=@academic, license=@license, training=@training, honor=@honor, experience=@experience, project=@project, lastest_update=@lastest_update";
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
                Ability userAbility = new Ability();
                // 專業能力_領域技能
                List<Skill> domainScores = getAllScores.Where(x => x.skill_type.Equals("domain")).ToList();
                double domainScore = 0.0;
                foreach (Skill skill in domainScores)
                {
                    if (skill.scores != null)
                    {
                        try
                        {
                            Score score = skill.scores.Where(x => x.empno.Equals(user.empno)).FirstOrDefault();
                            if (score != null)
                            {
                                domainScore += score.score;
                                if (score.score >= 4)
                                {
                                    userAbility.domainSkill += skill.content + "：" + score.score + "\n";
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                // 專業能力_核心技能
                List<Skill> coreScores = getAllScores.Where(x => x.skill_type.Equals("core")).ToList();
                double coreScore = 0.0;
                foreach(Skill skill in coreScores)
                {
                    if(skill.scores != null)
                    {
                        try
                        {
                            Score score = skill.scores.Where(x => x.empno.Equals(user.empno)).FirstOrDefault();
                            if(score != null)
                            {
                                coreScore += score.score;
                                if(score.score >= 4)
                                {
                                    userAbility.coreSkill += skill.content + "：" + score.score + "\n";
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                double professionScore = (domainScore + coreScore) / (domainScores.Count() + coreScores.Count());
                // 管理能力
                List<Skill> manageScores = getAllScores.Where(x => x.skill_type.Equals("manage")).ToList();
                double manageScore = 0.0;
                foreach(Skill skill in manageScores)
                {
                    if (skill.scores != null)
                    {
                        try
                        {
                            Score score = skill.scores.Where(x => x.empno.Equals(user.empno)).FirstOrDefault();
                            if (score != null)
                            {
                                manageScore += score.score;
                                if (score.score >= 4)
                                {
                                    userAbility.manageSkill += skill.content + "：" + score.score + "\n";
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                manageScore = manageScore / manageScores.Count();
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
        public List<CV> ImportPDFFile(HttpPostedFileBase file)
        {
            string ret = "";
            List<CV> userCVs = new List<CV>();
            try
            {
                if (Path.GetExtension(file.FileName) != ".pdf") throw new ApplicationException("請使用PDF(.pdf)格式");
                string folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Files"));
                // 檢查資料夾是否存在
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Files"), file.FileName);
                file.SaveAs(path); // 將檔案存到Server

                string empno = Regex.Replace(Path.GetFileName(file.FileName), "[^0-9]", ""); // 僅保留數字
                FilterReader rilterReader = new FilterReader(path);
                string line = rilterReader.ReadToEnd();
                // 解析PDF內的優、缺點
                int advantages = line.LastIndexOf("1.3 典型優勢");
                int disadvantage = line.LastIndexOf("1.4 典型劣勢");
                int life = line.LastIndexOf("1.5 關鍵的⼈⽣問題");
                int work = line.IndexOf("4.2 價值觀: ⼯作成果");
                int values = line.IndexOf("4.3 價值觀: ⼈⽣價值觀");

                CV userCV = new CV();
                userCV.empno = empno;
                userCV.advantage = line.Substring(advantages, disadvantage - advantages).Trim().Replace("1.3 典型優勢", "").Replace(".", "。\n").Replace("\uff00", ";"); // 優勢
                userCV.disadvantage = line.Substring(disadvantage, life - disadvantage).Trim().Replace("1.4 典型劣勢", "").Replace(".", "。\n").Replace("\uff00", ";"); // 劣勢
                userCV.test = line.Substring(work, values - work).Trim().Replace("4.2 價值觀: ⼯作成果", "").Replace(".", "。\n").Replace("\uff00", ";"); // 工作成果
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
                catch (Exception) { }
            }
            catch (Exception)
            {

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