using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    public class ProjectItemTxtRepository : IProjectItemRepository
    {
        private string _appData = "";

        public ProjectItemTxtRepository()
        {
            //Web
            _appData = HttpContext.Current.Server.MapPath("~/App_Data");

            //Console
            //_appData = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\TEEmployee\App_Data");               
        }

        public List<ProjectItem> GetAll()
        {
            List<ProjectItem> projectItems = new List<ProjectItem>();


            try
            {
                string fn = Path.Combine(_appData, "ProjectItem.txt");
                List<string> lines = System.IO.File.ReadAllLines(fn).ToList();


                //========= v1 ===============  
                //lines.RemoveAt(0);

                //foreach (var item in lines)
                //{
                //    string[] subs = item.Split('\t');
                //    ProjectItem projectItem = new ProjectItem();

                //    projectItem.empno = subs[0];
                //    projectItem.depno = subs[1];
                //    projectItem.yymm = subs[2];
                //    projectItem.projno = subs[3];
                //    projectItem.itemno = subs[4];
                //    projectItem.workHour = Convert.ToInt32(subs[5]);

                //    projectItems.Add(projectItem);
                //}

                //========== v2 =================

                foreach (var item in lines)
                {
                    string[] subs = item.Split('\t');
                    ProjectItem projectItem = new ProjectItem();

                    projectItem.empno = subs[0];
                    projectItem.projno = subs[2];
                    projectItem.itemno = subs[3];
                    projectItem.yymm = subs[4];

                    // work type
                    if (Convert.ToInt32(subs[6]) == 0)
                        projectItem.overtime = Convert.ToInt32(subs[5]);
                    else
                        projectItem.workHour = Convert.ToInt32(subs[5]);

                    var ret = projectItems.Find(x =>
                                                x.empno == projectItem.empno &&
                                                x.projno == projectItem.projno &&
                                                x.yymm == projectItem.yymm &&
                                                x.itemno == projectItem.itemno);

                    if (ret is object)
                    {
                        ret.workHour += projectItem.workHour;
                        ret.overtime += projectItem.overtime;
                    }
                    else
                        projectItems.Add(projectItem);

                }


                //-------------------------------------------------------------

                projectItems = projectItems.OrderBy(x => x.empno).ToList();

                // Delete the resource after reading it

                //File.Delete(fn);
                


            }
            catch
            {

            }
            

            return projectItems;
        }

        public bool Delete(ProjectItem projectItem)
        {
            throw new NotImplementedException();
        }
        

        public void Dispose()
        {
            return;
        }

        public ProjectItem Get(int id)
        {
            throw new NotImplementedException();
        }       

        public bool Insert(ProjectItem projectItem)
        {
            throw new NotImplementedException();
        }

        public bool Update(ProjectItem projectItem)
        {
            throw new NotImplementedException();
        }

        public bool Upsert(ProjectItem projectItem)
        {
            throw new NotImplementedException();
        }

        public bool Insert(List<ProjectItem> projectItem)
        {
            throw new NotImplementedException();
        }

        public bool Upsert(List<ProjectItem> projectItem)
        {
            throw new NotImplementedException();
        }

        public bool Delete(List<ProjectItem> projectItems)
        {
            throw new NotImplementedException();
        }
    }
}