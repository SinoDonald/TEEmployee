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
        public void Dispose()
        {
            return;
        }

        public bool Update(List<Assessment> assessments, string user)
        {
            string fn = Path.Combine(_appData, $"ManageResponse/{user}.txt");
            bool ret = false;
            try
            {
                List<string> responses = new List<string>();

                foreach (var item in assessments)
                {
                    responses.Add($"{item.Id}/{item.CategoryId}/{item.Content}/{item.Choice}");
                }

                System.IO.File.WriteAllLines(fn, responses);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        public List<Assessment> GetResponse(string user)
        {
            List<Assessment> manageResponse = new List<Assessment>();

            try
            {
                string fn = Path.Combine(_appData, $"ManageResponse/{user}.txt");
                string[] lines = System.IO.File.ReadAllLines(fn);
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment manageAssessment = new Assessment();

                    manageAssessment.Id = Convert.ToInt32(subs[0]);
                    manageAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    manageAssessment.Content = subs[2];
                    manageAssessment.Choice = subs[3];
                    manageResponse.Add(manageAssessment);
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
    }
}