using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.AgentModel
{
    internal interface IAgentRepository : IDisposable
    {
        List<Agent> GetAllAgents(string manno);
        List<Agent> GetPageAgentsByUser(string empno, string page_id);
        bool InsertAgent(Agent agent);
        bool DeleteAgent(Agent agent);
        bool UpdateAgent(Agent agent);
    }
}
