﻿using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
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
        // 取得員工履歷
        public List<CV> Get(string empno)
        {
            List<CV> ret;

            string sql = @"SELECT * FROM userCV WHERE empno=@empno";
            if (empno == "4125")
            {
                sql = @"SELECT * FROM userCV ORDER BY empno";
            }

            ret = _conn.Query<CV>(sql, new { empno }).ToList();

            return ret;
        }
        // 讀取Word人員履歷表
        public List<CV> ReadWord()
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
                                    userCVs.Add(userCV);
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
                            // 民國轉西元後計算年齡
                            CultureInfo culture = new CultureInfo("zh-TW");
                            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                            DateTime birthday = DateTime.Parse(cells[1].InnerText, culture);
                            DateTime now = DateTime.Now;
                            int age = now.Year - birthday.Year;
                            if(now.Month < birthday.Month || (now.Month == birthday.Month && now.Day < birthday.Day))
                            {
                                age--;
                            }
                            userCV.age = age.ToString();
                            break;
                        case "出 生 地：":
                            userCV.address = cells[1].InnerText;
                            break;
                        case "學　　歷：":
                            userCV.educational = cells[1].Elements<Paragraph>().Select(o => o.InnerText).FirstOrDefault();
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
            return returnParagraph;
        }
        // 更新資料庫
        public bool UpdateUserCV(List<CV> userCVs)
        {
            bool ret = false;

            _conn.Open();
            // 先確認資料庫中是否已更新當季的資料
            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"DELETE FROM userCV";
                _conn.Execute(sql);

                sql = @"INSERT INTO userCV (empno, name, birthday, age, address, educational, expertise, treatise, language, academic, license, training, honor, experience, project, lastest_update, planning, test, advantage, developed, future)
                            VALUES(@empno, @name, @birthday, @age, @address, @educational, @expertise, @treatise, @language, @academic, @license, @training, @honor, @experience, @project, @lastest_update, @planning, @test, @advantage, @developed, @future)";
                _conn.Execute(sql, userCVs);

                tran.Commit();
            }

            _conn.Close();

            return ret;
        }

        public void Dispose()
        {
            return;
        }
    }
}