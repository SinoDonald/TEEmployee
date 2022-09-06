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
            //_appData = HttpContext.Current.Server.MapPath("~/App_Data");
            _appData = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\TEEmployee\App_Data");               
        }

        public List<ProjectItem> GetAll()
        {
            string fn = Path.Combine(_appData, "ProjectItem.txt");
            List<string> lines = System.IO.File.ReadAllLines(fn).ToList();
            List<ProjectItem> projectItems = new List<ProjectItem>();
            lines.RemoveAt(0);

            foreach (var item in lines)
            {
                string[] subs = item.Split('\t');
                ProjectItem projectItem = new ProjectItem();

                projectItem.empno = subs[0];
                projectItem.depno = subs[1];
                projectItem.yymm = subs[2];
                projectItem.projno = subs[3];
                projectItem.itemno = subs[4];
                projectItem.workHour = Convert.ToInt32(subs[5]);

                projectItems.Add(projectItem);
            }

            projectItems = projectItems.OrderBy(x => x.empno).ToList();

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
    }
}