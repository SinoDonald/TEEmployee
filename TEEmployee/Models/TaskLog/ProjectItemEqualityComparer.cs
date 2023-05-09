using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.TaskLog
{
    public class ProjectItemEqualityComparer : IEqualityComparer<ProjectItem>
    {

        public bool Equals(ProjectItem p1, ProjectItem p2)
        {
            return p1.empno == p2.empno && p1.projno == p2.projno && p1.yymm == p2.yymm;
        }

        public int GetHashCode(ProjectItem p)
        {            
            return p.GetHashCode();
        }

    }
}