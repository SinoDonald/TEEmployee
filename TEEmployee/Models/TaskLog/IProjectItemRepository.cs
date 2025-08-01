﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.TaskLog
{
    interface IProjectItemRepository
    {
        ProjectItem Get(int id);
        List<ProjectItem> GetAll();
        bool Insert(ProjectItem projectItem);
        bool Insert(List<ProjectItem> projectItem);
        bool Upsert(ProjectItem projectItem);
        bool Upsert(List<ProjectItem> projectItem);
        bool Update(ProjectItem projectItem);
        bool Delete(ProjectItem projectItem);
        bool Delete(List<ProjectItem> projectItems);
        List<ProjectItem> GetProjectItemsByEmpnoAndYYMM(string empno, string yymm);
        void Dispose();
    }
}
