using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Ballot
{
    interface IBallotRepository : IDisposable
    {
        List<Ballot> GetBallotsByEvent(string event_name);
        Ballot GetBallotByUserAndEvent(string empno, string event_name);
        bool InsertBallot(Ballot ballot);
        bool DeleteAll();
    }
}
