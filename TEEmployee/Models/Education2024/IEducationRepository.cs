using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Education2024 
{
    internal interface IEducationRepository : IDisposable
    {
        List<Content> GetAllContents();
    }
}
