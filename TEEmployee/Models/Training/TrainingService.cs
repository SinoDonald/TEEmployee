using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using TEEmployee.Models.Issue;
using TEEmployee.Models.Promotion;

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
            //var ret = _trainingRepository.GetAllRecordsByUser(user_empno).OrderByDescending(x => x.start_date).ToList();
            var ret = _trainingRepository.GetAllRecordsByUser(user_empno);

            // update: Add records created by external training
            ret.AddRange(GetExternalTrainingRecords(user_empno));

            ret = ret.OrderByDescending(x => x.start_date).ToList();

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

            // update: Add records created by external training
            ret.AddRange(GetExternalTrainingRecords(user_empno));

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
                        //record.start_date = DateTime.Parse(parts[7]);
                        //record.end_date = DateTime.Parse(parts[8]);
                        record.start_date = parts[7];
                        record.end_date = parts[8];
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

            //records = records.Where(x => x.start_date.Year - 1911 == year).ToList();
            records = records.Where(x => int.Parse(x.start_date.Substring(0, 4)) - 1911 == year).ToList();
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
                        //sheet.Cells[row, 2].Value = item.start_date.ToString("yyyy/MM/dd");
                        sheet.Cells[row, 2].Value = item.start_date;
                        sheet.Cells[row, 3].Value = item.duration;

                    }

                    sheet.Cells[row, sorted_users.FindIndex(x => x.empno == item.empno) + 4].Value = "⭐️";

                }


                sheet.Cells["A:C"].AutoFitColumns();

                var excelData = package.GetAsByteArray();  // byte or stream


                return excelData;
            }

        }

        public bool UpdateUserRecords(List<Record> records)
        {
            //bool ret = _trainingRepository.UpdateRecords(records);
            //bool ret = _trainingRepository.UpsertRecords(records);

            // Update: upsert both on records and external records.
            var internalRecords = records.Where(x => !x.isExternal).ToList();
            bool ret = _trainingRepository.UpsertRecords(internalRecords);
            var externalRecords = records.Where(x => x.isExternal).ToList();
            bool ret2 = _trainingRepository.UpsertExternalRecords(externalRecords);

            return ret;
        }

        public List<Record> GetRecentRecords()
        {
            // recent one week
            var records = _trainingRepository.GetAllRecords();

            DateTime oneWeekAgo = DateTime.Now.AddDays(-30);
            List<Record> recentRecords = new List<Record>();

            foreach (var record in records)
            {
                if (DateTime.TryParseExact(record.start_date, "yyyy/M/d",
                        new CultureInfo("zh-TW"), DateTimeStyles.None, out DateTime parsedDate))
                {
                    if (parsedDate >= oneWeekAgo && parsedDate <= DateTime.Now)
                    {
                        recentRecords.Add(record);
                    }
                }
            }

            return recentRecords;
        }

        public dynamic GetRecentRecordsObject()
        {
            var userEmail = _userRepository.GetAll().ToDictionary(d => d.empno, d => d.email);
            dynamic ret = new JArray();

            // records which hours >= 8
            var records = _trainingRepository.GetAllRecords().Where(x => x.duration >= 8);

            // recent one month
            DateTime oneWeekAgo = DateTime.Now.AddDays(-30);
            List<Record> recentRecords = new List<Record>();

            foreach (var record in records)
            {
                if (DateTime.TryParseExact(record.start_date, "yyyy/M/d",
                        new CultureInfo("zh-TW"), DateTimeStyles.None, out DateTime parsedDate))
                {
                    if (parsedDate >= oneWeekAgo && parsedDate <= DateTime.Now)
                    {
                        recentRecords.Add(record);
                    }
                }
            }

            // get traning extra, filter out already emailSent records
            recentRecords = _trainingRepository
                .GetRecordExtraByRecords(recentRecords)
                .Where(x => !x.emailSent).ToList();

            // create dynamic object
            foreach (var record in recentRecords)
            {
                dynamic recordObj = new JObject();
                recordObj.record = JObject.FromObject(record);
                recordObj.email = userEmail[record.empno];
                ret.Add(recordObj);
            }

            return JsonConvert.SerializeObject(ret);
        }

        public List<ExternalTraining> GetExternalTrainingsByGroup(string empno)
        {
            var user = _userRepository.Get(empno);
            var ret = new List<ExternalTraining>();

            if (user.group_manager)
                ret = _trainingRepository.GetExternalTrainingsByGroup(user.group);

            return ret;
        }

        private List<Record> CreateExternalTrainingRecords(string empno)
        {
            List<Record> records = new List<Record>();
            var user = _userRepository.Get(empno);
            var groupExternalTrainings = _trainingRepository.GetExternalTrainingsByGroup(user.group);

            foreach (var item in groupExternalTrainings)
            {
                if (item.members.Contains(empno))
                {
                    Record record = Record.FromExternalTraining(item);
                    record.empno = empno;
                    records.Add(record);
                }

            }

            return records;
        }

        private List<Record> GetExternalTrainingRecords(string empno)
        {
            var ret = _trainingRepository.GetExternalRecordsByUser(empno);
            return ret;
        }

        public dynamic GetGroupAuthorization(string empno)
        {
            User user = _userRepository.Get(empno);
            List<User> users = _userRepository.GetAll();
            dynamic authorization = new JObject();
            authorization.Users = new JArray();

            if (user.group_manager)
            {
                users = users.Where(x => x.group == user.group).ToList();
            }

            users = users.Where(x => !string.IsNullOrEmpty(x.group_one)).OrderBy(x => x.group_one).ToList();

            foreach (var item in users)
            {
                dynamic userObj = JObject.FromObject(item);
                authorization.Users.Add(userObj);
            }

            authorization.User = JObject.FromObject(user);
            return JsonConvert.SerializeObject(authorization);
        }

        //public bool CreateExternalTraining(ExternalTraining training)
        //{
        //    var ret = _trainingRepository.InsertExternalTraining(training);
        //    return ret;
        //}

        public bool CreateExternalTraining(HttpPostedFileBase file, ExternalTraining training)
        {
            try
            {
                if (file != null)
                {
                    string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Training");

                    if (!Directory.Exists(_uploadFolder))
                        Directory.CreateDirectory(_uploadFolder);

                    training.filepath = file.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(_uploadFolder, fileName + extension);

                    while (File.Exists(fullPath))
                    {
                        string randomSuffix = "_" + Guid.NewGuid().ToString("N").Substring(0, 6); // 6-char random string
                        training.filepath = fileName + randomSuffix + extension;
                        fullPath = Path.Combine(_uploadFolder, fileName + randomSuffix + extension);
                    }

                    file.SaveAs(fullPath);
                }

                var ret = _trainingRepository.InsertExternalTraining(training);
                return ret;
            }
            catch
            {
                return false;
            }

        }

        public bool UpdateExternalTraining(HttpPostedFileBase file, ExternalTraining training, string fakename)
        {
            string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Training");

            try
            {
                if (file != null)
                {
                    // create
                    if (string.IsNullOrEmpty(training.filepath))
                    {

                    }
                    else // replace
                    {
                        File.Delete(Path.Combine(_uploadFolder, training.filepath));
                    }

                    training.filepath = file.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(_uploadFolder, fileName + extension);

                    while (File.Exists(fullPath))
                    {
                        string randomSuffix = "_" + Guid.NewGuid().ToString("N").Substring(0, 6); // 6-char random string
                        training.filepath = fileName + randomSuffix + extension;
                        fullPath = Path.Combine(_uploadFolder, fileName + randomSuffix + extension);
                    }

                    file.SaveAs(fullPath);

                }
                else
                {
                    // Do nothing
                    if (string.IsNullOrEmpty(training.filepath))
                    {

                    }
                    else
                    {
                        // Delete
                        if (string.IsNullOrEmpty(fakename))
                        {
                            File.Delete(Path.Combine(_uploadFolder, training.filepath));
                            training.filepath = null;
                        }
                        else // Keep original file
                        {

                        }

                    }

                }

                var ret = _trainingRepository.UpdateExternalTraining(training);
                return ret;
            }
            catch
            {
                return false;
            }

        }

        public byte[] DownloadFile(ExternalTraining training)
        {
            string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Training");
            string fn = Path.Combine(_uploadFolder, training.filepath);

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

        public bool DeleteExternalTraining(ExternalTraining training)
        {
            string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Training");

            var ret = _trainingRepository.DeleteExternalTraining(training);

            if (ret && !string.IsNullOrWhiteSpace(training.filepath))
            {
                File.Delete(Path.Combine(_uploadFolder, training.filepath));
            }

            return ret;
        }

        public bool SendExternalTrainingMail(ExternalTraining training)
        {
            bool ret = true;

            string[] dateSplit = training.start_date.Split('/'); 

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("軌二部門信箱", "mr84124259@mail.sinotech.com.tw"));
            message.Subject = $"【培訓通知】{training.roc_year}年{dateSplit[1]}月{dateSplit[2]}日「{training.title}」培訓課程-參訓通知";

            string[] memebers = training.members.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                //message.To.Add(new MailboxAddress(user.name, user.email));

                foreach (var item in memebers)
                {
                    var user = _userRepository.Get(item);

                    if (!string.IsNullOrEmpty(user.email))
                    {
                        message.To.Add(new MailboxAddress(user.name, user.email));
                    }
                   
                }

                var builder = new BodyBuilder();

                //builder.HtmlBody = "<html xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" xmlns:m=\"http://schemas.microsoft.com/office/2004/12/omml\" xmlns=\"http://www.w3.org/TR/REC-html40\"><head><meta http-equiv=Content-Type content=\"text/html; charset=utf-8\"><meta name=Generator content=\"Microsoft Word 15 (filtered medium)\"><style><!--\r\n/* Font Definitions */\r\n@font-face\r\n\t{font-family:新細明體;\r\n\tpanose-1:2 2 5 0 0 0 0 0 0 0;}\r\n@font-face\r\n\t{font-family:細明體;\r\n\tpanose-1:2 2 5 9 0 0 0 0 0 0;}\r\n@font-face\r\n\t{font-family:\"Cambria Math\";\r\n\tpanose-1:2 4 5 3 5 4 6 3 2 4;}\r\n@font-face\r\n\t{font-family:Calibri;\r\n\tpanose-1:2 15 5 2 2 2 4 3 2 4;}\r\n@font-face\r\n\t{font-family:\"\\@新細明體\";\r\n\tpanose-1:2 1 6 1 0 1 1 1 1 1;}\r\n@font-face\r\n\t{font-family:\"\\@細明體\";\r\n\tpanose-1:2 1 6 9 0 1 1 1 1 1;}\r\n/* Style Definitions */\r\np.MsoNormal, li.MsoNormal, div.MsoNormal\r\n\t{margin:0cm;\r\n\tmargin-bottom:.0001pt;\r\n\tfont-size:12.0pt;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tmso-ligatures:standardcontextual;}\r\na:link, span.MsoHyperlink\r\n\t{mso-style-priority:99;\r\n\tcolor:#0563C1;\r\n\ttext-decoration:underline;}\r\na:visited, span.MsoHyperlinkFollowed\r\n\t{mso-style-priority:99;\r\n\tcolor:#954F72;\r\n\ttext-decoration:underline;}\r\np.MsoListParagraph, li.MsoListParagraph, div.MsoListParagraph\r\n\t{mso-style-priority:34;\r\n\tmargin-top:0cm;\r\n\tmargin-right:0cm;\r\n\tmargin-bottom:0cm;\r\n\tmargin-left:24.0pt;\r\n\tmargin-bottom:.0001pt;\r\n\tmso-para-margin-top:0cm;\r\n\tmso-para-margin-right:0cm;\r\n\tmso-para-margin-bottom:0cm;\r\n\tmso-para-margin-left:2.0gd;\r\n\tmso-para-margin-bottom:.0001pt;\r\n\tfont-size:12.0pt;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tmso-ligatures:standardcontextual;}\r\np.msonormal0, li.msonormal0, div.msonormal0\r\n\t{mso-style-name:msonormal;\r\n\tmso-margin-top-alt:auto;\r\n\tmargin-right:0cm;\r\n\tmso-margin-bottom-alt:auto;\r\n\tmargin-left:0cm;\r\n\tfont-size:12.0pt;\r\n\tfont-family:\"新細明體\",serif;}\r\nspan.EmailStyle19\r\n\t{mso-style-type:personal;\r\n\tfont-family:\"Times New Roman\",serif;\r\n\tcolor:windowtext;\r\n\tfont-weight:normal;\r\n\tfont-style:normal;}\r\nspan.EmailStyle20\r\n\t{mso-style-type:personal;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tcolor:#1F497D;}\r\nspan.EmailStyle21\r\n\t{mso-style-type:personal-reply;\r\n\tfont-family:\"Calibri\",sans-serif;\r\n\tcolor:#1F497D;}\r\n.MsoChpDefault\r\n\t{mso-style-type:export-only;\r\n\tfont-size:10.0pt;}\r\n@page WordSection1\r\n\t{size:612.0pt 792.0pt;\r\n\tmargin:72.0pt 90.0pt 72.0pt 90.0pt;}\r\ndiv.WordSection1\r\n\t{page:WordSection1;}\r\n/* List Definitions */\r\n@list l0\r\n\t{mso-list-id:2098211590;\r\n\tmso-list-type:hybrid;\r\n\tmso-list-template-ids:-1835215490 67698703 67698713 67698715 67698703 67698713 67698715 67698703 67698713 67698715;}\r\n@list l0:level1\r\n\t{mso-level-tab-stop:none;\r\n\tmso-level-number-position:left;\r\n\tmargin-left:24.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level2\r\n\t{mso-level-number-format:ideograph-traditional;\r\n\tmso-level-text:%2、;\r\n\tmso-level-tab-stop:none;\r\n\tmso-level-number-position:left;\r\n\tmargin-left:48.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level3\r\n\t{mso-level-number-format:roman-lower;\r\n\tmso-level-tab-stop:none;\r\n\tmso-level-number-position:right;\r\n\tmargin-left:72.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level4\r\n\t{mso-level-tab-stop:none;\r\n\tmso-level-number-position:left;\r\n\tmargin-left:96.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level5\r\n\t{mso-level-number-format:ideograph-traditional;\r\n\tmso-level-text:%5、;\r\n\tmso-level-tab-stop:none;\r\n\tmso-level-number-position:left;\r\n\tmargin-left:120.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level6\r\n\t{mso-level-number-format:roman-lower;\r\n\tmso-level-tab-stop:none;\r\n\tmso-level-number-position:right;\r\n\tmargin-left:144.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level7\r\n\t{mso-level-tab-stop:none;\r\n\tmso-level-number-position:left;\r\n\tmargin-left:168.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level8\r\n\t{mso-level-number-format:ideograph-traditional;\r\n\tmso-level-text:%8、;\r\n\tmso-level-tab-stop:none;\r\n\tmso-level-number-position:left;\r\n\tmargin-left:192.0pt;\r\n\ttext-indent:-24.0pt;}\r\n@list l0:level9\r\n\t{mso-level-number-format:roman-lower;\r\n\tmso-level-tab-stop:none;\r\n\tmso-level-number-position:right;\r\n\tmargin-left:216.0pt;\r\n\ttext-indent:-24.0pt;}\r\nol\r\n\t{margin-bottom:0cm;}\r\nul\r\n\t{margin-bottom:0cm;}\r\n--></style><!--[if gte mso 9]><xml>\r\n<o:shapedefaults v:ext=\"edit\" spidmax=\"1026\" />\r\n</xml><![endif]--><!--[if gte mso 9]><xml>\r\n<o:shapelayout v:ext=\"edit\">\r\n<o:idmap v:ext=\"edit\" data=\"1\" />\r\n</o:shapelayout></xml><![endif]--></head><body lang=ZH-TW link=\"#0563C1\" vlink=\"#954F72\" style='text-justify-trim:punctuation'><div class=WordSection1><p class=MsoNormal><b><span style='font-size:11.0pt;font-family:細明體'>各位同仁好：</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p></o:p></span></b></p><p class=MsoNormal><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p>&nbsp;</o:p></span></b></p><p class=MsoNormal><b><span style='font-size:11.0pt;font-family:細明體'>提醒您，上半年績效評核即將開始，煩請各位同仁至員工平台填寫相關表單，以利績效評核進行，各階段作業內容及時間如下：</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p></o:p></span></b></p><p class=MsoNormal><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p>&nbsp;</o:p></span></b></p><p class=MsoListParagraph style='margin-left:24.0pt;mso-para-margin-left:0gd;text-indent:-24.0pt;mso-list:l0 level1 lfo2'><![if !supportLists]><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><span style='mso-list:Ignore'>1.<span style='font:7.0pt \"Times New Roman\"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </span></span></span></b><![endif]><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>(</span></b><b><span style='font-size:11.0pt;font-family:細明體'>全體</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>)<span style='color:#1F497D'> </span></span></b><b><span style='font-size:11.0pt;font-family:細明體'>填寫自我評估表、給予主管建議評估表</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>(</span></b><b><span style='font-size:11.0pt;font-family:細明體'>務必填寫協理及技術經理</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>)</span></b><b><span style='font-size:11.0pt;font-family:細明體'>、</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>KPI</span></b><b><span style='font-size:11.0pt;font-family:細明體'>自評</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'> - 5/1~5/10<o:p></o:p></span></b></p><p class=MsoListParagraph style='margin-left:24.0pt;mso-para-margin-left:0gd;text-indent:-24.0pt;mso-list:l0 level1 lfo2'><![if !supportLists]><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><span style='mso-list:Ignore'>2.<span style='font:7.0pt \"Times New Roman\"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </span></span></span></b><![endif]><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>(</span></b><b><span style='font-size:11.0pt;font-family:細明體'>主管</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>)<span style='color:#1F497D'> </span></span></b><b><span style='font-size:11.0pt;font-family:細明體'>填寫主管給予員工建議表、</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>KPI</span></b><b><span style='font-size:11.0pt;font-family:細明體'>回饋</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'> - 5/11~5/20<o:p></o:p></span></b></p><p class=MsoListParagraph style='margin-left:24.0pt;mso-para-margin-left:0gd;text-indent:-24.0pt;mso-list:l0 level1 lfo2'><![if !supportLists]><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><span style='mso-list:Ignore'>3.<span style='font:7.0pt \"Times New Roman\"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </span></span></span></b><![endif]><b><span style='font-size:11.0pt;font-family:細明體'>績效評核面談，並請各技術經理將相關意見回饋予部門</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'> - 5/21~5/31<o:p></o:p></span></b></p><p class=MsoNormal><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p>&nbsp;</o:p></span></b></p><p class=MsoNormal><b><span style='font-size:11.0pt;font-family:細明體'>以上，若有任何問題請向組長或技術經理反應，感謝各位同仁的配合。</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p></o:p></span></b></p><p class=MsoNormal><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'><o:p>&nbsp;</o:p></span></b></p><p class=MsoNormal><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>(</span></b><b><span style='font-size:11.0pt;font-family:細明體'>本信件為自動發信，請勿回覆</span></b><b><span lang=EN-US style='font-size:11.0pt;font-family:\"Times New Roman\",serif'>)</span></b><span lang=EN-US style='font-family:\"新細明體\",serif;mso-ligatures:none'><o:p></o:p></span></p></div></body></html>";

                builder.HtmlBody = $@"
                    <html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=""http://www.w3.org/TR/REC-html40"">
                    <head>
                        <meta http-equiv=Content-Type content=""text/html; charset=big5"">
                    </head>
                    <body lang=ZH-TW>
                        <p class=MsoNormal>
                            <span style='font-size:11.0pt;font-family:細明體'>您好：</span>
                        </p>
                        <p class=MsoNormal>
                            <span style='font-size:11.0pt;font-family:細明體'>您已受技術經理指派參加</span>
                            <b><span>{training.roc_year}</span></b>
                            <b><span>年</span></b>
                            <b><span>{dateSplit[1]}</span></b>
                            <b><span>月</span></b>
                            <b><span>{dateSplit[2]}</span></b>
                            <b><span>日「{training.title}」</span></b>
                            <span>培訓課程，</span>
                        </p>
                        <p class=MsoNormal>
                          <span style='font-size:11.0pt;font-family:細明體'>
                            相關培訓課程資訊請詳附件，或至員工平台培訓紀錄與規劃模組中查詢。
                          </span>
                        </p>
                        <p class=MsoNormal>
                          <span style='font-size:11.0pt;font-family:細明體'>
                            請協助留意培訓時間地點，若有問題請及時與組長及技術經理回報，以利後續調整作業。
                          </span>
                        </p>
                        <p class=MsoNormal>
                          <span lang=EN-US style='font-size:11.0pt;font-family:""Times New Roman"",serif;color:#7F7F7F'>
                            (*</span><span style='font-size:11.0pt;font-family:細明體;color:#7F7F7F'>
                            信件由系統自動發出，請勿回復此電子郵件
                          </span><span lang=EN-US style='font-size:11.0pt;font-family:""Times New Roman"",serif;color:#7F7F7F'>)
                          </span>
                        </p>
                        <p class=MsoNormal><span lang=EN-US><o:p>&nbsp;</o:p></span></p>
                        <p class=MsoNormal><span lang=EN-US><o:p>&nbsp;</o:p></span></p>
                    </body>
                    </html>";

                if (!string.IsNullOrEmpty(training.filepath))
                {
                    string _uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/Training");

                    //builder.Attachments.Add(Path.Combine(_uploadFolder, training.filepath));

                    var attachment = builder.Attachments.Add(Path.Combine(_uploadFolder, training.filepath));
                    attachment.ContentDisposition.FileName = training.filepath;
                    attachment.ContentType.Name = training.filepath;
                    foreach (var param in attachment.ContentDisposition.Parameters)
                        param.EncodingMethod = ParameterEncodingMethod.Rfc2047;
                    foreach (var param in attachment.ContentType.Parameters)
                        param.EncodingMethod = ParameterEncodingMethod.Rfc2047;
                }
                

                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("mail.sinotech.com.tw", 25, false);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("mr84124259@mail.sinotech.com.tw", "Sino4259!");

                    client.Send(message);
                    client.Disconnect(true);
                }


            }
            catch (Exception e)
            {
                ret = false;
            }



            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress("軌二部門信箱", "mr84124259@mail.sinotech.com.tw"));
            //message.To.Add(new MailboxAddress("Mrs. Chanandler Bong", "vincenthsu@mail.sinotech.com.tw"));
            ////message.To.Add(new MailboxAddress("賣當老叔叔", "donaldlu @mail.sinotech.com.tw"));

            //            message.Body = new TextPart("plain")
            //            {
            //                Text = @"Hey Chandler,

            //I just wanted to let you know that Monica and I were going to go play some paintball, you in?

            //-- Joey"

            //            };


            

            return ret;
        }

        public bool EnsureTablesExist()
        {
            var ret = (_trainingRepository as TrainingRepository).EnsureTablesExist();
            return ret;
        }

        public void Dispose()
        {
            _trainingRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}