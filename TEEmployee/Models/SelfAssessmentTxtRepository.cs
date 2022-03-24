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
            _appData = HttpContext.Current.Server.MapPath("~/App_Data/SelfAssessments.txt");
        }

        public Assessment Get(int Id)
        {
            string fn = Path.Combine(_appData, Id.ToString() + ".txt");
            string[] fileText = File.ReadAllLines(fn);
            Assessment selfAssessment = new Assessment();

            selfAssessment.Id = Convert.ToInt32(fileText[0]);
            selfAssessment.CategoryId = Convert.ToInt32(fileText[1]);
            selfAssessment.Content = fileText[2];
            return selfAssessment;
        }

        public List<Assessment> GetAll()
        {
            string[] lines = System.IO.File.ReadAllLines(_appData);
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

    }
}