using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.GEducation
{
    public class GEducationService
    {
        private IGEducationRepository _educationRepository;
        private IUserRepository _userRepository;

        public GEducationService()
        {
            _educationRepository = new GEducationRepository();
            _userRepository = new UserRepository();
        }

        public List<Chapter> GetAllChapters()
        {
            var ret = _educationRepository.GetAllChapters();
            return ret;
        }

        public bool UploadCourseFile(Stream input)
        {
            List<Chapter> chapters = processCourseXlsx(input);

            bool ret = _educationRepository.InsertChapters(chapters);

            return ret;
        }

        private List<Chapter> processCourseXlsx(Stream stream)
        {
            List<Chapter> chapters = new List<Chapter>();
            List<Chapter> chapter_collection = GetAllChapters();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets["軌二部"];

                int rowCount = worksheet.Dimension.End.Row;
                string current_course_title = "";

                for (int row = 4; row <= rowCount; row++)
                {
                    // Get values from columns
                    string columnAValue = worksheet.Cells[row, 1].Text;
                    string columnBValue = worksheet.Cells[row, 2].Text;
                    string columnCValue = worksheet.Cells[row, 3].Text;
                    string columnDValue = worksheet.Cells[row, 4].Text;
                    string columnEValue = worksheet.Cells[row, 5].Text;
                    string columnHValue = worksheet.Cells[row, 8].Text;
                    string columnIValue = worksheet.Cells[row, 9].Text;

                   
                    // True end of row
                    if (columnCValue == "")
                    {
                        break;
                    }

                    // Start of a new course
                    if (columnBValue != "")
                    {
                        current_course_title = columnBValue.Substring(4);
                    }



                    Chapter chapter = new Chapter();
                    string group_one = "";
                    string scope = "";
                    string type = "";


                    if (columnIValue.Contains('-'))
                    {
                        try
                        {
                            string[] words = columnIValue.Split('-');
                            group_one = words[0];
                            scope = words[1];
                            type = words[2];
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    chapter.id = int.Parse(columnAValue);

                    if (chapter_collection.Any(x => x.id == chapter.id))
                        continue;

                    chapter.course_group = columnHValue;
                    chapter.course_group_one = group_one;
                    chapter.course_title = current_course_title;

                    chapter.chapter_type = type;
                    chapter.chapter_scope = scope;
                    chapter.chapter_title = columnCValue;
                    chapter.duration = columnDValue;
                    chapter.createdTime = columnEValue;

                    chapter.chapter_code = buildChapterCode(chapter);

                    chapters.Add(chapter);

                }

            }

            return chapters;
        }

        private string buildChapterCode(Chapter chapter)
        {
            string chapterCode = "";

            switch (chapter.course_group)
            {
                case "通識":
                    chapterCode += "GN";
                    break;

                case "規劃":
                    chapterCode += "PL";
                    break;

                case "專管":
                    chapterCode += "PM";
                    break;

                case "設計":
                    chapterCode += "DE";
                    break;
            }

            switch (chapter.course_group_one)
            {
                case "全體":
                    chapterCode += "AL";
                    break;

                case "營運規劃組":
                    chapterCode += "OP";
                    break;

                case "交通工程組":
                    chapterCode += "TE";
                    break;

                case "排水管線組":
                    chapterCode += "DS";
                    break;

                case "用地開發組":
                    chapterCode += "LU";
                    break;

                case "工程管理組":
                    chapterCode += "EM";
                    break;

                case "成本契約組":
                    chapterCode += "CC";
                    break;

                case "工務組":
                    chapterCode += "PW";
                    break;

                case "地工組":
                    chapterCode += "GE";
                    break;

                case "界面組":
                    chapterCode += "II";
                    break;

                case "BIM組":
                    chapterCode += "BS";
                    break;
            }

            switch (chapter.chapter_scope)
            {
                case "創新領域":
                    chapterCode += "NEW";
                    break;

                case "領域專業":
                    chapterCode += "PRO";
                    break;

                case "通用專業":
                    chapterCode += "GNL";
                    break;

                case "經驗交流":
                    chapterCode += "EXP";
                    break;

                case "營運規劃":
                    chapterCode += "OPE";
                    break;

                case "效益評估":
                    chapterCode += "BEN";
                    break;

                case "路工":
                    chapterCode += "RDE";
                    break;

                case "排水":
                    chapterCode += "DRA";
                    break;

                case "管線":
                    chapterCode += "PIP";
                    break;

                case "都計變更/用地取得":
                    chapterCode += "URB";
                    break;

                case "施工規劃":
                    chapterCode += "CTP";
                    break;

                case "專案管理":
                    chapterCode += "PJM";
                    break;

                case "預算編製":
                    chapterCode += "BUD";
                    break;

                case "招標文件彙編":
                    chapterCode += "TEN";
                    break;

                case "工程契約":
                    chapterCode += "CON";
                    break;

                case "工程實務":
                    chapterCode += "CPE";
                    break;

                case "地質調查":
                    chapterCode += "GSV";
                    break;

                case "其他":
                    chapterCode += "OTH";
                    break;

                case "深開挖工程":
                    chapterCode += "DEX";
                    break;

                case "潛盾隧道":
                    chapterCode += "SHT";
                    break;

                case "高架基礎":
                    chapterCode += "VIA";
                    break;

                case "建物保護":
                    chapterCode += "BPT";
                    break;

                case "界面整合":
                    chapterCode += "ITF";
                    break;

                case "標誌設計":
                    chapterCode += "SGN";
                    break;

                case "建築機能":
                    chapterCode += "ARF";
                    break;

                case "(智慧)交通/交維":
                    chapterCode += "ITS";
                    break;

                case "車流模擬分析":
                    chapterCode += "TSA";
                    break;

                case "AutoCAD/C3D出圖":
                    chapterCode += "CAD";
                    break;

            }

            switch (chapter.chapter_type)
            {
                case "G通識":
                    chapterCode += "G";
                    break;

                case "0概論":
                    chapterCode += "0";
                    break;

                case "A基礎":
                    chapterCode += "A";
                    break;

                case "B進階":
                    chapterCode += "B";
                    break;

                case "C案例":
                    chapterCode += "C";
                    break;
            }

            return chapterCode;
        }

        public dynamic GetAuthorization(string empno)
        {
            User user = _userRepository.Get(empno);
            List<User> users = new List<User>();
            dynamic authorization = new JObject();
            authorization.Users = new JArray();

            users = _userRepository.GetAll();

            if (user.department_manager)
            {

            }
            else if (user.group_manager)
            {
                users = users.Where(x => x.group == user.group).ToList();
            }
            else if (user.group_one_manager)
            {
                users = users.Where(x => x.group_one == user.group_one).ToList();
            }
            else
            {
                users = users.Where(x => x.empno == user.empno).ToList();
            }

            users = users.Where(x => !string.IsNullOrEmpty(x.group_one)).ToList();

            foreach (var item in users)
            {
                dynamic userObj = JObject.FromObject(item);
                //userObj.records = JArray.FromObject(this.GetAllRecordsByUser(item.empno));
                authorization.Users.Add(userObj);
            }

            authorization.User = JObject.FromObject(user);
            return JsonConvert.SerializeObject(authorization);
        }

        public bool UpsertRecords(List<Record> records, string empno)
        {
            var ret = _educationRepository.UpsertRecords(records);
            return ret;
        }

        public Record UpdateRecordCompleted(Record record, string empno)
        {
            var ret = _educationRepository.UpdateRecordCompleted(record);
            return ret;
        }

        public Chapter UpdateChapterDigitalized(Chapter chapter, string empno)
        {
            var ret = _educationRepository.UpdateChapterDigitalized(chapter);
            return ret;
        }

        public List<Record> GetAllRecordsByUser(string empno)
        {
            var ret = _educationRepository.GetAllRecordsByUser(empno);
            return ret;
        }

        public void Dispose()
        {
            _educationRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}