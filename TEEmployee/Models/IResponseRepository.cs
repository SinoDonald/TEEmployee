using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models
{
    interface IResponseRepository
    {        
        void Dispose();
        Response Get(int Id);       
        bool Update(Response response);
    }
}
