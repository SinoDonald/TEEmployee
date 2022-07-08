using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models
{
    public class SelfAssessmentTxtRepository : IAssessmentRepository
    {
        private string _appData = "";

        public SelfAssessmentTxtRepository()
        {
            //_appData = HttpContext.Current.Server.MapPath("~/App_Data/SelfAssessments.txt");
            _appData = HttpContext.Current.Server.MapPath("~/App_Data");
        }

        public Assessment Get(int Id)
        {
            string fn = Path.Combine(_appData, Id.ToString() + "SelfAssessments.txt");
            string[] fileText = File.ReadAllLines(fn);
            Assessment selfAssessment = new Assessment();

            selfAssessment.Id = Convert.ToInt32(fileText[0]);
            selfAssessment.CategoryId = Convert.ToInt32(fileText[1]);
            selfAssessment.Content = fileText[2];
            return selfAssessment;
        }

        public List<Assessment> GetAll()
        {
            string fn = Path.Combine(_appData, "SelfAssessments.txt");
            string[] lines = System.IO.File.ReadAllLines(fn);
            List<Assessment> selfAssessments = new List<Assessment>();

            foreach (var item in lines)
            {
                string[] subs = item.Split('\t');                            
                Assessment selfAssessment = new Assessment();

                selfAssessment.Id = Convert.ToInt32(subs[0]);
                selfAssessment.CategoryId = Convert.ToInt32(subs[1]);
                selfAssessment.Content = subs[2];
                selfAssessments.Add(selfAssessment);
            }                      

            selfAssessments = selfAssessments.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();

            return selfAssessments;
        }

        //public SelfAssessmentTxtRepository()
        //{
        //    _appData = HttpContext.Current.Server.MapPath("~/App_Data/SelfAssessments");
        //} 

        //public SelfAssessment Get(int Id)
        //{
        //    string fn = Path.Combine(_appData, Id.ToString() + ".txt");
        //    string[] fileText = File.ReadAllLines(fn);
        //    SelfAssessment selfAssessment = new SelfAssessment();

        //    selfAssessment.Id = Convert.ToInt32(fileText[0]);
        //    selfAssessment.CategoryId = Convert.ToInt32(fileText[1]);
        //    selfAssessment.Content = fileText[2];
        //    return selfAssessment;
        //}

        //public List<SelfAssessment> GetAll()
        //{
        //    var fileList = System.IO.Directory.GetFiles(_appData, "*.txt");
        //    List<SelfAssessment> selfAssessments = new List<SelfAssessment>();
        //    foreach (var item in fileList)
        //    {
        //        string[] fileText = System.IO.File.ReadAllLines(item);
        //        SelfAssessment selfAssessment = new SelfAssessment();

        //        selfAssessment.Id = Convert.ToInt32(fileText[0]);
        //        selfAssessment.CategoryId = Convert.ToInt32(fileText[1]);
        //        selfAssessment.Content = fileText[2];
        //        selfAssessments.Add(selfAssessment);
        //    }

        //    selfAssessments = selfAssessments.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();

        //    return selfAssessments;            
        //}

        public void Dispose()
        {
            return;
        }

        public bool Update(List<Assessment> assessments, string user)
        {
            string fn = Path.Combine(_appData, $"Response/{user}.txt");

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

        public bool Update(List<Assessment> assessments, string user, string state, string year)
        {
            string fn = Path.Combine(_appData, $"Response/{year}/{state}/{user}.txt");

            bool ret = false;
            try
            {
                List<string> responses = new List<string>();

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



        public List<Assessment> GetResponse(string user)
        {
            List<Assessment> selfResponse = new List<Assessment>();
            try
            {
                string fn = Path.Combine(_appData, $"Response/{user}.txt");
                string[] lines = System.IO.File.ReadAllLines(fn);
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment selfAssessment = new Assessment();

                    selfAssessment.Id = Convert.ToInt32(subs[0]);
                    selfAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    selfAssessment.Content = subs[2];
                    selfAssessment.Choice = subs[3];
                    selfResponse.Add(selfAssessment);
                }

                selfResponse = selfResponse.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();
            }
            catch(System.IO.FileNotFoundException)
            {

            }
            catch
            {

            }

            return selfResponse;
        }

        public List<Assessment> GetResponse(string user, string state, string year)
        {
            List<Assessment> selfResponse = new List<Assessment>();
            
            try
            {
                string fn = Path.Combine(_appData, $"Response/{year}/{state}/{user}.txt");
                string[] lines = System.IO.File.ReadAllLines(fn);
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment selfAssessment = new Assessment();

                    selfAssessment.Id = Convert.ToInt32(subs[0]);
                    selfAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    selfAssessment.Content = subs[2];
                    selfAssessment.Choice = subs[3];
                    selfResponse.Add(selfAssessment);
                }

                selfResponse = selfResponse.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch
            {

            }
            finally
            {

            }

            return selfResponse;
        }



        public List<Assessment> GetAllResponses()
        {
            string fn = Path.Combine(_appData, "Response");

            var responseList = System.IO.Directory.GetFiles(fn, "*.txt").OrderBy(p =>
           System.IO.Path.GetFileName(p)).ToList();

            List<Assessment> allResponses = new List<Assessment>();

            foreach (var response in responseList)
            {

                string[] lines = System.IO.File.ReadAllLines(response);
                
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment selfAssessment = new Assessment();

                    selfAssessment.Id = Convert.ToInt32(subs[0]);
                    selfAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    selfAssessment.Content = subs[2];
                    selfAssessment.Choice = subs[3];
                    allResponses.Add(selfAssessment);
                }

            }
            return allResponses;
        }

        public bool UpdateMResponse(List<Assessment> assessments, string empId, string userId)
        {
            string fn = Path.Combine(_appData, $"Response/MResponse/{empId}_{userId}.txt");

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

        public List<Assessment> GetMResponse(string empId, string userId)
        {
            List<Assessment> selfResponse = new List<Assessment>();
            try
            {
                string fn = Path.Combine(_appData, $"Response/MResponse/{empId}_{userId}.txt");
                string[] lines = System.IO.File.ReadAllLines(fn);
                foreach (var item in lines)
                {
                    string[] subs = item.Split('/');
                    Assessment selfAssessment = new Assessment();

                    selfAssessment.Id = Convert.ToInt32(subs[0]);
                    selfAssessment.CategoryId = Convert.ToInt32(subs[1]);
                    selfAssessment.Content = subs[2];
                    selfAssessment.Choice = subs[3];
                    selfResponse.Add(selfAssessment);
                }

                selfResponse = selfResponse.OrderBy(a => a.CategoryId).ThenBy(a => a.Id).ToList();
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch
            {

            }

            return selfResponse;
        }


        public List<string> GetYearList(string userId)
        {
            List<string> years = new List<string>();

            string dn = Path.Combine(_appData, "Response");  
            var dirs = System.IO.Directory.GetDirectories(dn);

            foreach (var dir in dirs)
            {
                if(File.Exists(Path.Combine(dn, $"{dir}/submit/{userId}.txt")))
                {
                    years.Add((new DirectoryInfo(dir)).Name);
                }
                else if (File.Exists(Path.Combine(dn, $"{dir}/save/{userId}.txt")))
                {
                    years.Add((new DirectoryInfo(dir)).Name);
                }
            }

            if (!years.Contains(Utilities.DayStr()))
            {
                years.Add(Utilities.DayStr());
            }

            return years.OrderByDescending(a => a).ToList();
        }
        
    }
}