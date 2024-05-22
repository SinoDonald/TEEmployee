using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace TEEmployee.Models.Training
{
    public class TrainingService : IDisposable
    {
        private ITrainingRepository _trainingRepository;
        private IUserRepository _userRepository;

        public TrainingService()
        {
            _trainingRepository = new TrainingRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// 取得所有培訓紀錄
        /// </summary>
        /// <returns>培訓紀錄JSON</returns>
        /// <remarks>於後端轉換Date格式為string，並回傳JSON</remarks>
        public string GetAllRecordsJSON()
        {
            var ret = _trainingRepository.GetAllRecords();
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(ret, settings);
        }

        /// <summary>
        /// 根據權限，取得特定員工培訓紀錄
        /// </summary>
        /// <param name="user_empno">使用者員工編號</param>
        /// <param name="self_empno">欲查詢員工編號</param>
        /// <returns>培訓紀錄JSON</returns>
        public string GetAllRecordsByUserJSON(string user_empno, string self_empno)
        {
            var ret = _trainingRepository.GetAllRecordsByUser(user_empno).OrderByDescending(x => x.start_date);
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(ret, settings);
        }

        /// <summary>
        /// 取得個人培訓紀錄
        /// </summary>
        /// <param name="user_empno">員工編號</param>
        /// <returns>培訓紀錄列舉</returns>
        private List<Record> GetAllRecordsByUser(string user_empno)
        {
            var ret = _trainingRepository.GetAllRecordsByUser(user_empno).OrderByDescending(x => x.start_date).ToList();
            return ret;
        }

        /// <summary>
        /// 上傳培訓紀錄
        /// </summary>
        /// <param name="input">培訓紀錄檔案stream</param>
        /// <returns>是否上傳成功</returns>
        public bool UploadTrainingFile(Stream input)
        {
            // Insert KpiModels then Insert Kpiitems
            List<Record> records = ProcessTrainingStream(input);
            bool ret = _trainingRepository.InsertRecords(records);
            return ret;
        }

        /// <summary>
        /// 轉換培訓紀錄stream為培訓紀錄列舉
        /// </summary>
        /// <param name="stream">培訓紀錄檔案stream</param>
        /// <returns>培訓紀錄列舉</returns>
        private List<Record> ProcessTrainingStream(Stream stream)
        {
            List<Record> records = new List<Record>();

            using (StreamReader sr = new StreamReader(stream, Encoding.GetEncoding("big5")))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("培訓"))
                    {
                        string[] parts = line.Split('|');
                        Record record = new Record();

                        record.empno = parts[1];
                        record.roc_year = Int32.Parse(parts[2]);
                        record.training_type = parts[3];
                        record.training_id = parts[4];
                        record.title = parts[5];
                        record.organization = parts[6];
                        record.start_date = DateTime.Parse(parts[7]);
                        record.end_date = DateTime.Parse(parts[8]);
                        record.duration = float.Parse(parts[9]);

                        records.Add(record);
                    }
                }
            }

            return records;
        }

        /// <summary>
        /// 根據權限，取得權限下所有員工資訊與培訓紀錄
        /// </summary>
        /// <param name="empno">員工編號</param>
        /// <returns>員工資訊與培訓紀錄動態物件</returns>
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

                users = users.Where(x => !string.IsNullOrEmpty(x.group_one)).ToList();

                foreach (var item in users)
                {
                    dynamic userObj = JObject.FromObject(item);
                    userObj.trainings = JArray.FromObject(this.GetAllRecordsByUser(item.empno));
                    authorization.Users.Add(userObj);
                }

            }

            authorization.User = JObject.FromObject(user);
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(authorization, settings);
        }

        /// <summary>
        /// 下載群組培訓紀錄資料
        /// </summary>
        /// <param name="year">培訓紀錄年度</param>
        /// <param name="empno">員工編號</param>
        /// <returns>培訓紀錄位元資料</returns>
        public byte[] DownloadGroupExcel(int year, string empno)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            User user = _userRepository.Get(empno);
            List<User> users = new List<User>();
            dynamic authorization = new JObject();
            authorization.Users = new JArray();

            List<Record> records = new List<Record>();

            if (user.department_manager || user.group_manager || user.group_one_manager)
            {
                users = _userRepository.GetAll();

                if (user.group_manager)
                    users = users.Where(x => x.group == user.group).ToList();

                if (user.group_one_manager)
                    users = users.Where(x => x.group_one == user.group_one).ToList();

                users = users.Where(x => !string.IsNullOrEmpty(x.group_one)).ToList();

                foreach (var item in users)
                {                    
                    records.AddRange(this.GetAllRecordsByUser(item.empno));
                }
            }

            records = records.Where(x => x.start_date.Year - 1911 == year).ToList();
            records = records.OrderBy(x => x.start_date).ThenBy(x => x.training_id).ToList();

            List<User> sorted_users = users.OrderBy(x => x.group_one).ThenBy(x => x.empno).ToList();
            var group_query = sorted_users.GroupBy(x => x.group_one);

            List<Record> courses = records.GroupBy(x => x.training_id).Select(g => g.First()).ToList();


            //dynamic auth = JsonConvert.DeserializeObject<dynamic>(authStr);


            // version 2 - formal

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("群組培訓紀錄");
                int row = 1;

                //sheet.Cells["A:D"].Style.Font.Size = 12f;
                //sheet.Cells["A:D"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // column names and merge
                sheet.Cells["A1:C1"].Merge = true;
                sheet.Cells["A2:A3"].Merge = true;                
                sheet.Cells["B2:B3"].Merge = true;
                sheet.Cells["C2:C3"].Merge = true;

                sheet.Cells[1, 1].Value = $"{year}年度培訓紀錄";
                sheet.Cells[2, 1].Value = "課程名稱";
                sheet.Cells[2, 2].Value = "日期";
                sheet.Cells[2, 3].Value = "時數";


                // groupname 
                row = 2;
                int col = 3;

                foreach (var item in group_query)
                {
                    int new_col = col + item.Count();

                    sheet.Cells[row, col + 1, row, new_col].Merge = true;
                    sheet.Cells[row, col + 1].Value = $"{item.Key}({item.Count()}位)";

                    col = new_col;
                }



                // empno names 
                row = 3;

                for (int i = 0; i < sorted_users.Count; i++)
                {
                    sheet.Cells[row, i + 4].Value = sorted_users[i].name;
                }

                // courses and checks
                row = 3;
                string current_course = "";

                foreach (var item in records)
                {
                    if (current_course != item.training_id)
                    {
                        current_course = item.training_id;                                               
                        
                        row++;

                        sheet.Cells[row, 1].Value = item.title;
                        sheet.Cells[row, 2].Value = item.start_date.ToString("yyyy/MM/dd");
                        sheet.Cells[row, 3].Value = item.duration;

                    }

                    sheet.Cells[row, sorted_users.FindIndex(x => x.empno == item.empno) + 4].Value = "⭐️";
                   
                }


                sheet.Cells["A:C"].AutoFitColumns();

                var excelData = package.GetAsByteArray();  // byte or stream


                return excelData;
            }

        }

        public void Dispose()
        {
            _trainingRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}