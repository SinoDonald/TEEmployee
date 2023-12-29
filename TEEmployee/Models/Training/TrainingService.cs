using Newtonsoft.Json;
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

        public TrainingService()
        {
            _trainingRepository = new TrainingRepository();
        }

        public string GetAllRecords()
        {
            var ret = _trainingRepository.GetAllRecords();
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(ret, settings);
        }

        public string GetAllRecordsByUser(string user_empno, string self_empno)
        {
            var ret = _trainingRepository.GetAllRecordsByUser(user_empno).OrderBy(x => x.start_date);
            var settings = new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" };
            return JsonConvert.SerializeObject(ret, settings);
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

        public void Dispose()
        {
            _trainingRepository.Dispose();
            //_userRepository.Dispose();
        }
    }
}