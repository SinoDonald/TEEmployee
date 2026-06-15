using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEEmployee.Models.Forum;

namespace TEEmployee.Models.Wish
{
    interface IWishRepository : IDisposable
    {
        List<Wish> GetAll();
        bool InsertWish(Wish wish);
        bool UpdateWishStatus(Wish issue);
    }
}
