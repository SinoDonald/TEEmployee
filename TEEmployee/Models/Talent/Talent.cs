using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace TEEmployee.Models
{
    public class Ability
    {
        public string empno { get; set; }
        public string domainSkill { get; set; }
        public string coreSkill { get; set; }
        public string manageSkill { get; set; }
    }
}