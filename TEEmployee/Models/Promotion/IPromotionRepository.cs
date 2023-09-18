using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Promotion
{
    interface IPromotionRepository : IDisposable
    {
        List<Promotion> GetAll();
        List<Promotion> GetByUser(string empno);
        bool Insert(List<Promotion> promotions);
        bool Update(Promotion promotion);
    }
}
