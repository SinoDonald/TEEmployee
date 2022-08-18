using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class ManageAssessmentTxtRepository : IAssessmentRepository
    {
        private string _appData = "";

        public ManageAssessmentTxtRepository()
        {
            _appData = HttpContext.Current.Server.MapPath("~/App_Data");
        }
        public Assessment Get(int Id)
        {
            string fn = Path.Combine(_appData, Id.ToString() + "ManageAssessments.txt");
            string[] fileText = File.ReadAllLines(fn);
            Assessment selfAssessment = new Assessment();

            selfAssessment.Id = Convert.ToInt32(fileText[0]);
            selfAssessment.CategoryId = Convert.ToInt32(fileText[1]);
            selfAssessment.Content = fileText[2];
            return selfAssessment;
        }
        public List<Assessment> GetAll()
        {
            string fn = Path.Combine(_appData, "ManageAssessments.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);
            List<Assessment> manageAssessments = new List<Assessment>();

            foreach (var item in lines)
            {
                string[] subs = item.Split('\t');
                Assessment manageAssessment = new Assessment();

                manageAssessment.Id = Convert.ToInt32(subs[0]);
                manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                manageAssessment.Content = subs[2];
                manageAssessments.Add(manageAssessment);
            }                      

            manageAssessments = manageAssessments.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();

            return manageAssessments;
        }
        public List<string> GetManageYearList(string userId)
        {
            List<string> years = new List<string>();

            string dn = Path.Combine(_appData, "ManageResponse");
            var dirs = System.IO.Directory.GetDirectories(dn);

            // 讀取有資料的年份
            foreach (var dir in dirs)
            {
                if (Directory.Exists(Path.Combine(dn, $"{dir}")))
                {
                    years.Add((new DirectoryInfo(dir)).Name);
                }
            }
            // 沒有舊有資料則新增當年度的資料夾
            if (!years.Contains(Utilities.DayStr()))
            {
                years.Add(Utilities.DayStr());
            }

            return years.OrderByDescending(a => a).ToList();
        }
        public string GetStateOfResponse(string year, string manager, string user)
        {
            if (String.IsNullOrEmpty(year))
            {
                year = Utilities.DayStr();
            }
            string state = "";
            try
            {
                string fn = Path.Combine(_appData, $"ManageResponse/{year}/{manager}/{user}.txt");
                string line = File.ReadLines(fn).FirstOrDefault();
                if (line != "")
                    state = line.Split(';')[0];
            }
            catch
            {

            }

            return state;
        }
        public List<Assessment> GetResponse(string manager)
        {
            List<Assessment> manageResponse = new List<Assessment>();

            return manageResponse;
        }
        public List<Assessment> GetResponse(string year, string manager, string user)
        {
            if (String.IsNullOrEmpty(year))
            {
                year = Utilities.DayStr();
            }
            List<Assessment> manageResponse = new List<Assessment>();
            try
            {
                string dirPath = Path.Combine(_appData, $"ManageResponse/{year}/{manager}");
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                string fn = Path.Combine(_appData, $"ManageResponse/{year}/{manager}/{user}.txt");
                if (!File.Exists(fn))
                {
                    fn = Path.Combine(_appData, $"ManageResponse/ManageAssessments.txt");
                }
                string[] lines = System.IO.File.ReadAllLines(fn);
                foreach (var item in lines)
                {
                    try
                    {
                        string[] subs = item.Split('/');
                        Assessment manageAssessment = new Assessment();

                        manageAssessment.Id = Convert.ToInt32(subs[0]);
                        manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                        manageAssessment.Content = subs[2];
                        manageAssessment.Choice = subs[3];
                        manageResponse.Add(manageAssessment);
                    }
                    catch
                    {

                    }
                }
                manageResponse = manageResponse.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch
            {

            }

            return manageResponse;
        }
        public List<Assessment> GetAllResponses()
        {
            string fn = Path.Combine(_appData, "ManageResponse");
            var responseList = System.IO.Directory.GetFiles(fn, "*.txt").OrderBy(p => System.IO.Path.GetFileName(p)).ToList();

            List<Assessment> allResponses = new List<Assessment>();

            foreach (var response in responseList)
            {
                string[] lines = System.IO.File.ReadAllLines(response);
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment manageAssessment = new Assessment();

                    manageAssessment.Id = Convert.ToInt32(subs[0]);
                    manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    manageAssessment.Content = subs[2];
                    manageAssessment.Choice = subs[3];
                    allResponses.Add(manageAssessment);
                }
            }
            return allResponses;
        }
        public bool Update(List<Assessment> assessments, string state, string year, string manager, string user)
        {
            string fn = Path.Combine(_appData, $"ManageResponse/{year}/{manager}/{user}.txt");
            bool ret = false;
            try
            {
                List<string> responses = new List<string>();

                responses.Add($"{state};{DateTime.Now}");

                foreach (var item in assessments)
                {
                    responses.Add($"{item.Id}/{item.CategoryId}/{item.Content}/{item.Choice}");
                }

                (new FileInfo(fn)).Directory.Create();
                System.IO.File.WriteAllLines(fn, responses);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        public bool Update(List<Assessment> assessments, string user)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            return;
        }
    }
}