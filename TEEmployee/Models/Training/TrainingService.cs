using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public string GetAllRecordsJSON()
        {
            var ret = _trainingRepository.GetAllRecords();
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(ret, settings);
        }

        public string GetAllRecordsByUserJSON(string user_empno, string self_empno)
        {
            var ret = _trainingRepository.GetAllRecordsByUser(user_empno).OrderByDescending(x => x.start_date);
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(ret, settings);
        }

        private List<Record> GetAllRecordsByUser(string user_empno)
        {
            var ret = _trainingRepository.GetAllRecordsByUser(user_empno).OrderByDescending(x => x.start_date).ToList();
            return ret;
        }

        public bool UploadTrainingFile(Stream input)
        {
            // Insert KpiModels then Insert Kpiitems
            List<Record> records = ProcessTrainingStream(input);
            bool ret = _trainingRepository.InsertRecords(records);
            return ret;
        }

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

        public void Dispose()
        {
            _trainingRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}