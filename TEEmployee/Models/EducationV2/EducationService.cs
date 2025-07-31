using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Drawing;

namespace TEEmployee.Models.EducationV2
{
    public class EducationService
    {
        private IEducationRepository _educationRepository;
        private IUserRepository _userRepository;

        public EducationService()
        {
            _educationRepository = new EducationRepository();
            _userRepository = new UserRepository();
        }

        public List<Content> GetAllContents()
        {
            var ret = _educationRepository.GetAllContents();
            return ret;
        }

        // Get contents that have extra information set by department
        public List<Content> GetAllValidContents()
        {
            var ret = _educationRepository.GetAllContents();
            ret = ret.Where(x => !string.IsNullOrEmpty(x.content_code)).ToList();
            return ret;
        }

        public bool UploadCourseFile(Stream input, string empno)
        {            
            List<Content> contentExtras = new List<Content>();            
            contentExtras = processCourseXlsx(input);
            bool ret = _educationRepository.InsertContentExtras(contentExtras);

            return ret;
        }

        public bool UpsertAssignments(List<Assignment> assignments, string assigner)
        {
            assignments.ForEach(x => x.assigner = assigner);
            var ret = _educationRepository.UpsertAssignments(assignments);
            return ret;
        }

        public bool DeleteAssignments(List<Assignment> assignments, string assigner)
        {
            assignments.ForEach(x => x.assigner = assigner);
            var ret = _educationRepository.DeleteAssignments(assignments);
            return ret;
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

        public List<Assignment> GetAssignmentsByAssigner(List<string> empnos, string assigner)
        {
            var ret = _educationRepository.GetAssignmentsByAssigner(assigner);
            ret = ret.Where(x => empnos.Contains(x.empno)).ToList();

            return ret;
        }

        public List<Assignment> GetAssignmentsWithRecord(List<string> empnos)
        {
            var ret = _educationRepository.GetAssignmentsWithRecordByUsers(empnos);
            
            foreach (var item in ret)
            {
                if (item.record == null)
                {
                    item.completed = false;
                }                    
                else
                {
                    if (item.record.score > TimeSpanToSeconds(item.record.mediaLength))
                    {
                        item.completed = true;
                    }
                    else
                    {
                        item.completed = false;
                    }
                }
                   
            }
            
            return ret; 
        }
        

        private List<Content> processCourseXlsx(Stream stream)
        {
            List<Content> contentExtras = new List<Content>();

            // Dictionary to track counts for each code
            var codeCounters = new Dictionary<string, int>();

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



                    Content chapter = new Content();
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

                    //if (chapter_collection.Any(x => x.id == chapter.id))
                    //    continue;

                    chapter.course_group = columnHValue;
                    chapter.course_group_one = group_one;
                    chapter.course_title = current_course_title;

                    chapter.content_type = type;
                    chapter.content_scope = scope;

                    string code_prefix = buildChapterCode(chapter);

                    // Increment counter for this type
                    if (!codeCounters.ContainsKey(code_prefix))
                        codeCounters[code_prefix] = 1;
                    else
                        codeCounters[code_prefix]++;

                    // Assign the code
                    chapter.content_code = $"{code_prefix}{codeCounters[code_prefix]:D3}";
                   
                    contentExtras.Add(chapter);

                }

            }

            return contentExtras;
        }

        private int TimeSpanToSeconds(string timeSpanStr)
        {
            if (TimeSpan.TryParseExact(timeSpanStr, @"hh\:mm", null, out TimeSpan timeSpan))
            {
                return (int)timeSpan.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }


        private string buildChapterCode(Content chapter)
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

            switch (chapter.content_scope)
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

            switch (chapter.content_type)
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

        public void Dispose()
        {
            _educationRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}