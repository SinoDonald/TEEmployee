using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Education
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

        public List<Course> GetAllCourses()
        {
            var ret = _educationRepository.GetAllCourses();
            return ret;
        }

        public bool UploadCourseFile(Stream input)
        {           
            List<Course> courses = ProcessCourseXlsx(input);
            bool ret = _educationRepository.InsertCourses(courses);

            return ret;
        }

        private List<Course> ProcessCourseXlsx(Stream stream)
        {
            List<Course> courses = new List<Course>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets["軌二部"];

                int rowCount = worksheet.Dimension.End.Row;
                Course course = new Course() { chapters = new List<Chapter>() };

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
                        if (course.chapters.Count > 0)
                        {
                            courses.Add(course.ShallowCopy());
                        }
                        break;
                    }                        

                    // Start of a new course
                    if (columnBValue != "")
                    {
                        if (course.chapters.Count > 0)
                        {
                            courses.Add(course.ShallowCopy());
                        }

                        course = new Course() { chapters = new List<Chapter>() };
                        course.course_title = columnBValue.Substring(4);
                    }

                    Chapter chapter = new Chapter();
                    string group_one = "通識";
                    string subject = "通識";
                    string type = "通識";


                    if (columnIValue.Contains('-'))
                    {
                        try
                        {
                            string[] words = columnIValue.Split('-');
                            group_one = words[0];
                            subject = words[1];
                            type = words[2];
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    course.course_group = columnHValue;
                    course.course_group_one = group_one;
                    course.course_subject = subject;

                    chapter.id = int.Parse(columnAValue);
                    chapter.chapter_type = type;
                    chapter.chapter_title = columnCValue;
                    chapter.duration = columnDValue;
                    chapter.createdTime = columnEValue;

                    course.chapters.Add(chapter);

                }

            }

            return courses;
        }

        public List<Record> GetAllRecords()
        {
            var ret = _educationRepository.GetAllRecords();
            return ret;
        }

        public List<Record> GetAllRecordsByUser(string empno)
        {
            var ret = _educationRepository.GetAllRecordsByUser(empno);
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
                userObj.records = JArray.FromObject(this.GetAllRecordsByUser(item.empno));
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

        public void Dispose()
        {
            _educationRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}